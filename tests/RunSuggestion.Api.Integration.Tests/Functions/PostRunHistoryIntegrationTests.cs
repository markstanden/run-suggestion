using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RunSuggestion.Api.Dto;
using RunSuggestion.Api.Functions;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Models.Runs;
using RunSuggestion.Core.Models.Users;
using RunSuggestion.Core.Repositories;
using RunSuggestion.Core.Services;
using RunSuggestion.Core.Transformers;
using RunSuggestion.Core.Validators;
using RunSuggestion.TestHelpers.Creators;
using static RunSuggestion.TestHelpers.Constants.NonFunctionalRequirements;

namespace RunSuggestion.Api.Integration.Tests.Functions;

[JetBrains.Annotations.TestSubject(typeof(PostRunHistory))]
public class PostRunHistoryIntegrationTests
{
    private readonly TimeSpan _maxTestThresholdTime = TimeSpan.FromMilliseconds(
        GetTestThreshold(ApiResponse.MaxUploadFunctionRunTimeMs));

    private readonly Mock<ILogger<PostRunHistory>> _mockLogger = new();
    private readonly Mock<IAuthenticator> _authenticator = new();

    // Setting the UserRepository as an instance variable allows us to verify writes within tests.
    private readonly IUserRepository _userRepository;
    private readonly PostRunHistory _sut;


    public PostRunHistoryIntegrationTests()
    {
        const string connectionString = "Data Source=:memory:";
        _userRepository = new UserRepository(connectionString);
        ICsvParser csvParser = new CsvParser();
        IRunHistoryTransformer transformer = new CsvToRunHistoryTransformer(csvParser);
        IValidator<RunEvent> validator = new RunEventValidator(DateTime.Now);
        IRunHistoryAdder historyService = new TrainingPeaksHistoryService(_userRepository, transformer, validator);

        _sut = new PostRunHistory(
            _mockLogger.Object,
            _authenticator.Object,
            historyService);
    }

