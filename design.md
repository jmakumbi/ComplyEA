# ComplyEA - Application Design Document

## 1. Core Purpose & User Persona

### The "North Star"

ComplyEA automates the complete regulatory compliance lifecycle — from defining legal requirements through generating obligations, scheduling reminders, and delivering notifications — so legal firms and their corporate clients never miss a compliance deadline.

### Primary User Profile (Power User)

The **Compliance Officer** — a legal professional at a law firm managing regulatory obligations across multiple client companies simultaneously. They spend their day generating obligations from regulatory requirements (e.g., Companies Act 2015), tracking statuses across companies (ACME Corp, Beta Finance), reviewing upcoming deadlines, processing reminder schedules, and ensuring notifications are delivered. They need to context-switch between clients rapidly while maintaining a clear view of what's overdue, what's upcoming, and what's been completed.

**Secondary Power Users:**

- **Legal Firm Administrators** who configure client companies, contacts, and applicable regulations.
- **System Administrators** with full cross-tenant access for system configuration and user management.
- **Company Users** with read-only access to their own compliance data and document uploads.

### Usage Context

Office desktop environment, likely single or dual monitors. The application is a Blazor Server web app accessed via browser. Users deal with tabular data (grids of obligations, reminders, companies), date-driven workflows, and multi-step processes (generate -> track -> remind -> complete). The Kenya-focused regulatory data and Africa's Talking SMS integration suggest East African professional offices with standard desktop setups and reliable internet. No offline or mobile-first requirements are evident.

---

## 2. Visual & Emotional Tone

### Brand Keywords

**Professional, Trustworthy, Organized, Efficient, Clear**

