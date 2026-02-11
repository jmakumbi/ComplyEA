# ComplyEA - Compliance Management System

## Solution Description

ComplyEA is a comprehensive regulatory compliance management system designed for legal firms and their corporate clients. It automates the tracking, scheduling, and notification of compliance obligations arising from legislation such as the Companies Act 2015 (Kenya). Built on the DevExpress XAF (eXpressApp Framework) platform with Blazor Server and XPO persistence, it provides a complete end-to-end compliance workflow from regulatory requirement definition through obligation generation, reminder scheduling, and email notification delivery.

---

## Table of Contents

1. [Architecture](#architecture)
2. [Domain Model](#domain-model)
3. [Organization Management](#organization-management)
4. [Regulatory Framework](#regulatory-framework)
5. [Compliance Workflow](#compliance-workflow)
6. [Reminder & Notification System](#reminder--notification-system)
7. [Email Template Engine](#email-template-engine)
8. [Background Processing](#background-processing)
9. [Dashboard & Navigation](#dashboard--navigation)
10. [Security & Access Control](#security--access-control)
11. [Configuration Management](#configuration-management)
12. [Lookup Reference Data](#lookup-reference-data)
13. [Document Management](#document-management)
14. [Technology Stack](#technology-stack)
15. [Deployment & Database](#deployment--database)

---

## Architecture

### Project Structure

ComplyEA follows the standard XAF modular architecture with three projects:

```
ComplyEA/
├── ComplyEA.Module/                  # Platform-agnostic business logic (netstandard2.1)
│   ├── BusinessObjects/              # 37 XPO persistent classes
│   │   ├── Compliance/               # Obligations, Reminders, Documents, Templates
│   │   ├── Configuration/            # System settings, Email templates, Company settings
│   │   ├── Lookups/                  # 17 reference data lookup types
│   │   ├── NonPersistent/            # Dialog parameter objects
│   │   ├── Organization/             # Legal firms, Companies, Contacts
│   │   ├── Regulatory/               # Acts, Requirements, Applicable regulations
│   │   └── Security/                 # Users, Roles
│   ├── Controllers/Compliance/       # 5 XAF action controllers
│   ├── DatabaseUpdate/               # Schema migrations and data seeding
│   └── Services/                     # 4 service interfaces + 4 implementations
│
├── ComplyEA.Module.Blazor/           # Blazor-specific UI module (netstandard2.1)
│
└── ComplyEA.Blazor.Server/           # Blazor Server host application (netcoreapp3.1)
    ├── Startup.cs                    # DI registration and middleware
    ├── BlazorApplication.cs          # XAF application configuration
    └── Services/BackgroundJobs/      # Automatic reminder processing
```

### Design Principles

- **Service-Oriented Architecture**: Stateless services handle all business logic, injected via DI and consumed by controllers.
- **Database Lookup Tables**: All extensible reference data uses database-backed lookup tables rather than enums, allowing runtime configuration without code changes.
- **Multi-Tenant by Design**: Data is scoped by Legal Firm, supporting multiple firms each managing their own portfolio of client companies.
- **ObjectSpace Isolation**: All data operations go through XAF's `IObjectSpace` abstraction, ensuring proper transaction boundaries and cross-context safety.

---

## Domain Model

The domain model comprises 37 persistent classes organized into 7 categories.

### Entity Relationship Summary

```
LegalFirm
 └── Company (many)
      ├── CompanyContact (many)
      ├── ApplicableRegulation (many) ──→ RegulatoryAct
      │                                      └── ComplianceRequirement (many)
      ├── ComplianceObligation (many)
      │    ├── ComplianceReminder (many)
      │    └── ComplianceDocument (many)
      ├── CompanyReminderSettings (many)
      └── CompanyCalendarSettings (many)
```

---

## Organization Management

### Legal Firm

The top-level tenant entity representing a law firm that manages compliance on behalf of its corporate clients.

| Property | Description |
|----------|-------------|
| Name / ShortName | Firm identification |
| RegistrationNumber | Official registration |
| Email, Phone, Address | Contact details |
| SubscriptionType | Service tier (Trial, Basic, Professional, Enterprise) |
| SubscriptionStartDate / EndDate | Subscription validity period |
| IsActive | Soft delete / deactivation flag |

**Relationships**: Manages many Companies; has many Users.

### Company

Client organizations subject to regulatory compliance. Each company belongs to a single legal firm.

| Property | Description |
|----------|-------------|
| Name / ShortName | Company identification |
| RegistrationNumber | Corporate registration number (e.g., CPR/2020/123456) |
| TaxPin | Tax identification |
| CompanyType | Legal structure (Private Limited, Public, NGO, Partnership, Sole Proprietor) |
| Sector | Industry classification (Technology, Financial Services, etc.) |
| IncorporationDate | Date of incorporation |
| FinancialYearEnd | Financial year end date (drives annual obligation timing) |
| Email, Phone, Address | Contact details |

**Relationships**: Has many Contacts, ApplicableRegulations, Obligations, ReminderSettings, CalendarSettings.

### Company Contact

Personnel within a company who receive compliance notifications and are assigned to obligations.

| Property | Description |
|----------|-------------|
| FirstName / LastName / FullName | Contact identification |
| Title / JobTitle | Professional title |
| ComplianceRole | Role in compliance context (Compliance Officer, MD, CFO, Company Secretary, etc.) |
| Email / Phone / MobilePhone | Contact channels |
| IsPrimaryContact | Primary contact for compliance matters |
| ReceiveReminders | Opt-in/out for reminder notifications |
| PreferredNotificationChannel | Email, SMS, or Both |

---

## Regulatory Framework

### Regulatory Act

Legislation and regulations that impose compliance obligations on companies.

| Property | Description |
|----------|-------------|
| Name / ShortName | Act identification (e.g., "Companies Act 2015" / "Companies Act") |
| Year | Year of enactment |
| Jurisdiction | Geographic jurisdiction (e.g., "Kenya") |
| RegulatoryBody | Enforcing body (e.g., "Registrar of Companies") |
| ActType | Classification: Statute, Regulation, Code of Practice, Standard |
| RegulationScope | General or Sector-Specific |
| ApplicableSector | If sector-specific, which sector (null = applies to all) |
| EffectiveDate | When the act came into force |
| SourceUrl | Link to official gazette or publication |

**Relationships**: Has many ComplianceRequirements, ActAcronyms, and ApplicableRegulations.

### Act Acronym

Definitions of terms and acronyms used within regulatory acts.

| Property | Description |
|----------|-------------|
| Acronym | Short form (e.g., "AGM") |
| FullForm | Full meaning (e.g., "Annual General Meeting") |
| Definition | Detailed definition |
| SectionReference | Section where the term is defined (e.g., "Section 278") |

### Compliance Requirement

Specific obligations defined within a regulatory act. These serve as templates from which concrete obligations are generated for each company.

| Property | Description |
|----------|-------------|
| Title | Requirement name (e.g., "Annual Return Filing") |
| SectionReference | Legal section (e.g., "Section 658") |
| Description | Full requirement description |
| TimelineType | Frequency: Annual, Quarterly, Monthly, Event-driven, Fixed, or Always (continuous) |
| DueDayOfMonth | Day of month when due |
| DueMonth | Month when due (1-12, for annual/fixed requirements) |
| DaysAfterEvent | Days after triggering event (for event-driven requirements) |
| TriggerEvent | Description of the event that triggers this requirement |
| RiskRating | Low, Medium, or High |
| PenaltyAmount / PenaltyDescription | Non-compliance penalties |
| ApplicableCompanyType | Restriction to specific company types (null = all) |

### Applicable Regulation

The link between a Company and a Regulatory Act, representing "this company is subject to this act."

| Property | Description |
|----------|-------------|
| Company | The subject company |
| RegulatoryAct | The applicable act |
| EffectiveFrom / EffectiveTo | Period of applicability (null EffectiveTo = ongoing) |
| Notes | Additional context |

### Template Category

Hierarchical organization for compliance document templates.

| Property | Description |
|----------|-------------|
| Name / Code | Category identification |
| ParentCategory | Parent for nesting (self-referential) |
| SortOrder | Display ordering |

---

## Compliance Workflow

### Obligation Generation

The system automatically generates concrete compliance obligations from requirements using the **ObligationGenerationService**. This is the core workflow engine.

**How it works:**

1. A company has one or more **Applicable Regulations** linking it to regulatory acts.
2. Each act has **Compliance Requirements** with defined timeline types.
3. The generation service creates **Compliance Obligations** for each requirement, calculating due dates based on the timeline type.
4. Duplicate detection prevents the same obligation from being generated twice for the same period.

**Due Date Calculation by Timeline Type:**

| Timeline Type | Due Date Logic |
|---------------|---------------|
| **Annual** | Uses the requirement's DueMonth and DueDayOfMonth within the specified year |
| **Quarterly** | End of the quarter month plus DueDayOfMonth (Q1=Mar, Q2=Jun, Q3=Sep, Q4=Dec) |
| **Monthly** | Specified month plus DueDayOfMonth |
| **Event-driven** | Event date plus DaysAfterEvent |
| **Fixed** | Specific date using DueMonth and DueDayOfMonth |
| **Always** | December 31 of the specified year (continuous obligations) |

**Generation can be triggered via:**

- **Generate Obligations** popup action: Select year, quarter, month, and which timeline types to include. Available from the Company list view.
- **Generate for Current Year** quick action: One-click generation for the current year for the selected company.

### Compliance Obligation

A specific, dated instance of a compliance requirement for a particular company.

| Property | Description |
|----------|-------------|
| Company | The obligated company |
| ComplianceRequirement | The source requirement |
| Title | Obligation title (defaults to requirement title) |
| PeriodYear / PeriodQuarter / PeriodMonth | The compliance period |
| DueDate | Calculated deadline |
| CompletedDate | Date the obligation was fulfilled |
| Status | Current workflow status |
| AssignedTo | Responsible contact person |
| RiskRating | Overridable risk level |
| SubmissionReference | Filing or submission reference number |
| Notes | Additional notes |

**Computed Properties:**

- **IsOverdue**: True when DueDate is past and status is non-terminal.
- **DaysUntilDue**: Calculated days remaining until the due date.

### Status Workflow

Obligations progress through a defined status lifecycle:

```
PENDING  ──→  IN PROGRESS  ──→  SUBMITTED  ──→  COMPLETED (terminal)
   │              │
   │              │
   └──────────────┴──────────→  OVERDUE  ──→  COMPLETED

                                WAIVED (terminal)
```

**Status properties:**

| Status | RequiresAction | IsTerminal |
|--------|---------------|------------|
| Pending | Yes | No |
| In Progress | Yes | No |
| Submitted | Yes | No |
| Overdue | Yes | No |
| Completed | No | Yes |
| Waived | No | Yes |

**Available status actions:**

- **Change Status**: Dropdown action allowing selection of any active status. Works on single or multiple selected obligations.
- **Mark Complete**: Batch action that sets status to Completed and records the CompletedDate.
- **Mark Overdue**: Automatically finds and marks all past-due, non-terminal obligations as Overdue.

---

## Reminder & Notification System

### Reminder Generation

The **ReminderGenerationService** creates a schedule of reminder notifications for each obligation based on configurable timing.

**Default reminder schedule (days before due date):**

| Reminder Type | Days Before Due | Purpose |
|---------------|----------------|---------|
| Initial | 30 days | Early awareness notice |
| First Follow-up | 14 days | First follow-up reminder |
| Second Follow-up | 7 days | Urgent reminder |
| Final Notice | 3 days | Last warning before deadline |
| Escalation | 1 day | Management escalation (if enabled) |

**Features:**

- Per-company customizable reminder schedules via CompanyReminderSettings.
- Escalation reminders are only created when EscalateToManager is enabled for the company.
- Reminders are not generated for terminal (Completed/Waived) obligations.
- Regeneration support: when a due date changes, unsent reminders are deleted and new ones are created while preserving already-sent reminders.

**Available actions:**

- **Generate Reminders**: Create reminder schedule for selected obligation(s).
- **Regenerate Reminders**: Delete unsent reminders and recreate (used after due date changes).
- **Generate All Reminders**: Batch generate for all pending/in-progress obligations.

### Compliance Reminder

A scheduled notification linked to a specific obligation.

| Property | Description |
|----------|-------------|
| ComplianceObligation | The parent obligation |
| ReminderType | Initial, First, Second, Final, or Escalation |
| ScheduledDate | When the reminder should be sent |
| SentDate | When it was actually sent |
| DeliveryStatus | Pending, Sent, Failed, or Acknowledged |
| NotificationChannel | Email, SMS, or Both |
| Recipient | Target contact |
| RecipientEmail / RecipientPhone | Delivery addresses |
| MessageSubject / MessageBody | Rendered message content |
| ErrorMessage | Delivery failure details |
| RetryCount | Number of send attempts |

### Notification Delivery

The **NotificationService** handles the actual sending of reminders.

**Delivery workflow:**

1. Find the appropriate email template for the reminder type.
2. Process template placeholders with obligation/company data.
3. Populate the reminder's MessageSubject and MessageBody.
4. Send via the configured channel (currently Email via SMTP).
5. Update DeliveryStatus to Sent or Failed.
6. Record error details and increment retry count on failure.

**Available actions:**

- **Send Due Reminders**: Bulk-process all reminders scheduled for today or earlier. Displays results: "Processed X reminders: Y sent, Z failed."
- **Resend**: Reset a specific reminder to Pending and re-send.
- **Retry Failed**: Retry all failed reminders up to the configured maximum retry count.
- **Preview Message**: Render the email template with actual data for review before sending.

### Email Delivery

The **SmtpEmailService** provides SMTP-based email delivery.

- Reads SMTP configuration from SystemConfiguration at runtime.
- Supports TLS/SSL connections.
- Supports authenticated and anonymous SMTP.
- Sends both HTML and plain text message alternatives.
- Returns structured results with success/failure status and error messages.

---

## Email Template Engine

ComplyEA includes a full template engine for generating personalized compliance notification emails.

### Pre-built Templates

Six email templates are included out of the box, each with styled HTML:

| Template | Theme Color | Usage |
|----------|-------------|-------|
| Initial Reminder | Blue | 30-day advance notice |
| First Follow-up | Orange | 14-day follow-up |
| Second Follow-up | Red | 7-day urgent reminder |
| Final Notice | Dark Red | 3-day last warning |
| Escalation Notice | Dark/Black | Management escalation |
| Default Reminder | Gray | Fallback template |

### Supported Placeholders

Templates support 20+ data placeholders that are replaced at send time:

| Placeholder | Description |
|-------------|-------------|
| `{{CompanyName}}` | Company full name |
| `{{CompanyShortName}}` | Company short name |
| `{{RequirementTitle}}` | Compliance requirement title |
| `{{DueDate}}` | Formatted due date |
| `{{DaysUntilDue}}` | Calculated days remaining |
| `{{RecipientName}}` | Recipient full name |
| `{{RecipientFirstName}}` | Recipient first name |
| `{{ObligationStatus}}` | Current obligation status |
| `{{RegulatoryAct}}` | Act short name |
| `{{RegulatoryActFull}}` | Act full name |
| `{{PeriodYear}}` | Compliance period year |
| `{{PeriodQuarter}}` | Quarter (Q1, Q2, Q3, Q4) |
| `{{PeriodMonth}}` | Month name |
| `{{SectionReference}}` | Legal section reference |
| `{{RiskRating}}` | Risk level |
| `{{PenaltyAmount}}` | Non-compliance penalty |
| `{{ReminderType}}` | Reminder classification |
| `{{ScheduledDate}}` | Reminder scheduled date |
| `{{CurrentDate}}` | Today's date |
| `{{ActionUrl}}` | Link to the obligation in the system |

### Template Properties

| Property | Description |
|----------|-------------|
| Name / Code | Template identification |
| ReminderType | Which reminder type this template serves |
| Subject | Email subject line (supports placeholders) |
| BodyHtml | HTML email body (supports placeholders) |
| BodyText | Plain text fallback body |
| SmsTemplate | SMS variant (500 character limit) |
| IsDefault | Default template for the reminder type |

---

## Background Processing

### Reminder Processing Job

A hosted background service (`BackgroundService`) that automatically processes due reminders without manual intervention.

**Behavior:**

1. Starts with a 30-second delay after application launch to allow initialization.
2. Runs on a configurable interval (default: every 15 minutes).
3. On each iteration:
   - Reads configuration to check if processing is enabled.
   - Creates a fresh ObjectSpace for data access.
   - Calls `NotificationService.ProcessDueRemindersAsync()` to send all due reminders.
   - Calls `NotificationService.RetryFailedRemindersAsync()` to retry failures within the max retry limit.
   - Commits all changes.
   - Logs results.
4. Handles exceptions gracefully without crashing the application.
5. Supports gracellation via `CancellationToken` for clean shutdown.

**Configuration:**

| Key | Default | Description |
|-----|---------|-------------|
| Reminders.Processing.Enabled | true | Master enable/disable switch |
| Reminders.Processing.IntervalMinutes | 15 | Processing frequency |
| Reminders.Processing.MaxRetries | 3 | Maximum retry attempts for failed reminders |

---

## Dashboard & Navigation

The **ComplianceDashboardController** provides quick-access navigation actions available from any view in the application.

### Dashboard Actions

| Action | Filter Criteria | Purpose |
|--------|----------------|---------|
| **Overdue Obligations** | DueDate < today AND status not terminal | Items requiring immediate attention |
| **Upcoming Deadlines** | DueDate within next 30 days AND status not terminal | Forward planning view |
| **Pending Reminders** | DeliveryStatus = Pending AND SentDate is null | Unsent notifications |
| **All Obligations** | No filter | Complete obligation list |

Each action navigates to a filtered list view with an appropriate caption reflecting the applied filter.

---

## Security & Access Control

### Authentication

- Cookie-based authentication with a dedicated login page.
- Password storage and verification via XAF's built-in security framework.
- Session management with configurable timeouts.

### Authorization Roles

Four predefined roles with granular permissions:

| Role | Access Level | Key Permissions |
|------|-------------|-----------------|
| **System Administrator** | Full | All objects, all operations, administration access, multi-tenant visibility |
| **Legal Firm Administrator** | High | Manage firm settings, manage client companies, manage compliance, configure integrations |
| **Compliance Officer** | Medium | Manage compliance obligations, generate reminders, send notifications, view reports |
| **Company User** | Low | Read-only access to compliance data, upload documents |

### User Properties

| Property | Description |
|----------|-------------|
| UserName | Login identifier |
| FirstName / LastName / FullName | User identity |
| Email / Phone | Contact information |
| LegalFirm | Tenant association for multi-firm isolation |
| DefaultCompany | Default company context |
| IsSystemAdmin | Cross-tenant administrative access |
| LastLoginDate | Audit tracking |

---

## Configuration Management

### System Configuration

A key-value configuration store accessible through the administration interface. All settings are stored in the database and can be modified at runtime without application restart.

**Email SMTP Settings:**

| Key | Description | Example |
|-----|-------------|---------|
| Email.Smtp.Host | SMTP server hostname | smtp.gmail.com |
| Email.Smtp.Port | SMTP port number | 587 |
| Email.Smtp.UseSsl | Enable TLS/SSL | true |
| Email.Smtp.Username | SMTP authentication username | your@email.com |
| Email.Smtp.Password | SMTP authentication password (encrypted) | (app password) |
| Email.From.Address | Sender email address | noreply@complyea.com |
| Email.From.Name | Sender display name | ComplyEA |

**Reminder Processing Settings:**

| Key | Description | Default |
|-----|-------------|---------|
| Reminders.Processing.Enabled | Enable automatic processing | true |
| Reminders.Processing.IntervalMinutes | Processing interval | 15 |
| Reminders.Processing.MaxRetries | Maximum retry attempts | 3 |

**Application Settings:**

| Key | Description | Default |
|-----|-------------|---------|
| App.BaseUrl | Application base URL | https://localhost:5001 |
| App.Name | Application display name | ComplyEA |
| App.SupportEmail | Support contact | support@complyea.com |

**Configuration Properties:**

| Property | Description |
|----------|-------------|
| Key | Unique configuration identifier |
| Value | Configuration value |
| Category | Grouping (Email, Reminders, Application) |
| DataType | Value type (String, Int, Bool, DateTime, Json) |
| IsEncrypted | Whether the value is stored encrypted |
| IsSystemOnly | Restrict visibility to system administrators |

**Helper methods** for typed access: `GetIntValue()`, `GetBoolValue()`, `GetDateTimeValue()`.

### Company Reminder Settings

Per-company overrides for the default reminder schedule.

| Property | Description | Default |
|----------|-------------|---------|
| InitialReminderDays | Days before due for initial reminder | 30 |
| FirstReminderDays | Days before due for first follow-up | 14 |
| SecondReminderDays | Days before due for second follow-up | 7 |
| FinalNoticeDays | Days before due for final notice | 3 |
| DefaultChannel | Preferred notification channel | Email |
| EscalateToManager | Enable escalation reminders | false |
| EscalationContact | Manager to escalate to | (none) |

### Company Calendar Settings

Configuration for external calendar synchronization (future feature).

| Property | Description |
|----------|-------------|
| CalendarProvider | Google or Outlook |
| SyncDueDates | Create calendar events for due dates |
| SyncReminders | Create calendar reminders |
| CalendarId | External calendar identifier |
| LastSyncDate / LastSyncError | Sync status tracking |

---

## Lookup Reference Data

ComplyEA uses 17 database-backed lookup tables for all reference data, ensuring extensibility without code changes.

### Subscription Management

| Code | Name | Max Companies | Max Users |
|------|------|--------------|-----------|
| TRIAL | Trial | Limited | Limited |
| BASIC | Basic | Limited | Limited |
| PROFESSIONAL | Professional | Higher limits | Higher limits |
| ENTERPRISE | Enterprise | Unlimited | Unlimited |

### Company Classifications

**Company Types:** Private Limited, Public Limited, NGO, Partnership, Sole Proprietor

**Sectors:** Energy, Financial Services, Insurance, Manufacturing, Technology, Healthcare, Retail, Agriculture, Construction

### Compliance Reference Data

**Obligation Statuses:** Pending, In Progress, Submitted, Overdue, Completed (terminal), Waived (terminal)

**Timeline Types:** Always (continuous), Annual (365 days), Quarterly (90 days), Monthly (30 days), Event-driven, Fixed (one-time)

**Risk Ratings:** Low (severity 1), Medium (severity 2), High (severity 3)

**Reminder Types:** Initial (30 days), First Follow-up (14 days), Second Follow-up (7 days), Final Notice (3 days), Escalation (1 day)

### Communication

**Notification Channels:** Email, SMS, Both

**Delivery Statuses:** Pending, Sent (successful), Failed, Acknowledged (successful)

### Document Classifications

**Document Types:** Submission, Supporting, Audit, Correspondence, Other

**File Formats:** PDF, DOCX, XLSX, DOC, XLS (with MIME type mappings)

### Integration Providers

**Email Providers:** Gmail, Outlook

**SMS Providers:** Africa's Talking, Twilio

**Calendar Providers:** Google Calendar, Outlook Calendar

### Compliance Roles

Roles that contacts can hold within the compliance context: Compliance Officer (CO), Managing Director (MD), Deputy Managing Director (DMD), Chief Finance Officer (CFO), Company Secretary (CS), Head of Department (HOD), Other.

### Regulatory Classifications

**Act Types:** Statute, Regulation, Code of Practice, Standard

**Regulation Scopes:** General (applies to all), Sector-Specific

---

## Document Management

### Compliance Document

Evidence documents uploaded against compliance obligations, with full versioning support.

| Property | Description |
|----------|-------------|
| FileName / DisplayName | File identification |
| DocumentType | Classification (Submission, Supporting, Audit, etc.) |
| FileFormat | Format with MIME type |
| FileContent | Binary content (delayed loading for performance) |
| FileSize | File size in bytes |
| UploadedOn / UploadedBy | Upload audit trail |
| Version | Version number string |
| IsCurrentVersion | Whether this is the latest version |
| PreviousVersion | Link to prior version (for version chain) |

### Compliance Template

Reusable document templates that can be associated with specific compliance requirements.

| Property | Description |
|----------|-------------|
| Name / Code | Template identification |
| Category | Hierarchical classification |
| ComplianceRequirement | Associated requirement (optional) |
| FileFormat | Template file format |
| TemplateContent | Binary content (delayed loading) |
| Version | Template version |
| Instructions | Usage instructions for template completion |

---

## Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| **Framework** | .NET Core | 3.1 |
| **UI Platform** | Blazor Server | (via XAF) |
| **Application Framework** | DevExpress XAF | 20.2.13 |
| **ORM / Persistence** | DevExpress XPO | 20.2.13 |
| **Database** | SQL Server / LocalDB | 2019+ |
| **Authentication** | ASP.NET Core Cookie Auth | Built-in |
| **Background Jobs** | .NET BackgroundService | Built-in |
| **Email** | System.Net.Mail (SMTP) | Built-in |

---

## Deployment & Database

### Connection Strings

| Configuration | Database | Purpose |
|--------------|----------|---------|
| Default | ComplyEA | Development and production |
| EasyTest | ComplyEAEasyTest | Automated UI testing |

### Database Management

- **Automatic schema creation**: The database schema is created and updated automatically on application startup in Debug mode.
- **Automatic seeding**: All lookup data, security roles, test users, sample regulatory data, email templates, and system configuration are seeded on first run via the `Updater.cs` module updater.
- **Safe date handling**: Built-in `CreateDateSafe()` utility handles month-end edge cases when calculating due dates.

### Build Configurations

| Configuration | Purpose |
|--------------|---------|
| Debug | Development with automatic DB updates |
| Release | Production deployment |
| EasyTest | Automated UI testing with separate database |

### Feature Flags (appsettings.json)

| Flag | Default | Description |
|------|---------|-------------|
| EnableEmailIntegration | true | SMTP email sending |
| EnableCalendarSync | false | External calendar synchronization |
| EnableSMS | false | SMS notification delivery |

---

## Seeded Test Data

The system ships with comprehensive test data for immediate use after first launch.

### Test Users

| Username | Password | Role |
|----------|----------|------|
| Admin | ComplyEA123! | System Administrator |
| FirmAdmin | Test123! | Legal Firm Administrator |
| ComplianceOfficer | Test123! | Compliance Officer |
| CompanyUser | Test123! | Company User |

### Test Organizations

**Legal Firm:** Test Law Associates (Professional subscription, Active)

| Company | Short Name | Type | Sector | Registration |
|---------|------------|------|--------|--------------|
| ACME Corporation | ACME | Private Limited | Technology | CPR/2020/123456 |
| Beta Finance Ltd | BFL | Private Limited | Financial Services | CPR/2019/789012 |

### Test Contacts

| Company | Name | Email | Role |
|---------|------|-------|------|
| ACME Corporation | Alice Johnson | alice@acmecorp.com | Compliance Officer |
| Beta Finance Ltd | Bob Kamau | bob.kamau@betafinance.co.ke | CFO |

### Sample Regulatory Data

**Regulatory Act:** Companies Act 2015 (Kenya)

| Requirement | Section | Timeline | Due Date Logic |
|------------|---------|----------|----------------|
| Annual Return Filing | Section 658 | Annual | DueMonth + DueDayOfMonth |
| Annual General Meeting | Section 278 | Annual | DueMonth + DueDayOfMonth |
| Quarterly Board Meeting | Section 141 | Quarterly | End of quarter |
| Director Appointment/Resignation | Section 148 | Event-driven | Event date + DaysAfterEvent |
| Beneficial Ownership Register | Section 93 | Event-driven | Event date + DaysAfterEvent |

---

## Summary

ComplyEA provides a complete compliance management lifecycle:

1. **Define** regulatory acts and their specific requirements with timeline types and due date rules.
2. **Link** companies to applicable regulations.
3. **Generate** concrete, dated obligations automatically with intelligent due date calculation.
4. **Track** obligation status through a defined workflow (Pending → In Progress → Submitted → Completed).
5. **Schedule** tiered reminder notifications with customizable timing per company.
6. **Send** personalized email notifications using template placeholders with 20+ data fields.
7. **Automate** reminder processing via background job running at configurable intervals.
8. **Monitor** compliance health through dashboard views of overdue items, upcoming deadlines, and pending reminders.
9. **Document** compliance with versioned file attachments and reusable templates.
10. **Secure** access through role-based permissions across four organizational levels.
