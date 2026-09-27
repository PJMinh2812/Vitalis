# Azure Deployment Notes

Deployed on the "Azure for Students" subscription. This subscription is
policy-restricted to five regions (`indonesiacentral`, `japaneast`,
`centralindia`, `uaenorth`, `malaysiawest`) — everything below runs in
**Central India**, the most full-featured of the allowed set.

## Resources (resource group `vitalis-rg`)

| Resource | Name | Notes |
|---|---|---|
| Container Registry | `vitalisacr2026` | Basic SKU, admin login enabled |
| SQL Server | `vitalis-sql-2026` | admin user `vitalisadmin`, password in `.azure-sql-password.txt` (gitignored, local only) |
| SQL Database | `Vitalis` | Basic tier |
| Container Apps environment | `vitalis-env` | Consumption ("Express") plan |
| Container App | `vitalis-webapi` | public: https://vitalis-webapi.calmrock-daf97560.centralindia.azurecontainerapps.io |
| Container App | `vitalis-web` | public: https://vitalis-web.calmrock-daf97560.centralindia.azurecontainerapps.io |

JWT signing secret is in `.azure-jwt-secret.txt` (gitignored, local only).

## Known limitations of this deployment

- **No Redis in Azure.** The Consumption/Express Container Apps environment
  doesn't support raw TCP ingress (needed to self-host a `redis:7-alpine`
  container), and Azure Managed Redis (the classic Cache for Redis is being
  retired) only has Enterprise-tier pricing, too costly for a student
  subscription. `ConnectionStrings__Redis` points at a placeholder host
  that's always unreachable. This is safe: `RedisCacheService` and the
  `IConnectionMultiplexer` registration were made resilient (see commit
  `b0bc05d`) — a down Redis degrades to "always cache-miss" instead of
  crashing `/api/specialties`, `/api/services`, or `/health`.
- **Swagger UI is off** (`ASPNETCORE_ENVIRONMENT=Production`) — kept off
  deliberately (public API surface disclosure), per project decision.
- **ACR Tasks (remote build) is blocked** on this subscription — images are
  built locally with Docker Desktop and pushed, not built in the cloud.

## Redeploying after a code change

```bash
# from the repo root, with Docker Desktop running and `docker login` done once:
docker build -f Dockerfile.webapi -t vitalisacr2026.azurecr.io/vitalis-webapi:latest .
docker push vitalisacr2026.azurecr.io/vitalis-webapi:latest

docker build -f Dockerfile.web -t vitalisacr2026.azurecr.io/vitalis-web:latest .
docker push vitalisacr2026.azurecr.io/vitalis-web:latest
```

```powershell
# then force each Container App to pull the new :latest image
az containerapp update --name vitalis-webapi --resource-group vitalis-rg `
  --image vitalisacr2026.azurecr.io/vitalis-webapi:latest
az containerapp update --name vitalis-web --resource-group vitalis-rg `
  --image vitalisacr2026.azurecr.io/vitalis-web:latest
```

## Applying a new EF Core migration to Azure SQL

```bash
SQLPASS=$(cat .azure-sql-password.txt)
export ConnectionStrings__DefaultConnection="Server=tcp:vitalis-sql-2026.database.windows.net,1433;Database=Vitalis;User ID=vitalisadmin;Password=$SQLPASS;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
dotnet ef database update -p src/Vitalis.Infrastructure -s src/Vitalis.WebApi
```

## Tearing everything down (stop billing)

```powershell
az group delete --name vitalis-rg --yes --no-wait
```

This deletes the registry, SQL server/database, and both container apps in
one shot. Do this once grading is done — the resources otherwise keep
running (and consuming the Azure for Students credit) indefinitely.
