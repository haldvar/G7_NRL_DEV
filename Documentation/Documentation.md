# NRL_PROJECT – Technical Documentation  
Version: 1.1  
Target Framework: .NET 9  
Repository: G7_NRL_DEV  

This document provides a structured, developer-oriented overview of the NRL_PROJECT web application.  
It supplements the user-facing README (in Norwegian) with architectural, technical, and operational details in English, aligning with common industry practice for source code documentation.

---

## 1. Overview

NRL_PROJECT is an ASP.NET Core MVC web application used for **reporting, storing, and reviewing aviation obstacles**.  
The system supports pilots, external organizations, register officers, and administrators through a full workflow:

1. Pilots report obstacles using a **Leaflet.js** map interface  
2. Geometry is submitted as **GeoJSON**, normalized into relational form  
3. Register officers review, update, assign, accept or reject reports  
4. Administrators manage users, roles, and organizations  

Technical highlights:
- ASP.NET Core MVC (.NET 9)
- Entity Framework Core (MariaDB backend)
- ASP.NET Identity for authentication & authorization
- Tailwind CSS for UI styling
- Leaflet + Leaflet.pm for map interactions
- Docker support (application + database)

---

## 2. High-Level Architecture
```
┌──────────────────────────────────────────┐
│ UI Layer │
│ Razor Views · Leaflet · Tailwind │
└───────────────────────┬──────────────────┘
│
▼
┌──────────────────────────────────────────┐
│ ASP.NET Core MVC Layer │
│ Controllers · ViewModels · Validation │
└───────────────────────┬──────────────────┘
│ EF Core
▼
┌──────────────────────────────────────────┐
│ Database Layer │
│ MariaDB · Tables: Users, Obstacles, │
│ MapData, MapCoordinates, Reports, Orgs │
└──────────────────────────────────────────┘
```

### Core components:
```
| Layer | Description |
|-------|-------------|
| **Presentation Layer** | Razor views, Tailwind, Leaflet map client |
| **Controller Layer** | MapController, RegistrarController, HomeController, Account controllers |
| **Domain Model** | `ObstacleData`, `MapData`, `MapCoordinate`, `ObstacleReportData`, `User`, `Organisation` |
| **Persistence Layer** | EF Core with MariaDB; migrations enabled |
| **Infrastructure** | Custom model binders, file storage utilities, seeding |
```

---

## 3. Key Application Flows

### 3.1 Submitting an Obstacle (Pilot Flow)
Triggered by **MapController.SubmitObstacleWithLocation**:

1. User draws geometry in Leaflet  
2. Leaflet.pm outputs **GeoJSON**  
3. Backend parses the GeoJSON into:
   - `MapData`
   - `MapCoordinate` collection  
4. Optional image file is validated and stored in `/wwwroot/uploads`  
5. System creates:
   - `ObstacleData`  
   - `ObstacleReportData` with status *New*  
6. Confirmation view is shown  

Validation includes:
- Required fields (type, geometry)
- Accepted file types and max size
- GeoJSON structure validation

---

### 3.2 Register Officer Workflow
Handled in **RegistrarController**:

- Filter, view, and search incoming reports  
- Update report status:
  - `New → InProgress → Resolved/Deleted`  
- Update obstacle attributes (type, height, width)
- Delegate reports to other officers  
- Add or modify internal comments  
- View geometry and metadata in the admin UI  

---

### 3.3 GeoJSON Endpoint
`MapController.GetObstacles()` returns a full **GeoJSON FeatureCollection** for rendering obstacles in the client map.

This endpoint powers:
- Map overlays  
- Report preview  
- Register officer visualization  

---

## 4. Data Model

