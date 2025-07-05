using RunSuggestion.Core.Models.Runs;
using RunSuggestion.Core.Repositories;
using Range = System.Range;

namespace RunSuggestion.Core.Tests.Repositories;

public class UserRepositoryTests
{
    private readonly UserRepository _sut;
    private const string TestConnectionString = "Data Source=:memory:";

    public UserRepositoryTests()
    {
        _sut = new UserRepository(TestConnectionString);
    }

    private string CreateFakeEntraId() => Guid.NewGuid().ToString();

    [Fact]
    public async Task AddRunHistory_WithSingleRunEvent_ReturnsOneRowAffected()
    {
        // Arrange
        int expectedRowCount = 1;
        int userId = await _sut.CreateUserAsync(CreateFakeEntraId());
        RunEvent runEvent = new()
        {
            Id = userId,
            Date = DateTime.Today,
            Distance = 5000,
            Effort = 5,
            Duration = TimeSpan.FromMinutes(30)
        };

        // Act
        int result = await _sut.AddRunHistoryAsync(userId, [runEvent]);

        // Assert
        result.ShouldBe(expectedRowCount);
    }

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
        int userId1 = await _sut.CreateUserAsync(entraId);

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


}