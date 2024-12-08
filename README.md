# Clean Architecture Template

What's included in the template?

- SharedKernel project with common Domain-Driven Design abstractions.
- Domain layer with sample entities.
- Application layer with abstractions for:
  - CQRS
  - Example use cases
  - Cross-cutting concerns (logging, validation)
- Infrastructure layer with:
  - Authentication
  - Permission authorization
  - EF Core, PostgreSQL
  - Serilog
- Seq for searching and analyzing structured logs
  - Seq is available at http://localhost:8081 by default
- Testing projects
  - Architecture testing

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

I'm open to hearing your feedback about the template and what you'd like to see in future iterations.

If you're ready to learn more, check out [**Pragmatic Clean Architecture**](https://www.milanjovanovic.tech/pragmatic-clean-architecture?utm_source=ca-template):

- Domain-Driven Design
- Role-based authorization
- Permission-based authorization
- Distributed caching with Redis
- OpenTelemetry
- Outbox pattern
- API Versioning
- Unit testing
- Functional testing
- Integration testing

Stay awesome!