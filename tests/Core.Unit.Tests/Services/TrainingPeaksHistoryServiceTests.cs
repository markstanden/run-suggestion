using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Services;
using RunSuggestion.Shared.Models.Runs;
using RunSuggestion.Shared.Models.Users;
using RunSuggestion.TestHelpers.Creators;

namespace RunSuggestion.Core.Unit.Tests.Services;

[TestSubject(typeof(TrainingPeaksHistoryService))]
public class TrainingPeaksHistoryServiceTests
{
    private readonly Mock<IRunHistoryTransformer> _mockTransformer = new();
    private readonly Mock<IUserRepository> _mockRepository = new();
    private readonly Mock<IValidator<RunEvent>> _mockValidator = new();
    private readonly TrainingPeaksHistoryService _sut;

    public TrainingPeaksHistoryServiceTests()
    {
        _sut = new TrainingPeaksHistoryService(_mockRepository.Object, _mockTransformer.Object, _mockValidator.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullRepositoryArgument_ThrowsArgumentNullException()
    {
        // Arrange
        const string expectedParamName = "userRepository";
        IUserRepository nullRepositoryArgument = null!;

        // Act
        Func<TrainingPeaksHistoryService> withNullRepositoryArgument = () =>
            new TrainingPeaksHistoryService(nullRepositoryArgument, _mockTransformer.Object, _mockValidator.Object);

        // Assert
        ArgumentNullException ex = withNullRepositoryArgument.ShouldThrow<ArgumentNullException>();
        ex.ParamName.ShouldBe(expectedParamName);
    }

    [Fact]
    public void Constructor_WithNullTransformerArgument_ThrowsArgumentNullException()
    {
        // Arrange
        const string expectedParamName = "runHistoryTransformer";
        IRunHistoryTransformer nullTransformerArgument = null!;

        // Act
        Func<TrainingPeaksHistoryService> withNullTransformerArgument = () =>
            new TrainingPeaksHistoryService(_mockRepository.Object, nullTransformerArgument, _mockValidator.Object);

        // Assert
        ArgumentNullException ex = withNullTransformerArgument.ShouldThrow<ArgumentNullException>();
        ex.ParamName.ShouldBe(expectedParamName);
    }

    [Fact]
    public void Constructor_WithNullValidatorArgument_ThrowsArgumentNullException()
    {
        // Arrange
        const string expectedParamName = "validator";
        IValidator<RunEvent> nullValidator = null!;

        // Act
        Func<TrainingPeaksHistoryService> withNullValidatorArgument = () =>
            new TrainingPeaksHistoryService(_mockRepository.Object, _mockTransformer.Object, nullValidator);

        // Assert
        ArgumentNullException ex = withNullValidatorArgument.ShouldThrow<ArgumentNullException>();
        ex.ParamName.ShouldBe(expectedParamName);
    }

    #endregion

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task AddRunHistory_WithInvalidEntraId_ThrowsArgumentException(string invalidEntraId)
    {
        // Arrange
        string validCsv = new TrainingPeaksCsvBuilder().Build();

        // Act
        Func<Task<int>> withInvalidEntraId = async () => await _sut.AddRunHistory(invalidEntraId, validCsv);

        // Assert
        Exception ex = await withInvalidEntraId.ShouldThrowAsync<ArgumentException>();
        ex.Message.ShouldContain("Invalid");
        ex.Message.ShouldContain("entraId");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task AddRunHistory_WithInvalidCsv_ThrowsArgumentException(string? invalidCsv)
    {
        // Arrange
        string validEntraId = EntraIdFakes.CreateEntraId();

        // Act
        Func<Task<int>> withInvalidCsv = async () => await _sut.AddRunHistory(validEntraId, invalidCsv!);

        // Assert
        Exception ex = await withInvalidCsv.ShouldThrowAsync<ArgumentException>();
        ex.Message.ShouldContain("Invalid");
        ex.Message.ShouldContain("csv");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task AddRunHistory_WithInvalidCsv_CreatesUser(string? invalidCsv)
    {
        // Arrange
        string validEntraId = EntraIdFakes.CreateEntraId();

        // Act
        Func<Task<int>> withInvalidCsv = async () => await _sut.AddRunHistory(validEntraId, invalidCsv!);
        await withInvalidCsv.ShouldThrowAsync<ArgumentException>();

        // Assert
        _mockRepository.Verify(x => x.CreateUserAsync(validEntraId), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task AddRunHistory_WithInvalidCsv_DoesNotCallTransformerWithInvalidCsv(string? invalidCsv)
    {
        // Arrange
        string validEntraId = EntraIdFakes.CreateEntraId();

        // Act
        Func<Task<int>> withInvalidCsv = async () => await _sut.AddRunHistory(validEntraId, invalidCsv!);
        await withInvalidCsv.ShouldThrowAsync<ArgumentException>();

        // Assert
        _mockTransformer.Verify(x => x.Transform(invalidCsv!), Times.Never);
    }

    [Fact]
    public async Task AddRunHistory_WhenPassedValidCsv_CallsTransformerOnceWithPassedCsv()
    {
        // Arrange
        string validEntraId = EntraIdFakes.CreateEntraId();
        string validCsv = new TrainingPeaksCsvBuilder().Build();

        // Act
        await _sut.AddRunHistory(validEntraId, validCsv);

        // Assert
        _mockTransformer.Verify(x => x.Transform(validCsv), Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task AddRunHistory_WithExistingUser_CallsRepositoryWithCorrectUserId(int expectedUserId)
    {
        // Arrange
        string entraId = EntraIdFakes.CreateEntraId();
        string validCsv = new TrainingPeaksCsvBuilder().Build();
        UserData userData = new() { UserId = expectedUserId, EntraId = entraId };
        _mockRepository.Setup(x => x.GetUserDataByEntraIdAsync(entraId))
            .ReturnsAsync(userData);

        // Act
        await _sut.AddRunHistory(entraId, validCsv);

        // Assert
        _mockRepository.Verify(x => x.AddRunEventsAsync(expectedUserId, It.IsAny<IEnumerable<RunEvent>>()),
                               Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task AddRunHistory_WithNewUser_CallsRepositoryWithCorrectUserId(int expectedUserId)
    {
        // Arrange
        string entraId = EntraIdFakes.CreateEntraId();
        string validCsv = new TrainingPeaksCsvBuilder().Build();
        // Call to get userdata returns null for a not found user
        _mockRepository.Setup(x => x.GetUserDataByEntraIdAsync(entraId))
            .ReturnsAsync(null as UserData);
        // Call to create a new user returns our expectedUserId
        _mockRepository.Setup(x => x.CreateUserAsync(entraId))
            .ReturnsAsync(expectedUserId);

        // Act
        await _sut.AddRunHistory(entraId, validCsv);

        // Assert
        _mockRepository.Verify(x => x.AddRunEventsAsync(expectedUserId, It.IsAny<IEnumerable<RunEvent>>()),
                               Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task AddRunHistory_WhenTransformerReturnsValidRunEvents_CallsRepositoryWithCorrectRunEvents(
        int expectedEventCount)
    {
        // Arrange
        string validEntraId = EntraIdFakes.CreateEntraId();
        string validCsv = new TrainingPeaksCsvBuilder().Build();
        IEnumerable<RunEvent> expectedRunEvents = RunBaseFakes.CreateRunEvents(expectedEventCount).ToList();
        _mockTransformer.Setup(x => x.Transform(It.IsAny<string>()))
            .Returns(expectedRunEvents);
        _mockRepository.Setup(x => x.AddRunEventsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<RunEvent>>()))
            .ReturnsAsync(expectedEventCount);

        // Act
        await _sut.AddRunHistory(validEntraId, validCsv);

        // Assert
        _mockRepository.Verify(x => x.AddRunEventsAsync(It.IsAny<int>(), expectedRunEvents), Times.Once);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(10, 0)]
    [InlineData(10, 1)]
    [InlineData(10, 10)]
    [InlineData(100, 0)]
    [InlineData(100, 1)]
    [InlineData(100, 99)]
    [InlineData(100, 100)]
    public async Task AddRunHistory_WhenRepositoryMethodReturns_ReturnsActualAffectedLines(int expectedEventCount,
        int actualAffectedLines)
    {
        // Arrange
        string validEntraId = EntraIdFakes.CreateEntraId();
        string validCsv = new TrainingPeaksCsvBuilder().Build();
        IEnumerable<RunEvent> expectedRunEvents = RunBaseFakes.CreateRunEvents(expectedEventCount);
        _mockTransformer.Setup(x => x.Transform(It.IsAny<string>()))
            .Returns(expectedRunEvents);
        _mockRepository.Setup(x => x.AddRunEventsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<RunEvent>>()))
            .ReturnsAsync(actualAffectedLines);

        // Act
        int result = await _sut.AddRunHistory(validEntraId, validCsv);

        // Assert
        result.ShouldBe(actualAffectedLines);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task AddRunHistory_AfterTransformation_CallsValidatorWithTransformedEvents(int eventCount)
    {
        // Arrange
        string validEntraId = EntraIdFakes.CreateEntraId();
        string validCsv = new TrainingPeaksCsvBuilder().Build();
        IEnumerable<RunEvent> expectedRunEvents = RunBaseFakes.CreateRunEvents(eventCount).ToList();

        _mockTransformer.Setup(x => x.Transform(validCsv))
            .Returns(expectedRunEvents);
        _mockValidator.Setup(x => x.Validate(expectedRunEvents))
            .Returns([]);

        // Act
        await _sut.AddRunHistory(validEntraId, validCsv);

        // Assert
        _mockValidator.Verify(x => x.Validate(expectedRunEvents), Times.Once);
    }

    [Theory]
    [InlineData("0: Invalid run date 2027/01/01 - cannot be in the future")]
    [InlineData("1: Invalid run distance - it must be a positive integer")]
    [InlineData("10: Invalid run effort - it must be between 0 and 10")]
    [InlineData("100: Invalid run duration - it must be greater than 0")]
    public async Task
        AddRunHistory_WithRunHistoryValidationErrors_ThrowsArgumentExceptionWithReasonForValidationFailure(string error)
    {
        // Arrange
        string validEntraId = EntraIdFakes.CreateEntraId();
        string validCsv = new TrainingPeaksCsvBuilder().Build();
        IEnumerable<RunEvent> expectedRunEvents = RunBaseFakes.CreateRunEvents().ToList();

        _mockTransformer.Setup(x => x.Transform(validCsv))
            .Returns(expectedRunEvents);
        _mockValidator.Setup(x => x.Validate(expectedRunEvents))
            .Returns([error]);

        // Act
        Func<Task<int>> withInvalidRunEvents = async () => await _sut.AddRunHistory(validEntraId, validCsv);

        // Assert
        Exception ex = await withInvalidRunEvents.ShouldThrowAsync<ArgumentException>();
        ex.Message.ShouldContain(error);
    }

    [Fact]
    public async Task
        AddRunHistory_WithAnyValidationFailure_DoesNotCallRepositoryAddRunEventMethodWithInvalidRunEvents()
    {
        // Arrange
        string validEntraId = EntraIdFakes.CreateEntraId();
        string invalidCsv = new TrainingPeaksCsvBuilder().Build();

        _mockValidator.Setup(x => x.Validate(It.IsAny<IEnumerable<RunEvent>>()))
            .Returns(["Any validation error"]);

        // Act
        Func<Task<int>> withInvalidCsv = async () => await _sut.AddRunHistory(validEntraId, invalidCsv);
        await withInvalidCsv.ShouldThrowAsync<ArgumentException>();

        // Assert
        _mockRepository.Verify(x => x.AddRunEventsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<RunEvent>>()),
                               Times.Never);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task AddRunHistory_WithMultipleValidationErrors_IncludesAllErrorsInException(int errorCount)
    {
        // Arrange
        string validEntraId = EntraIdFakes.CreateEntraId();
        string validCsv = new TrainingPeaksCsvBuilder().Build();
        IEnumerable<string> errors = Enumerable.Range(1, errorCount).Select(x => $"Error {x}").ToList();

        _mockValidator.Setup(x => x.Validate(It.IsAny<IEnumerable<RunEvent>>()))
            .Returns(errors);

        // Act
        Func<Task<int>> withMultipleErrors = async () => await _sut.AddRunHistory(validEntraId, validCsv);

        // Assert
        Exception ex = await withMultipleErrors.ShouldThrowAsync<ArgumentException>();
        foreach (string error in errors)
        {
            ex.Message.ShouldContain(error);
        }
    }
}
