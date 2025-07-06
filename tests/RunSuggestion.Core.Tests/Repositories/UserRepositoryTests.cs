using RunSuggestion.Core.Models.Runs;
using RunSuggestion.Core.Repositories;
using RunSuggestion.Core.Tests.TestHelpers;
using Range = System.Range;

namespace RunSuggestion.Core.Tests.Repositories;

public class UserRepositoryTests
{
    #region Common Test Setup
    private readonly UserRepository _sut;
    private const string TestConnectionString = "Data Source=:memory:";

    public UserRepositoryTests()
    {
        _sut = new UserRepository(TestConnectionString);
    }
    #endregion

    #region CreateUserAsync Tests
    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("f0f0f0f0-f0f0-f0f0-f0f0-f0f0f0f0f0f0")]
    [InlineData("ffffffff-ffff-ffff-ffff-ffffffffffff")]
    [InlineData("Unique identification string")]
    public async Task CreateUserAsync_WithAValidEntraId_ReturnsInternalUserId(string entraId)
    {
        // Act
        int userId = await _sut.CreateUserAsync(entraId);

        // Assert
        userId.ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task CreateUserAsync_WithAValidEntraId_ShouldReturnAValidUserId(int existingEntries)
    {
        // Arrange
        // Pre-seed the database with the required number of users
        foreach (var _ in Enumerable.Range(0, existingEntries))
        {
            await _sut.CreateUserAsync(Fakes.CreateEntraId());
        }

        // Act
        int result = await _sut.CreateUserAsync(Fakes.CreateEntraId());

        // Assert
        result.ShouldBeGreaterThan(existingEntries);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("f0f0f0f0-f0f0-f0f0-f0f0-f0f0f0f0f0f0")]
    [InlineData("ffffffff-ffff-ffff-ffff-ffffffffffff")]
    [InlineData("Unique identification string")]
    public async Task CreateUserAsync_WithADuplicateEntraId_ShouldThrowInvalidArgument(string entraId)
    {
        // Arrange
        await _sut.CreateUserAsync(entraId);

        // Act
        var duplicateEntraId = async () => await _sut.CreateUserAsync(entraId);

        // Assert
        Exception ex = await duplicateEntraId.ShouldThrowAsync<ArgumentException>();
        ex.Message.ShouldContain("EntraID");
        ex.Message.ShouldContain("already exists");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n")]
    [InlineData("\r")]
    [InlineData("\r\n")]
    [InlineData(null)]
    public async Task CreateUserAsync_WithAnInvalidEntraId_ShouldThrowInvalidArgument(string? invalidEntraId)
    {
        // Act
        var nullEntraId = async () => await _sut.CreateUserAsync(invalidEntraId!);

        // Assert
        Exception ex = await nullEntraId.ShouldThrowAsync<ArgumentException>();
        ex.Message.ShouldContain("entraID");
    }
    #endregion
    
    #region AddRunEventAsync Tests
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task AddRunEventAsync_WithMultipleRunEvents_ReturnsExpectedRowsAffected(int rowCount)
    {
        // Arrange
        int userId = await _sut.CreateUserAsync(Fakes.CreateEntraId());
        IEnumerable<RunEvent> runEvents = Enumerable.Range(0, rowCount)
            .Select(_ => Fakes.CreateRunEvent());

        // Act
        int result = await _sut.AddRunEventsAsync(userId, runEvents);

        // Assert
        result.ShouldBe(rowCount);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(Int32.MinValue)]
    public async Task AddRunEventAsync_WithInvalidUserId_ThrowsArgumentException(int? invalidId)
    {
        // Arrange
        IEnumerable<RunEvent> runEvents = [Fakes.CreateRunEvent()];

        // Act
        var addsWithInvalidId = async () => await _sut.AddRunEventsAsync(invalidId!.Value, runEvents);

        // Assert
        Exception ex = await addsWithInvalidId.ShouldThrowAsync<ArgumentOutOfRangeException>();
        ex.Message.ShouldContain("userId");
    }
    
    [Fact]
    public async Task AddRunEventAsync_WithNullRunEvents_ThrowsArgumentException()
    {
        // Arrange
        IEnumerable<RunEvent> nullEvents = null!;

        int userId = await _sut.CreateUserAsync(Fakes.CreateEntraId());
        IEnumerable<RunEvent> runEvents = nullEvents;

        // Act
        var addsWithNullEvent = async () => await _sut.AddRunEventsAsync(userId, runEvents);

        // Assert
        Exception ex = await addsWithNullEvent.ShouldThrowAsync<ArgumentException>();
        ex.Message.ShouldContain("RunEvents");
    }
    
    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(1, 10)]
    [InlineData(100, 1)]
    [InlineData(100, 10)]
    [InlineData(100, 100)]
    public async Task AddRunEventAsync_WithNullRunEvents_ShouldGracefullyDiscardAndShouldNotThrowException(int validEventQty, int nullEventQty)
    {
        // Arrange
        RunEvent CreateNullEvent(int _) => null!;
        IEnumerable<RunEvent> validEvents = Enumerable.Range(0, validEventQty).Select(_ => Fakes.CreateRunEvent());
        IEnumerable<RunEvent> fakeEvents = Enumerable.Range(0, nullEventQty).Select(CreateNullEvent);
        IEnumerable<RunEvent> shuffledEvents = validEvents
            .Concat(fakeEvents)
            .OrderBy(_ => Random.Shared.Next());
        int userId = await _sut.CreateUserAsync(Fakes.CreateEntraId());

        // Act
        int result = await _sut.AddRunEventsAsync(userId, shuffledEvents);

        // Assert
        result.ShouldBe(validEventQty);
    } 
    
    
    #endregion
}