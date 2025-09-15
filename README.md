# Commerce Circle Backend - Integration API

## Description
The current software project is developed in .Net 8.0, based on Clean Architecture patterns, with the main goal of promoting customer loyalty through a gamification strategy: Loyalty points, loyalty levels, and gamification

## Modules
This project includes the following modules:
- **Api:** This serves as the entry point for the Web API application. It handles incoming HTTP requests through GraphQL, and coordinates actions across different layers . The API layer interacts with the application layer to process requests and provide responses.
- **Application:** The Application layer encapsulates application-specific business rules, use cases, and application services, consumers (BillPayment, Referral, Transaction). It acts as a mediator between the domain layer and the infrastructure layer.
- **Domain:** It holds domain entities: Constans, Entities, Enums, Value Objects.
- **EventWorker:** It is background worker or service that processes events asynchronously. It handle event-driven tasks like sending notifications and performing other background processing..
- **Infrastructure:** This layer contains implementation details. It includes data access, tables configurations, interactions, external services (LoyaltiEngine, Wallet Service), Migrations, and other infrastructure concerns. Infrastructure components interact with the domain layer and provide necessary services to the application.
- **Test:** Write unit tests for domain logic, integration tests for Application services and Consumers for the entire system. Ensure that your tests cover all layers and interactions.


## Type
This project is classified as:
- **Type:** `Back-End`

## Language/Tech Stack
The project is built using the following technologies:
- **Programming Language:** `C#`
- **Frameworks/Libraries:** `ASP.NET Core` | `Microsoft.EntityFrameworkCore` | `RulesEngine` | `N1co.BusinessEvents` | `TalonOne` | `FluentValidations` | `MassTransit` | `MediatR` | `Serilog` | `Swashbuckle` | `Microsoft.Azure.Functions` | `xUnit, FluentValidation, Microsoft.NET.Test.Sdk, coverlet.collector`
- **Databases:** `SQL Server`
- **Tools/Platforms:** `Docker` | `Kubernetes` | `Azure`


## Parent System
This project is part of the following parent system(s):
- **System:** Loyalty

