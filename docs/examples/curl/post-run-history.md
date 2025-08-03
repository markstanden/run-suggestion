# RunSuggestion API - CURL Examples

## Setup

Navigate to the api project directory and build and start the app running locally:

```bash
cd src/RunSuggestion.Api/
func start --port 7071
```

This makes the api endpoints available on `http://localhost:7071/api/`

## POST: run history csv endpoint

The main data ingestion function for the application. It is designed to ingest a CSV file of workouts exported from
[TrainingPeaks](https://www.trainingpeaks.com). A sample workout.csv with multiple types of activity (including runs)
has been provided [here](../../sample-data/workouts.csv)

Run the following from the project root:

```bash
curl --request POST "http://localhost:7071/api/PostRunHistory" \
     --header "Content-Type: text/csv" \
     --header "Authorization: Bearer AUTH_TOKEN" \
     --data-binary @docs/sample-data/mixed-workouts.csv
```

### Expected `Successful` Response

The api should return `200 OK` response with the message:

```json
{
  "message": "Successfully processed CSV",
  "rowsAdded": 19
}
```
