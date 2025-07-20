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
    /// Test helper to setup mock authenticator to return a created entraId.
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
    [InlineData(1000)]
    public async Task PostRunHistory_WithValidCsvAndAuthentication_ReturnsSuccessAndStoresData(int rowCount)
    {
        // Arrange
        string authToken = $"Bearer {Guid.NewGuid()}";
        string entraId = SetupAuthenticatorMock(authToken);

        string csv = TrainingPeaksCsvBuilder.CsvFromActivities(TrainingPeaksActivityFakes.CreateRandomRuns(rowCount));
        HttpRequest request = HttpRequestHelper.CreateHttpRequestWithHeader(
            "Authorization",
            authToken,
            "POST",
            csv,
            "text/csv");

        // Act
        IActionResult result = await _sut.Run(request);

        // Assert
        OkObjectResult okResult = result.ShouldBeOfType<OkObjectResult>();
        UploadResponse response = okResult.Value.ShouldBeOfType<UploadResponse>();
        response.RowsAdded.ShouldBe(rowCount);
        response.Message.ShouldContain("Success");
    }
}
