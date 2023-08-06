# ConfigHub - Configuration as a Service

ConfigHub is a configuration-as-a-service component built using .NET 6 and MongoDB. It provides a centralized solution to store and manage configuration data for multiple applications (clients). Each application can have multiple components, and each component can have multiple configuration keys.

## Project Structure

The solution consists of the following projects:

### ConfigHub.API

This project contains the Web API that exposes endpoints for fetching configuration data. It provides endpoints to retrieve configuration data based on the component and key names. The client certificate authentication for secure access is in progress.

### ConfigHub.Client

The ConfigHub.Client project contains the client library that allows other applications to consume the ConfigHub API. It provides an easy-to-use interface to interact with the API and fetch configuration data.

### ConfigHub.Domain

This project contains the domain models and interfaces used by both the API and the client. It defines the data models for configuration items and other shared entities.

### ConfigHub.Infrastructure

The ConfigHub.Infrastructure project contains the implementation of the interfaces defined in the domain project. It includes the MongoDB repository and the configuration service, which interact with the database to store and retrieve configuration data.

### ConfigHub.Mongo

The ConfigHub.Mongo project contains the MongoDB context and the generic repository factory. The context provides access to the MongoDB database, and the repository factory generates the generic repository for a specific collection and database name.

### ConfigHub.Shared

The ConfigHub.Shared project contains shared enums and common utilities used across the solution  

---

# How to Use ConfigHubClient

The ConfigHubClient is a client library that allows applications to consume the ConfigHub API and fetch configuration data. To use the ConfigHubClient in your application, follow these steps:

### Register ConfigHubClient with Dependency Injection

In your application's Startup.cs or a similar configuration file, register the ConfigHubClient with dependency injection.

#### Scenario 1: Using ConfigHub Service

If you want to use the ConfigHub service for configuration data, provide the required options such as `BaseAddress`, `ClientCertificate`, and `ApplicationId`.


```markdown
```csharp
services.AddConfigHubClient(new ConfigHubOptions
{
    UseConfigHubService = true,
    BaseAddress = "https://confighub.example.com",
    ClientCertificate = new X509Certificate2("path/to/certificate.pfx", "certificatePassword"),
    ApplicationId = "YourApplicationId"
});
```
![Sequence Diagram]("UML/ConfgHubClientAsService.png")

#### Scenario 2: Using Direct MongoDB Connection

If you prefer to connect directly to MongoDB for configuration data, provide the necessary MongoDB options such as `ConnectionString`, `DatabaseName`, and `ConfigCollectionName`.

```markdown
```csharp
services.AddConfigHubClient(new ConfigHubOptions
{
    UseConfigHubService = false,
    ConnectionString = "mongodb://localhost:27017",
    DatabaseName = "YourDatabaseName",
    ConfigCollectionName = "YourConfigCollectionName"
});
```
![Sequence Diagram]("UML/ConfgHubClientAsLibrary.png")

ConfigHub provides a centralized and secure solution for managing configuration data. By using the ConfigHub.Client library, you can easily consume the ConfigHub API in your application, ensuring that your application always has the most up-to-date configuration settings. Feel free to use and customize this solution for your configuration management needs. If you have any questions or issues, please feel free to reach out to us or open an issue on GitHub.

Happy coding! 😊
