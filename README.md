# ComplyEA

A comprehensive compliance management system built with DevExpress XAF (eXpressApp Framework) Blazor Server and XPO (eXpress Persistent Objects).

## Overview

ComplyEA helps legal firms and companies manage regulatory compliance obligations by:
- Tracking regulatory requirements and deadlines
- Auto-generating compliance obligations based on applicable regulations
- Sending automated reminders before due dates
- Managing compliance workflows with status tracking
- Providing dashboard views for overdue and upcoming obligations

## Features

### Phase 1: Domain Model & Security (Complete)
- 33+ business objects for comprehensive compliance management
- Multi-tenant architecture (Legal Firms, Companies, Contacts)
- Role-based access control with 4 predefined roles
- Cookie-based authentication

### Phase 2: Business Logic & Workflows (Complete)
- **Obligation Generation**: Auto-generate obligations from compliance requirements
- **Due Date Calculation**: Supports Annual, Quarterly, Monthly, Event-driven, and Fixed timelines
- **Reminder System**: Configurable reminder schedule (30, 14, 7, 3, 1 days before due)
- **Email Notifications**: SMTP-based email with template placeholders
- **Background Processing**: Automatic reminder processing job
- **Dashboard Actions**: Quick navigation to overdue/upcoming/pending items

## Prerequisites

- .NET Core 3.1 SDK
- PostgreSQL 15+
- DevExpress Universal Subscription (v20.2.13)

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/jmakumbi/ComplyEA.git
cd ComplyEA
```

### 2. Set up PostgreSQL

Create the database user and databases:

```sql
CREATE USER complyea WITH PASSWORD '<your-password>';
CREATE DATABASE "ComplyEA" OWNER complyea;
CREATE DATABASE "ComplyEAEasyTest" OWNER complyea;
```

### 3. Configure the database connection

Edit `ComplyEA.Blazor.Server/appsettings.json` with your PostgreSQL server details:

```json
{
  "ConnectionStrings": {
    "ConnectionString": "XpoProvider=Postgres;Server=<host>;Port=5432;Database=ComplyEA;User Id=complyea;Password=<password>",
    "EasyTestConnectionString": "XpoProvider=Postgres;Server=<host>;Port=5432;Database=ComplyEAEasyTest;User Id=complyea;Password=<password>"
  }
}
```

### 4. Build and run

```bash
dotnet build ComplyEA.sln
dotnet run --project ComplyEA.Blazor.Server
```

The application will be available at `https://localhost:5001`.

### 5. Login

**Default Admin Account:**
- Username: `Admin`
- Password: `ComplyEA123!`

**Test Accounts:**
| Username | Password | Role |
|----------|----------|------|
| FirmAdmin | Test123! | Legal Firm Administrator |
| ComplianceOfficer | Test123! | Compliance Officer |
| CompanyUser | Test123! | Company User (Read-only) |

## Project Structure

```
ComplyEA/
├── ComplyEA.Module/              # Platform-agnostic business logic
│   ├── BusinessObjects/          # XPO persistent classes
│   │   ├── Compliance/           # Obligations, Reminders, Documents
│   │   ├── Configuration/        # Settings, Templates
│   │   ├── Lookups/              # Reference data
│   │   ├── NonPersistent/        # Dialog parameters
│   │   ├── Organization/         # Companies, Contacts, Firms
│   │   ├── Regulatory/           # Acts, Requirements
│   │   └── Security/             # Users, Roles
│   ├── Controllers/              # XAF action controllers
│   │   └── Compliance/           # Obligation, Reminder, Notification controllers
│   ├── DatabaseUpdate/           # Schema migrations and seeding
│   └── Services/                 # Business logic services
├── ComplyEA.Module.Blazor/       # Blazor-specific UI customizations
└── ComplyEA.Blazor.Server/       # Blazor Server host application
    └── Services/
        └── BackgroundJobs/       # Background processing jobs
```

## Key Components

### Business Objects

| Category | Objects |
|----------|---------|
| Compliance | ComplianceObligation, ComplianceReminder, ComplianceDocument, ComplianceTemplate |
| Organization | LegalFirm, Company, CompanyContact |
| Regulatory | RegulatoryAct, ComplianceRequirement, ApplicableRegulation |
| Configuration | SystemConfiguration, CompanyReminderSettings, EmailTemplate |
| Lookups | 17 lookup types (Status, Timeline, Risk, etc.) |

### Services

| Service | Purpose |
|---------|---------|
| ObligationGenerationService | Generate obligations from requirements |
| ReminderGenerationService | Create reminder schedules |
| NotificationService | Process and send reminders |
| SmtpEmailService | SMTP email delivery |

### Controllers (Actions)

| Controller | Actions |
|------------|---------|
| ObligationGenerationController | Generate Obligations, Generate for Current Year |
| ObligationStatusController | Change Status, Mark Complete, Mark Overdue |
| ReminderGenerationController | Generate Reminders, Regenerate Reminders |
| NotificationController | Send Due Reminders, Resend, Retry Failed |
| ComplianceDashboardController | View Overdue, Upcoming, Pending, All |

## Configuration

### Email Settings

Configure SMTP in **Administration > System Configuration**:

