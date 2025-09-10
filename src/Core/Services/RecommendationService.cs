using Microsoft.Extensions.Logging;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Shared.Models.Runs;

namespace RunSuggestion.Core.Services;

public class RecommendationService : IRecommendationService
{
    private readonly ILogger<RecommendationService> _logger;
    private readonly IUserRepository _userRepository;

    public RecommendationService(ILogger<RecommendationService> logger, IUserRepository userRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <inheritdoc/>
    public Task<RunRecommendation> GetRecommendation(string entraId)
    {
        if (string.IsNullOrWhiteSpace(entraId))
        {
            throw new ArgumentException("Invalid EntraId - cannot be null or whitespace", nameof(entraId));
        }

        RunRecommendation result = new()
        {
            RunRecommendationId = default,
            Date = default,
            Distance = default,
            Effort = default,
            Duration = default
        };

        return Task.FromResult(result);
    }
}
