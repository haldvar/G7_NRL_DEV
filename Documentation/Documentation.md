# NRL_PROJECT — Documentation

Version: 1.0  
Target framework: `.NET 9`  
Repository: `G7_NRL_DEV` (branch: `Karoline14_finish`)

This file is a concise, developer-focused documentation for the `NRL_PROJECT` web application. It is intended to be checked into the repository root as `DOCUMENTATION.md`.

---

## Overview

`NRL_PROJECT` is a Razor/ASP.NET Core MVC web application for reporting aviation obstacles. The system stores obstacle geometry as GeoJSON (`MapData.GeoJsonCoordinates`) and normalizes geometry into `MapCoordinate` rows for queries and display. The UI uses Tailwind CSS and Leaflet.js. Identity is implemented with ASP.NET Core Identity (roles: `Admin`, `Pilot`, `Registrar`, `ExternalOrg`). The project targets `.NET 9` and includes a Dockerfile for containerized runs.

---

## High-level architecture

- Presentation: Razor Views under `Views/` with shared layouts (`Views/Shared/_Layout.cshtml`, `Views/Shared/_LoginLayout.cshtml`).
- Controllers: `HomeController`, `MapController`, account and other controllers exist in the solution.
- Data access: Entity Framework Core `Data/NRL_Db_Context.cs` (inherits `IdentityDbContext<User>`).
- Models: Domain models in `Models/` (`User`, `MapData`, `MapCoordinate`, `ObstacleData`, `ObstacleReportData`, `Organisation`).
- Seeding: `Data/DataSeeder.cs` (creates roles, organisations, demo users, demo reports).
- Tests: `NRL_PROJECT.Tests` uses EF Core InMemory for unit tests.
- Containerization: `Dockerfile` (multi-stage) builds and publishes the app.

---

## Important files

- `Program.cs` — app startup, service registration, Identity and cookie config, DB registration, security headers, pipeline.
- `Data/NRL_Db_Context.cs` — EF Core DbContext and relationship configuration.
- `Data/DataSeeder.cs` — migration + seed logic executed on startup.
- `Controllers/MapController.cs` — map views, GeoJSON parsing, image upload, report creation, GeoJSON endpoint (`GetObstacles`).
- `Controllers/HomeController.cs` — index / about / privacy / error pages.
- `Models/*` — domain entities and view models.
- `Views/*` — Razor views and partials.
- `Dockerfile` — multi-stage image for .NET 9.
- `NRL_PROJECT.Tests/*` — unit tests and test README.

---

## Running locally

Prerequisites:
- .NET 9 SDK
- Visual Studio 2022 (recommended) or another IDE/CLI
- (Optional) Docker for container runs
- A MySQL/MariaDB instance for production-like runs; tests use InMemory provider.

From Visual Studio:
1. Open the solution.
2. Make sure `NRL_PROJECT` is the startup project: right-click project → __Set as Startup Project__.
3. Start debugging: press __F5__ (or use __Debug > Start Debugging__). The app runs using the configured `DefaultConnection`.
4. To run without the debugger: use __Debug > Start Without Debugging__.

From CLI: dotnet build dotnet run --project NRL_PROJECT/NRL_PROJECT.csproj

Docker: docker build -t nrl_project:local -f NRL_PROJECT/Dockerfile . docker run -p 8080:8080 nrl_project:local
The `Dockerfile` exposes ports `8080` and `8081`.

---

## Configuration

- Connection strings: key `DefaultConnection` in appsettings or environment.
- Admin seeding uses configuration keys `AdminUser:Email` and `AdminUser:Password` (configure via environment or secrets).
- Use __User Secrets__ for local secret configuration during development, or environment variables / secret manager in CI/CD for production.

---

## Authentication & Authorization

- Identity configured in `Program.cs` with:
  - Password policy: min 8 chars, digits, uppercase, lowercase, non-alphanumeric.
  - Lockout: 5 failed attempts, 5 minutes.
  - Cookie paths: login `/Account/Login`, logout `/Account/Logout`, access denied `/Account/AccessDenied`.
- Global MVC authorization policy requires authenticated users by default. Use `[AllowAnonymous]` for public actions.

---

## Security considerations

- Server header is disabled in Kestrel (`serverOptions.AddServerHeader = false`).
- Middleware adds security headers:
  - `X-Content-Type-Options: nosniff`
  - `X-Frame-Options: DENY`
  - `X-XSS-Protection`
  - `Strict-Transport-Security`
  - `Referrer-Policy`
  - `Content-Security-Policy` (CSP is restrictive but currently allows specific CDNs and tile providers; tighten in production).
- Tailwind via CDN uses `unsafe-inline` allowances in CSP. For production, pre-build Tailwind CSS and remove `unsafe-inline`.
- File uploads are written to `wwwroot/uploads`. Validate types and sizes and consider storing outside webroot or in an object store (S3/Blob) with signed access.

