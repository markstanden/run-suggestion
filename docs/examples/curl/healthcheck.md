# RunSuggestion API - CURL Examples

## Setup

- Navigate to the api project directory and build and start the app running locally:

  ```bash
  cd src/RunSuggestion.Api/
  func start --port 7071
  ```

- This makes the api endpoints available on `http://localhost:7071/api/`

## Health Check Endpoint

A simple health check endpoint to verify the API is running.

```bash
curl -X GET "http://localhost:7071/api/HealthCheck" \
     -H "Content-Type: application/json"
```

### Expected `Healthy` Response

The api should return `200 OK` response with the message `System Healthy`