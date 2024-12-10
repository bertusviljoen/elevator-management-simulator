# Project Overview

The following project is a simulation of an elevator system. The system is designed to manage a building with multiple elevators. The system is designed to manage the movement of the elevators and the passengers that are waiting for the elevators.

# Check out the project

The project is available at GitHub: [Elevator Simulator](https://github.com/bertusviljoen/elevator-management-simulator)

run the following command to clone the project:

```sh
git clone https://github.com/bertusviljoen/elevator-management-simulator.git
```

# Project dependencies

To run the project, you will need to have the following installed on your machine:
- .NET 8.0 SDK
- VS Code or Visual Studio 

The application uses user secrets to store the connection string to the database. See the section below on how to initialize user secrets.


## Initializing User Secrets

To initialize user secrets for the project, follow these steps:

1. Open a command prompt or terminal in the root directory of your project.
2. Run the following command to initialize user secrets:

    ```sh
    dotnet user-secrets init --project src/Presentation/Presentation.csproj
    ```
3. Create empty secret.json file in the root directory of your project.

    For Linux and macOS:
    ```sh
    touch secret.json
    ```
    
    For Windows:
    ```sh
    echo "" > secret.json        
    ```   

4. Update the secret.json file with your secrets. For example:

    ```json
    {
      "ConnectionStrings": {
        "Database": "Data Source=elevator-simulator.db"
      },
      "Jwt": {
        "Secret": "super-duper-secret-value-that-should-be-in-user-secrets",
        "Issuer": "elevator-simulator",
        "Audience": "developers",
        "ExpirationInMinutes": 60
      }
    }
   ```

5. Add your secrets using the following command:

    ```sh
    type .\secret.json | dotnet user-secrets set --project src/Presentation/Presentation.csproj
    ```
   
6. List your secrets using the following command:

    ```sh
    dotnet user-secrets list --project src/Presentation/Presentation.csproj
    ```

You can access these secrets in your application through the `IConfiguration` interface.

# Running the project

To run the project, follow these steps:
- Open the project in VS Code or Visual Studio
- Open a terminal in the root directory of the project
- Run the following command to start the project:

```sh
dotnet run --project src/Presentation/Presentation.csproj
```

# Application Features

The application is designed to manage a building with multiple elevators. The application is designed to manage the movement of the elevators and the passengers that are waiting for the elevators.

Screens Available:
- Main Screen to show a menu
- Dashboard screen to show the status of the elevators
- Elevator request screen request single or multiple elevators
- Login screen to login to the back system for system management
- Register screen to register a new user

Disclaimer: The application is a work in progress.

# Wish list:

The following features are on my wish list for the application:

- Options pattern for service configuration and validation
- Observer pattern to notify clients about the state of the elevator changes
- Manage building detail screens
- Manage elevator detail screens
- Auditing of changes by registered users using pipeline behaviors
- General Serilog configuration
- Observable Screen to view real time updates

# Credits:

The project was initialized by the Clean Architecture template by Milan Jovanovic Tech. The template is available at GitHub: [Clean Architecture](https://www.milanjovanovic.tech/templates/clean-architecture)

I took the template and modified it to suit the requirements of the elevator simulator project.

# Project Structure (Clean Architecture)

This project follows the principles of Clean Architecture, separating concerns into distinct layers:

* **Presentation Layer (`src/Presentation`):** This layer handles user interface and interaction.  It's responsible for displaying information to the user and receiving user input.  It interacts with the Application Layer to perform actions.

* **Application Layer (`src/Application`):** This layer contains the business logic and use cases. It defines services and commands that encapsulate the application's functionality.  It interacts with the Domain Layer to access and manipulate domain objects and with the Infrastructure Layer to access external resources like databases.

* **Domain Layer (`src/Domain`):** This layer contains the core business rules and entities. It defines the domain model, including entities, value objects, and domain events.  This layer is independent of any specific infrastructure or presentation concerns.

* **Infrastructure Layer (`src/Infrastructure`):** This layer provides the implementation details for accessing external resources, such as databases, message queues, and external APIs.  It interacts with the Application Layer to provide data access and other infrastructure services.  This layer is responsible for persistence, authentication, and other cross-cutting concerns.


This layered approach promotes maintainability, testability, and a clear separation of concerns, key principles of Clean Architecture.

Basic unit tests are included in the `tests` directory for each layer of the application and including architecture tests to ensure that dependencies are correctly configured.

# Technologies and Principles applied

- .NET 8.0 SDK - C# framework
- SQLite - Database
- Entity Framework Core - ORM
- Clean Architecture - Architecture
- SOLID Principles - Design Principles
- Dependency Injection - Design Pattern
- CQRS Pattern (Without the read side because of the simplicity of the application) - Design Pattern
- MediatR - Library for Mediator Pattern
- FluentValidation - Library for Validation
- Serilog - Library for Logging
- xUnit - Testing Framework
- Moq - Mocking Library
- Faker - Library for generating fake data
- Asynchronous programming - Design Pattern
- User Secrets - Configuration
- Global Package Management - NuGet
- Github Actions for CI/CD - Continuous Integration and Continuous Deployment

