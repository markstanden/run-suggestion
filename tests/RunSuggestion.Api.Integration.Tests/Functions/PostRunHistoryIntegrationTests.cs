using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RunSuggestion.Api.Dto;
using RunSuggestion.Api.Functions;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Models.Runs;
using RunSuggestion.Core.Repositories;
using RunSuggestion.Core.Services;
using RunSuggestion.Core.Transformers;
using RunSuggestion.Core.Validators;
using RunSuggestion.TestHelpers.Creators;

namespace RunSuggestion.Api.Integration.Tests.Functions;

public class PostRunHistoryIntegrationTests
{
    private readonly Mock<ILogger<PostRunHistory>> _mockLogger = new();
    private readonly Mock<IAuthenticator> _authenticator = new();

    private readonly PostRunHistory _sut;


    public PostRunHistoryIntegrationTests()
    {
        string connectionString = "Data Source=:memory:";
        IUserRepository userRepository = new UserRepository(connectionString);
        ICsvParser csvParser = new CsvParser();
        IRunHistoryTransformer transformer = new CsvToRunHistoryTransformer(csvParser);
        IValidator<RunEvent> validator = new RunEventValidator(DateTime.Now);
        IRunHistoryAdder historyService = new TrainingPeaksHistoryService(userRepository, transformer, validator);

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
    public async Task PostRunHistory_WithValidCsvAndAuthentication_ReturnsSuccessAndStoresData(int rowCount)
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
    [InlineData(1000, 10000)] // 1,000 run events would be nearly 3 years of daily runs
    [InlineData(10000, 10000)] // 10,000 run events would be nearly 30 years of daily runs!
    public async Task PostRunHistory_WithValidLargeCsvAndAuthentication_ReturnsSuccessAndStoresDataWithinPermittedTime(
        int rowCount, int nfrMaxPermittedRequestDurationMs = 10000)
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
        OkObjectResult okResult = result.ShouldBeOfType<OkObjectResult>();
        UploadResponse response = okResult.Value.ShouldBeOfType<UploadResponse>();
        response.RowsAdded.ShouldBe(rowCount);
        response.Message.ShouldContain("Success");
        stopwatch.ElapsedMilliseconds.ShouldBeLessThanOrEqualTo(nfrMaxPermittedRequestDurationMs);
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
