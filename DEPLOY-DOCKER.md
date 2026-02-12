# ComplyEA Docker Deployment Prompt

## Context

You are working on **ComplyEA**, a DevExpress XAF 20.2.13 Blazor Server application targeting .NET Core 3.1 with XPO (eXpress Persistent Objects) for data persistence.

### Project Structure

```
ComplyEA/
├── ComplyEA.Module/              (netstandard2.1 - business logic)
├── ComplyEA.Module.Blazor/       (netstandard2.1 - Blazor UI)
├── ComplyEA.Blazor.Server/       (netcoreapp3.1 - host app)
│   ├── Startup.cs
│   ├── BlazorApplication.cs
│   ├── appsettings.json
│   └── Services/BackgroundJobs/
├── ComplyEA.sln
├── CLAUDE.md
└── README.md
```

### Current State

- The app currently uses **SQL Server LocalDB** (`(localdb)\\mssqllocaldb`) for development
- DevExpress packages come from the **DevExpress NuGet feed** (requires authentication)
- The connection string is in `appsettings.json` under `ConnectionStrings:ConnectionString`

### Target State

- **Database**: External PostgreSQL server at `192.168.0.7` (this is the CasaOS/ZimaOS host running PostgreSQL)
  - Username: `casaos`
  - Password: `casaos`
  - Database name: `ComplyEA`
  - Port: `5432` (default)
- **Docker**: Single-container deployment (app only — no database container needed)
- **Deployment targets**: Local registry → GitHub Container Registry → ZimaOS

---

## Phase 0: Migrate from SQL Server to PostgreSQL

This is a prerequisite before Dockerising. XPO supports PostgreSQL natively via the `PostgreSqlConnectionProvider` — no ORM changes needed.

### 0.1 Add the Npgsql NuGet package

Add the `Npgsql` package to **both** `ComplyEA.Module` and `ComplyEA.Blazor.Server`:

```bash
# Npgsql 4.1.x is the last version compatible with netstandard2.1 / netcoreapp3.1
# Do NOT use Npgsql 5+ (requires .NET 5) or Npgsql 8+ (requires .NET 6)
dotnet add ComplyEA.Module/ComplyEA.Module.csproj package Npgsql --version 4.1.13
dotnet add ComplyEA.Blazor.Server/ComplyEA.Blazor.Server.csproj package Npgsql --version 4.1.13
```

> **Critical version constraint**: The project targets `netcoreapp3.1` / `netstandard2.1`. Npgsql 4.1.x is the correct version line. Later versions have minimum framework requirements that exceed .NET Core 3.1.

### 0.2 Update the connection string format

Change the connection string in `ComplyEA.Blazor.Server/appsettings.json` from SQL Server format to XPO PostgreSQL format:

**Before (SQL Server / LocalDB):**
```json
{
  "ConnectionStrings": {
    "ConnectionString": "Integrated Security=SSPI;Pooling=false;Data Source=(localdb)\\mssqllocaldb;Initial Catalog=ComplyEA"
  }
}
```

**After (PostgreSQL):**
```json
{
  "ConnectionStrings": {
    "ConnectionString": "XpoProvider=Postgres;Server=192.168.0.7;Port=5432;User ID=casaos;Password=casaos;Database=ComplyEA;Encoding=UNICODE"
  }
}
```

Key points about the XPO PostgreSQL connection string:
- `XpoProvider=Postgres` tells XPO to use `PostgreSqlConnectionProvider` — this is mandatory
- `Encoding=UNICODE` is recommended for proper character set handling
- Do **not** include `Pooling=false` (that was SQL Server-specific; Npgsql handles pooling differently)

### 0.3 Verify BlazorApplication.cs connection string handling

Check `ComplyEA.Blazor.Server/BlazorApplication.cs` (around lines 27-29) where the connection string is set. The XPO data store provider should already work with the new connection string because XPO auto-selects the provider based on the `XpoProvider=` parameter. However, verify:

