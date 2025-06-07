namespace RunSuggestion.Core.Models;

public class RunActivity
{
    public int Id { get; set; }
    public float Distance { get; set; }
    public byte Effort { get; set; }
    public TimeSpan Duration { get; set; }
}
