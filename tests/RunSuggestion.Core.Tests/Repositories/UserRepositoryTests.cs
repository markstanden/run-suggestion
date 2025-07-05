using RunSuggestion.Core.Models.Runs;
using RunSuggestion.Core.Repositories;

namespace RunSuggestion.Core.Tests.Repositories;

public class UserRepositoryTests
{
    private readonly UserRepository _sut;
    private const string TestConnectionString = "Data Source=:memory:";
    
    public UserRepositoryTests()
    {
        _sut = new UserRepository(TestConnectionString);
    }
    
    [Fact]
    public async Task AddRunHistory_WithSingleRunEvent_ReturnsOneRowAffected()
    {
        // Arrange
        int expectedRowCount = 1;
        int userId = await _sut.CreateUserAsync(Guid.NewGuid().ToString());
        RunEvent runEvent = new() {
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
    [InlineData(null)]
    [InlineData("550e8400-e29b-41d4-a716-446655440000")]
    [InlineData("another-entra-id-guid")]
    public async Task CreateUserAsync_WithOrWithoutAnEntraId_ReturnsInternalUserId(string? entraId)
    {
        // Act
        int userId = await _sut.CreateUserAsync(entraId);
    
        // Assert
        userId.ShouldBeGreaterThan(0);
    }
    
    [Theory]
    [InlineData("550e8400-e29b-41d4-a716-446655440000")]
    [InlineData("another-entra-id-guid")]
    public async Task CreateUserAsync_WithADuplicateEntraId_ShouldThrowInvalidArgument(string? entraId)
    {
        // Arrange
        int userId1 = await _sut.CreateUserAsync(entraId);
        
        // Act
        var duplicateEntraId = () => _sut.CreateUserAsync(entraId);
    
        // Assert
        Exception ex = duplicateEntraId.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain("EntraID");
        ex.Message.ShouldContain("already exists");
    }
}