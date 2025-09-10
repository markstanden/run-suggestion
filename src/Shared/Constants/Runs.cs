namespace RunSuggestion.Shared.Constants;

public static class Runs
{
    public const int RunDistanceBaseMetres = 2500;
    public const int RunDistanceBaseDurationMinutes = 30;
    public const byte RunEffortBase = 1;

    public static readonly TimeSpan RunDistanceBaseDurationTimeSpan
        = TimeSpan.FromMinutes(RunDistanceBaseDurationMinutes);
}
