using RunSuggestion.Core.Models.Runs;
using RunSuggestion.Core.Models.Users;
using RunSuggestion.Core.Repositories;
using RunSuggestion.Core.Tests.TestHelpers;

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

    #region Common Test Helpers

    /// <summary>
    /// Preseeds the database with the specified number of users each with a specified number of events registered
    /// </summary>
    /// <param name="userCount">The number of users to register, defaults to 100</param>
    /// <param name="eventsPerUser">The number of events to register per user, defaults to 10</param>
    private async Task PreseedDatabase(int userCount = 100, int eventsPerUser = 10)
    {
        foreach (int _ in Enumerable.Range(0, userCount))
        {
            int userId = await _sut.CreateUserAsync(Fakes.CreateEntraId());
            await _sut.AddRunEventsAsync(userId, Fakes.CreateRunEvents(eventsPerUser));
        }
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

    #region GetUserDataByUserIdAsync Tests

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task GetUserDataByUserIdAsync_WithMultipleRunEvents_ReturnsExpectedRunHistoryCount(int eventQty)
    {
        // Arrange
        IEnumerable<RunEvent> runEvents = Fakes.CreateRunEvents(eventQty);
        int userId = await _sut.CreateUserAsync(Fakes.CreateEntraId());
        await _sut.AddRunEventsAsync(userId, runEvents);

        // Act
        UserData? userData = await _sut.GetUserDataByUserIdAsync(userId);

        // Assert
        userData.ShouldNotBeNull();
        userData.RunHistory.Count().ShouldBe(eventQty);
    }

    [Fact]
    public async Task GetUserDataByUserIdAsync_WithMultipleRunEvents_ReturnsCorrectRunHistory()
    {
        // Arrange
        DateTime baseDate = DateTime.UtcNow;
        RunEvent runEvent1 = Fakes.CreateRunEvent(dateTime: baseDate.AddDays(-1));
        RunEvent runEvent2 = Fakes.CreateRunEvent(dateTime: baseDate.AddDays(-2));
        RunEvent runEvent3 = Fakes.CreateRunEvent(dateTime: baseDate.AddDays(-3));
        IEnumerable<RunEvent> runEvents = [runEvent1, runEvent2, runEvent3];
        int userId = await _sut.CreateUserAsync(Fakes.CreateEntraId());
        await _sut.AddRunEventsAsync(userId, runEvents);

        // Act
        UserData? userData = await _sut.GetUserDataByUserIdAsync(userId);

        // Assert
        userData.ShouldNotBeNull();
        userData.RunHistory.ShouldContain(runEvent1);
        userData.RunHistory.ShouldContain(runEvent2);
        userData.RunHistory.ShouldContain(runEvent3);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task GetUserDataByUserIdAsync_WithMultipleRunEvents_ReturnsCorrectUserId(int eventQty)
    {
        // Arrange
        IEnumerable<RunEvent> runEvents = Fakes.CreateRunEvents(eventQty);
        int userId = await _sut.CreateUserAsync(Fakes.CreateEntraId());
        await _sut.AddRunEventsAsync(userId, runEvents);

        // Act
        UserData? userData = await _sut.GetUserDataByUserIdAsync(userId);

        // Assert
        userData.ShouldNotBeNull();
        userData.UserId.ShouldBe(userId);
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public async Task GetUserDataByUserIdAsync_WithNonExistentUserId_ReturnsNull(int invalidUserId)
    {
        // Arrange
        await PreseedDatabase();

        // Act
        UserData? userData = await _sut.GetUserDataByUserIdAsync(invalidUserId);

        // Assert
        userData.ShouldBeNull();
    }

    #endregion

    #region AddRunEventAsync Tests

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task AddRunEventAsync_WithMultipleRunEvents_ReturnsExpectedRowsAffected(int eventQty)
    {
        // Arrange
        int userId = await _sut.CreateUserAsync(Fakes.CreateEntraId());
        IEnumerable<RunEvent> runEvents = Fakes.CreateRunEvents(eventQty);

        // Act
        int result = await _sut.AddRunEventsAsync(userId, runEvents);

        // Assert
        result.ShouldBe(eventQty);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(Int32.MinValue)]
    public async Task AddRunEventAsync_WithInvalidUserId_ThrowsArgumentException(int? invalidId)
    {
        // Arrange
        IEnumerable<RunEvent> runEvents = Fakes.CreateRunEvents();

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
        int userId = await _sut.CreateUserAsync(Fakes.CreateEntraId());
        IEnumerable<RunEvent> runEvents = null!;

        // Act
        var addsWithNullEventCollection = async () => await _sut.AddRunEventsAsync(userId, runEvents);

        // Assert
        Exception ex = await addsWithNullEventCollection.ShouldThrowAsync<ArgumentException>();
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
        IEnumerable<RunEvent> validEvents = Fakes.CreateRunEvents(validEventQty);
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

    #region GetRunEventsByUserIdAsync Tests

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task GetRunEventsByUserIdAsync_WithMultipleEvents_ReturnsAllEvents(int eventQty)
    {
        // Arrange
        int userId = await _sut.CreateUserAsync(Fakes.CreateEntraId());
        IEnumerable<RunEvent> events = Fakes.CreateRunEvents(eventQty);
        await _sut.AddRunEventsAsync(userId, events);

        // Act
        IEnumerable<RunEvent> result = await _sut.GetRunEventsByUserIdAsync(userId);

        // Assert
        result.Count().ShouldBe(eventQty);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 3)]
    [InlineData(3, 10)]
    [InlineData(5, 2)]
    public async Task GetRunEventsByUserIdAsync_WithMultipleUsersAndEvents_ReturnsExpectedEventCountForUser(int userCount, int eventsPerUser)
    {
        // Arrange
        List<int> userIds = [];
        foreach (int _ in Enumerable.Range(0, userCount))
        {
            int userId = await _sut.CreateUserAsync(Fakes.CreateEntraId());
            userIds.Add(userId);

            IEnumerable<RunEvent> events = Fakes.CreateRunEvents(eventsPerUser);
            await _sut.AddRunEventsAsync(userId, events);
        }

        // Act
        IEnumerable<RunEvent> result = await _sut.GetRunEventsByUserIdAsync(userIds.First());

        // Assert
        result.Count().ShouldBe(eventsPerUser);
    }

    [Theory]
    [InlineData(2, 1000)]
    [InlineData(10, 100)]
    [InlineData(100, 10)]
    public async Task GetRunEventsByUserIdAsync_WithMultipleInsertions_ReturnsAllEvents(int totalInsertions, int eventsPerInsertion)
    {
        // Arrange
        int expectedRecordCount = totalInsertions * eventsPerInsertion;
        int userId = await _sut.CreateUserAsync(Fakes.CreateEntraId());
        foreach (int _ in Enumerable.Range(0, totalInsertions))
        {
            IEnumerable<RunEvent> events = Fakes.CreateRunEvents(eventsPerInsertion);
            await _sut.AddRunEventsAsync(userId, events);
        }

        // Act
        IEnumerable<RunEvent> result = await _sut.GetRunEventsByUserIdAsync(userId);

        // Assert
        result.Count().ShouldBe(expectedRecordCount);
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public async Task GetRunEventsByUserIdAsync_WithInvalidUserId_ReturnsEmptyCollection(int invalidUserId)
    {
        // Arrange
        await PreseedDatabase();

        // Act
        IEnumerable<RunEvent> result = await _sut.GetRunEventsByUserIdAsync(invalidUserId);

        // Assert
        result.ShouldBeEmpty();
    }
    #endregion
}