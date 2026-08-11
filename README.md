# Vitalis

![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor-5C2D91?style=for-the-badge&logo=blazor&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![Status](https://img.shields.io/badge/status-in%20development-F5A623?style=for-the-badge)

> Latin for "of life" — a clinic management system.

Vitalis is a full-stack clinic management system: patient records, doctor scheduling, appointment booking with conflict prevention, medical records, prescriptions, and billing. Built as a solo capstone project, structured the way a real production system would be.

## Tech Stack

- **Backend:** ASP.NET Core Web API (.NET 10), Swagger (Swashbuckle) for API docs
- **Frontend:** Blazor Server
- **Database:** SQL Server (Azure SQL in production), EF Core Code First + Migrations
- **Auth:** JWT with refresh tokens, role-based authorization (Admin / Receptionist / Doctor / Patient)
- **Caching:** Redis (distributed) + in-memory
- **Testing:** xUnit, Moq, FluentAssertions
- **Cross-cutting:** FluentValidation, Serilog (structured logging), global exception handling with RFC 7807 ProblemDetails, Health Checks
- **Deployment:** Docker, Azure App Service + Azure SQL Database

## Architecture

Clean Architecture with a **Repository + Service + UnitOfWork** pattern — deliberately not MediatR/CQRS, since the service layer is the pattern this project is built to demonstrate.

```
src/
├── Vitalis.Domain          Entities, enums. No dependencies on anything else.
├── Vitalis.Application     Interfaces (IRepository, IUnitOfWork, services), DTOs, business logic
├── Vitalis.Infrastructure  EF Core DbContext, repository implementations, JWT, caching, migrations
├── Vitalis.WebApi          Controllers, middleware, filters
└── Vitalis.Web             Blazor Server UI
tests/
└── Vitalis.Application.UnitTests
```

Request flow: `Blazor → API Controller → Service → UnitOfWork/Repository → DbContext → SQL Server`. Dependencies point inward only — `Vitalis.Domain` has zero external references.

The database is organized into 4 schemas (`auth`, `scheduling`, `clinical`, `billing`) to keep bounded contexts explicit from day one, without paying any microservices tax now.

## Features

**Core**
- [ ] Auth & authorization — register, login (JWT), refresh token, logout, 4 roles
- [ ] Patient management — CRUD, search by name/phone, visit history
- [ ] Doctor & specialty management — CRUD, specialty assignment, working hours
- [ ] Appointment booking ⭐ — conflict-free scheduling within working hours, confirm/cancel/reschedule, check-in, status history
- [ ] Medical records — created against an appointment, symptoms/diagnosis, history view
- [ ] Prescriptions — linked to a medical record, medicines + dosage + instructions
- [ ] Services & pricing — CRUD for the clinic's service catalog
- [ ] Invoices & payments — generated from a visit (services + medicines), payment tracking (unpaid / partial / paid)

**Extended**
- [ ] Dashboard & reports — revenue by day/month, visits per doctor, new patients
- [ ] Medicine inventory — stock tracking, auto-deduct on prescription, low-stock alerts
- [ ] Email notifications — appointment reminders on booking/cancellation
- [ ] Redis caching, health checks
- [ ] Docker + Azure deployment

Not in scope: splitting into microservices.

## Getting Started

**Prerequisites:** .NET 10 SDK, SQL Server (or LocalDB), Redis (for caching features).

```bash
dotnet build                                     # build the whole solution
dotnet watch run --project src/Vitalis.WebApi    # run the API (Swagger at /swagger)
dotnet test                                      # run unit tests

# EF Core migrations
dotnet ef migrations add <Name> -p src/Vitalis.Infrastructure -s src/Vitalis.WebApi
dotnet ef database update -p src/Vitalis.Infrastructure -s src/Vitalis.WebApi
```

## Status

Actively under development. See the checklists above for progress.