### 4.1 Entity Diagram (Textual Overview)
```
User (IdentityUser)
├─ OrgID → Organisation
└─ ObstacleReports (submitted)

Organisation
└─ Users (one-to-many)

ObstacleData
├─ ObstacleID
├─ MapDataID → MapData
└─ ObstacleReports (one-to-many)

ObstacleReportData
├─ SubmittedByUserId → User
├─ ReviewedByUserID → User
├─ ObstacleID → ObstacleData
└─ MapDataID → MapData

MapData
├─ GeoJsonCoordinates
├─ GeometryType (Point/LineString)
└─ Coordinates (MapCoordinate*)

MapCoordinate
├─ Latitude
├─ Longitude
└─ OrderIndex
```

---

### 4.2 Entities Summary

#### **MapData**
Stores spatial data in both:
- Raw GeoJSON (for frontend)
- Normalized coordinates (for backend queries)

#### **MapCoordinate**
A single ordered coordinate belonging to a geometry.

#### **ObstacleData**
A domain entity describing the physical obstacle (type, height, width, optional image).

#### **ObstacleReportData**
Tracks:
- Who submitted  
- Who reviewed  
- Status  
- Comments  
- Associated geometry  

#### **User**
Extends IdentityUser with:
- `FirstName`, `LastName`
- `OrgID`, `OrgName`

---

## 5. Security & Identity

### 5.1 Authentication
Managed by ASP.NET Identity with:

- Local accounts  
- Identity cookies  
- Login, logout, access denied paths configured  

### 5.2 Authorization
Global authorization policy requires authentication unless `[AllowAnonymous]`.

Roles:
- `Admin` – full access  
- `Pilot` – submit + view own reports  
- `Registrar` – full report management  
- `ExternalOrg` – read-only access to org-related data  

### 5.3 Security Headers
Added in middleware:
- `X-Frame-Options: DENY`
- `X-Content-Type-Options: nosniff`
- `Referrer-Policy: no-referrer`
- Strict CSP with allow-listing for CDN resources

### 5.4 File Upload Security
- Max 5MB  
- MIME type validation  
- Stored under `/wwwroot/uploads` with unique filenames  

---

## 6. Configuration

#### **Database Configuration**
Connection strings read from:
- `appsettings.json`
- environment variables  
- Docker secrets when containerized  

#### **Admin Seeding**
Configured via:

AdminUser:Email
AdminUser:Password

#### **Migrations**
Run via:

```dotnet ef database update```

---

## 7. Docker & Deployment

### 7.1 Dockerfile
Multi-stage build:
1. Restore + build  
2. Publish  
3. Serve via ASP.NET runtime image  

### 7.2 Ports
Exposed: `8080`, `8081` depending on environment.

### 7.3 Compose Setup
Runs:
- Application container  
- MariaDB container  

---

## 8. Testing

Test project: `NRL_PROJECT.Tests`

#### Coverage includes:
- Model behavior (default values, validation)
- GeoJSON parsing
- Controller logic for obstacle submission
- Image upload handling
- Register officer workflow
- Authorization rules
- EF Core InMemory database integrity

Run tests:

```dotnet test ./NRL_PROJECT.Tests/NRL_PROJECT.Tests.csproj```

All 70 tests should pass.

---

## 9. Seeding

`DataSeeder.SeedAsync()` performs:

- Database migrations  
- Role creation  
- Organisation seeding  
- Demo user creation  
- Creation of ~25 demo reports & obstacles  

Executed automatically at application startup.

---

## 10. Developer Notes & Extension Points

Recommended improvements for future versions:

- Replace inline Tailwind with precompiled CSS to strengthen CSP
- Externalize file storage (Azure Blob / AWS S3)
- Introduce DTOs for cleaner controller-action binding
- Add integration tests with `WebApplicationFactory`
- Add Playwright/Selenium tests for map interactions
- Move image uploads outside webroot for security hardening

---

## 11. Changelog

#### **v1.1 – Updated Technical Documentation (2025-12-04)**
- Full rewrite for clarity and professional structure  
- Added architecture diagrams and flow descriptions  
- Clarified security configuration and deployment notes  
- Added test coverage summary  

---

## 12. Contact

For further development or questions, refer to the repository maintainers (Group 7 – UiA IT & IS).
