namespace RunSuggestion.Shared.EnvironmentVariables;

public static class BaseUrl
{
    private const string LocalHost = "http://localhost";
    private const string SwaCliExposedPort = "4280";

    private const string BaseUrlEnvironmentVariable = "BASE_URL";
    private const string BaseUrlDefaultLocalValue = $"{LocalHost}:{SwaCliExposedPort}";

    /// <summary>
    ///     Gets the base URL for tests from the BASE_URL environment variable.
    ///     Falls back to localhost for local testing if not set.
    /// </summary>
    public static string Value =>
        Environment.GetEnvironmentVariable(BaseUrlEnvironmentVariable) ?? BaseUrlDefaultLocalValue;
}