Rationale: The domain is regulatory compliance for legal firms — accuracy and reliability are paramount. The existing orange (#fe7109) brand accent on an "Office White" theme signals energy and urgency (appropriate for deadline management) within a clean, professional frame. The "ComplyEA" wordmark is minimal dark gray (#4A4A4A), reinforcing a no-nonsense, business-first identity.

### Information Density

**Compact view** — strongly recommended for the following reasons:

- The Compliance Officer's core workflow involves scanning grids of obligations filtered by status, due date, and company. They need maximum rows visible to triage quickly.
- The data model has 37 business objects with many cross-references. Users will frequently be in `ListView` grids scanning obligation statuses, reminder delivery states, and upcoming deadlines.
- The Dashboard controller provides 4 filtered views (Overdue, Upcoming 30 days, Pending Reminders, All Obligations) — all grid-heavy.
- XAF's default grid-based UI naturally suits compact density. Spacious layouts would reduce the number of visible obligations per screen, forcing more scrolling in a time-sensitive workflow.

**Exception:** Detail Views (individual obligation/company records) can afford slightly more breathing room since they are form-based single-record views.

---

## 3. Functional Architecture

### Top Interaction (Hero Action)

The **Compliance Dashboard** — specifically the **"Overdue Obligations"** and **"Upcoming Deadlines"** filtered views. This is the landing screen that drives all downstream actions. From here, the Compliance Officer triages and then:

1. **Generates Obligations** — periodic batch action via popup with year/quarter/month selection
2. **Changes Obligation Status** — Pending -> In Progress -> Completed workflow
3. **Sends Due Reminders** — batch notification processing

The single most frequent micro-action is **status triage and update** — scanning the obligation grid, identifying items needing attention, and progressing their status. The "Generate Obligations" action is the most impactful batch operation (the one that creates all the work items).

### Navigation Preference

The application uses **Sidebar (Accordion)** navigation with collapsible groups:

| Group | Items |
|-------|-------|
| **Organization** | Legal Firms, Companies, Company Contacts |
| **Regulatory** | Regulatory Acts, Compliance Requirements, Applicable Regulations, Act Acronyms |
| **Compliance** | Obligations, Reminders, Documents |
| **Configuration** | Email Templates, Compliance Templates |
| **Administration** | System Config, all Lookup tables (17+ items) |

**Rationale:**

- 5 logical groups map cleanly to accordion sections.
- Compliance Officers primarily live in the "Compliance" section with occasional jumps to "Organization."
- Admins need access to the deep "Administration" section without it cluttering the Compliance Officer's view (role-based visibility handles this).
- A tree menu would add unnecessary depth; top tabs would be too flat for 20+ navigation items.

---

## 4. Technical Constants

### Platform Target

**Blazor Server only** (ASP.NET Core 3.1). The solution consists of:

- `ComplyEA.Module` (netstandard2.1) — platform-agnostic business logic
- `ComplyEA.Module.Blazor` (netstandard2.1) — Blazor-specific UI customizations
- `ComplyEA.Blazor.Server` (netcoreapp3.1) — Blazor Server host

DevExpress XAF version: **v20.2.13**

### Existing Brand Assets

| Asset | Details |
|-------|---------|
| **Logo** | `Logo.svg` — "ComplyEA" wordmark in dark gray (#4A4A4A), 180x24px |
| **Splash** | `SplashScreen.svg` — White "EA" letters, 46x30px |
| **Primary Accent** | Orange `#fe7109` (from Office White theme) |
| **Theme** | "Office White" DevExpress theme (currently active default) |
| **Available Themes** | 13 themes configured in appsettings.json (Blazing Berry, Purple, plus Bootswatch variants) |

This is **not a blank slate** — the orange + white + dark gray identity is established. Any UI design work should build on these existing assets. However, the branding is still young enough that refinement is possible (e.g., evolving from the generic DevExpress "Office White" theme to a custom-branded variation that better expresses the "Professional, Trustworthy, Organized" tone).

---

## 5. Domain Model Summary

### Organization Layer

- **LegalFirm** — multi-tenant entity with subscription management (Trial/Basic/Professional/Enterprise)
- **Company** — client organizations with registration details, financial year end, type, and sector
- **CompanyContact** — personnel with compliance roles, notification preferences, and obligation assignments

### Regulatory Framework

- **RegulatoryAct** — legislation (e.g., Companies Act 2015, Kenya) with type and scope
- **ComplianceRequirement** — specific obligations within an act, with timeline types (Annual/Quarterly/Monthly/Event-driven/Fixed/Always)
- **ApplicableRegulation** — link between Company and RegulatoryAct ("this company is subject to this act")
- **ActAcronym** — term definitions and section references

### Compliance Workflow

- **ComplianceObligation** — concrete instance of a requirement for a specific company and period, with status workflow (Pending -> InProgress -> Submitted -> Completed/Overdue/Waived)
- **ComplianceReminder** — scheduled notification tied to an obligation, with delivery tracking (Pending/Sent/Failed/Acknowledged)
- **ComplianceDocument** — versioned evidence files attached to obligations

### Configuration & Templates

- **SystemConfiguration** — database-backed key-value store for SMTP, reminder processing, and app settings
- **EmailTemplate** — 6 pre-configured notification templates with 20+ placeholders
- **CompanyReminderSettings** — per-company reminder schedule customization
- **CompanyCalendarSettings** — future calendar sync feature
- **ComplianceTemplate** — reusable compliance document templates

### Lookup Reference Data (17 types)

SubscriptionType, CompanyType, Sector, ComplianceRole, TimelineType, RiskRating, ObligationStatus, ReminderType, NotificationChannel, DeliveryStatus, DocumentType, FileFormat, ActType, RegulationScope, EmailProvider, SMSProvider, CalendarProvider

### Security

- **ApplicationUser** — system users with LegalFirm tenant association
- **ApplicationRole** — System Administrator, Legal Firm Administrator, Compliance Officer, Company User

---

## 6. Core Services

| Service | Responsibility |
|---------|---------------|
| **ObligationGenerationService** | Generates concrete obligations from compliance requirements for specified periods. Supports 6 timeline types with intelligent due date calculation. Includes duplicate detection and company type filtering. |
| **ReminderGenerationService** | Creates tiered reminder schedules (Initial 30d, First 14d, Second 7d, Final 3d, Escalation 1d) based on obligation due dates. Per-company customizable settings. Supports regeneration preserving sent reminders. |
| **NotificationService** | Processes and sends reminder notifications. Template rendering with 20+ placeholders. Email template selection by ReminderType. Delivery status tracking and retry logic. |
| **SmtpEmailService** | SMTP email delivery with TLS/SSL support. Configuration via SystemConfiguration keys. Supports both HTML and plain text alternatives. |

---

## 7. Controllers & Actions

| Controller | Key Actions |
|------------|------------|
| **ObligationGenerationController** | "Generate Obligations" (popup dialog), "Generate for Current Year" (quick action) |
| **ObligationStatusController** | "Change Status" (dropdown), "Mark Complete", "Mark Overdue" (bulk) |
| **ReminderGenerationController** | "Generate Reminders", "Regenerate Reminders", "Generate All Reminders" (batch) |
| **NotificationController** | "Send Due Reminders", "Resend", "Retry Failed", "Preview Message" |
| **ComplianceDashboardController** | "Overdue Obligations", "Upcoming Deadlines", "Pending Reminders", "All Obligations" |

---

## 8. Background Processing

- **ReminderProcessingJob** — .NET BackgroundService running every 15 minutes (configurable)
- 30-second startup delay for initialization
- Automatic retry for failed reminders (max 3 attempts, configurable)
- Configuration-driven enable/disable via SystemConfiguration

---

## 9. Test Data & Users

| Username | Password | Role |
|----------|----------|------|
| Admin | ComplyEA123! | System Administrator |
| FirmAdmin | Test123! | Legal Firm Administrator |
| ComplianceOfficer | Test123! | Compliance Officer |
| CompanyUser | Test123! | Company User |

**Sample Data:**

- Legal Firm: Test Law Associates (Professional subscription)
- Companies: ACME Corporation (Technology sector), Beta Finance Ltd (Financial Services sector)
- Regulatory Act: Companies Act 2015 (Kenya) with 5 compliance requirements
- Contacts: Alice Johnson (ACME, CO), Bob Kamau (Beta Finance, CFO)
- 6 pre-configured email templates with styled HTML

---

## 10. Feature Flags

| Flag | Default | Description |
|------|---------|-------------|
| EnableEmailIntegration | true | SMTP email delivery |
| EnableCalendarSync | false | Calendar integration (future) |
| EnableSMS | false | SMS notifications (future) |
