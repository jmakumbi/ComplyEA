# ComplyEA - East African Regulatory Compliance Management System
## Claude Code Implementation Prompt

## Project Overview
Build **ComplyEA**, a comprehensive regulatory compliance management system for the East African market using DevExpress XAF 20.2.13, targeting law firms, corporate secretarial services, and enterprise legal departments.

## Business Context
- **Target Market**: Uganda and East Africa
- **Primary Users**: Law firms (multi-client), corporate legal departments (single entity)
- **Key Regulations**: Companies Act 2012, Data Protection and Privacy Act 2019, Anti-Money Laundering Act, sector-specific regulations (Electricity Act, Insurance Act, Capital Markets Act, etc.)
- **Core Value Proposition**: Automated compliance tracking, deadline management, template distribution, and audit trail

## Technology Stack
- **Platform**: DevExpress XAF 20.2.13
- **Framework**: .NET Core 3.1 with C# 8.0
- **ORM**: XPO (eXpress Persistent Objects)
- **UI**: Blazor Server (primary web interface)
- **Database**: 
  * Development: LocalDB (SQL Server Express LocalDB)
  * Production: SQL Server 2017+ or Azure SQL Database
- **Email Integration**:
  * Gmail API (OAuth 2.0)
  * Microsoft Graph API (for Outlook/Microsoft 365)
- **Calendar Integration**:
  * Google Calendar API
  * Microsoft Outlook Calendar (via Graph API)