---

## Data model (brief)

- `MapData`
  - `MapDataID`, `GeometryType` (`Point`|`LineString`), `MapZoomLevel`, `GeoJsonCoordinates`, `Coordinates` (`MapCoordinate` collection).
  - Computed property `CoordinateSummary` (`[NotMapped]`) for UI display.
- `MapCoordinate`
  - `CoordinateId`, `Latitude`, `Longitude`, `OrderIndex`, `MapDataID`.
- `ObstacleData`
  - `ObstacleID`, `ObstacleType`, `ObstacleHeight`, `ObstacleWidth`, `ObstacleImageURL`, `MapDataID`.
- `ObstacleReportData`
  - `ObstacleReportID`, `SubmittedByUserId`, `ReviewedByUserID`, `ObstacleReportStatus` (enum), `CoordinateSummary`, `MapDataID`, etc.
- `User` extends `IdentityUser` with `FirstName`, `LastName`, `OrgID`, `OrgName`, navigation collections.

Relationships are configured in `NRL_Db_Context.OnModelCreating`.

---

## Key flows

- Submit obstacle with location (`MapController.SubmitObstacleWithLocation`):
  1. Parse `MapData.GeoJsonCoordinates` (GeoJSON) and build `MapCoordinate` list.
  2. Save `MapData` and coordinates.
  3. Save uploaded image to `wwwroot/uploads` (unique filename).
  4. Save `ObstacleData` and create `ObstacleReportData`.
  5. Show `MapConfirmation` view with saved `MapData`.

- GeoJSON endpoint: `MapController.GetObstacles()` returns a FeatureCollection built from `ObstacleData` + `MapData` for client map rendering.

---

## Seeding

`DataSeeder.SeedAsync` runs on startup and:
- Applies migrations (`context.Database.MigrateAsync()`).
- Seeds roles: `Admin`, `Pilot`, `Registrar`, `ExternalOrg`.
- Seeds organisations and demo users (`Pilot`, `Registrar`, `ExternalOrg`) plus admin from configured credentials.
- Seeds demo obstacles and reports (25 demo reports).

Seeding is executed from `Program.cs` within a `CreateScope()` at application startup.

---

## Tests

- Run tests: dotnet test ./NRL_PROJECT.Tests/NRL_PROJECT.Tests.csproj

- The test project uses EF Core InMemory for model and DB-related tests (examples: saving/loading `MapData` with coordinates, default `ObstacleReportData` fields).
- Test recommendations:
  - Use `WebApplicationFactory<TEntryPoint>` for integration tests that need full ASP.NET services.
  - Mock `UserManager`/`SignInManager` for isolated controller unit tests.
  - Use in-memory streams and mocked `IWebHostEnvironment.WebRootPath` for testing file uploads.

---

## Deployment checklist & recommendations

- Replace Tailwind CDN + inline styles with prebuilt CSS to remove `unsafe-inline` from CSP.
- Replace `AuthMessageSender` (console sink) with a production `IEmailSender` implementation (SMTP or transactional email provider).
- Harden file uploads: validate MIME type, extension, and size; consider virus scanning and store files outside webroot or in object storage.
- Use secure secret storage: __User Secrets__ (dev), environment variables, or a secret manager (Azure Key Vault) in production.
- Centralized logging (Serilog or similar) and structured logs for diagnostics.
- Database backups and migrations handled in CI/CD pipeline, not automatically on app start in production (consider controlled migration strategy).
- Tighten CSP and review dependencies allowed in CSP and external CDNs.

---

## Developer notes & extension points

- `Infrastructure/InvariantDoubleModelBinder` accepts both `.` and `,` as decimal separators. Register the provider early if custom model binding precedence is needed.
- Consider introducing DTOs / ViewModels for complex views to separate domain models from view concerns (some views currently use domain models directly).
- Abstract upload logic behind an `IFileStorage` service to enable switching to cloud storage (S3/Blob) later.
- Add integration tests for one controller using `WebApplicationFactory` as an example and a CI pipeline job to run them.
- Consider adding end-to-end tests (Playwright) for client-heavy behavior (map interactions, map confirmation flow, upload).

---

## How to contribute

- Follow branch/PR workflow used in the repository (create a branch, open a pull request to `origin`).
- Run `dotnet test` before opening PR.
- Keep secrets out of Git — use __User Secrets__ or env vars.
- Use descriptive commit messages and include screenshots or short recordings for UI changes where applicable.

---

## Contact & support

For questions about code, architecture or seeding values, refer to the authors listed in `README.md` and open an issue or PR on the repository.

---

## Changelog

- 2025-11-23: Added `DOCUMENTATION.md` — consolidated architecture, run instructions, security, seeding, tests and developer notes.

---