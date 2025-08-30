using RunSuggestion.Core.Interfaces;
using RunSuggestion.Shared.Models.DataSources.TrainingPeaks;
using RunSuggestion.Shared.Models.Runs;

namespace RunSuggestion.Core.Transformers;

/// <summary>
/// Implementation to create RunHistory from a provided csv file.
/// CSV parser may be overkill for this project as the CSV is single source, with known delimiters and format,
/// but is highly recommended
/// <see cref="https://stackoverflow.com/questions/3507498/reading-csv-files-using-c-sharp"/>
/// </summary>
public class CsvToRunHistoryTransformer : IRunHistoryTransformer
{
    private readonly ICsvParser _csvParser;

    /// <summary>
    /// Transformer class constructor.  Arguments are passed in via dependency injection within the Program.cs
    /// </summary>
    /// <param name="csvParser">Instance of ICsvParser dependency injected into the constructor on initialisation.</param>
    public CsvToRunHistoryTransformer(ICsvParser csvParser)
    {
        _csvParser = csvParser ?? throw new ArgumentNullException(nameof(csvParser));
    }

    /// <summary>
    /// Transforms a csv string into an IEnumerable of RunEvents.
    /// Only Runs will be included in the returned IEnumerable, other
    /// activity types will be discarded.
    /// </summary>
    /// <param name="csv">The CSV string to be parsed</param>
    /// <returns>an IEnumerable of RunEvents</returns>
    public IEnumerable<RunEvent> Transform(string csv)
    {
        IEnumerable<TrainingPeaksActivity> csvData = _csvParser.Parse<TrainingPeaksActivity>(csv);

        return csvData
            .Where(TrainingPeaksActivity.IsRunActivity)
            .Select(MapToRunEvent);
    }

    /// <summary>
    /// Maps a parsed TrainingPeaksActivity to a new RunEvent instance
    /// </summary>
    /// <param name="activity"></param>
    /// <returns>A mapped RunEvent</returns>
    private RunEvent MapToRunEvent(TrainingPeaksActivity activity) =>
        new()
        {
            RunEventId = 0,
            Date = activity.WorkoutDay ?? DateTime.MinValue,
            Distance = (int)Math.Round(activity.DistanceInMeters ?? 0),
            Effort = (byte)(activity.Rpe ?? 0),
            Duration = TimeSpan.FromHours(activity.TimeTotalInHours ?? 0)
        };
}