| Key | Description | Example |
|-----|-------------|---------|
| Email.Smtp.Host | SMTP server | smtp.gmail.com |
| Email.Smtp.Port | SMTP port | 587 |
| Email.Smtp.UseSsl | Use TLS/SSL | true |
| Email.Smtp.Username | Auth username | your@email.com |
| Email.Smtp.Password | Auth password | (app password) |
| Email.From.Address | Sender email | noreply@complyea.com |
| Email.From.Name | Sender name | ComplyEA |

### Reminder Processing

| Key | Description | Default |
|-----|-------------|---------|
| Reminders.Processing.Enabled | Enable auto-processing | true |
| Reminders.Processing.IntervalMinutes | Processing interval | 15 |
| Reminders.Processing.MaxRetries | Max retry attempts | 3 |

## Email Template Placeholders

Templates support the following placeholders:

| Placeholder | Description |
|-------------|-------------|
| `{{CompanyName}}` | Company full name |
| `{{CompanyShortName}}` | Company short name |
| `{{RequirementTitle}}` | Compliance requirement title |
| `{{DueDate}}` | Due date (formatted) |
| `{{DaysUntilDue}}` | Days remaining |
| `{{RecipientName}}` | Recipient full name |
| `{{RecipientFirstName}}` | Recipient first name |
| `{{ObligationStatus}}` | Current status |
| `{{RegulatoryAct}}` | Act short name |
| `{{PeriodYear}}` | Period year |
| `{{PeriodQuarter}}` | Quarter (Q1, Q2, etc.) |
| `{{RiskRating}}` | Risk rating |
| `{{PenaltyAmount}}` | Penalty amount |

## Testing

See [TEST-SCENARIOS.md](TEST-SCENARIOS.md) for comprehensive end-to-end test scenarios.

### Quick Test Steps

1. Login as `ComplianceOfficer` (Test123!)
2. Navigate to **Organization > Companies**
3. Select "ACME Corporation"
4. Click **"Generate for Current Year"**
5. Navigate to **Compliance > Compliance Obligations**
6. Select an obligation and click **"Generate Reminders"**
7. Navigate to **Compliance > Compliance Reminders**
8. Click **"Preview Message"** to see template rendering

## Development

### Database Migrations

Database schema updates are automatic when running in Debug mode. For production, see the [XAF Database Update documentation](https://docs.devexpress.com/eXpressAppFramework/112829/deployment/updating-applications).

### Adding Business Objects

1. Create XPO persistent class in `ComplyEA.Module/BusinessObjects`
2. Add `[DefaultClassOptions]` attribute for navigation
3. Rebuild and run - schema updates automatically

### EasyTest (Automated UI Testing)

```bash
dotnet build -c EasyTest
```

Uses separate database `ComplyEAEasyTest`.

## Security Roles

| Role | Permissions |
|------|-------------|
| System Administrator | Full access including multi-tenant admin |
| Legal Firm Administrator | Manage firm, clients, compliance |
| Compliance Officer | Manage compliance, view reports |
| Company User | View-only access, upload documents |

## Docker Deployment

### Prerequisites

- Docker and Docker Compose
- DevExpress NuGet API key (from [nuget.devexpress.com](https://nuget.devexpress.com))
- PostgreSQL 15+ accessible from the Docker host

### Build locally with Docker Compose

1. Copy `.env.example` to `.env` and fill in your values:
   ```bash
   cp .env.example .env
   # Edit .env with your DevExpress NuGet key
   ```

2. Build and run:
   ```bash
   docker compose up --build -d
   ```

3. Access the application at `http://localhost:8080`

### Build image only

```bash
docker build \
  --build-arg DEVEXPRESS_NUGET_KEY=<your-key> \
  --build-arg DEVEXPRESS_NUGET_USER=<your-email> \
  -t complyea .
```

### CI/CD

GitHub Actions workflow (`.github/workflows/build-and-push.yml`) automatically builds and pushes to GitHub Container Registry on every push to `master` or version tags.

**Docker image**: `ghcr.io/jmakumbi/complyea:master` (private)

**Required GitHub Secrets:**
| Secret | Description |
|--------|-------------|
| `DEVEXPRESS_NUGET_KEY` | DevExpress NuGet API key |
| `DEVEXPRESS_NUGET_USER` | DevExpress NuGet username |

(`GITHUB_TOKEN` is provided automatically by GitHub Actions.)

### ZimaOS Deployment (Production)

ComplyEA deploys to ZimaOS as a single Docker container, connecting to an external PostgreSQL database on the host.

**Setup overview:**

1. Create a GitHub PAT with `read:packages` scope (no expiration)
2. SSH into ZimaOS and authenticate Docker to ghcr.io:
   ```bash
   docker login ghcr.io -u jmakumbi -p <PAT>
   ```
3. Install via **ZimaOS App Store > Install a Custom App** using the docker-compose template from `deploy/zimaos/docker-compose.example.yml`
4. Access at `http://<zimaos-ip>:8080`

**Updating after a new build:**
```bash
docker pull ghcr.io/jmakumbi/complyea:master
docker stop complyea && docker rm complyea
cd /opt/complyea && docker compose up -d
```

See `deploy_to_zimaos.md` (local, not in git) for full deployment instructions with credentials.

## License

Proprietary. All rights reserved.

## Support

For issues and feature requests, please use the GitHub issue tracker.
