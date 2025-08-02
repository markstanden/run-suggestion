namespace RunSuggestion.TestHelpers.Constants;

/// <summary>
/// Non-functional requirements constants used for testing and validation purposes.
/// These values represent the minimum and maximum acceptable thresholds defined in the project documentation.
/// </summary>
public static class NonFunctionalRequirements
{
    /// <summary>
    /// 
    /// </summary>
    private const double ConfidenceBufferRatio = 0.33;

    /// <summary>
    /// Performance requirements for API response times
    /// </summary>
    public static class ApiResponse
    {
        /// <summary>
        /// Non-Functional Requirement limit for data ingestion request as specified in the project documentation.
        /// NFR: "WHEN a user uploads run history data, the system SHALL process the data within 30 seconds"
        /// </summary>
        public const int MaxUploadFunctionRunTimeMs = 30000;

        /// <summary>
        /// Non-Functional Requirement limit for a Run Recommendation request as specified in the project documentation.
        /// Requirement: "WHEN a user requests a run recommendation, the system SHALL process the request within 5 seconds"
        /// </summary>
        public const int MaxRecommendationFunctionRunTimeMs = 5000;

        /// <summary>
        /// Non-Functional Requirement limit for a Rule Customisation request as specified in the project documentation.
        /// Requirement: "WHEN a user accesses the rule customisation endpoint, the system SHALL process the request within 5 seconds"
        /// </summary>
        public const int MaxRuleCustomizationRunTimeMs = 5000;

        /// <summary>
        /// Non-Functional Requirement limit for a Feedback request as specified in the project documentation.
        /// Requirement: "WHEN a user accesses the recommendation feedback endpoint, the system SHALL process the request within 5 seconds"
        /// </summary>
        public const int MaxFeedbackRunTimeMs = 5000;
    }

    /// <summary>
    /// Performance requirements for cold start scenarios
    /// </summary>
    public static class ColdStart
    {
        /// <summary>
        /// Non-Functional Requirement limit for a dormant endpoint to be ready to start processing the request as specified in the project documentation.
        /// Requirement: "WHEN a user accesses any dormant endpoint, the system SHALL be ready to process the data within 30 seconds for 95% of requests"
        /// NOTE: Not particularly reliable to test within an integration test, included for reference.
        /// </summary>
        public const int ActualColdStartMs = 30000;
    }

    /// <summary>
    /// Reliability and availability requirements
    /// </summary>
    public static class Reliability
    {
        /// <summary>
        /// Minimum system availability percentage.
        /// Requirement: "WHEN a user makes a request, the system SHALL be available for use for 99.9% of the time"
        /// NOTE: Not testable within an integration test, included for reference.
        /// </summary>
        public const double MinAvailabilityPercent = 99.9;
    }
}