- **Additional**: 
  * SMS gateway integration (Africa's Talking API for Uganda)
  * Identity management for OAuth flows

## System Architecture Requirements

### 0. OAuth Configuration & Setup

**Google Cloud Console Setup:**
```
1. Create Project: "ComplyEA" or "ComplyEA-Dev" for development
2. Enable APIs:
   - Gmail API
   - Google Calendar API
3. Create OAuth 2.0 Client ID:
   - Application type: Web application
   - Authorized JavaScript origins:
     * Development: https://localhost:5001
     * Production: https://complyea.yourdomain.com
   - Authorized redirect URIs:
     * Development: https://localhost:5001/signin-google
     * Production: https://complyea.yourdomain.com/signin-google
4. Configure OAuth Consent Screen:
   - App name: ComplyEA
   - User support email: your-email@domain.com
   - Scopes:
     * https://www.googleapis.com/auth/gmail.send
     * https://www.googleapis.com/auth/gmail.readonly
     * https://www.googleapis.com/auth/calendar.events
   - Test users (for development): Add your Gmail addresses
5. Download client credentials (JSON)
6. Extract Client ID and Client Secret for appsettings.json
```

**Azure AD App Registration Setup:**
```
1. Go to Azure Portal > Azure Active Directory > App registrations
2. New Registration:
   - Name: ComplyEA
   - Supported account types: 
     * "Accounts in any organizational directory and personal Microsoft accounts" (for multi-tenant)
   - Redirect URI: 
     * Platform: Web
     * Development: https://localhost:5001/signin-microsoft
     * Production: https://complyea.yourdomain.com/signin-microsoft
3. API Permissions:
   - Microsoft Graph > Delegated permissions:
     * Mail.Send
     * Mail.Read
     * Calendars.ReadWrite
     * offline_access (for refresh tokens)
   - Grant admin consent (if you're admin)
4. Certificates & Secrets:
   - New client secret
   - Description: "ComplyEA Production"
   - Expires: 24 months (maximum)
   - Copy secret value immediately (shown only once)
5. Copy Application (client) ID and Directory (tenant) ID
6. In appsettings.json:
   - ClientId: Application (client) ID
   - TenantId: Directory (tenant) ID or "common" for multi-tenant
   - ClientSecret: The secret value
```

**OAuth Flow Implementation in Blazor:**
```csharp
// Startup.cs or Program.cs (.NET Core 3.1)
public void ConfigureServices(IServiceCollection services)
{
    // Google OAuth
    services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = Configuration["OAuth:Google:ClientId"];
            options.ClientSecret = Configuration["OAuth:Google:ClientSecret"];
            options.Scope.Add("https://www.googleapis.com/auth/gmail.send");
            options.Scope.Add("https://www.googleapis.com/auth/calendar.events");
            options.AccessType = "offline"; // Request refresh token
            options.SaveTokens = true; // Save tokens to authentication properties
        });
    
    // Microsoft OAuth
    services.AddAuthentication()
        .AddMicrosoftAccount(options =>
        {
            options.ClientId = Configuration["OAuth:Microsoft:ClientId"];
            options.ClientSecret = Configuration["OAuth:Microsoft:ClientSecret"];
            options.Scope.Add("Mail.Send");
            options.Scope.Add("Calendars.ReadWrite");
            options.Scope.Add("offline_access");
            options.SaveTokens = true;
        });
}
```

**Redirect URI Format:**
- **Google**: `{BaseURL}/signin-google`
- **Microsoft**: `{BaseURL}/signin-microsoft`
- **Development**: `https://localhost:5001`
- **Production**: `https://your-domain.com` (must be HTTPS)

**Testing OAuth Locally:**
```bash
# Generate self-signed certificate for localhost HTTPS (required for OAuth)
dotnet dev-certs https --trust

# Run application
dotnet run --urls="https://localhost:5001"

# OAuth callback will work at:
# https://localhost:5001/signin-google
# https://localhost:5001/signin-microsoft
```

### 1. Multi-Tenancy Model
```
- Support two deployment models:
  a) SaaS Multi-tenant: Law firms managing multiple client companies
  b) Single-tenant: Corporate legal departments managing one organization
  
- Tenant isolation at data level
- Shared infrastructure, separate data schemas
- Role-based access control per tenant
```

### 2. Core Domain Model

#### Organization Management
```
BusinessObject: LegalFirm
- Properties:
  * Name (String, required)
  * RegistrationNumber (String, unique)
  * PhysicalAddress (String)
  * EmailAddress (String)
  * PhoneNumber (String)
  * IsActive (Boolean)
  * SubscriptionType (Enum: Trial, Basic, Professional, Enterprise)
  * SubscriptionExpiryDate (DateTime)
  
- Associations:
  * ManagedCompanies (Collection of Company)
  * Users (Collection of SystemUser)
```

```
BusinessObject: Company
- Properties:
  * Name (String, required)
  * RegistrationNumber (String, unique, indexed)
  * TIN (String)
  * IncorporationDate (DateTime)
  * CompanyType (Enum: Private, Public, NGO, Partnership)
  * Sector (Enum: Energy, Financial, Insurance, Manufacturing, Services, Other)
  * PhysicalAddress (String)
  * PostalAddress (String)
  * EmployeeCount (Int)
  * IsActive (Boolean)
  
- Associations:
  * LegalFirm (LegalFirm, required for multi-tenant)
  * ComplianceOfficer (CompanyContact, required)
  * Contacts (Collection of CompanyContact)
  * ApplicableRegulations (Collection of ApplicableRegulation)
  * ComplianceObligations (Collection of ComplianceObligation)
  * ReminderSettings (CompanyReminderSettings)
```

```
BusinessObject: CompanyContact
- Properties:
  * FullName (String, required)
  * Position (String)
  * RoleInCompliance (Enum: ComplianceOfficer, MD, DMD, CFO, CTO, GM, HOD, Other)
  * EmailAddress (String, required)
  * PhoneNumber (String)
  * ReceiveReminders (Boolean, default: true)
  * IsActive (Boolean)
  
- Associations:
  * Company (Company, required)
  * AssignedObligations (Collection of ComplianceObligation)
```

#### Regulatory Framework Management

```
BusinessObject: RegulatoryAct
- Properties:
  * Name (String, required, e.g., "Companies Act")
  * FullTitle (String)
  * EffectiveDate (DateTime)
  * IssuingAuthority (String, e.g., "Parliament of Uganda", "ERA")
  * Scope (Enum: General, SectorSpecific)
  * Sector (Enum: nullable, Energy, Financial, Insurance, etc.)
  * ActType (Enum: Statute, Regulation, Code, Standard)
  * DocumentURL (String, nullable)
  * IsActive (Boolean)
  
- Associations:
  * ComplianceRequirements (Collection of ComplianceRequirement)
  * Acronyms (Collection of ActAcronym) // For terminology reference
```

```
BusinessObject: ComplianceRequirement
- Properties:
  * ReferenceSection (String, e.g., "Section 40", "Regulation 5")
  * RequirementTitle (String, e.g., "Annual Return Filing")
  * DetailedObligation (String, rich text, maps to "WHAT" column)
  * Timeline (String, e.g., "Annually", "Within 60 days")
  * TimelineType (Enum: Always, Annual, Quarterly, Monthly, EventDriven, FixedDeadline)
  * BaselineDate (DateTime?, for annual/recurring)
  * DaysFromEvent (Int?, for "Within X days" scenarios)
  * EventTrigger (String?, e.g., "Share allotment", "Licence transfer")
  * Sanctions (String, rich text, maps to "WHY" column)
  * DefaultRiskRating (Enum: Low, Medium, High)
  * RiskRatingReason (String)
  * AuditProcedures (String, rich text)
  * IsActive (Boolean)
  
- Associations:
  * RegulatoryAct (RegulatoryAct, required)
  * DefaultResponsibleRole (Enum: MD, DMD, CFO, CTO, GM, HOD, Other)
  * LinkedTemplates (Collection of ComplianceTemplate)
```

```
BusinessObject: ApplicableRegulation
- Properties:
  * AppliesFrom (DateTime)
  * CustomNotes (String)
  
- Associations:
  * Company (Company)
  * RegulatoryAct (RegulatoryAct)
```

#### Compliance Obligation Tracking

```
BusinessObject: ComplianceObligation
- Properties:
  * ObligationCode (String, auto-generated, format: COM-YYYY-NNNN)
  * Status (Enum: Pending, InProgress, Submitted, Overdue, Completed, Waived)
  * DueDate (DateTime, calculated)
  * CompletionDate (DateTime?)
  * SubmissionEvidence (String, file path/URL)
  * InternalNotes (String)
  * AuditFindings (String)
  * ActionPoints (String)
  * CreatedDate (DateTime)
  * LastReminderSent (DateTime?)
  * ReminderCount (Int)
  
- Associations:
  * Company (Company, required)
  * ComplianceRequirement (ComplianceRequirement, required)
  * AssignedTo (CompanyContact, required)
  * SubmittedBy (CompanyContact?)
  * Reminders (Collection of ComplianceReminder)
  * Documents (Collection of ComplianceDocument)
```

```
BusinessObject: ComplianceReminder
- Properties:
  * ReminderDate (DateTime)
  * ReminderType (Enum: Initial, FirstReminder, SecondReminder, FinalNotice, Escalation)
  * DaysBeforeDue (Int)
  * Subject (String)
  * MessageBody (String, rich text)
  * SentDate (DateTime?)
  * SentVia (Enum: Email, SMS, Both)
  * DeliveryStatus (Enum: Pending, Sent, Failed, Acknowledged)
  * ErrorMessage (String?)
  
- Associations:
  * ComplianceObligation (ComplianceObligation, required)
  * Recipients (Collection of CompanyContact)
```

#### Template & Document Management

```
BusinessObject: ComplianceTemplate
- Properties:
  * TemplateName (String, required)
  * Description (String)
  * FileFormat (Enum: DOCX, XLSX, PDF, Custom)
  * TemplateFile (FileData)
  * Version (String, e.g., "1.0", "2.1")
  * EffectiveDate (DateTime)
  * IsActive (Boolean)
  * MergeFields (String, JSON array of field names for pre-filling)
  
- Associations:
  * ComplianceRequirement (ComplianceRequirement)
  * TemplateCategory (TemplateCategory)
```

```
BusinessObject: ComplianceDocument
- Properties:
  * DocumentName (String)
  * DocumentFile (FileData)
  * UploadDate (DateTime)
  * UploadedBy (String)
  * DocumentType (Enum: Submission, Supporting, Audit, Other)
  * Notes (String)
  
- Associations:
  * ComplianceObligation (ComplianceObligation)
```

#### Configuration & Settings

```
BusinessObject: CompanyReminderSettings
- Properties:
  * EnableEmailReminders (Boolean, default: true)
  * EnableSMSReminders (Boolean, default: false)
  * InitialReminderDays (Int, default: 30) // Days before due date
  * FirstReminderDays (Int, default: 14)
  * SecondReminderDays (Int, default: 7)
  * FinalNoticeDays (Int, default: 1)
  * EscalationEnabled (Boolean, default: true)
  * EscalationEmail (String) // CC senior management
  
- Associations:
  * Company (Company, One-to-One)
```

```
BusinessObject: SystemConfiguration
- Properties (Key-Value pairs):
  * SMSGatewayProvider (Enum: AfricasTalking, Twilio)
  * SMSAPIKey (String, encrypted)
  * SMSSenderID (String)
  * GoogleClientId (String, for Gmail/Calendar OAuth)
  * GoogleClientSecret (String, encrypted)
  * MicrosoftClientId (String, for Outlook/Calendar OAuth)
  * MicrosoftClientSecret (String, encrypted)
  * MicrosoftTenantId (String, for Azure AD)
  * DataRetentionMonths (Int, default: 60) // 5 years
  * AuditLogEnabled (Boolean, default: true)
  * EnableEmailIntegration (Boolean, default: true)
  * EnableCalendarSync (Boolean, default: false) // Optional feature
  * DefaultEmailProvider (Enum: Gmail, Outlook, SMS)
```

### 3. Business Logic Requirements

#### Deadline Calculation Engine
```
Implement service class: ComplianceDeadlineCalculator

Methods:
- CalculateDueDate(ComplianceObligation obligation): DateTime
  Logic:
  * If TimelineType = Always: No due date (ongoing monitoring)
  * If TimelineType = Annual: 
    - Use BaselineDate (e.g., incorporation date, financial year end)
    - Add 1 year, repeat annually
  * If TimelineType = Quarterly: BaselineDate + 3 months, repeat
  * If TimelineType = Monthly: BaselineDate + 1 month, repeat
  * If TimelineType = FixedDeadline: Use specified date
  * If TimelineType = EventDriven:
    - Wait for event trigger (manual or system-detected)
    - Calculate DueDate = EventDate + DaysFromEvent
    
- GenerateObligationsForCompany(Company company, Int forwardMonths = 12): void
  * For each ApplicableRegulation
  * For each ComplianceRequirement under that regulation
  * Calculate all due dates for next X months
  * Create ComplianceObligation records if not already exist
  * Assign to DefaultResponsibleRole mapped to CompanyContact
```

#### Reminder Generation Service
```
Implement service class: ReminderSchedulerService

Methods:
- GenerateReminders(DateTime targetDate): void
  * Query all ComplianceObligations with DueDate within targetDate range
  * For each obligation:
    - Check CompanyReminderSettings
    - Calculate reminder dates based on settings
    - Create ComplianceReminder records
    - Populate message templates with merge fields from EmailTemplate
    
- SendPendingReminders(): void
  * Query ComplianceReminders with Status = Pending and ReminderDate <= Today
  * For each reminder:
    - Get user's preferred email provider (Gmail/Outlook) from UserEmailAccount
    - Use EmailServiceFactory to get appropriate service
    - Send via configured channels (Email/SMS/Calendar)
    - Update SentDate and DeliveryStatus
    - Log delivery status and message ID
    - Handle failures with retry logic (max 3 attempts)
    - For email: Store messageId for delivery tracking
    
- CreateCalendarEvents(List<ComplianceObligation> obligations): void
  * For companies with EnableCalendarSync = true
  * Get user's calendar provider from CompanyCalendarSettings
  * Create calendar events for all pending obligations
  * Store CalendarEventId in ComplianceObligation
  * Handle API rate limiting
  
- UpdateCalendarEvents(List<ComplianceObligation> modifiedObligations): void
  * For obligations with due date changes
  * Update existing calendar events
  * Handle deleted obligations (remove from calendar)
```

#### Template Merge Service
```
Implement service class: TemplateMergeService

Methods:
- MergeTemplate(ComplianceTemplate template, Company company): byte[]
  * Load template file
  * Extract MergeFields JSON
  * Replace placeholders with company data:
    - {CompanyName}, {RegistrationNumber}, {TIN}
    - {Address}, {ComplianceOfficer}, {Date}, etc.
  * Return merged document as byte array for download
  
- SupportedFormats: DOCX (using OpenXML SDK), XLSX (using EPPlus/ClosedXML)
```

#### CRMP Import Service
```
Implement service class: CRMPImportService

Methods:
- ImportFromExcel(byte[] fileContent, RegulatoryAct act): ImportResult
  * Parse Excel file (expecting standard CRMP format)
  * Map columns:
    - Column 1 → RequirementTitle
    - Column 2 → ReferenceSection
    - Column 3 → DetailedObligation
    - Column 4 → Timeline
    - Column 5 → Sanctions
    - Column 6 → DefaultResponsibleRole
    - Column 7 → DefaultRiskRating
    - Column 8 → RiskRatingReason
  * Create ComplianceRequirement records
  * Return ImportResult with success/error details
  
- ValidateImport(byte[] fileContent): ValidationResult
  * Check for required columns
  * Validate data types
  * Flag duplicate references
```

### 4. User Interface Requirements

#### Blazor Server Interface

**Dashboard View:**
- Company selector (for multi-tenant users)
- Summary widgets:
  * Total obligations by status (pie chart)
  * Overdue items count (red alert)
  * Due this week/month (amber warning)
  * Compliance score (percentage)
  * Calendar sync status indicator
- Timeline view: Upcoming deadlines (calendar/Gantt)
- Quick actions: Add manual obligation, upload evidence, sync calendar

**Company Management:**
- CRUD operations for companies
- Regulatory profile configuration:
  * Select applicable acts (checkboxes with sector filtering)
  * Set baseline dates (incorporation, financial year, etc.)
- Contact management with role assignment
- Reminder settings configuration
- Calendar sync settings:
  * Enable/disable calendar integration
  * Connect Google/Outlook account (OAuth flow)
  * Configure sync preferences

**User Settings:**
- Email Account Setup:
  * Connect Gmail (OAuth 2.0 flow with consent screen)
  * Connect Outlook/Microsoft 365 (OAuth 2.0 flow)
  * Set default email provider
  * Test email sending
- Calendar Account Setup:
  * Connect Google Calendar (OAuth with calendar scope)
  * Connect Outlook Calendar (OAuth with calendar scope)
  * Select default calendar
  * Test calendar sync
- Notification Preferences:
  * Email notifications ON/OFF
  * SMS notifications ON/OFF
  * Calendar sync ON/OFF
  * Reminder frequency settings

**OAuth Integration Screens:**
- Gmail/Google OAuth:
  * "Connect with Google" button
  * Redirect to Google consent screen
  * Handle callback with authorization code
  * Store refresh token securely
  * Display connected email address
  * "Disconnect" option
  
- Outlook/Microsoft OAuth:
  * "Connect with Microsoft" button
  * Redirect to Microsoft login
  * Handle callback with authorization code
  * Store refresh token securely
  * Display connected email address
  * "Disconnect" option

**Compliance Calendar:**
- Filter by: Status, Act, Risk Rating, Assigned To, Date Range
- Grid columns: 
  * Obligation Code, Requirement Title, Act, Due Date, Status, Assigned To, Risk
  * Calendar sync status icon (synced/pending/failed)
- Row actions: 
  * View details, Upload document, Mark complete, Defer, Waive
  * "Add to Calendar" button (manual sync)
- Bulk actions: Send reminders, Export to Excel, Sync to Calendar

**Regulatory Library:**
- Browse Acts and Requirements
- Search by keyword, section, act
- View linked templates
- Import CRMP from Excel (admin only)

**Template Library:**
- List templates by category
- Download template
- Preview template (if supported)
- Upload new versions (admin only)

**Reports & Analytics:**
- Compliance status report (by company, by act)
- Overdue items report
- Audit trail report (who did what when)
- Risk heatmap (matrix of requirements by risk rating)
- Email delivery report (sent/failed/pending)
- Calendar sync report
- Export to PDF/Excel

**Admin Configuration:**
- System-wide settings
- OAuth credentials configuration:
  * Google Cloud Console setup instructions
  * Azure AD app registration instructions
  * Client ID/Secret input
- Email provider configuration
- SMS gateway setup
- CRMP import wizard
- User management
- Audit logs viewer

### 5. Security & Compliance Requirements

**Authentication & Authorization:**
- Role-based security:
  * SystemAdmin: Full access, multi-tenant, OAuth configuration
  * LegalFirmAdmin: Manage own firm and clients, can connect OAuth accounts
  * ComplianceOfficer: Manage assigned company, can connect OAuth for notifications
  * CompanyUser: View and upload documents, read-only access
  
**OAuth Security Best Practices:**

1. **Token Storage:**
   - Refresh tokens encrypted using ASP.NET Core Data Protection API
   - Access tokens stored in memory only (short-lived, 1 hour)
   - Never log OAuth tokens
   - Store tokens in separate encrypted database column
   
2. **OAuth Flow Security:**
   - Use state parameter to prevent CSRF attacks
   - Validate redirect URI matches registered URIs
   - Use PKCE (Proof Key for Code Exchange) for additional security
   - Implement token revocation on account disconnect
   
3. **Scope Minimization:**
   - Gmail: Only request `gmail.send` and `gmail.readonly` (not full gmail scope)
   - Google Calendar: Only request `calendar.events` (not full calendar scope)
   - Microsoft Graph: Only request `Mail.Send`, `Mail.Read`, `Calendars.ReadWrite`
   - Request offline_access for refresh tokens
   
4. **Token Refresh:**
   - Automatic token refresh before expiration
   - Graceful handling of refresh failures (prompt user to reconnect)
   - Monitor for revoked tokens (user disconnected from Google/Microsoft)
   
5. **Consent Management:**
   - Display clear consent screen explaining what data will be accessed
   - Allow users to disconnect accounts at any time
   - Automatically revoke tokens on disconnect
   - Log all OAuth connections/disconnections for audit

**Data Protection (DPPA Compliance):**
- Data encryption at rest (database-level TDE if using SQL Server)
- Data encryption in transit (HTTPS only, TLS 1.2+)
- Audit logging of all data access
- Data retention policy enforcement (configurable, default 5 years)
- Export/delete company data (GDPR-style right to erasure)
- OAuth tokens considered personal data - encrypt and protect

**Audit Trail:**
- Log all CRUD operations on:
  * ComplianceObligations (status changes, assignments)
  * Document uploads/downloads
  * Reminder sending (email/SMS/calendar)
  * User access to sensitive data
  * OAuth connections/disconnections
  * Email sent via Gmail/Outlook (store message IDs)
  * Calendar events created/updated/deleted
  
**Backup & Disaster Recovery:**
- Automated daily database backups (includes encrypted OAuth tokens)
- Point-in-time recovery capability
- Offsite backup storage
- Test OAuth token recovery after restore

### 6. Integration Requirements

#### Email Integration (Gmail & Microsoft Outlook)

**Gmail Integration via Gmail API:**
```
NuGet Packages:
- Google.Apis.Gmail.v1
- Google.Apis.Auth

Implementation Requirements:
1. OAuth 2.0 Authentication Flow
   - Register ComplyEA in Google Cloud Console
   - Obtain Client ID and Client Secret
   - Implement OAuth consent screen
   - Request scopes: gmail.send, gmail.readonly (for read receipts)
   
2. Service Class: GmailEmailService
   Methods:
   - AuthenticateUser(string userId): UserCredential
     * Store refresh tokens securely in database
     * Handle token refresh automatically
   
   - SendEmail(string to, string subject, string body, List<Attachment> attachments): SendResult
     * Compose MIME message
     * Send via Gmail API
     * Return message ID for tracking
   
   - SendBulkEmails(List<EmailRecipient> recipients, string subject, string body): BulkSendResult
     * Batch sending with rate limiting (Gmail: 100 emails/second for Workspace)
     * Track individual delivery status
   
   - GetDeliveryStatus(string messageId): DeliveryStatus
     * Query sent messages for delivery confirmation
     
3. User Settings Storage
   BusinessObject: UserEmailAccount
   - Properties:
     * User (SystemUser)
     * Provider (Enum: Gmail, Outlook)
     * EmailAddress (String)
     * RefreshToken (String, encrypted)
     * TokenExpiryDate (DateTime)
     * IsDefault (Boolean)
     * IsActive (Boolean)
```

**Microsoft Outlook Integration via Microsoft Graph API:**
```
NuGet Packages:
- Microsoft.Graph
- Microsoft.Identity.Client (MSAL)

Implementation Requirements:
1. OAuth 2.0 Authentication Flow (Microsoft Identity Platform)
   - Register ComplyEA in Azure AD
   - Configure API permissions: Mail.Send, Mail.Read, Calendars.ReadWrite
   - Implement MSAL authentication
   
2. Service Class: OutlookEmailService
   Methods:
   - AuthenticateUser(string userId): IPublicClientApplication
     * Use MSAL for token acquisition
     * Store tokens in database
   
   - SendEmail(string to, string subject, string body, List<Attachment> attachments): SendResult
     * Use Microsoft.Graph.Message
     * Send via GraphServiceClient
     * Support HTML formatting
   
   - SendBulkEmails(List<EmailRecipient> recipients, string subject, string body): BulkSendResult
     * Batch requests to Graph API
     * Handle throttling (Graph API: 10,000 requests per 10 minutes)
   
   - GetDeliveryStatus(string messageId): DeliveryStatus
     * Query sent items folder
```

**Unified Email Service Interface:**
```
Interface: IEmailService
- SendEmail(EmailMessage message): SendResult
- SendBulkEmails(List<EmailMessage> messages): BulkSendResult
- GetDeliveryStatus(string messageId): DeliveryStatus

Factory Pattern: EmailServiceFactory
- GetEmailService(EmailProvider provider): IEmailService
  * Return GmailEmailService or OutlookEmailService based on user preference
  * Load user credentials from database
```

**Email Templates:**
```
BusinessObject: EmailTemplate
- Properties:
  * TemplateName (String)
  * Subject (String, supports merge fields)
  * BodyHTML (String, rich text with merge fields)
  * BodyPlainText (String, fallback)
  * Category (Enum: InitialReminder, FollowUp, FinalNotice, Escalation, Custom)
  * IsActive (Boolean)
  
Merge Fields Support:
- {CompanyName}, {ObligationTitle}, {DueDate}, {DaysUntilDue}
- {ComplianceOfficer}, {ReminderType}, {Sanctions}
- {DownloadTemplateLink}, {SubmitEvidenceLink}
```

#### Calendar Integration (Google Calendar & Outlook Calendar)

**Google Calendar Integration:**
```
NuGet Packages:
- Google.Apis.Calendar.v3

Implementation Requirements:
1. Service Class: GoogleCalendarService
   Methods:
   - CreateEvent(ComplianceObligation obligation): CalendarEvent
     * Event title: "[ComplyEA] {RequirementTitle}"
     * Event description: Detailed obligation text + link to ComplyEA
     * Event date: DueDate
     * Reminders: Set based on CompanyReminderSettings
     * Attendees: ComplianceOfficer + assigned contact
   
   - UpdateEvent(string eventId, ComplianceObligation obligation): CalendarEvent
     * Handle due date changes
     * Update attendees if assignment changes
   
   - DeleteEvent(string eventId): void
     * Remove when obligation is completed or waived
   
   - SyncObligations(Company company): SyncResult
     * Batch create/update events for all pending obligations
     * Store CalendarEventId in ComplianceObligation
```

**Outlook Calendar Integration (via Microsoft Graph):**
```
Service Class: OutlookCalendarService
   Methods:
   - CreateEvent(ComplianceObligation obligation): CalendarEvent
     * Use Microsoft.Graph.Event
     * Same structure as Google Calendar
     * Support categories/colors for risk rating
   
   - UpdateEvent(string eventId, ComplianceObligation obligation): CalendarEvent
   
   - DeleteEvent(string eventId): void
   
   - SyncObligations(Company company): SyncResult
```

**Calendar Sync Configuration:**
```
BusinessObject: CompanyCalendarSettings
- Properties:
  * Company (Company, One-to-One)
  * EnableCalendarSync (Boolean, default: false)
  * CalendarProvider (Enum: GoogleCalendar, OutlookCalendar)
  * SyncPendingObligations (Boolean, default: true)
  * SyncCompletedObligations (Boolean, default: false)
  * ReminderOffsets (String, JSON: ["30 days", "14 days", "7 days", "1 day"])
  * CalendarId (String, target calendar ID if not default)
  * LastSyncDate (DateTime)
  
- Associations:
  * UserCalendarAccount (UserCalendarAccount)
  
BusinessObject: UserCalendarAccount
- Properties:
  * User (SystemUser)
  * Provider (Enum: GoogleCalendar, OutlookCalendar)
  * CalendarEmail (String)
  * RefreshToken (String, encrypted)
  * TokenExpiryDate (DateTime)
  * DefaultCalendarId (String)
  * IsActive (Boolean)
```

**Calendar Event Tracking:**
```
BusinessObject: CalendarEvent (extend ComplianceObligation)
- Properties:
  * CalendarEventId (String, from Google/Outlook)
  * CalendarProvider (Enum: GoogleCalendar, OutlookCalendar)
  * LastSyncedDate (DateTime)
  * SyncStatus (Enum: Synced, PendingSync, SyncFailed, Deleted)
  * SyncErrorMessage (String)
```

**Two-Way Sync Considerations:**
- **One-way sync (recommended for MVP)**: ComplyEA → Calendar
  * When obligation created: Create calendar event
  * When due date changes: Update calendar event
  * When completed: Delete calendar event or mark as completed
  
- **Two-way sync (future enhancement)**: Calendar ↔ ComplyEA
  * Watch for calendar event changes (webhooks)
  * If event rescheduled in calendar: Update due date in ComplyEA
  * If event deleted in calendar: Flag for review in ComplyEA
  * Requires webhook setup and conflict resolution logic

**SMS Integration:**
- Primary: Africa's Talking API (Ugandan SMS gateway)
  * RESTful API
  * SMS sending endpoint
  * Delivery status callback
- Fallback: Twilio (international backup)
- Character limit handling (160 chars, auto-split)

**Document Storage:**
- FileData type for templates and submissions
- Option 1: Database BLOB storage (for small files)
- Option 2: Azure Blob Storage / AWS S3 (for scalability)
- File versioning for templates

### 7. Scheduled Jobs & Background Tasks

**Daily Jobs (run at 2 AM EAT):**
- GenerateObligations: Create next 12 months' obligations for all companies
- GenerateReminders: Create reminders for obligations due within reminder window
- SendPendingReminders: Send all pending reminders via Gmail/Outlook/SMS
- SyncCalendarEvents: Sync pending obligations to Google/Outlook calendars for companies with sync enabled
- RefreshExpiredTokens: Refresh OAuth tokens expiring within 7 days
- ArchiveCompletedObligations: Move completed items older than retention period
- BackupDatabase: Trigger database backup
- CleanupFailedSyncEvents: Retry failed calendar syncs from previous days

**Hourly Jobs:**
- CheckEmailDeliveryStatus: Query Gmail/Outlook for message delivery status
- CheckSMSDeliveryStatus: Query SMS gateway for delivery reports
- ProcessPendingCalendarUpdates: Sync modified obligations to calendars

**Real-time Jobs (triggered by events):**
- OnObligationCreated: Create calendar event immediately if sync enabled
- OnObligationUpdated: Update calendar event if due date changed
- OnObligationCompleted: Remove from calendar or mark as completed
- OnOAuthTokenReceived: Store and encrypt refresh token

**Implementation:**
- Use Hangfire or Quartz.NET for job scheduling
- Jobs should be fault-tolerant (retry on failure with exponential backoff)
- Log all job executions with detailed error messages
- Monitor OAuth token refresh failures and alert admin
- Handle API rate limiting gracefully (Gmail: 250 quota units/user/second, Graph API: 10,000 requests/10 minutes)

### 8. Reporting Requirements

**Standard Reports:**

1. **Compliance Dashboard Report (PDF)**
   - Company: {Name}, Period: {Date Range}
   - Summary: Total obligations, Completed, Pending, Overdue
   - Breakdown by Act
   - Top 5 overdue items
   - Compliance score trend (line chart)

2. **Overdue Items Report (Excel)**
   - Columns: Obligation Code, Requirement, Act, Due Date, Days Overdue, Assigned To, Risk Rating
   - Sort by Days Overdue DESC
   - Highlight high-risk items

3. **Audit Trail Report (PDF/Excel)**
   - Filter: Date range, User, Entity type
   - Columns: Timestamp, User, Action, Entity, Old Value, New Value

4. **Annual Compliance Report (PDF)**
   - For board/management review
   - Summary of all obligations completed in year
   - Penalties avoided
   - Recommendations for next year

**Custom Reports:**
- Allow admin to define custom reports using XtraReports designer
- Save custom report definitions

### 9. Implementation Phases

**Phase 1: Foundation (Weeks 1-2)**
- Setup XAF Blazor Server solution (.NET Core 3.1)
- Configure LocalDB for development
- Create domain model (all BusinessObjects)
- Setup security and authentication
- Basic CRUD operations for all entities
- Admin interface for system configuration

**Phase 2: Core Compliance Engine (Weeks 3-4)**
- Implement ComplianceDeadlineCalculator
- Implement ReminderSchedulerService
- Create obligation generation logic
- CRMP import service
- Build compliance calendar view

**Phase 3: OAuth & Email Integration (Week 5)**
- Setup Google Cloud Console project
- Setup Azure AD app registration
- Implement Gmail OAuth flow (authentication + token storage)
- Implement Outlook OAuth flow (authentication + token storage)
- Create GmailEmailService
- Create OutlookEmailService
- Build EmailServiceFactory
- Email template management
- User email account connection UI

**Phase 4: Calendar Integration (Week 6)**
- Implement GoogleCalendarService
- Implement OutlookCalendarService
- User calendar account connection UI
- Calendar sync settings UI
- One-way sync: ComplyEA → Calendar
- Calendar event tracking
- Sync status monitoring

**Phase 5: SMS & Notification Engine (Week 7)**
- SMS integration (Africa's Talking)
- Unified notification service (Email/SMS/Calendar)
- Reminder generation and sending
- Delivery tracking dashboard
- Failed notification retry logic

**Phase 6: Document Management (Week 8)**
- Template upload and management
- TemplateMergeService
- Document upload to obligations
- File storage configuration (database or cloud)
- Download/preview functionality

**Phase 7: Dashboard & Reporting (Week 9)**
- Blazor dashboard with widgets
- Compliance calendar advanced features
- Company management screens
- Template library
- Standard reports (PDF/Excel)
- Email delivery reports
- Calendar sync reports

**Phase 8: Testing & Optimization (Week 10)**
- Unit testing (business logic, deadline calculations)
- Integration testing (Gmail/Outlook/Calendar APIs)
- OAuth flow testing
- API rate limiting tests
- UAT with sample data
- Performance optimization (indexing, caching)

**Phase 9: Security Hardening (Week 11)**
- OAuth token encryption
- Secure credential storage
- DPPA compliance audit
- Penetration testing
- Security documentation

**Phase 10: Deployment & Launch (Week 12)**
- Import initial regulatory data (Companies Act, etc.)
- Production database setup (SQL Server or Azure SQL)
- Production OAuth app registration (Google/Microsoft)
- Onboard pilot customers
- User training materials
- Production deployment
- Monitoring and alerting setup

### 10. Technical Specifications

**DevExpress Components to Use:**
- XPO for ORM (ensure compatibility with .NET Core 3.1)
- DashboardDesigner for analytics dashboard
- XtraReports for PDF reports
- XtraGrid for data grids (Blazor DataGrid)
- XtraScheduler for calendar view (optional, or use third-party Blazor calendar)
- XtraTreeList for hierarchical data (Acts > Requirements)
- Blazor Rich Text Editor for DetailedObligation field editing

**NuGet Packages:**
```xml
<!-- DevExpress -->
<PackageReference Include="DevExpress.ExpressApp.Blazor" Version="20.2.13" />
<PackageReference Include="DevExpress.ExpressApp.Xpo" Version="20.2.13" />
<PackageReference Include="DevExpress.ExpressApp.Security" Version="20.2.13" />

<!-- Google APIs -->
<PackageReference Include="Google.Apis.Gmail.v1" Version="1.55.0.2393" />
<PackageReference Include="Google.Apis.Calendar.v3" Version="1.55.0.2481" />
<PackageReference Include="Google.Apis.Auth" Version="1.55.0" />

<!-- Microsoft Graph -->
<PackageReference Include="Microsoft.Graph" Version="4.15.0" />
<PackageReference Include="Microsoft.Identity.Client" Version="4.48.1" />

<!-- Job Scheduling -->
<PackageReference Include="Hangfire.Core" Version="1.7.28" />
<PackageReference Include="Hangfire.AspNetCore" Version="1.7.28" />
<PackageReference Include="Hangfire.SqlServer" Version="1.7.28" />

<!-- Utilities -->
<PackageReference Include="Newtonsoft.Json" Version="13.0.1" />
<PackageReference Include="Serilog.AspNetCore" Version="4.1.0" />
<PackageReference Include="ClosedXML" Version="0.95.4" /> <!-- for Excel export -->
```

**Code Style:**
- Follow C# 8.0 conventions
- Use async/await for all I/O operations (API calls, database queries)
- Use nullable reference types for safety
- Implement repository pattern for data access
- Use dependency injection for services
- Comprehensive XML comments on public methods
- Follow XAF naming conventions for BusinessObjects and Controllers

**OAuth Implementation Patterns:**
```csharp
// Store OAuth tokens securely
public class OAuthTokenService
{
    private readonly IXpoDataStoreProvider dataStore;
    private readonly IDataProtectionProvider dataProtection;
    
    public async Task<string> StoreTokenAsync(string userId, string provider, string refreshToken)
    {
        var protector = dataProtection.CreateProtector("OAuthTokens");
        var encryptedToken = protector.Protect(refreshToken);
        // Store in database
    }
    
    public async Task<string> RetrieveTokenAsync(string userId, string provider)
    {
        var protector = dataProtection.CreateProtector("OAuthTokens");
        // Retrieve from database and decrypt
        return protector.Unprotect(encryptedToken);
    }
}
```

**API Rate Limiting Pattern:**
```csharp
public class RateLimitedApiClient
{
    private readonly SemaphoreSlim throttle;
    private readonly TimeSpan minInterval;
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> apiCall)
    {
        await throttle.WaitAsync();
        try
        {
            return await apiCall();
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            // Exponential backoff
            await Task.Delay(GetBackoffDelay(retryCount));
            return await ExecuteAsync(apiCall); // Retry
        }
        finally
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(minInterval);
                throttle.Release();
            });
        }
    }
}
```

**Error Handling:**
- Global exception handler in XAF Blazor
- User-friendly error messages for OAuth failures
- Detailed logging (Serilog) to file and database
- Email alerts for critical errors (to admin)
- Specific handling for:
  * OAuth token expiration → auto-refresh
  * API rate limiting → exponential backoff
  * Calendar sync conflicts → flag for manual review
  * Email delivery failures → retry queue

**Performance:**
- Index frequently queried fields (CompanyOID, DueDate, Status, CalendarEventId)
- Pagination for large lists (50 items per page)
- Lazy loading for collections
- Cache regulatory data (Acts, Requirements) in memory
- Cache OAuth tokens in memory with expiration
- Batch API calls where possible (Graph API batch requests)
- Use async streaming for large result sets

**Security:**
- OAuth tokens encrypted at rest using ASP.NET Core Data Protection API
- HTTPS enforced (required for OAuth callbacks)
- CORS configuration for Blazor
- XAF Security System for role-based access
- Audit trail for OAuth connections/disconnections
- Secure token storage separate from application logs

### 11. Data Seeding Requirements

**Initial Data to Import:**

1. **Companies Act 2012:**
   - Use provided CRMP-Companies Act.xlsx
   - ~150+ requirements

2. **Electricity Act:**
   - Use provided 3.CRMP - Electricity Act.xlsx

3. **Capital Markets Act:**
   - Use provided CRMP-Capital markets Act.xlsx

4. **Quality of Service Code:**
   - Use provided CRMP-Quality of Service Code.xlsx

5. **Safety Code:**
   - Use provided CRMP-Safety Code.xlsx

**Seed Data Script:**
- Create migration or seed script to import from Excel
- Run as part of initial deployment
- Version-controlled in repository

### 12. Testing Requirements

**Unit Tests:**
- ComplianceDeadlineCalculator logic
- Timeline parsing (Annual, "Within X days", etc.)
- Risk rating calculations
- Template merge logic
- OAuth token encryption/decryption
- Email message composition
- Calendar event creation logic

**Integration Tests:**
- CRMP import from Excel
- Gmail OAuth flow (use test account)
- Outlook OAuth flow (use test account)
- Gmail API email sending
- Microsoft Graph email sending
- Google Calendar event creation/update/deletion
- Outlook Calendar event creation/update/deletion
- SMS sending (use sandbox API)
- Reminder generation and scheduling
- OAuth token refresh
- API rate limiting handling

**OAuth Flow Tests:**
- Gmail OAuth consent screen
- Outlook OAuth consent screen
- Authorization code exchange
- Refresh token storage
- Token expiration handling
- Multiple account connections
- Account disconnection

**Calendar Sync Tests:**
- One-way sync: Create obligation → Calendar event created
- Update obligation due date → Calendar event updated
- Complete obligation → Calendar event deleted
- Bulk sync for company
- Sync failure recovery
- API quota exceeded scenarios

**UAT Scenarios:**
1. Register new company
2. Connect Gmail account (OAuth flow)
3. Connect Google Calendar (OAuth flow)
4. Apply Companies Act to company
5. Generate obligations for next year
6. Enable calendar sync
7. Verify calendar events created
8. Receive email reminder
9. Upload submission document
10. Mark obligation complete
11. Verify calendar event removed
12. View compliance dashboard
13. Export overdue items report
14. Test email delivery tracking

**Security Testing:**
- OAuth token encryption verification
- HTTPS enforcement
- XSS prevention in Blazor components
- SQL injection prevention (XPO parameterization)
- CSRF protection (Blazor built-in)
- OAuth state parameter validation
- Redirect URI validation

**Performance Testing:**
- 1000 obligations calendar sync time
- 100 simultaneous email sends
- Large CRMP import (500+ requirements)
- Dashboard load time with 50 companies
- API rate limiting stress test

### 13. Deployment Architecture

**Development Environment:**
- LocalDB (SQL Server Express LocalDB) - included with Visual Studio
  * Connection String: `Server=(localdb)\mssqllocaldb;Database=ComplyEA;Integrated Security=true`
  * Automatic instance creation
  * No installation required
  * File-based database in user profile
- OAuth Sandbox:
  * Google Cloud Console: Test OAuth credentials
  * Azure AD: Test app registration
- SMS sandbox mode (Africa's Talking test credentials)
- Development secrets management: User Secrets or appsettings.Development.json

**Staging Environment:**
- SQL Server Express (free) or SQL Server Developer Edition
- Azure SQL Database (Basic tier for testing)
- OAuth Test Apps:
  * Separate Google Cloud project for staging
  * Separate Azure AD app registration for staging
- SendGrid SMTP (free tier for email testing)
- Africa's Talking sandbox API

**Production Environment:**
- **Database Options:**
  * Option 1: Azure SQL Database (S1 Standard tier or higher)
    - Automatic backups
    - Geo-replication available
    - Scalable
    - Data residency: South Africa region (closest to Uganda)
  * Option 2: On-premise SQL Server 2017+ (Standard Edition)
    - Full control over data
    - DPPA compliance (data stays in Uganda)
    - Manual backup management
    - Requires dedicated infrastructure
  * Recommendation: Azure SQL for ease of management, or on-premise if strict data localization required

- **Web Hosting:**
  * Azure App Service (Linux, .NET Core 3.1 runtime)
  * Or on-premise IIS 10.0+ on Windows Server 2016+
  
- **OAuth Production Apps:**
  * Google Cloud Console: Production OAuth 2.0 credentials
    - Configure authorized redirect URIs for production domain
    - Enable Gmail API and Google Calendar API
    - Verify domain ownership
  * Azure AD: Production app registration
    - Configure redirect URIs
    - Request admin consent for Mail.Send and Calendars.ReadWrite
    - Multi-tenant if serving multiple organizations
    
- **Email/Calendar APIs:**
  * Gmail API (production quota: 1 billion quota units/day)
  * Microsoft Graph API (production throttling limits apply)
  
- **SMS Gateway:**
  * Africa's Talking production API
  * Sender ID registration with UCC (Uganda Communications Commission)
  
- **SSL Certificate:**
  * Required for HTTPS (OAuth callback requirement)
  * Let's Encrypt (free) or commercial certificate
  
- **Secrets Management:**
  * Azure Key Vault for OAuth secrets and API keys
  * Or use appsettings.Production.json with file encryption
  
- **Backups:**
  * Automated daily database backups
  * Point-in-time recovery enabled
  * Offsite backup storage (different region/location)
  * Test restore procedures monthly

**Configuration Management:**
```json
// appsettings.json structure
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ComplyEA;Integrated Security=true"
  },
  "OAuth": {
    "Google": {
      "ClientId": "...",
      "ClientSecret": "..."
    },
    "Microsoft": {
      "ClientId": "...",
      "ClientSecret": "...",
      "TenantId": "common"
    }
  },
  "SMS": {
    "Provider": "AfricasTalking",
    "ApiKey": "...",
    "Username": "...",
    "SenderId": "ComplyEA"
  },
  "Features": {
    "EnableEmailIntegration": true,
    "EnableCalendarSync": true,
    "EnableSMS": true
  }
}
```

### 14. Documentation Requirements

**Technical Documentation:**
- API documentation for services
- Database schema diagram
- Deployment guide
- Administrator manual (system configuration)
- OAuth setup guide:
  * Google Cloud Console configuration
  * Azure AD app registration
  * Production domain verification
  * API quota management
- Calendar integration architecture
- Troubleshooting guide for OAuth issues

**User Documentation:**
- User guide (PDF with screenshots)
- Video tutorials:
  * Company registration
  * Connecting Gmail/Outlook account
  * Enabling calendar sync
  * Uploading documents
  * Viewing dashboard
- Email integration FAQ:
  * Why connect Gmail/Outlook?
  * Is my email data secure?
  * How to disconnect account
  * Troubleshooting email sending
- Calendar sync FAQ:
  * Which calendar will be used?
  * Can I sync to shared calendars?
  * What happens when I complete an obligation?
  * How to disable sync

**Training Materials:**
- Compliance officer onboarding checklist
- Admin training slides (includes OAuth setup)
- Email/Calendar integration demo
- Sample data for demos

**Setup Guides:**
1. **Google Cloud Console Setup:**
   - Create new project
   - Enable Gmail API and Calendar API
   - Create OAuth 2.0 credentials
   - Configure consent screen
   - Add authorized redirect URIs
   - Verify domain ownership (for production)

2. **Azure AD App Registration:**
   - Register new application
   - Configure authentication (Web platform)
   - Add redirect URIs
   - API permissions: Mail.Send, Mail.Read, Calendars.ReadWrite
   - Request admin consent
   - Create client secret
   - Configure for multi-tenant (optional)

### 15. Success Metrics

**Technical KPIs:**
- System uptime: 99.5%
- Email delivery rate: >95% (via Gmail/Outlook APIs)
- SMS delivery rate: >90%
- Calendar sync success rate: >98%
- OAuth token refresh success rate: >99%
- Average response time: <2 seconds
- API error rate: <1%

**Business KPIs:**
- Number of companies onboarded
- Number of obligations tracked
- Compliance rate improvement (before/after)
- Email open rate for reminders
- Calendar event acceptance rate
- Customer satisfaction (NPS score)
- Average response time to obligations (measured in days)

### 16. API Quotas and Cost Considerations

**Gmail API Quotas (per project, free):**
- **Quota**: 1 billion quota units per day
- **Send email**: 100 quota units per request
- **Theoretical max**: ~10 million emails/day (won't hit this limit)
- **Rate limit**: 250 quota units per user per second
- **Cost**: Free (included with Google account)
- **Monitoring**: Enable quota tracking in Google Cloud Console

**Google Calendar API Quotas (free):**
- **Quota**: 1 million requests per day
- **Create event**: 5 quota units
- **Update event**: 5 quota units
- **Typical usage**: 1000 companies × 50 obligations = 50,000 events = well within quota
- **Cost**: Free

**Microsoft Graph API Limits (per tenant):**
- **Rate limit**: 10,000 requests per 10 minutes per app
- **Throttling**: HTTP 429 when limit exceeded
- **Outlook email**: No hard daily limit, but subject to throttling
- **Calendar**: Same throttling limits
- **Cost**: Free (included with Microsoft 365 or Outlook.com account)
- **Monitoring**: Use Graph API throttling guidance headers

**Africa's Talking SMS Pricing (Uganda):**
- **Cost**: ~UGX 70-100 per SMS (bulk rates)
- **Delivery rate**: ~95%
- **Example calculation**:
  * 100 companies × 20 obligations/year = 2000 obligations
  * 3 reminders per obligation × 2 contacts = 12,000 SMS/year
  * Cost: 12,000 × UGX 80 = UGX 960,000/year (~$260/year)
- **Recommendation**: Make SMS optional, default to email

**Estimated Infrastructure Costs (Monthly):**

**Option 1: Azure Cloud (Recommended for MVP)**
- Azure App Service (B1 Basic): $13/month
- Azure SQL Database (Basic tier): $5/month
- Azure Storage (for documents): $2/month
- Total: ~$20/month (UGX 75,000)

**Option 2: Azure Production Scale**
- Azure App Service (S1 Standard): $70/month
- Azure SQL Database (S1 Standard): $30/month
- Azure Storage: $5/month
- SendGrid (optional backup): $15/month (40k emails)
- Total: ~$120/month (UGX 450,000)

**Option 3: On-Premise (Uganda Data Center)**
- Server hosting: $50-100/month
- SQL Server license: Included if using Express (free) or Web Edition ($350/year)
- Internet bandwidth: $20-50/month
- Total: ~$100-150/month initial

**Cost Per Customer Calculation:**
- Infrastructure: $120/month ÷ 50 companies = $2.40/company/month
- SMS (optional): UGX 80,000/year ÷ 100 companies ÷ 12 = UGX 67/company/month
- Support & maintenance: 20% overhead
- **Recommended pricing**: UGX 50,000-100,000/company/month (profitable)

**Scaling Thresholds:**
- **0-100 companies**: Basic Azure tier adequate
- **100-500 companies**: Standard Azure tier, consider caching
- **500-2000 companies**: Premium Azure tier, database optimization, CDN
- **2000+ companies**: Multi-region deployment, dedicated infrastructure

**API Quota Monitoring Strategy:**
```csharp
public class ApiQuotaMonitor
{
    // Track daily usage
    public async Task LogApiCallAsync(string provider, string apiType)
    {
        // Increment counter in database or Redis
        // Alert if approaching 80% of quota
        // Throttle if approaching 95% of quota
    }
    
    // Dashboard widget showing quota usage
    public QuotaStatus GetCurrentUsage()
    {
        return new QuotaStatus
        {
            GmailQuotaUsed = 5000,  // out of 1 billion
            GmailQuotaLimit = 1000000000,
            CalendarQuotaUsed = 2000, // out of 1 million
            CalendarQuotaLimit = 1000000,
            GraphApiCallsToday = 500 // out of 10,000 per 10 min
        };
    }
}
```

---

## Development Instructions for Claude Code

When implementing this system:

1. **Always search DevExpress documentation** using the MCP server before implementing any feature
2. **Use XAF scaffolding** extensively - let XAF generate CRUD operations
3. **Follow XAF best practices**: Controllers for UI logic, BusinessObjects for data
4. **Implement services as NonPersistentObjects** for business logic
5. **Use XAF Actions** for custom buttons and menu items
6. **Security first**: Apply SecurityStrategyComplex from the start
7. **Test incrementally**: Each phase should have working functionality
8. **Comment thoroughly**: Explain business logic, especially deadline calculations
9. **Version control**: Commit after each working feature
10. **Sample data**: Create seed data for testing each feature

## Priority Order
1. **Domain model** (BusinessObjects) - Foundation for everything
2. **Deadline calculation logic** (core business value) - Why customers will pay
3. **Basic Blazor UI** (prove concept) - Make it tangible
4. **CRMP import** (real data ingestion) - Populate with actual regulations
5. **OAuth Gmail integration** (email automation) - Critical differentiator
6. **OAuth Outlook integration** (email automation) - Enterprise market need
7. **Reminder generation** (automation value) - Time-saving for users
8. **Google Calendar sync** (workflow integration) - Convenience feature
9. **Outlook Calendar sync** (workflow integration) - Enterprise adoption
10. **Dashboard and reports** (analytics) - Business intelligence
11. **Document templates** (compliance deliverables) - Practical output
12. **SMS integration** (optional channel) - Regional preference
13. **Polish and optimization** - Production-ready

**MVP Definition (First 6 weeks):**
- Companies Act compliance tracking
- Gmail email reminders
- Basic dashboard
- Document upload
- CRMP import

**Version 1.0 (12 weeks):**
- Multi-tenant support
- Gmail + Outlook email
- Google + Outlook calendar sync
- Multiple regulatory acts
- Template management
- Full reporting suite

---

## Questions Before Starting Implementation

**Technology Stack Confirmed:**
✅ Platform: DevExpress XAF 20.2.13  
✅ Framework: .NET Core 3.1  
✅ UI: Blazor Server  
✅ Development Database: LocalDB  
✅ Email: Gmail API + Microsoft Graph (OAuth 2.0)  
✅ Calendar: Google Calendar + Outlook Calendar (OAuth 2.0)  

**Still Need Clarification On:**

1. **Production Database**: 
   - Azure SQL Database (recommended for ease) or On-premise SQL Server (for data residency)?
   - If on-premise: Do you have existing SQL Server infrastructure?

2. **Data Residency**: 
   - Must data physically stay in Uganda (DPPA strict interpretation)?
   - Or is South Africa Azure region acceptable?

3. **OAuth Accounts**:
   - Do you have Google Workspace account for testing?
   - Do you have Microsoft 365 account for testing?
   - Or should we create free Gmail/Outlook.com accounts for development?

4. **Initial Deployment Model**: 
   - Multi-tenant SaaS (law firms managing multiple clients) - higher complexity, higher value
   - Single-tenant (one company per instance) - simpler, faster to market
   - Recommendation: Start multi-tenant, target law firms

5. **Initial Regulatory Scope**: 
   - Companies Act only (universal, ~150 requirements)
   - Companies Act + one sector-specific (e.g., Financial Services)
   - All provided CRMPs from day one
   - Recommendation: Companies Act first, expand later

6. **Calendar Sync Scope**:
   - MVP: One-way sync (ComplyEA → Calendar) - simpler
   - Full: Two-way sync (Calendar ↔ ComplyEA) - requires webhooks, more complex
   - Recommendation: One-way for MVP

7. **Expected User Volume** (for planning):
   - Number of law firms (tenants): 5? 20? 50?
   - Number of companies per firm: 10? 50? 200?
   - Number of users per company: 3? 10?

8. **SMS Integration Priority**:
   - Critical for MVP (need Africa's Talking account)
   - Nice-to-have (can add later)
   - Not needed initially
   - Recommendation: Nice-to-have, focus on email/calendar first

9. **Pilot Customers**:
   - Do you have 2-3 friendly law firms or companies committed to testing?
   - Or is this purely speculative/bootstrap development?

10. **Timeline**:
    - Need MVP in X months for specific customer/investor demo?
    - Or flexible development timeline?

**Immediate Next Steps:**
Once you answer these questions, I recommend:
1. Start with Phase 1-2 (Foundation + Core Engine) - 4 weeks
2. Get early feedback with basic compliance tracking
3. Then add OAuth integration (Phase 3-4) - 4 weeks
4. Polish and pilot (Phase 5-7) - 4 weeks
5. Total: ~12 weeks to pilot-ready system

Would you like me to create:
- **Project plan Gantt chart** with milestones?
- **Budget estimate** (developer time, infrastructure costs, API costs)?
- **Go-to-market strategy** document?
- **Investor pitch deck** for fundraising?
