using System.Globalization;
using CsvHelper;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Models;
using RunSuggestion.Core.Models.DataSources.TrainingPeaks;

namespace RunSuggestion.Core.Transformers;

/// <summary>
/// Implementation to create RunHistory from a provided csv file.
/// CSV parser may be overkill for this project as the CSV is single source, with known delimiters and format,
/// but is highly recommended
/// <see cref="https://stackoverflow.com/questions/3507498/reading-csv-files-using-c-sharp"/>
/// </summary>
public class CsvToRunHistoryTransformer : IRunHistoryTransformer
{
    public const string TRAININGPEAKS_RUNNING_TITLE = "Running";

    /// <summary>
    /// Transforms a csv string into an IEnumerable of RunEvents.
    /// Only Runs will be included in the returned IEnumerable, other
    /// activity types will be discarded.
    /// </summary>
    /// <param name="csv">The CSV string to be parsed</param>
    /// <returns>an IEnumerable of RunEvents</returns>
    public IEnumerable<RunEvent> Transform(string csv)
    {
        IEnumerable<TrainingPeaksActivity> csvData = ParseCsvData<TrainingPeaksActivity>(csv);

        return csvData
            .Where(OnlyRunEvents)
            .Select(MapToRunEvent);
    }

    /// <summary>
    /// Generic method to parse the provided CSV into an IEnumerable of T.
    /// </summary>
    /// <param name="csv">The csv string to parse</param>
    /// <typeparam name="T">The model to deserialise into</typeparam>
    /// <returns>An IEnumerable of type T</returns>
    private IEnumerable<T> ParseCsvData<T>(string csv)
    {
        using StringReader reader = new(csv);
        using CsvReader csvReader = new(reader, CultureInfo.InvariantCulture);
        return csvReader.GetRecords<T>().ToList();
    }

    /// <summary>
    /// Maps a parsed TrainingPeaksActivity to a new RunEvent instance
    /// </summary>
    /// <param name="activity"></param>
    /// <returns>A mapped RunEvent</returns>
    private RunEvent MapToRunEvent(TrainingPeaksActivity activity) =>
        new RunEvent()
        {
            Id = 0,
            Date = activity.WorkoutDay,
            Distance = (int)Math.Round(activity.DistanceInMeters),
            Effort = (byte)(activity.Rpe ?? 0),
            Duration = TimeSpan.FromHours(activity.TimeTotalInHours),
        };

    /// <summary>
    /// Predicate that returns true only if the TrainingPeaks activity is a Run
    /// </summary>
    /// <param name="activity">Parsed line from Trainingpeaks CSV</param>
    /// <returns>True if the activity is a Run</returns>
    private bool OnlyRunEvents(TrainingPeaksActivity activity) =>
        activity.Title == TRAININGPEAKS_RUNNING_TITLE;
}