1. The connection string is read from `IConfiguration["ConnectionStrings:ConnectionString"]` (not hardcoded)
2. If there is any SQL Server-specific code (e.g., explicit `MSSqlConnectionProvider` usage), remove it and let XPO auto-detect from the `XpoProvider` parameter
3. The `ConnectionStringDataStoreProvider` or `XPObjectSpaceProvider` should receive the raw connection string — XPO parses the `XpoProvider` prefix and routes to `PostgreSqlConnectionProvider` automatically

### 0.4 Check for SQL Server-specific code in business objects

Search the entire `ComplyEA.Module/` project for any SQL Server-specific patterns:
- `[DbType("nvarchar...")]` or `[DbType("varchar...")]` attributes — these may need adjustment for PostgreSQL (XPO usually handles this, but custom `DbType` overrides won't translate)
- Direct SQL queries using T-SQL syntax (e.g., `GETDATE()`, `ISNULL()`, `TOP N`) — replace with ANSI SQL or XPO criteria
- Any `SqlCommand` or `SqlConnection` direct usage

### 0.5 Create the PostgreSQL database

Before first run, the database must exist on the target server. XPO will create tables automatically but needs the database to be present:

```bash
# Connect to PostgreSQL on the ZimaOS host
psql -h 192.168.0.7 -U casaos -d postgres

# Create the database
CREATE DATABASE "ComplyEA" OWNER casaos;

# Verify
\l
\q
```

> **Note**: PostgreSQL database names are case-sensitive when quoted. The connection string uses `Database=ComplyEA` which XPO will pass as-is. Create the database with matching case.

### 0.6 Update the EasyTest connection string

Also update the EasyTest connection string in `appsettings.json` if present:

```json
{
  "ConnectionStrings": {
    "ConnectionString": "XpoProvider=Postgres;Server=192.168.0.7;Port=5432;User ID=casaos;Password=casaos;Database=ComplyEA;Encoding=UNICODE",
    "EasyTestConnectionString": "XpoProvider=Postgres;Server=192.168.0.7;Port=5432;User ID=casaos;Password=casaos;Database=ComplyEAEasyTest;Encoding=UNICODE"
  }
}
```

### 0.7 Build and test the migration

```bash
dotnet build ComplyEA.sln
dotnet run --project ComplyEA.Blazor.Server
```

Verify:
- The app starts without connection errors
- XPO creates all tables in PostgreSQL automatically
- Login with `Admin / ComplyEA123!` works
- Seed data (lookup tables, test users) is created correctly
- Navigate through key screens to confirm data reads/writes work

---

## Phase 1: Dockerise the Application

### 1.1 Create `NuGet.Config` in the solution root

DevExpress packages require an authenticated feed. Credentials are injected at build time via Docker build args — **never baked into the image**.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="DXFeed" value="https://nuget.devexpress.com/api" />
  </packageSources>
  <packageSourceCredentials>
    <DXFeed>
      <add key="Username" value="%DEVEXPRESS_NUGET_USER%" />
      <add key="ClearTextPassword" value="%DEVEXPRESS_NUGET_KEY%" />
    </DXFeed>
  </packageSourceCredentials>
</configuration>
```

### 1.2 Create multi-stage `Dockerfile` in the solution root

Key requirements:

**Build stage:**
- Base image: `mcr.microsoft.com/dotnet/core/sdk:3.1`
- Accept `DEVEXPRESS_NUGET_KEY` as a build arg (username is typically `DevExpress`)
- Copy `NuGet.Config` and inject credentials as environment variables during `dotnet restore`
- Build in Release configuration
- Publish `ComplyEA.Blazor.Server`

**Runtime stage:**
- Base image: `mcr.microsoft.com/dotnet/core/aspnet:3.1`
- Expose port **5000** (HTTP only — HTTPS termination handled externally)
- Set `ASPNETCORE_URLS=http://+:5000`
- Set `ASPNETCORE_ENVIRONMENT=Production`
- The connection string comes from environment variable `ConnectionStrings__ConnectionString` (double underscore is .NET's convention for nested config via env vars)
- **Do NOT embed the DevExpress NuGet key in the runtime image**

### 1.3 Create `docker-compose.yml` in the solution root

Since the database is external (PostgreSQL on 192.168.0.7), this is a **single-service** compose file:

```yaml
version: "3.8"

services:
  app:
    build:
      context: .
      dockerfile: Dockerfile
      args:
        DEVEXPRESS_NUGET_KEY: ${DEVEXPRESS_NUGET_KEY}
    container_name: complyea-app
    ports:
      - "${WEB_PORT:-8080}:5000"
    environment:
      - ConnectionStrings__ConnectionString=XpoProvider=Postgres;Server=${DB_HOST:-192.168.0.7};Port=${DB_PORT:-5432};User ID=${DB_USER:-casaos};Password=${DB_PASSWORD:-casaos};Database=${DB_NAME:-ComplyEA};Encoding=UNICODE
      - ASPNETCORE_ENVIRONMENT=Production
    restart: unless-stopped
    extra_hosts:
      - "host.docker.internal:host-gateway"
```

> **`extra_hosts` note**: If the PostgreSQL server is on the Docker host itself (which it is on ZimaOS at 192.168.0.7), the container needs network access to the host. The `extra_hosts` mapping ensures `host.docker.internal` resolves correctly. However, since we're using the explicit IP `192.168.0.7`, this works as long as the container can reach the host network — which Docker's default bridge network allows for LAN IPs. If you encounter connectivity issues, add `network_mode: host` as a fallback (but this loses port mapping).

### 1.4 Create `.env.example` (template — do NOT commit the actual `.env`)

```env
# DevExpress NuGet feed API key (get from https://nuget.devexpress.com/)
DEVEXPRESS_NUGET_KEY=your_devexpress_api_key_here

# PostgreSQL connection (defaults match ZimaOS CasaOS PostgreSQL)
DB_HOST=192.168.0.7
DB_PORT=5432
DB_USER=casaos
DB_PASSWORD=casaos
DB_NAME=ComplyEA

# Web UI port exposed on the host
WEB_PORT=8080
```

### 1.5 Create `.dockerignore`

Exclude: `bin/`, `obj/`, `.vs/`, `*.user`, `*.suo`, `.env`, `node_modules/`, `*.md` (except `CLAUDE.md`), and any test databases (`.mdf`, `.ldf`).

### 1.6 Create `appsettings.Production.json`

In `ComplyEA.Blazor.Server/`, create a production config that will be overridden by the environment variable:

```json
{
  "ConnectionStrings": {
    "ConnectionString": "XpoProvider=Postgres;Server=192.168.0.7;Port=5432;User ID=casaos;Password=casaos;Database=ComplyEA;Encoding=UNICODE"
  }
}
```

### 1.7 Verify connection string override works

Ensure `BlazorApplication.cs` reads the connection string from `IConfiguration` so that the `ConnectionStrings__ConnectionString` environment variable takes precedence over `appsettings.json`. The .NET Core configuration system automatically maps `ConnectionStrings__ConnectionString` env var to `ConnectionStrings:ConnectionString` in the config hierarchy.

---

## Phase 2: Local Docker Registry

### 2.1 Run a local registry

```bash
docker run -d -p 5050:5000 --restart=always --name registry registry:2
```

### 2.2 Create `scripts/docker-local.sh`

```bash
#!/bin/bash
set -e

REGISTRY=localhost:5050
IMAGE_NAME=complyea
TAG=${1:-latest}

# Build
docker compose build app

# Tag
docker tag complyea-app:latest ${REGISTRY}/${IMAGE_NAME}:${TAG}

# Push
docker push ${REGISTRY}/${IMAGE_NAME}:${TAG}

echo "Pushed ${REGISTRY}/${IMAGE_NAME}:${TAG}"
```

### 2.3 Test locally

```bash
# Copy .env.example to .env and fill in your DevExpress key
cp .env.example .env
# Edit .env with your DEVEXPRESS_NUGET_KEY

# Build and run
docker compose up --build -d

# Verify at http://localhost:8080
# Login: Admin / ComplyEA123!

# Check logs if needed
docker compose logs -f app
```

---

## Phase 3: GitHub Container Registry (ghcr.io)

### 3.1 Create `.github/workflows/build-and-push.yml`

```yaml
name: Build and Push ComplyEA

on:
  push:
    branches: [main]
    tags: ['v*']
  pull_request:
    branches: [main]

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  build-and-push:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write

    steps:
      - uses: actions/checkout@v4

      - name: Log in to GitHub Container Registry
        uses: docker/login-action@v3
        with:
          registry: ghcr.io
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Extract metadata
        id: meta
        uses: docker/metadata-action@v5
        with:
          images: ghcr.io/${{ env.IMAGE_NAME }}
          tags: |
            type=ref,event=branch
            type=semver,pattern={{version}}
            type=sha,prefix=

      - name: Build and push
        uses: docker/build-push-action@v5
        with:
          context: .
          push: ${{ github.event_name != 'pull_request' }}
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
          build-args: |
            DEVEXPRESS_NUGET_KEY=${{ secrets.DEVEXPRESS_NUGET_KEY }}
```

### 3.2 Repository secrets required

Configure in GitHub → Repository → Settings → Secrets and Variables → Actions:
- `DEVEXPRESS_NUGET_KEY` — your DevExpress NuGet API key

(`GITHUB_TOKEN` is automatically available — no setup needed.)

---

## Phase 4: Deploy to ZimaOS

### 4.1 Create `deploy/zimaos/docker-compose.yml`

ZimaOS supports custom app installation via docker-compose. Since PostgreSQL is **already running on the ZimaOS host** (192.168.0.7), this is a single-service compose file:

```yaml
name: complyea
services:
  app:
    image: ghcr.io/<YOUR_GITHUB_USERNAME>/complyea:latest
    container_name: complyea-app
    ports:
      - "${WEBUI_PORT:-8080}:5000"
    environment:
      - ConnectionStrings__ConnectionString=XpoProvider=Postgres;Server=192.168.0.7;Port=5432;User ID=casaos;Password=casaos;Database=ComplyEA;Encoding=UNICODE
      - ASPNETCORE_ENVIRONMENT=Production
    restart: unless-stopped
    labels:
      icon: https://raw.githubusercontent.com/<YOUR_GITHUB_USERNAME>/complyea/main/assets/icon.png
```

Key ZimaOS notes:
- **No database container** — PostgreSQL is already on the host at 192.168.0.7
- **`WEBUI_PORT`** — ZimaOS assigns an available port via this variable
- **`name: complyea`** — Required. ZimaOS uses the compose project name to identify custom apps; without a unique name, multiple custom apps conflict with each other
- **Labels** — Optional `icon` label for the ZimaOS dashboard
- **Network access** — The container uses Docker's default bridge network. Since `192.168.0.7` is the host's LAN IP, the container can reach it directly. If connectivity fails, add `network_mode: host` (but this disables port mapping, so the app would be on port 5000 directly)

### 4.2 Ensure PostgreSQL accepts connections from Docker

PostgreSQL on the ZimaOS host must allow connections from the Docker bridge network. Check/update on the host:

**`pg_hba.conf`** — add a line for the Docker subnet:
```
# Allow Docker containers to connect
host    all    casaos    172.16.0.0/12    md5
host    all    casaos    192.168.0.0/24   md5
```

**`postgresql.conf`** — ensure it listens beyond localhost:
```
listen_addresses = '*'
```

Restart PostgreSQL after changes:
```bash
sudo systemctl restart postgresql
```

### 4.3 ZimaOS deployment steps

1. **Create the database** (if not already done):
   ```bash
   psql -h 192.168.0.7 -U casaos -d postgres -c 'CREATE DATABASE "ComplyEA" OWNER casaos;'
   ```

2. **Open ZimaOS web UI** → **App Store** → **Install a Custom App**

3. **Paste** the contents of `deploy/zimaos/docker-compose.yml` (with your actual GitHub username substituted)

4. **Click Submit / Install** — ZimaOS will pull the image from ghcr.io and start the container

5. **Access ComplyEA** at `http://192.168.0.7:<assigned-port>`
   - Default login: `Admin / ComplyEA123!`

### 4.4 Architecture considerations

- ZimaOS runs on **x86-64** — the .NET Core 3.1 image is amd64, no cross-compilation needed
- **Memory**: Without a database container, the app itself needs ~256-512MB RAM. Much lighter than the SQL Server stack
- **GitHub Container Registry visibility**: By default, ghcr.io packages are **private**. Either:
  - Make the package public: GitHub → Packages → Package Settings → Change visibility
  - Or configure Docker auth on ZimaOS: `docker login ghcr.io` with a personal access token that has `read:packages` scope

---

## Additional Files to Create

### `scripts/docker-build.sh`

Convenience script for local Docker builds:

```bash
#!/bin/bash
set -e

if [ ! -f .env ]; then
    echo "ERROR: .env file not found. Copy .env.example to .env and fill in values."
    exit 1
fi

source .env

docker compose build --build-arg DEVEXPRESS_NUGET_KEY=${DEVEXPRESS_NUGET_KEY} app
docker compose up -d

echo ""
echo "ComplyEA is starting..."
echo "Access at http://localhost:${WEB_PORT:-8080}"
echo "Default login: Admin / ComplyEA123!"
```

### Update `README.md`

Add a **Docker Deployment** section covering:
- Prerequisites (Docker, Docker Compose, DevExpress NuGet key, PostgreSQL on 192.168.0.7)
- Quick start with `docker compose up`
- Environment variable reference table
- ZimaOS deployment instructions linking to `deploy/zimaos/`

### Update `.gitignore`

Ensure these are excluded:
```
.env
*.user
*.suo
.vs/
*.mdf
*.ldf
```

---

## Execution Order

1. **Phase 0 first** — migrate to PostgreSQL and verify the app works against `192.168.0.7` before touching Docker
2. **Phase 1** — create Dockerfile, docker-compose, .env, .dockerignore
3. **Test locally**: `docker compose up --build` — verify at `http://localhost:8080`, login with `Admin / ComplyEA123!`
4. **Phase 2** — set up local registry, push, verify pull-and-run
5. **Phase 3** — push to GitHub, configure secrets, verify ghcr.io builds
6. **Phase 4** — create ZimaOS compose file, deploy on ZimaOS device

## Critical Reminders

- **Npgsql version**: Must be `4.1.x` for .NET Core 3.1 compatibility. Npgsql 5+ requires .NET 5, Npgsql 7+ requires .NET 6. Using the wrong version will cause build failures.
- **XPO connection string**: Must start with `XpoProvider=Postgres;` — this is how XPO knows to use `PostgreSqlConnectionProvider`. Without this prefix, XPO defaults to SQL Server and the connection will fail.
- **PostgreSQL case sensitivity**: Database name `ComplyEA` must be created with matching case using double quotes in psql: `CREATE DATABASE "ComplyEA"`. Without quotes, PostgreSQL lowercases it to `complyea`.
- **DevExpress NuGet key must NEVER appear in committed files** — use `.env` locally, GitHub Secrets in CI
- **Database auto-migration**: XAF handles schema creation automatically. In `BlazorApplication.cs`, check `DatabaseUpdateMode` — use `UpdateDatabaseAlwaysMode` for initial deployments, consider switching to `Never` once stable.
- **HTTPS**: Not handled inside the container. Use a reverse proxy (Nginx, Caddy, or ZimaOS built-in proxy) for TLS termination.
- **PostgreSQL `pg_hba.conf`**: The database server MUST allow connections from the Docker bridge subnet (172.16.0.0/12). Without this, the container will get "connection refused" or authentication errors.
- **.NET Core 3.1 is end-of-life** — Docker images still exist on MCR but won't receive security patches. This is a known constraint of DevExpress XAF 20.2.13. Flag for future migration in TODO.md.
