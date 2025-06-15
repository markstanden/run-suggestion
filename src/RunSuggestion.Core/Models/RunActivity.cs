namespace RunSuggestion.Core.Models;

public class RunActivity
{
    public int Id { get; init; }
    public DateTime Date { get; init; }
    public int Distance { get; init; }
    public byte Effort { get; init; }
    public TimeSpan Duration { get; init; }
}
