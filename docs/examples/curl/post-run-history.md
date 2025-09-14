# RunSuggestion API - CURL Examples

## Setup

Navigate to the api project directory and build and start the app running locally:

```bash
swa start test
```

or

```bash
swa start --config-name test
```

This makes the api endpoints available on `http://localhost:7071/api/`

## POST: run history csv endpoint

The main data ingestion function for the application. It is designed to ingest a CSV file of workouts exported from
[TrainingPeaks](https://www.trainingpeaks.com). A sample workout.csv with multiple types of activity (including runs)
has been provided [here](../../sample-data/mixed-workouts.csv)

To add persona data a number of personas have been developed to test ingestion:

### Steady Progression

A user that is increasing distance slowly at 500m per run per week, approximately 5% load increase:
[Steady Progression](../../sample-data/steady-progression.csv

### Create a Base64 encoded principal

- userId should be set to a unique value - identity provider MUST remain github.

```bash
PRINCIPAL='{"userId":"User12345","identityProvider":"github"}'
echo $PRINCIPAL | base64
# Base64: eyJ1c2VySWQiOiJVc2VyMTIzNDUiLCJpZGVudGl0eVByb3ZpZGVyIjoiZ2l0aHViIn0K
```

Run the following from the project root:

```bash
curl --request POST "http://localhost:7071/api/history" \
     --header "Content-Type: text/csv" \
     --header "x-ms-client-principal: eyJ1c2VySWQiOiJVc2VyMTIzNDUiLCJpZGVudGl0eVByb3ZpZGVyIjoiZ2l0aHViIn0K" \
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
