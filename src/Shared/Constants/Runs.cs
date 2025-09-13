namespace RunSuggestion.Shared.Constants;

/// <summary>
/// Provides static constants and defaults
/// related to run recommendation calculations and configurations.
/// </summary>
public static class Runs
{
    public const int RestDistance = 0;
    public static readonly TimeSpan RestDuration = TimeSpan.Zero;

    /// <summary>
    /// Defaults provided for run recommendations where insufficient
    /// run history has been provided
    /// </summary>
    public static class InsufficientHistory
    {
        public const int RunDistanceMetres = 1000;
        public const byte RunEffort = EffortLevel.Easy;
        public const int RunPaceMinsPerKm = 15;

        public static TimeSpan RunDurationTimeSpan(int distanceMetres)
            => TimeSpan.FromMinutes(distanceMetres / 1000D * RunPaceMinsPerKm);
    }

    /// <summary>
    /// Verbalised effort levels used within Garmin's `Perceived Effort`
    /// field. Using constants rather than hardcoded values allows for
    /// boundaries to be moved within configuration more easily.
    /// </summary>
    public static class EffortLevel
    {
        public const byte Rest = 0;
        public const byte Recovery = 1;
        public const byte Easy = 3;
        public const byte Medium = 5;
        public const byte Strong = 7;
        public const byte Hard = 9;
    }
}
