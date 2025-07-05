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
}