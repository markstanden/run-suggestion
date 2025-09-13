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

        /// <summary>
        /// Calculates the estimated duration for a run based on the provided distance in metres,
        /// where insufficient run history has been provided.
        /// Returns a rest day duration if the provided distanceMetres is <see cref="Runs.RestDistance"/>.
        /// </summary>
        /// <param name="distanceMetres">
        /// The distance of the run in metres. Must be a non-negative integer.
        /// </param>
        /// <param name="runPaceMinsPerKm">
        /// The optional run pace to use for the calculation in mins/km,
        /// defaults to <see cref="InsufficientHistory.RunPaceMinsPerKm"/>
        /// </param>
        /// <returns>
        /// A <see cref="TimeSpan"/> representing the duration of the run.
        /// Returns zero duration if the distance is zero.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the provided distanceMetres is a negative value,
        /// or if the provided runPaceMinsPerKm is a zero or negative value
        /// </exception>
        public static TimeSpan RunDurationTimeSpan(int distanceMetres, int runPaceMinsPerKm = RunPaceMinsPerKm)
        {
            if (distanceMetres < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(distanceMetres),
                                                      "Distance must be a positive integer - cannot be negative.");
            }
            if (runPaceMinsPerKm <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(runPaceMinsPerKm),
                                                      "Pace (mins/km) must be greater than zero.");
            }
            return distanceMetres == RestDistance
                ? RestDuration
                : TimeSpan.FromMinutes(Math.Round(distanceMetres / 1000D * runPaceMinsPerKm));
        }
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
