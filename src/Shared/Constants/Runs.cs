namespace RunSuggestion.Shared.Constants;

public static class Runs
{
    public const int RunDistanceBaseMetres = 1000;
    public const int RunDistanceBaseDurationMinutes = 15;

    public static readonly TimeSpan RunDistanceBaseDurationTimeSpan
        = TimeSpan.FromMinutes(RunDistanceBaseDurationMinutes);

    public static class EffortLevel
    {
        public const byte Recovery = 1;
        public const byte Easy = 2;
        public const byte Medium = 3;
        public const byte Strong = 4;
        public const byte Hard = 5;
    }
}
