using RunSuggestion.Core.Models.Runs;
using RunSuggestion.Core.Models.Users;
using RunSuggestion.Core.Repositories;
using RunSuggestion.TestHelpers.Assertions;
using RunSuggestion.TestHelpers.Creators;

namespace RunSuggestion.Core.Unit.Tests.Repositories;

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
            int userId = await _sut.CreateUserAsync(EntraIdFakes.CreateEntraId());
            await _sut.AddRunEventsAsync(userId, RunEventFakes.CreateRunEvents(eventsPerUser));
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
        foreach (int _ in Enumerable.Range(0, existingEntries))
        {
            await _sut.CreateUserAsync(EntraIdFakes.CreateEntraId());
        }

        // Act
        int result = await _sut.CreateUserAsync(EntraIdFakes.CreateEntraId());

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
        Func<Task<int>> duplicateEntraId = async () => await _sut.CreateUserAsync(entraId);

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
        Func<Task<int>> nullEntraId = async () => await _sut.CreateUserAsync(invalidEntraId!);

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
        IEnumerable<RunEvent> runEvents = RunEventFakes.CreateRunEvents(eventQty);
        int userId = await _sut.CreateUserAsync(EntraIdFakes.CreateEntraId());
        await _sut.AddRunEventsAsync(userId, runEvents);

        // Act
        UserData? userData = await _sut.GetUserDataByUserIdAsync(userId);

        // Assert
        userData.ShouldNotBeNull();
        userData.RunHistory.Count().ShouldBe(eventQty);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task GetUserDataByUserIdAsync_WithMultipleRunEvents_ReturnsCorrectRunHistory(int runEventQty)
    {
        // Arrange
        List<RunEvent> runEvents = RunEventFakes.CreateRunEvents(runEventQty).ToList();
        int userId = await _sut.CreateUserAsync(EntraIdFakes.CreateEntraId());
        await _sut.AddRunEventsAsync(userId, runEvents);

        // Act
        UserData? userData = await _sut.GetUserDataByUserIdAsync(userId);

        // Assert
        userData.ShouldNotBeNull();
        userData.RunHistory.Count().ShouldBe(runEventQty);
        userData.RunHistory.ShouldMatchRunEvents(runEvents);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task GetUserDataByUserIdAsync_WithMultipleRunEvents_ReturnsCorrectUserId(int runEventQty)
    {
        // Arrange
        IEnumerable<RunEvent> runEvents = RunEventFakes.CreateRunEvents(runEventQty);
        int userId = await _sut.CreateUserAsync(EntraIdFakes.CreateEntraId());
        await _sut.AddRunEventsAsync(userId, runEvents);

        // Act
        UserData? userData = await _sut.GetUserDataByUserIdAsync(userId);

        // Assert
        userData.ShouldNotBeNull();
        userData.UserId.ShouldBe(userId);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(int.MinValue)]
    public async Task GetUserDataByUserIdAsync_WithInvalidUserId_ThrowsArgumentException(int invalidUserId)
    {
        // Act
        Func<Task<UserData?>> withInvalidUserId = async () => await _sut.GetUserDataByUserIdAsync(invalidUserId);

        // Assert
        Exception ex = await withInvalidUserId.ShouldThrowAsync<ArgumentOutOfRangeException>();
        ex.Message.ShouldContain("Invalid");
        ex.Message.ShouldContain("userId");
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(99999)]
    [InlineData(int.MaxValue)]
    public async Task GetUserDataByUserIdAsync_WithValidNonExistentUserId_ReturnsNull(int invalidUserId)
    {
        // Arrange
        await PreseedDatabase();

        // Act
        UserData? userData = await _sut.GetUserDataByUserIdAsync(invalidUserId);

        // Assert
        userData.ShouldBeNull();
    }

    #endregion

    #region GetUserDataByEntraIdAsync Tests

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("f0f0f0f0-f0f0-f0f0-f0f0-f0f0f0f0f0f0")]
    [InlineData("ffffffff-ffff-ffff-ffff-ffffffffffff")]
    public async Task GetUserDataByEntraIdAsync_WithValidEntraId_ReturnsExpectedUserIdData(string entraId)
    {
        // Arrange
        int userId = await _sut.CreateUserAsync(entraId);
        List<RunEvent> runEvents = RunEventFakes.CreateRunEvents(3).ToList();
        await _sut.AddRunEventsAsync(userId, runEvents);

        // Act
        UserData? userData = await _sut.GetUserDataByEntraIdAsync(entraId);

        // Assert
        userData.ShouldNotBeNull();
        userData.EntraId.ShouldBe(entraId);
        userData.UserId.ShouldBe(userId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task GetUserDataByEntraIdAsync_WithValidEntraId_ReturnsExpectedRunHistoryData(int runEventQty)
    {
        // Arrange
        string entraId = EntraIdFakes.CreateEntraId();
        int userId = await _sut.CreateUserAsync(entraId);
        List<RunEvent> runEvents = RunEventFakes.CreateRunEvents(runEventQty).ToList();
        await _sut.AddRunEventsAsync(userId, runEvents);

        // Act
        UserData? userData = await _sut.GetUserDataByEntraIdAsync(entraId);

        // Assert
        userData.ShouldNotBeNull();
        userData.EntraId.ShouldBe(entraId);
        userData.RunHistory.Count().ShouldBe(runEventQty);
        userData.RunHistory.ShouldMatchRunEvents(runEvents);
    }

    [Theory]
    [InlineData("unused-entra-authentication-id")]
    [InlineData("12345678-1234-1234-1234-123456789abc")]
    [InlineData("ffffffff-ffff-ffff-ffff-ffffffffffff")]
    public async Task GetUserDataByEntraIdAsync_WithValidNonExistentEntraId_ReturnsNull(string unusedEntraId)
    {
        // Arrange
        string usedEntraId = "00000000-0000-0000-0000-000000000000";
        await _sut.CreateUserAsync(usedEntraId);

        // Act
        UserData? userData = await _sut.GetUserDataByEntraIdAsync(unusedEntraId);

        // Assert
        userData.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetUserDataByEntraIdAsync_WithInvalidEntraId_ThrowsArgumentException(string? invalidEntraId)
    {
        // Act
        Func<Task<UserData?>> withInvalidEntraId = async () => await _sut.GetUserDataByEntraIdAsync(invalidEntraId!);

        // Assert
        Exception ex = await withInvalidEntraId.ShouldThrowAsync<ArgumentException>();
        ex.Message.ShouldContain("Invalid");
        ex.Message.ShouldContain("entraId");
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
        int userId = await _sut.CreateUserAsync(EntraIdFakes.CreateEntraId());
        IEnumerable<RunEvent> runEvents = RunEventFakes.CreateRunEvents(eventQty);

        // Act
        int result = await _sut.AddRunEventsAsync(userId, runEvents);

        // Assert
        result.ShouldBe(eventQty);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task AddRunEventAsync_WithInvalidUserId_ThrowsArgumentException(int? invalidId)
    {
        // Arrange
        IEnumerable<RunEvent> runEvents = RunEventFakes.CreateRunEvents();

        // Act
        Func<Task<int>> addsWithInvalidId = async () => await _sut.AddRunEventsAsync(invalidId!.Value, runEvents);

        // Assert
        Exception ex = await addsWithInvalidId.ShouldThrowAsync<ArgumentOutOfRangeException>();
        ex.Message.ShouldContain("userId");
    }

    [Fact]
    public async Task AddRunEventAsync_WithNullRunEvents_ThrowsArgumentException()
    {
        // Arrange
        int userId = await _sut.CreateUserAsync(EntraIdFakes.CreateEntraId());
        IEnumerable<RunEvent> runEvents = null!;

        // Act
        Func<Task<int>> addsWithNullEventCollection = async () => await _sut.AddRunEventsAsync(userId, runEvents);

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
    public async Task AddRunEventAsync_WithNullRunEvents_ShouldGracefullyDiscardAndShouldNotThrowException(
        int validEventQty, int nullEventQty)
    {
        // Arrange
        RunEvent CreateNullEvent(int _) => null!;
        IEnumerable<RunEvent> validEvents = RunEventFakes.CreateRunEvents(validEventQty);
        IEnumerable<RunEvent> fakeEvents = Enumerable.Range(0, nullEventQty).Select(CreateNullEvent);
        IEnumerable<RunEvent> shuffledEvents = validEvents
            .Concat(fakeEvents)
            .OrderBy(_ => Random.Shared.Next());
        int userId = await _sut.CreateUserAsync(EntraIdFakes.CreateEntraId());

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
        int userId = await _sut.CreateUserAsync(EntraIdFakes.CreateEntraId());
        IEnumerable<RunEvent> events = RunEventFakes.CreateRunEvents(eventQty);
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
    public async Task GetRunEventsByUserIdAsync_WithMultipleUsersAndEvents_ReturnsExpectedEventCountForUser(
        int userCount, int eventsPerUser)
    {
        // Arrange
        List<int> userIds = [];
        foreach (int _ in Enumerable.Range(0, userCount))
        {
            int userId = await _sut.CreateUserAsync(EntraIdFakes.CreateEntraId());
            userIds.Add(userId);

            IEnumerable<RunEvent> events = RunEventFakes.CreateRunEvents(eventsPerUser);
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
    public async Task GetRunEventsByUserIdAsync_WithMultipleInsertions_ReturnsAllEvents(int totalInsertions,
        int eventsPerInsertion)
    {
        // Arrange
        int expectedRecordCount = totalInsertions * eventsPerInsertion;
        int userId = await _sut.CreateUserAsync(EntraIdFakes.CreateEntraId());
        foreach (int _ in Enumerable.Range(0, totalInsertions))
        {
            IEnumerable<RunEvent> events = RunEventFakes.CreateRunEvents(eventsPerInsertion);
            await _sut.AddRunEventsAsync(userId, events);
        }

        // Act
        IEnumerable<RunEvent> result = await _sut.GetRunEventsByUserIdAsync(userId);

        // Assert
        result.Count().ShouldBe(expectedRecordCount);
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(int.MaxValue)]
    public async Task GetRunEventsByUserIdAsync_WithValidNonExistentUserId_ReturnsEmptyCollection(int invalidUserId)
    {
        // Arrange
        await PreseedDatabase();

        // Act
        IEnumerable<RunEvent> result = await _sut.GetRunEventsByUserIdAsync(invalidUserId);

        // Assert
        result.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(int.MinValue)]
    public async Task GetRunEventsByUserIdAsync_WithInvalidUserId_ThrowsArgumentException(int invalidUserId)
    {
        // Act
        Func<Task<IEnumerable<RunEvent>>> withInvalidUserId =
            async () => await _sut.GetRunEventsByUserIdAsync(invalidUserId);

        // Assert
        Exception ex = await withInvalidUserId.ShouldThrowAsync<ArgumentOutOfRangeException>();
        ex.Message.ShouldContain("Invalid");
        ex.Message.ShouldContain("userId");
    }

    #endregion
}
