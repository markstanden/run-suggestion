namespace RunSuggestion.Core.Unit.Tests.TestHelpers.Doubles;

public static class EntraIdFakes
{
    /// <summary>
    /// Method returns a fake entra id
    /// </summary>
    /// <returns>GUID string to simulate a entraId</returns>
    public static string CreateEntraId()
        => Guid.NewGuid().ToString();
}
