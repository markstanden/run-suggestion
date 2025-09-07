# Run Suggestion Engine

A running recommendation engine built on Azure Functions, built using TDD practices, to an initial pre-designed UML
design.

## Project Structure

```bash
.
├── docs/
│   ├── examples/
│   │   └── curl/
│   └── sample-data/
├── src/
│   ├── Api/
│   │   ├── Constants/
│   │   ├── Extensions/
│   │   ├── Functions/
│   │   ├── Properties/
│   │   └── Program.cs
│   ├── Core/
│   │   ├── Constants/
│   │   ├── Interfaces/
│   │   ├── Repositories/
│   │   ├── Services/
│   │   ├── Sql/
│   │   ├── Transformers/
│   │   └── Validators/
│   ├── Shared/
│   │   ├── Constants/
│   │   ├── Extensions/
│   │   └── Models/
│   └── Web/
│       ├── Authentication/
│       ├── Constants/
│       ├── Layout/
│       ├── Pages/
│       ├── Services/
│       └── wwwroot/
├── tests/
│   ├── Api.Integration.Tests/
│   ├── Api.Unit.Tests/
│   ├── Core.Unit.Tests/
│   ├── Shared.Unit.Tests/
│   ├── TestHelpers/
│   └── Web.Unit.Tests/
├── swa-cli.config.json
└── RunSuggestion.sln
```

## Technology Stack

- **.NET 8**: Core framework
- **Azure Functions**: Serverless compute
- **Azure AD B2C**: Authentication
- **Testing**: xUnit, Moq, Shouldly

## Development

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)
- [Azure Functions Core](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)

### Infrastructure Management

This project uses [terraform-tools](https://github.com/markstanden/terraform-tools) for managing Terraform commands.
Although not required, it simplifies the creation of infrastructure by sourcing environment variables when working
locally.

This is a personal workaround for the
following [terraform issue](https://github.com/hashicorp/terraform-provider-azurerm/issues/27423)

## Getting Started

1. Clone the repository
2. Copy the local settings template:

    ```bash
    cp src/Api/local.settings.template.json src/Api/local.settings.json
    ```

3. Build the solution:

   ```bash
   dotnet build
   ```

4. Run tests:

   ```bash
   dotnet test
   ```

5. Run locally

   - Full stack via Static Web Apps CLI (builds Web and API) - requires Azure SWA CLI tool (recommended):

     ```bash
     swa start --config swa-cli.config.json
     ```

   - Separately 

   - API (Azure Functions):

     ```bash
     cd src/Api
     func start
     ```

   - Web (Blazor):

     ```bash
     dotnet watch run --project src/Web/Web.csproj
     ```

## CI/CD Pipeline

This project uses GitHub Actions to automate basic quality checks, with the pipeline running on pushes or PRs to `main`.

The pipeline jobs are held in my [coding-standards repo](https://github.com/markstanden/coding-standards), allowing for
usage across multiple projects.

- **Format Check**: Enforces code formatting standards using `.editorconfig` and dotnet's built in code formatter:
   ```bash
   dotnet format --verify-no-changes
   ```

- **Build**: Compiles the solution and validates dependencies using dotnet's build command to flag errors.
   ```bash
   dotnet build
   ```

- **Unit Tests**: Runs all tests in all test projects with the suffix `Unit.Tests`
   ```bash
   dotnet test --filter 'FullyQualifiedName~Unit.Tests'
   ```

- **Integration Tests**: Runs all tests in all test projects with the suffix `Integration.Tests`
   ```bash
   dotnet test --filter 'FullyQualifiedName~Integration.Tests'
   ```

## Code Quality

- Code quality is improved by static analysis during development using SonarQube with IDE integration.
- SonarQube inclusion within the pipeline provides an enforced quality gate on PRs.
- Force pushes to `main` are prohibited - requiring PRs and enabling review before merge.
- Code Rabbit also provides LLM-assisted code reviews, helping to highlight development errors prior to merges.