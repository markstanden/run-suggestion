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
    public IEnumerable<RunEvent> Transform(string csv)
    {
        IEnumerable<TrainingPeaksActivity> csvData = ParseCsvData<TrainingPeaksActivity>(csv);

        return csvData
            .Where(row => row.Title == "Running")
            .Select(row => new RunEvent()
            {
                Id = 0,
                Date = row.WorkoutDay,
                Distance = (int)Math.Round(row.DistanceInMeters),
                Effort = (byte)(row.Rpe ?? 0),
                Duration = TimeSpan.FromHours(row.TimeTotalInHours),
            });
    }

    private IEnumerable<T> ParseCsvData<T>(string csv)
    {
        using StringReader reader = new(csv);
        using CsvReader csvReader = new(reader, CultureInfo.InvariantCulture);
        return csvReader.GetRecords<T>().ToList();
    }
}



