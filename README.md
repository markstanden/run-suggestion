# Run Suggestion Engine

A running recommendation engine built on Azure Functions, built using TDD practices, to an initial pre-designed UML
design.

## Project Structure

```bash
.
├── infrastructure
│   └── terraform
├── src
│   ├── RunSuggestion.Api
│   │   ├── Functions
│   │   ├── Models
│   │   └── Services
│   └── RunSuggestion.Core
│       ├── Interfaces
│       ├── Models
│       └── Services
├── tests
│   ├── RunSuggestion.Api.Tests
│   └── RunSuggestion.Core.Tests
└── tools
    └── terraform
```

## Technology Stack

- **.NET 8**: Core framework
- **Azure Functions**: Serverless compute
- **Azure AD B2C**: Authentication
- **Terraform**: Infrastructure as Code
- **Testing**: xUnit, Moq, Shouldly

## Development

### Prerequisites

#### Bare Metal

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)
- [Azure Functions Core](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- [Terraform](https://developer.hashicorp.com/terraform/tutorials/aws-get-started/install-cli)

#### DevContainer

Docker-based development environment to provide reliable, repeatable compilation and testing of the project independent
of users machines.

- [Docker](https://docs.docker.com/desktop/)
- [VSCode](https://code.visualstudio.com/download)

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
    cp src/RunSuggestion.Api/local.settings.template.json src/RunSuggestion.Api/local.settings.json
    ```

3. Build the solution:

   ```bash
   dotnet build
   ```

4. Run tests:

   ```bash
   dotnet test
   ```

5. Run the project

    ```bash
   func host start 
   ```

## CI/CD Pipeline

This project uses GitHub Actions to automate basic quality checks, with the pipeline running on pushes or PRs to

- `main`
- `dev`
- `prod`

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

- **Unit Tests**: Runs all tests in all test projects without the suffix `Integration.Tests`
   ```bash
   dotnet test --filter "Category!=Integration&FullyQualifiedName!~Integration.Tests"
   ```