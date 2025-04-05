# Run Suggestion Engine

A running recommendation engine built on Azure Functions, built using TDD in sprint like iterations.

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
- [Terraform](https://developer.hashicorp.com/terraform/tutorials/aws-get-started/install-cli)

#### DevContainer

Docker-based development environment to provide reliable, repeatable compilation and testing of the project independent of users machines.

- [Docker](https://docs.docker.com/desktop/)
- [VSCode](https://code.visualstudio.com/download)

### Infrastructure Management

This project uses [terraform-tools](https://github.com/markstanden/terraform-tools) for managing Terraform commands.  Although not required, it simplifies the creation of infrastructure by sourcing environment variables when working locally.

This is a personal workaround for the following [terraform issue](https://github.com/hashicorp/terraform-provider-azurerm/issues/27423)

## Getting Started

1. Clone the repository
2. Initialize the infrastructure:
   ```
   tf init
   ```
3. Build the solution:
   ```
   dotnet build
   ```
4. Run tests:
   ```
   dotnet test
   ```