    /// <summary>
    /// Test helper to set up mock authenticator to return a created entraId.
    /// </summary>
    /// <param name="authToken">authentication token to require</param>
    /// <returns>created entraId</returns>
    private string SetupAuthenticatorMock(string? authToken = null)
    {
        string entraId = EntraIdFakes.CreateEntraId();
        _authenticator.Setup(x => x.Authenticate(authToken ?? It.IsAny<string>())).Returns(entraId);
        return entraId;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task PostRunHistory_NewUserWithValidCsvAndAuthentication_ReturnsSuccessAndUpdatedRows(int rowCount)
    {
        // Arrange
        string authToken = $"Bearer {Guid.NewGuid()}";
        SetupAuthenticatorMock(authToken);

        string csv = TrainingPeaksCsvBuilder.CsvFromActivities(TrainingPeaksActivityFakes.CreateRandomRuns(rowCount));
        HttpRequest request = HttpCsvRequestHelpers.CreateCsvUploadRequest(authToken, csv);

        // Act
        IActionResult result = await _sut.Run(request);

        // Assert
        OkObjectResult okResult = result.ShouldBeOfType<OkObjectResult>();
        UploadResponse response = okResult.Value.ShouldBeOfType<UploadResponse>();
        response.RowsAdded.ShouldBe(rowCount);
        response.Message.ShouldContain("Success");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task PostRunHistory_NewUserWithValidCsvAndAuthentication_WritesRunEventsToDB(int rowCount)
    {
        // Arrange
        string authToken = $"Bearer {Guid.NewGuid()}";
        string entraId = SetupAuthenticatorMock(authToken);

        string csv = TrainingPeaksCsvBuilder.CsvFromActivities(TrainingPeaksActivityFakes.CreateRandomRuns(rowCount));
        HttpRequest request = HttpCsvRequestHelpers.CreateCsvUploadRequest(authToken, csv);

        // Act
        await _sut.Run(request);

        // Assert
        UserData? userData = await _userRepository.GetUserDataByEntraIdAsync(entraId);
        userData.ShouldNotBeNull();
        userData.RunHistory.ShouldNotBeNull();
        userData.RunHistory.Count().ShouldBe(rowCount);
    }

    [Fact]
    public async Task PostRunHistory_NewUserWithValidEmptyCsvAndValidAuthentication_WritesNoRunEventsToDB()
    {
        // Arrange
        int zeroRowCount = 0;
        string authToken = $"Bearer {Guid.NewGuid()}";
        string entraId = SetupAuthenticatorMock(authToken);

        string csv =
            TrainingPeaksCsvBuilder.CsvFromActivities(TrainingPeaksActivityFakes.CreateRandomRuns(zeroRowCount));
        HttpRequest request = HttpCsvRequestHelpers.CreateCsvUploadRequest(authToken, csv);

        // Act
        await _sut.Run(request);

        // Assert
        UserData? userData = await _userRepository.GetUserDataByEntraIdAsync(entraId);
        userData.ShouldNotBeNull();
        userData.RunHistory.ShouldNotBeNull();
        userData.RunHistory.Count().ShouldBe(zeroRowCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("    ")]
    public async Task PostRunHistory_NewUserWithInvalidCsvAndValidAuthentication_CreatesUserButWritesNoRunEventsToDB(
        string csv)
    {
        // Arrange
        string authToken = $"Bearer {Guid.NewGuid()}";
        string entraId = SetupAuthenticatorMock(authToken);
        HttpRequest request = HttpCsvRequestHelpers.CreateCsvUploadRequest(authToken, csv);

        // Act
        await _sut.Run(request);

        // Assert
        UserData? userData = await _userRepository.GetUserDataByEntraIdAsync(entraId);
        userData.ShouldNotBeNull();
        userData.RunHistory.ShouldNotBeNull();
        userData.RunHistory.Count().ShouldBe(0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task PostRunHistory_NewUserWithInvalidCsvData_CreatesUserButWritesNoRunEventsToDB(int validRowCount)
    {
        // Arrange
        string authToken = $"Bearer {Guid.NewGuid()}";
        string entraId = SetupAuthenticatorMock(authToken);
        string csv =
            TrainingPeaksCsvBuilder.CsvFromActivities(
                TrainingPeaksActivityFakes.CreateRandomRuns(validRowCount,
                                                            TrainingPeaksActivityFakes.DefaultDateSpread * -1));
        HttpRequest request = HttpCsvRequestHelpers.CreateCsvUploadRequest(authToken, csv);

        // Act
        await _sut.Run(request);

        // Assert
        UserData? userData = await _userRepository.GetUserDataByEntraIdAsync(entraId);
        userData.ShouldNotBeNull();
        userData.RunHistory.ShouldNotBeNull();
        userData.RunHistory.Count().ShouldBe(0);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(10, 5)]
    [InlineData(100, 10)]
    public async Task PostRunHistory_ExistingUserWithValidCsvAndAuthentication_WritesAllRunEventsToDB(int rowCount,
        int uploadCount)
    {
        // Arrange
        int expectedRowTotal = rowCount * uploadCount;
        IEnumerable<int> uploadRange = Enumerable.Range(0, uploadCount);
        string authToken = $"Bearer {Guid.NewGuid()}";
        string entraId = SetupAuthenticatorMock(authToken);

        IEnumerable<HttpRequest> requests =
            uploadRange.Select(upload => TrainingPeaksCsvBuilder.CsvFromActivities(
                                   TrainingPeaksActivityFakes.CreateRandomRuns(
                                       rowCount,
                                       upload * rowCount))) // Offset each run history to ensure unique dates
                .Select(csv => HttpCsvRequestHelpers.CreateCsvUploadRequest(authToken, csv));

        // Act
        foreach (HttpRequest request in requests)
        {
            await _sut.Run(request);
        }

        // Assert
        UserData? userData = await _userRepository.GetUserDataByEntraIdAsync(entraId);
        userData.ShouldNotBeNull();
        userData.RunHistory.ShouldNotBeNull();
        userData.RunHistory.Count().ShouldBe(expectedRowTotal);
    }

    [Theory]
    [InlineData(1000)] // 1,000 run events would be nearly 3 years of daily runs
    [InlineData(10000)] // 10,000 run events would be nearly 30 years of daily runs!
    public async Task PostRunHistory_WithValidLargeCsvAndAuthentication_ReturnsSuccessWithinPermittedTime(
        int rowCount)
    {
        // Arrange
        string authToken = $"Bearer {Guid.NewGuid()}";
        SetupAuthenticatorMock(authToken);

        string csv = TrainingPeaksCsvBuilder.CsvFromActivities(TrainingPeaksActivityFakes.CreateRandomRuns(rowCount));
        HttpRequest request = HttpCsvRequestHelpers.CreateCsvUploadRequest(authToken, csv);
        Stopwatch stopwatch = Stopwatch.StartNew();

        // Act
        IActionResult result = await _sut.Run(request);

        // Assert
        stopwatch.Stop();
        // Test that the ingestion succeeds, as a failed import may return early giving a false positive
        OkObjectResult okResult = result.ShouldBeOfType<OkObjectResult>();
        UploadResponse response = okResult.Value.ShouldBeOfType<UploadResponse>();
        response.RowsAdded.ShouldBe(rowCount);
        // Test that the ingestion takes less than the time permitted by the non-functional requirements
        response.Message.ShouldContain("Success");
        stopwatch.Elapsed.ShouldBeLessThanOrEqualTo(_maxTestThresholdTime);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task PostRunHistory_WithInvalidAuthentication_ReturnsUnauthorized(int rowCount)
    {
        // Arrange
        string authToken = $"Bearer {Guid.NewGuid()}";
        string nullEntraId = null!;
        string csv = TrainingPeaksCsvBuilder.CsvFromActivities(TrainingPeaksActivityFakes.CreateRandomRuns(rowCount));
        HttpRequest request = HttpCsvRequestHelpers.CreateCsvUploadRequest(authToken, csv);

        _authenticator.Setup(x => x.Authenticate(authToken))
            .Returns(nullEntraId);

        // Act
        IActionResult result = await _sut.Run(request);

        // Assert
        result.ShouldBeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task PostRunHistory_WithEmptyCsv_ReturnsBadRequest()
    {
        // Arrange
        string authToken = $"Bearer {Guid.NewGuid()}";
        SetupAuthenticatorMock(authToken);

        HttpRequest request = HttpCsvRequestHelpers.CreateCsvUploadRequest(authToken, string.Empty);

        // Act
        IActionResult result = await _sut.Run(request);

        // Assert
        BadRequestObjectResult badResult = result.ShouldBeOfType<BadRequestObjectResult>();
        UploadResponse response = badResult.Value.ShouldBeOfType<UploadResponse>();
        response.Message.ShouldContain("Invalid CSV");
        response.RowsAdded.ShouldBe(0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task PostRunHistory_WithFutureDatesInCsv_ReturnsBadRequest(int rowCount)
    {
        // Arrange
        string authToken = $"Bearer {Guid.NewGuid()}";
        SetupAuthenticatorMock(authToken);
        string invalidCsv = TrainingPeaksCsvBuilder.CsvFromActivities(
            TrainingPeaksActivityFakes.CreateRandomRuns(rowCount, -10)); // Negative offset produces dates in the future

        HttpRequest request = HttpCsvRequestHelpers.CreateCsvUploadRequest(authToken, invalidCsv);

        // Act
        IActionResult result = await _sut.Run(request);

        // Assert
        BadRequestObjectResult badResult = result.ShouldBeOfType<BadRequestObjectResult>();
        UploadResponse response = badResult.Value.ShouldBeOfType<UploadResponse>();
        response.Message.ShouldContain("Invalid CSV");
        response.RowsAdded.ShouldBe(0);
    }
}
