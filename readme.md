# CF User Service

The **CF User Service** is a containerized .NET API responsible for managing user accounts within the CaterFlow ecosystem.  
It runs inside Docker, while PostgreSQL runs **locally** on the host machine for fast development and persistent storage.

This setup provides:

- Isolated API container  
- Local, persistent PostgreSQL database  
- Clean separation between application and data layers  
- Fast development workflow without rebuilding DB containers  

---

## Architecture

| Component | Runs In | Purpose |
|----------|---------|---------|
| **User Service API** | Docker container | Handles user CRUD and domain logic |
| **PostgreSQL** | Local machine | Persistent development database |
| **Docker Compose** | Host | Builds and runs the API container |

The API connects to the local database using:
Host=host.docker.internal

This hostname allows containers to reach services running on the Windows host.

## Docker Setup

### Dockerfile
The API is containerized using a multi‑stage .NET build:

- Build stage compiles the project  
- Runtime stage hosts the published app  
- Exposes ports 8080 (HTTP) and 8081 (HTTPS)  

### docker-compose.yml
Compose is used to build and run **only the API container**.  
PostgreSQL is intentionally *not* included, since it runs locally.

Example environment variable:

```yaml
ConnectionStrings__DefaultConnection: "Host=host.docker.internal;Port=5432;Database=CaterFlow;Username=postgres;Password=YOURPASSWORD"
```

## Running the Service
There are two supported workflows for running the API.
Both work — but one is recommended.

***Recommended Workflow (Manual CLI)***

This gives you the cleanest, most predictable behavior.
1. Start PostgreSQL locally
Ensure your local Postgres instance is running on:
localhost:5432


With a database named:
CaterFlow


2. Run the API container manually
From the User/ folder:
docker compose up --build


This will:
- Build the API image
- Start the container
- Bind ports 8080/8081
- Connect to your local Postgres
3. Test the API
http://localhost:8080/api/appuser or
https://localhost:8081/api/appuser



Alternative Workflow (Visual Studio)
Visual Studio can also orchestrate Docker Compose using:
- docker-compose.dcproj
- docker-compose.yml
- docker-compose.override.yml
To use this workflow:
- Set docker-compose as the startup project
- Press Run (F5)
- Visual Studio will build and run the containers automatically
This workflow is convenient for debugging because breakpoints work out of the box.

## Development Notes
Visual Studio Launch Profiles
Visual Studio includes several run options:
- IIS Express → runs API locally (not in Docker)
- http/https → runs API via Kestrel (not in Docker)
- Container (Dockerfile) → Visual Studio builds & runs its own container
- docker-compose → runs the entire compose stack
Recommended usage:
- Use docker compose up for clean, manual control
- Use docker-compose (VS) only when debugging inside Docker
- Avoid Container (Dockerfile) unless you explicitly want a standalone debug container
This prevents duplicate containers like CF_User or cfuser.

## Project Structure
Your solution and compose files live one level above the project folder.
This is intentional and required for Visual Studio’s Docker Compose integration.
```
User/
│   CF User.sln
│   docker-compose.yml
│   docker-compose.override.yml
│   docker-compose.dcproj
│   launchSettings.json
│
├── .vs/
├── bin/
├── obj/
└── CF User/                ← Git repo root (.git lives here)
    │   CF User.csproj
    │   Dockerfile
    │   Program.cs
    │   appsettings.json
    │   appsettings.Development.json
    │   readme.md
    │
    ├── Controllers/
    ├── Data/
    ├── Migrations/
    ├── Model/
    ├── Repo/
    └── Services/
```

Why this layout?
- Visual Studio requires the .sln to be above the project folder
- Docker Compose requires the Dockerfile to be below the compose file
- context: CF User works because the Dockerfile lives inside that folder
This layout satisfies all three requirements.

##  Configuration
appsettings.json (container runtime)
```json 
"DefaultConnection": "Host=host.docker.internal;Port=5432;Database=CaterFlow;Username=postgres;Password=YOURPASSWORD"
```

appsettings.Development.json (local run)
```json 
"DefaultConnection": "Host=localhost;Port=5432;Database=CaterFlow;Username=postgres;Password=YOURPASSWORD"
```

## API Endpoints

The CF User Service exposes a simple set of CRUD endpoints for managing user accounts.  
All routes are served under the base path:
| Method | Endpoint                          | Description            |
|--------|------------------------------------|------------------------|
| POST   | `/api/appuser`                     | Create a new user      |
| GET    | `/api/appuser/by-email?email=`     | Get a user by email    |
| PUT    | `/api/appuser/{id}`                | Update a user by ID    |
| DELETE | `/api/appuser/{id}`                | Delete a user by ID    |


## Future Enhancements
- JWT authentication
- Refresh tokens
- KYC/KYE workflows
- Additional microservices (Menu, Orders, Billing)
- API Gateway integration
- Centralized logging & metrics
