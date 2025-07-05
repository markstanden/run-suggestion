using RunSuggestion.Core.Models.Runs;
using RunSuggestion.Core.Repositories;
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

    #region Test Helper Methods
    private string CreateFakeEntraId() => Guid.NewGuid().ToString();

    /// <summary>
    /// Test helper to create a fake run event.
    /// The pattern of providing default null values for parameters and then assigning a default value within the helper
    /// Allows default values to be provided by functions, which are not allowed in method signature. 
    /// </summary>
    /// <param name="userId">The UserId to assign the RunEvent to, defaults to 0</param>
    /// <param name="dateTime">The date the event took place, defaults to Now</param>
    /// <param name="distanceMetres">the distance of the run in metres - defaults to 5km</param>
    /// <param name="effort">The effort score for the run (1-10), defaults to 5</param>
    /// <param name="duration">The duration of the event as a TimeSpan.  Defaults to 30mins</param>
    /// <returns>RunEvent</returns>
    private RunEvent CreateFakeRunEvent(
        int? userId = null, 
        DateTime? dateTime = null, 
        int? distanceMetres = null,
        byte? effort = null,
        TimeSpan? duration = null) => new RunEvent
    {
        Id = userId ?? 0,
        Date = dateTime ?? DateTime.Now,
        Distance = distanceMetres ?? 5000,
        Effort = effort ?? 5,
        Duration = duration ?? TimeSpan.FromMinutes(30)
    };

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
            await _sut.CreateUserAsync(CreateFakeEntraId());
        }

        // Act
        int result = await _sut.CreateUserAsync(CreateFakeEntraId());

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
        int userId = await _sut.CreateUserAsync(CreateFakeEntraId());
        IEnumerable<RunEvent> runEvents = Enumerable.Range(0, rowCount)
            .Select(_ => CreateFakeRunEvent());

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
        IEnumerable<RunEvent> runEvents = [CreateFakeRunEvent()];

        // Act
        var addsWithInvalidId = async () => await _sut.AddRunEventsAsync(invalidId!.Value, runEvents);

        // Assert
        Exception ex = await addsWithInvalidId.ShouldThrowAsync<ArgumentOutOfRangeException>();
        ex.Message.ShouldContain("userId");
    }
    
    [Fact]
    public async Task AddRunEventAsync_WithFutureRunEventDate_ThrowsArgumentException()
    {
        // Arrange
        DateTime tomorrow = DateTime.Today.AddDays(1);
        int userId = await _sut.CreateUserAsync(CreateFakeEntraId());
        IEnumerable<RunEvent> runEvents = [CreateFakeRunEvent(dateTime: tomorrow)];

        // Act
        var addsWithInvalidDate = async () => await _sut.AddRunEventsAsync(userId, runEvents);

        // Assert
        Exception ex = await addsWithInvalidDate.ShouldThrowAsync<ArgumentException>();
        ex.Message.ShouldContain("date");
    } 
    #endregion
}