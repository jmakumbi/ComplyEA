using System;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Updating;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using ComplyEA.Module.BusinessObjects.Compliance;
using ComplyEA.Module.BusinessObjects.Configuration;
using ComplyEA.Module.BusinessObjects.Lookups;
using ComplyEA.Module.BusinessObjects.Organization;
using ComplyEA.Module.BusinessObjects.Regulatory;
using ComplyEA.Module.BusinessObjects.Security;

namespace ComplyEA.Module.DatabaseUpdate {
    public class Updater : ModuleUpdater {
        public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
            base(objectSpace, currentDBVersion) {
        }

        public override void UpdateDatabaseAfterUpdateSchema() {
            base.UpdateDatabaseAfterUpdateSchema();

            // Seed lookup tables
            SeedSubscriptionTypes();
            SeedCompanyTypes();
            SeedSectors();
            SeedComplianceRoles();
            SeedRegulationScopes();
            SeedActTypes();
            SeedTimelineTypes();
            SeedRiskRatings();
            SeedObligationStatuses();
            SeedReminderTypes();
            SeedNotificationChannels();
            SeedDeliveryStatuses();
            SeedDocumentTypes();
            SeedFileFormats();
            SeedEmailProviders();
            SeedCalendarProviders();
            SeedSMSProviders();

            // Seed security data
            SeedRolesAndPermissions();
            SeedAdminUser();

            // Seed sample regulatory data
            SeedCompaniesAct();

            // Seed email templates and system configuration
            SeedEmailTemplates();
            SeedSystemConfiguration();

            // Seed test data for end-to-end testing
            SeedTestUsers();
            SeedTestData();

            ObjectSpace.CommitChanges();
        }

        #region Lookup Data Seeding

        private void SeedSubscriptionTypes() {
            CreateLookup<SubscriptionType>("TRIAL", "Trial", "30-day trial subscription", 1, t => {
                t.MonthlyPrice = 0;
                t.MaxCompanies = 2;
                t.MaxUsers = 2;
            });
            CreateLookup<SubscriptionType>("BASIC", "Basic", "Basic subscription for small firms", 2, t => {
                t.MonthlyPrice = 99;
                t.MaxCompanies = 5;
                t.MaxUsers = 5;
            });
            CreateLookup<SubscriptionType>("PROFESSIONAL", "Professional", "Professional subscription for medium firms", 3, t => {
                t.MonthlyPrice = 299;
                t.MaxCompanies = 20;
                t.MaxUsers = 20;
            });
            CreateLookup<SubscriptionType>("ENTERPRISE", "Enterprise", "Enterprise subscription for large firms", 4, t => {
                t.MonthlyPrice = 599;
                t.MaxCompanies = -1; // Unlimited
                t.MaxUsers = -1; // Unlimited
            });
        }

        private void SeedCompanyTypes() {
            CreateLookup<CompanyType>("PRIVATE", "Private Limited Company", "Private company limited by shares", 1);
            CreateLookup<CompanyType>("PUBLIC", "Public Limited Company", "Public company listed on exchange", 2);
            CreateLookup<CompanyType>("NGO", "Non-Governmental Organization", "Non-profit organization", 3);
            CreateLookup<CompanyType>("PARTNERSHIP", "Partnership", "Business partnership", 4);
            CreateLookup<CompanyType>("SOLE", "Sole Proprietorship", "Individual business ownership", 5);
        }

        private void SeedSectors() {
            CreateLookup<Sector>("ENERGY", "Energy", "Energy and utilities sector", 1, s => s.RequiresSpecificRegulation = true);
            CreateLookup<Sector>("FINANCIAL", "Financial Services", "Banking and financial services", 2, s => s.RequiresSpecificRegulation = true);
            CreateLookup<Sector>("INSURANCE", "Insurance", "Insurance sector", 3, s => s.RequiresSpecificRegulation = true);
            CreateLookup<Sector>("MANUFACTURING", "Manufacturing", "Manufacturing and production", 4);
            CreateLookup<Sector>("TECHNOLOGY", "Technology", "Information technology services", 5);
            CreateLookup<Sector>("HEALTHCARE", "Healthcare", "Healthcare and medical services", 6, s => s.RequiresSpecificRegulation = true);
            CreateLookup<Sector>("RETAIL", "Retail", "Retail and consumer goods", 7);
            CreateLookup<Sector>("AGRICULTURE", "Agriculture", "Agricultural sector", 8);
            CreateLookup<Sector>("CONSTRUCTION", "Construction", "Construction and real estate", 9);
            CreateLookup<Sector>("OTHER", "Other", "Other sectors", 99);
        }

        private void SeedComplianceRoles() {
            CreateLookup<ComplianceRole>("CO", "Compliance Officer", "Primary compliance officer", 1, r => {
                r.CanReceiveReminders = true;
                r.CanApproveSubmissions = true;
            });
            CreateLookup<ComplianceRole>("MD", "Managing Director", "Managing Director / CEO", 2, r => {
                r.CanReceiveReminders = true;
                r.CanApproveSubmissions = true;
            });
            CreateLookup<ComplianceRole>("DMD", "Deputy Managing Director", "Deputy Managing Director", 3, r => {
                r.CanReceiveReminders = true;
                r.CanApproveSubmissions = true;
            });
            CreateLookup<ComplianceRole>("CFO", "Chief Financial Officer", "Chief Financial Officer", 4, r => {
                r.CanReceiveReminders = true;
                r.CanApproveSubmissions = true;
            });
            CreateLookup<ComplianceRole>("CS", "Company Secretary", "Company Secretary", 5, r => {
                r.CanReceiveReminders = true;
                r.CanApproveSubmissions = true;
            });
            CreateLookup<ComplianceRole>("HOD", "Head of Department", "Department head", 6, r => {
                r.CanReceiveReminders = true;
                r.CanApproveSubmissions = false;
            });
            CreateLookup<ComplianceRole>("OTHER", "Other", "Other compliance role", 99, r => {
                r.CanReceiveReminders = true;
                r.CanApproveSubmissions = false;
            });
        }

        private void SeedRegulationScopes() {
            CreateLookup<RegulationScope>("GENERAL", "General", "Applies to all companies", 1);
            CreateLookup<RegulationScope>("SECTOR", "Sector-Specific", "Applies to specific sectors only", 2);
        }

        private void SeedActTypes() {
            CreateLookup<ActType>("STATUTE", "Statute", "Primary legislation (Act of Parliament)", 1);
            CreateLookup<ActType>("REGULATION", "Regulation", "Subsidiary legislation / Regulations", 2);
            CreateLookup<ActType>("CODE", "Code of Practice", "Industry code of practice", 3);
            CreateLookup<ActType>("STANDARD", "Standard", "Industry standard or guideline", 4);
        }

        private void SeedTimelineTypes() {
            CreateLookup<TimelineType>("ALWAYS", "Always", "Continuous obligation with no specific deadline", 1);
            CreateLookup<TimelineType>("ANNUAL", "Annual", "Due once per year", 2, t => t.FrequencyDays = 365);
            CreateLookup<TimelineType>("QUARTERLY", "Quarterly", "Due once per quarter", 3, t => t.FrequencyDays = 90);
            CreateLookup<TimelineType>("MONTHLY", "Monthly", "Due once per month", 4, t => t.FrequencyDays = 30);
            CreateLookup<TimelineType>("EVENT", "Event-Driven", "Due after a triggering event", 5);
            CreateLookup<TimelineType>("FIXED", "Fixed Deadline", "One-time deadline", 6);
        }

        private void SeedRiskRatings() {
            CreateLookup<RiskRating>("LOW", "Low", "Low risk - minor consequences", 1, r => r.SeverityLevel = 1);
            CreateLookup<RiskRating>("MEDIUM", "Medium", "Medium risk - moderate consequences", 2, r => r.SeverityLevel = 2);
            CreateLookup<RiskRating>("HIGH", "High", "High risk - severe consequences", 3, r => r.SeverityLevel = 3);
        }

        private void SeedObligationStatuses() {
            CreateLookup<ObligationStatus>("PENDING", "Pending", "Not yet started", 1, s => {
                s.IsTerminal = false;
                s.RequiresAction = true;
            });
            CreateLookup<ObligationStatus>("INPROGRESS", "In Progress", "Work in progress", 2, s => {
                s.IsTerminal = false;
                s.RequiresAction = true;
            });
            CreateLookup<ObligationStatus>("SUBMITTED", "Submitted", "Submitted for review/approval", 3, s => {
                s.IsTerminal = false;
                s.RequiresAction = false;
            });
            CreateLookup<ObligationStatus>("OVERDUE", "Overdue", "Past due date", 4, s => {
                s.IsTerminal = false;
                s.RequiresAction = true;
            });
            CreateLookup<ObligationStatus>("COMPLETED", "Completed", "Successfully completed", 5, s => {
                s.IsTerminal = true;
                s.RequiresAction = false;
            });
            CreateLookup<ObligationStatus>("WAIVED", "Waived", "Requirement waived", 6, s => {
                s.IsTerminal = true;
                s.RequiresAction = false;
            });
        }

        private void SeedReminderTypes() {
            CreateLookup<ReminderType>("INITIAL", "Initial Reminder", "First notification of upcoming deadline", 1, r => {
                r.DefaultDaysBeforeDue = 30;
                r.IsEscalation = false;
            });
            CreateLookup<ReminderType>("FIRST", "First Follow-up", "First follow-up reminder", 2, r => {
                r.DefaultDaysBeforeDue = 14;
                r.IsEscalation = false;
            });
            CreateLookup<ReminderType>("SECOND", "Second Follow-up", "Second follow-up reminder", 3, r => {
                r.DefaultDaysBeforeDue = 7;
                r.IsEscalation = false;
            });
            CreateLookup<ReminderType>("FINAL", "Final Notice", "Final notice before deadline", 4, r => {
                r.DefaultDaysBeforeDue = 3;
                r.IsEscalation = false;
            });
            CreateLookup<ReminderType>("ESCALATION", "Escalation", "Escalation to management", 5, r => {
                r.DefaultDaysBeforeDue = 1;
                r.IsEscalation = true;
            });
        }

        private void SeedNotificationChannels() {
            CreateLookup<NotificationChannel>("EMAIL", "Email", "Email notification", 1);
            CreateLookup<NotificationChannel>("SMS", "SMS", "SMS text message", 2);
            CreateLookup<NotificationChannel>("BOTH", "Email & SMS", "Both email and SMS", 3);
        }

        private void SeedDeliveryStatuses() {
            CreateLookup<DeliveryStatus>("PENDING", "Pending", "Awaiting delivery", 1, s => s.IsSuccessful = false);
            CreateLookup<DeliveryStatus>("SENT", "Sent", "Successfully sent", 2, s => s.IsSuccessful = true);
            CreateLookup<DeliveryStatus>("FAILED", "Failed", "Delivery failed", 3, s => s.IsSuccessful = false);
            CreateLookup<DeliveryStatus>("ACKNOWLEDGED", "Acknowledged", "Recipient acknowledged", 4, s => s.IsSuccessful = true);
        }

        private void SeedDocumentTypes() {
            CreateLookup<DocumentType>("SUBMISSION", "Submission", "Document submitted to regulator", 1, d => d.IsEvidence = true);
            CreateLookup<DocumentType>("SUPPORTING", "Supporting Document", "Supporting documentation", 2, d => d.IsEvidence = true);
            CreateLookup<DocumentType>("AUDIT", "Audit Document", "Audit trail document", 3, d => d.IsEvidence = true);
            CreateLookup<DocumentType>("CORRESPONDENCE", "Correspondence", "Communication records", 4, d => d.IsEvidence = false);
            CreateLookup<DocumentType>("OTHER", "Other", "Other document type", 99, d => d.IsEvidence = false);
        }

        private void SeedFileFormats() {
            CreateLookup<FileFormat>("PDF", "PDF", "Portable Document Format", 1, f => {
                f.Extension = "pdf";
                f.MimeType = "application/pdf";
            });
            CreateLookup<FileFormat>("DOCX", "Word Document", "Microsoft Word Document", 2, f => {
                f.Extension = "docx";
                f.MimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            });
            CreateLookup<FileFormat>("XLSX", "Excel Spreadsheet", "Microsoft Excel Spreadsheet", 3, f => {
                f.Extension = "xlsx";
                f.MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            });
            CreateLookup<FileFormat>("DOC", "Word Document (Legacy)", "Microsoft Word Document (Legacy)", 4, f => {
                f.Extension = "doc";
                f.MimeType = "application/msword";
            });
            CreateLookup<FileFormat>("XLS", "Excel Spreadsheet (Legacy)", "Microsoft Excel Spreadsheet (Legacy)", 5, f => {
                f.Extension = "xls";
                f.MimeType = "application/vnd.ms-excel";
            });
        }

        private void SeedEmailProviders() {
            CreateLookup<EmailProvider>("GMAIL", "Gmail", "Google Gmail", 1, p => {
                p.OAuthEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
                p.ApiEndpoint = "https://gmail.googleapis.com/gmail/v1";
            });
            CreateLookup<EmailProvider>("OUTLOOK", "Outlook", "Microsoft Outlook", 2, p => {
                p.OAuthEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
                p.ApiEndpoint = "https://graph.microsoft.com/v1.0";
            });
        }

        private void SeedCalendarProviders() {
            CreateLookup<CalendarProvider>("GOOGLE", "Google Calendar", "Google Calendar", 1, p => {
                p.OAuthEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
                p.ApiEndpoint = "https://www.googleapis.com/calendar/v3";
            });
            CreateLookup<CalendarProvider>("OUTLOOK", "Outlook Calendar", "Microsoft Outlook Calendar", 2, p => {
                p.OAuthEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
                p.ApiEndpoint = "https://graph.microsoft.com/v1.0";
            });
        }

        private void SeedSMSProviders() {
            CreateLookup<SMSProvider>("AFRICASTALKING", "Africa's Talking", "Africa's Talking SMS Gateway", 1, p => {
                p.ApiEndpoint = "https://api.africastalking.com/version1/messaging";
                p.SupportsUnicode = true;
            });
            CreateLookup<SMSProvider>("TWILIO", "Twilio", "Twilio SMS Gateway", 2, p => {
                p.ApiEndpoint = "https://api.twilio.com/2010-04-01";
                p.SupportsUnicode = true;
            });
        }

        private void CreateLookup<T>(string code, string name, string description, int sortOrder, Action<T> configure = null)
            where T : BaseLookup {
            var existing = ObjectSpace.FindObject<T>(CriteriaOperator.Parse("Code = ?", code));
            if (existing == null) {
                var lookup = ObjectSpace.CreateObject<T>();
                lookup.Code = code;
                lookup.Name = name;
                lookup.Description = description;
                lookup.SortOrder = sortOrder;
                lookup.IsActive = true;
                configure?.Invoke(lookup);
            }
        }

        #endregion

        #region Security Seeding

        private void SeedRolesAndPermissions() {
            // System Administrator Role
            var adminRole = ObjectSpace.FindObject<ApplicationRole>(CriteriaOperator.Parse("Name = ?", "System Administrator"));
            if (adminRole == null) {
                adminRole = ObjectSpace.CreateObject<ApplicationRole>();
                adminRole.Name = "System Administrator";
                adminRole.Description = "Full system access including multi-tenant administration";
                adminRole.IsSystemRole = true;
                adminRole.IsAdministrative = true;
                adminRole.CanManageOwnFirm = true;
                adminRole.CanManageClients = true;
                adminRole.CanManageCompliance = true;
                adminRole.CanViewReports = true;
                adminRole.CanUploadDocuments = true;
                adminRole.CanConfigureIntegrations = true;
            }

            // Legal Firm Administrator Role
            var firmAdminRole = ObjectSpace.FindObject<ApplicationRole>(CriteriaOperator.Parse("Name = ?", "Legal Firm Administrator"));
            if (firmAdminRole == null) {
                firmAdminRole = ObjectSpace.CreateObject<ApplicationRole>();
                firmAdminRole.Name = "Legal Firm Administrator";
                firmAdminRole.Description = "Manage own firm and client companies";
                firmAdminRole.IsSystemRole = true;
                firmAdminRole.CanManageOwnFirm = true;
                firmAdminRole.CanManageClients = true;
                firmAdminRole.CanManageCompliance = true;
                firmAdminRole.CanViewReports = true;
                firmAdminRole.CanUploadDocuments = true;
                firmAdminRole.CanConfigureIntegrations = true;
            }

            // Compliance Officer Role
            var complianceRole = ObjectSpace.FindObject<ApplicationRole>(CriteriaOperator.Parse("Name = ?", "Compliance Officer"));
            if (complianceRole == null) {
                complianceRole = ObjectSpace.CreateObject<ApplicationRole>();
                complianceRole.Name = "Compliance Officer";
                complianceRole.Description = "Manage compliance for assigned companies";
                complianceRole.IsSystemRole = true;
                complianceRole.CanManageOwnFirm = false;
                complianceRole.CanManageClients = false;
                complianceRole.CanManageCompliance = true;
                complianceRole.CanViewReports = true;
                complianceRole.CanUploadDocuments = true;
                complianceRole.CanConfigureIntegrations = false;
            }

            // Company User Role (Read-only)
            var userRole = ObjectSpace.FindObject<ApplicationRole>(CriteriaOperator.Parse("Name = ?", "Company User"));
            if (userRole == null) {
                userRole = ObjectSpace.CreateObject<ApplicationRole>();
                userRole.Name = "Company User";
                userRole.Description = "View compliance status and upload documents";
                userRole.IsSystemRole = true;
                userRole.CanManageOwnFirm = false;
                userRole.CanManageClients = false;
                userRole.CanManageCompliance = false;
                userRole.CanViewReports = true;
                userRole.CanUploadDocuments = true;
                userRole.CanConfigureIntegrations = false;
            }
        }

        private void SeedAdminUser() {
            var adminUser = ObjectSpace.FindObject<ApplicationUser>(CriteriaOperator.Parse("UserName = ?", "Admin"));
            if (adminUser == null) {
                adminUser = ObjectSpace.CreateObject<ApplicationUser>();
                adminUser.UserName = "Admin";
                adminUser.FirstName = "System";
                adminUser.LastName = "Administrator";
                adminUser.Email = "admin@complyea.com";
                adminUser.IsSystemAdmin = true;
                adminUser.SetPassword("ComplyEA123!");

                var adminRole = ObjectSpace.FindObject<ApplicationRole>(CriteriaOperator.Parse("Name = ?", "System Administrator"));
                if (adminRole != null) {
                    adminUser.Roles.Add(adminRole);
                }
            }
        }

        #endregion

        #region Regulatory Data Seeding

        private void SeedCompaniesAct() {
            var actType = ObjectSpace.FindObject<ActType>(CriteriaOperator.Parse("Code = ?", "STATUTE"));
            var regulationScope = ObjectSpace.FindObject<RegulationScope>(CriteriaOperator.Parse("Code = ?", "GENERAL"));

            var companiesAct = ObjectSpace.FindObject<RegulatoryAct>(CriteriaOperator.Parse("ShortName = ?", "Companies Act"));
            if (companiesAct == null) {
                companiesAct = ObjectSpace.CreateObject<RegulatoryAct>();
                companiesAct.Name = "The Companies Act, 2015";
                companiesAct.ShortName = "Companies Act";
                companiesAct.ActType = actType;
                companiesAct.RegulationScope = regulationScope;
                companiesAct.Year = 2015;
                companiesAct.Jurisdiction = "Kenya";
                companiesAct.RegulatoryBody = "Registrar of Companies";
                companiesAct.EffectiveDate = new DateTime(2015, 9, 11);
                companiesAct.Description = "An Act of Parliament to reform the law relating to the incorporation, registration, operation and management of companies; to provide for the appointment of a Registrar of Companies and for related matters.";
                companiesAct.IsActive = true;

                // Add sample acronyms
                var agmAcronym = ObjectSpace.CreateObject<ActAcronym>();
                agmAcronym.RegulatoryAct = companiesAct;
                agmAcronym.Acronym = "AGM";
                agmAcronym.FullForm = "Annual General Meeting";
                agmAcronym.Definition = "A mandatory yearly gathering of a company's shareholders.";
                agmAcronym.SectionReference = "Section 278";

                var eodAcronym = ObjectSpace.CreateObject<ActAcronym>();
                eodAcronym.RegulatoryAct = companiesAct;
                eodAcronym.Acronym = "EOD";
                eodAcronym.FullForm = "Exempt Private Company";
                eodAcronym.Definition = "A private company where no body corporate holds any beneficial interest in its shares.";
                eodAcronym.SectionReference = "Section 2";
            }
        }

        #endregion

        #region Email Templates Seeding

        private void SeedEmailTemplates() {
            var availablePlaceholders = @"Available placeholders:
{{CompanyName}} - Company name
{{CompanyShortName}} - Company short name
{{RequirementTitle}} - Compliance requirement title
{{ObligationTitle}} - Obligation title
{{DueDate}} - Due date (full format)
{{DueDateShort}} - Due date (short format)
{{DaysUntilDue}} - Days until due date
{{RecipientName}} - Recipient full name
{{RecipientFirstName}} - Recipient first name
{{ObligationStatus}} - Current obligation status
{{RegulatoryAct}} - Regulatory act short name
{{RegulatoryActFull}} - Regulatory act full name
{{PeriodYear}} - Period year
{{PeriodQuarter}} - Period quarter (Q1, Q2, etc.)
{{PeriodMonth}} - Period month name
{{SectionReference}} - Section reference in act
{{RiskRating}} - Risk rating
{{PenaltyAmount}} - Penalty amount
{{ReminderType}} - Reminder type name
{{ScheduledDate}} - Scheduled reminder date
{{CurrentDate}} - Current date
{{ActionUrl}} - Application link";

            // Initial Reminder Template
            CreateEmailTemplate("INITIAL", "Initial Reminder", "INITIAL",
                "[Initial Reminder] {{RequirementTitle}} due {{DueDate}}",
                GetInitialReminderBody(),
                "REMINDER: {{RequirementTitle}} due in {{DaysUntilDue}} days. Please take action.",
                availablePlaceholders);

            // First Follow-up Template
            CreateEmailTemplate("FIRST", "First Follow-up Reminder", "FIRST",
                "[Reminder] {{RequirementTitle}} due in {{DaysUntilDue}} days",
                GetFirstReminderBody(),
                "FOLLOW-UP: {{RequirementTitle}} due in {{DaysUntilDue}} days. Action required.",
                availablePlaceholders);

            // Second Follow-up Template
            CreateEmailTemplate("SECOND", "Second Follow-up Reminder", "SECOND",
                "[Urgent] {{RequirementTitle}} due in {{DaysUntilDue}} days",
                GetSecondReminderBody(),
                "URGENT: {{RequirementTitle}} due in {{DaysUntilDue}} days. Immediate action required.",
                availablePlaceholders);

            // Final Notice Template
            CreateEmailTemplate("FINAL", "Final Notice", "FINAL",
                "[FINAL NOTICE] {{RequirementTitle}} due in {{DaysUntilDue}} days",
                GetFinalNoticeBody(),
                "FINAL NOTICE: {{RequirementTitle}} due in {{DaysUntilDue}} days!",
                availablePlaceholders);

            // Escalation Template
            CreateEmailTemplate("ESCALATION", "Escalation Notice", "ESCALATION",
                "[ESCALATION] {{RequirementTitle}} requires immediate attention",
                GetEscalationBody(),
                "ESCALATION: {{RequirementTitle}} requires immediate management attention.",
                availablePlaceholders);

            // Default Template
            CreateEmailTemplate("DEFAULT", "Default Reminder", null,
                "[ComplyEA] Compliance Reminder: {{RequirementTitle}}",
                GetDefaultReminderBody(),
                "ComplyEA Reminder: {{RequirementTitle}} due {{DueDate}}",
                availablePlaceholders);
        }

        private void CreateEmailTemplate(string code, string name, string reminderTypeCode, string subject, string bodyHtml, string smsTemplate, string availablePlaceholders) {
            var existing = ObjectSpace.FindObject<EmailTemplate>(CriteriaOperator.Parse("Code = ?", code));
            if (existing == null) {
                var template = ObjectSpace.CreateObject<EmailTemplate>();
                template.Code = code;
                template.Name = name;
                template.Subject = subject;
                template.BodyHtml = bodyHtml;
                template.SmsTemplate = smsTemplate;
                template.AvailablePlaceholders = availablePlaceholders;
                template.IsActive = true;
                template.IsDefault = true;

                if (!string.IsNullOrEmpty(reminderTypeCode)) {
                    var reminderType = ObjectSpace.FindObject<ReminderType>(CriteriaOperator.Parse("Code = ?", reminderTypeCode));
                    template.ReminderType = reminderType;
                }
            }
        }

        private string GetInitialReminderBody() {
            return @"<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #2c5282; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background: #f7fafc; }
        .details { background: white; padding: 15px; margin: 15px 0; border-left: 4px solid #2c5282; }
        .footer { padding: 20px; text-align: center; font-size: 12px; color: #718096; }
        .btn { display: inline-block; background: #2c5282; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Compliance Reminder</h2>
        </div>
        <div class='content'>
            <p>Dear {{RecipientFirstName}},</p>

            <p>This is a friendly reminder that a compliance obligation is coming due in <strong>{{DaysUntilDue}} days</strong>.</p>

            <div class='details'>
                <p><strong>Company:</strong> {{CompanyName}}</p>
                <p><strong>Requirement:</strong> {{RequirementTitle}}</p>
                <p><strong>Regulatory Act:</strong> {{RegulatoryAct}}</p>
                <p><strong>Due Date:</strong> {{DueDate}}</p>
                <p><strong>Current Status:</strong> {{ObligationStatus}}</p>
            </div>

            <p>Please ensure this obligation is addressed before the due date to maintain compliance.</p>

            <p style='text-align: center;'>
                <a href='{{ActionUrl}}' class='btn'>View Obligation</a>
            </p>
        </div>
        <div class='footer'>
            <p>This is an automated message from ComplyEA Compliance Management System.</p>
            <p>© {{CurrentDate}} ComplyEA</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetFirstReminderBody() {
            return @"<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #dd6b20; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background: #fffaf0; }
        .details { background: white; padding: 15px; margin: 15px 0; border-left: 4px solid #dd6b20; }
        .footer { padding: 20px; text-align: center; font-size: 12px; color: #718096; }
        .btn { display: inline-block; background: #dd6b20; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Follow-up Reminder</h2>
        </div>
        <div class='content'>
            <p>Dear {{RecipientFirstName}},</p>

            <p>This is a follow-up reminder. The following compliance obligation is due in <strong>{{DaysUntilDue}} days</strong>.</p>

            <div class='details'>
                <p><strong>Company:</strong> {{CompanyName}}</p>
                <p><strong>Requirement:</strong> {{RequirementTitle}}</p>
                <p><strong>Regulatory Act:</strong> {{RegulatoryAct}}</p>
                <p><strong>Due Date:</strong> {{DueDate}}</p>
                <p><strong>Current Status:</strong> {{ObligationStatus}}</p>
            </div>

            <p>Please take action to ensure timely compliance.</p>

            <p style='text-align: center;'>
                <a href='{{ActionUrl}}' class='btn'>Take Action</a>
            </p>
        </div>
        <div class='footer'>
            <p>This is an automated message from ComplyEA Compliance Management System.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetSecondReminderBody() {
            return @"<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #c53030; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background: #fff5f5; }
        .details { background: white; padding: 15px; margin: 15px 0; border-left: 4px solid #c53030; }
        .footer { padding: 20px; text-align: center; font-size: 12px; color: #718096; }
        .btn { display: inline-block; background: #c53030; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; }
        .warning { background: #fed7d7; border: 1px solid #c53030; padding: 10px; margin: 10px 0; border-radius: 5px; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>⚠️ Urgent Reminder</h2>
        </div>
        <div class='content'>
            <p>Dear {{RecipientFirstName}},</p>

            <div class='warning'>
                <strong>URGENT:</strong> Only <strong>{{DaysUntilDue}} days</strong> remain until this obligation is due.
            </div>

            <div class='details'>
                <p><strong>Company:</strong> {{CompanyName}}</p>
                <p><strong>Requirement:</strong> {{RequirementTitle}}</p>
                <p><strong>Regulatory Act:</strong> {{RegulatoryAct}}</p>
                <p><strong>Due Date:</strong> {{DueDate}}</p>
                <p><strong>Current Status:</strong> {{ObligationStatus}}</p>
            </div>

            <p><strong>Immediate action is required</strong> to avoid non-compliance penalties.</p>

            <p style='text-align: center;'>
                <a href='{{ActionUrl}}' class='btn'>Take Immediate Action</a>
            </p>
        </div>
        <div class='footer'>
            <p>This is an automated message from ComplyEA Compliance Management System.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetFinalNoticeBody() {
            return @"<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #742a2a; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background: #fff5f5; }
        .details { background: white; padding: 15px; margin: 15px 0; border-left: 4px solid #742a2a; }
        .footer { padding: 20px; text-align: center; font-size: 12px; color: #718096; }
        .btn { display: inline-block; background: #742a2a; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; }
        .critical { background: #742a2a; color: white; padding: 15px; margin: 10px 0; border-radius: 5px; text-align: center; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>🚨 FINAL NOTICE 🚨</h2>
        </div>
        <div class='content'>
            <p>Dear {{RecipientFirstName}},</p>

            <div class='critical'>
                <strong>CRITICAL:</strong> This obligation is due in <strong>{{DaysUntilDue}} days</strong>!
            </div>

            <div class='details'>
                <p><strong>Company:</strong> {{CompanyName}}</p>
                <p><strong>Requirement:</strong> {{RequirementTitle}}</p>
                <p><strong>Regulatory Act:</strong> {{RegulatoryAct}}</p>
                <p><strong>Due Date:</strong> {{DueDate}}</p>
                <p><strong>Current Status:</strong> {{ObligationStatus}}</p>
                <p><strong>Risk Rating:</strong> {{RiskRating}}</p>
            </div>

            <p>This is the <strong>final notice</strong> before the deadline. Failure to comply may result in penalties.</p>

            <p style='text-align: center;'>
                <a href='{{ActionUrl}}' class='btn'>Complete Now</a>
            </p>
        </div>
        <div class='footer'>
            <p>This is an automated message from ComplyEA Compliance Management System.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetEscalationBody() {
            return @"<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #1a202c; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background: #edf2f7; }
        .details { background: white; padding: 15px; margin: 15px 0; border-left: 4px solid #1a202c; }
        .footer { padding: 20px; text-align: center; font-size: 12px; color: #718096; }
        .btn { display: inline-block; background: #1a202c; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; }
        .escalation { background: #1a202c; color: white; padding: 15px; margin: 10px 0; border-radius: 5px; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>📢 MANAGEMENT ESCALATION</h2>
        </div>
        <div class='content'>
            <p>Dear {{RecipientFirstName}},</p>

            <div class='escalation'>
                <p><strong>This matter has been escalated to management attention.</strong></p>
                <p>A compliance obligation for {{CompanyName}} requires immediate intervention.</p>
            </div>

            <div class='details'>
                <p><strong>Company:</strong> {{CompanyName}}</p>
                <p><strong>Requirement:</strong> {{RequirementTitle}}</p>
                <p><strong>Regulatory Act:</strong> {{RegulatoryAct}}</p>
                <p><strong>Due Date:</strong> {{DueDate}}</p>
                <p><strong>Days Until Due:</strong> {{DaysUntilDue}}</p>
                <p><strong>Current Status:</strong> {{ObligationStatus}}</p>
                <p><strong>Risk Rating:</strong> {{RiskRating}}</p>
            </div>

            <p>Please review and ensure appropriate action is taken immediately.</p>

            <p style='text-align: center;'>
                <a href='{{ActionUrl}}' class='btn'>Review Now</a>
            </p>
        </div>
        <div class='footer'>
            <p>This is an automated escalation from ComplyEA Compliance Management System.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetDefaultReminderBody() {
            return @"<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #4a5568; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background: #f7fafc; }
        .details { background: white; padding: 15px; margin: 15px 0; border-left: 4px solid #4a5568; }
        .footer { padding: 20px; text-align: center; font-size: 12px; color: #718096; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Compliance Reminder</h2>
        </div>
        <div class='content'>
            <p>Dear {{RecipientFirstName}},</p>

            <p>This is a reminder about the following compliance obligation:</p>

            <div class='details'>
                <p><strong>Company:</strong> {{CompanyName}}</p>
                <p><strong>Requirement:</strong> {{RequirementTitle}}</p>
                <p><strong>Due Date:</strong> {{DueDate}}</p>
                <p><strong>Status:</strong> {{ObligationStatus}}</p>
            </div>

            <p>Please take appropriate action.</p>
        </div>
        <div class='footer'>
            <p>ComplyEA Compliance Management System</p>
        </div>
    </div>
</body>
</html>";
        }

        #endregion

        #region System Configuration Seeding

        private void SeedSystemConfiguration() {
            // Email SMTP Configuration
            CreateSystemConfig("Email.Smtp.Host", "", "SMTP server hostname (e.g., smtp.gmail.com)", "Email", "String", false, true);
            CreateSystemConfig("Email.Smtp.Port", "587", "SMTP server port (typically 587 for TLS, 465 for SSL)", "Email", "Int", false, true);
            CreateSystemConfig("Email.Smtp.UseSsl", "true", "Use SSL/TLS for SMTP connection", "Email", "Bool", false, true);
            CreateSystemConfig("Email.Smtp.Username", "", "SMTP authentication username", "Email", "String", false, true);
            CreateSystemConfig("Email.Smtp.Password", "", "SMTP authentication password", "Email", "String", true, true);
            CreateSystemConfig("Email.From.Address", "", "Default sender email address", "Email", "String", false, true);
            CreateSystemConfig("Email.From.Name", "ComplyEA", "Default sender display name", "Email", "String", false, false);

            // Reminder Processing Configuration
            CreateSystemConfig("Reminders.Processing.Enabled", "true", "Enable automatic reminder processing", "Reminders", "Bool", false, false);
            CreateSystemConfig("Reminders.Processing.IntervalMinutes", "15", "Interval in minutes between reminder processing runs", "Reminders", "Int", false, false);
            CreateSystemConfig("Reminders.Processing.MaxRetries", "3", "Maximum number of retry attempts for failed reminders", "Reminders", "Int", false, false);
            CreateSystemConfig("Reminders.Processing.RetryDelayMinutes", "60", "Delay in minutes between retry attempts", "Reminders", "Int", false, false);

            // Application Configuration
            CreateSystemConfig("App.BaseUrl", "https://localhost:5001", "Base URL for application links in emails", "Application", "String", false, false);
            CreateSystemConfig("App.Name", "ComplyEA", "Application display name", "Application", "String", false, false);
            CreateSystemConfig("App.SupportEmail", "support@complyea.com", "Support email address", "Application", "String", false, false);
        }

        private void CreateSystemConfig(string key, string value, string description, string category, string dataType, bool isEncrypted, bool isSystemOnly) {
            var existing = ObjectSpace.FindObject<SystemConfiguration>(CriteriaOperator.Parse("Key = ?", key));
            if (existing == null) {
                var config = ObjectSpace.CreateObject<SystemConfiguration>();
                config.Key = key;
                config.Value = value;
                config.Description = description;
                config.Category = category;
                config.DataType = dataType;
                config.IsEncrypted = isEncrypted;
                config.IsSystemOnly = isSystemOnly;
            }
        }

        #endregion

        #region Test Users Seeding

        private void SeedTestUsers() {
            // Firm Admin User
            var firmAdminUser = ObjectSpace.FindObject<ApplicationUser>(CriteriaOperator.Parse("UserName = ?", "FirmAdmin"));
            if (firmAdminUser == null) {
                firmAdminUser = ObjectSpace.CreateObject<ApplicationUser>();
                firmAdminUser.UserName = "FirmAdmin";
                firmAdminUser.FirstName = "Jane";
                firmAdminUser.LastName = "Smith";
                firmAdminUser.Email = "firmadmin@testfirm.com";
                firmAdminUser.IsSystemAdmin = false;
                firmAdminUser.SetPassword("Test123!");

                var firmAdminRole = ObjectSpace.FindObject<ApplicationRole>(CriteriaOperator.Parse("Name = ?", "Legal Firm Administrator"));
                if (firmAdminRole != null) {
                    firmAdminUser.Roles.Add(firmAdminRole);
                }
            }

            // Compliance Officer User
            var complianceUser = ObjectSpace.FindObject<ApplicationUser>(CriteriaOperator.Parse("UserName = ?", "ComplianceOfficer"));
            if (complianceUser == null) {
                complianceUser = ObjectSpace.CreateObject<ApplicationUser>();
                complianceUser.UserName = "ComplianceOfficer";
                complianceUser.FirstName = "John";
                complianceUser.LastName = "Doe";
                complianceUser.Email = "compliance@testfirm.com";
                complianceUser.IsSystemAdmin = false;
                complianceUser.SetPassword("Test123!");

                var complianceRole = ObjectSpace.FindObject<ApplicationRole>(CriteriaOperator.Parse("Name = ?", "Compliance Officer"));
                if (complianceRole != null) {
                    complianceUser.Roles.Add(complianceRole);
                }
            }

            // Company User
            var companyUser = ObjectSpace.FindObject<ApplicationUser>(CriteriaOperator.Parse("UserName = ?", "CompanyUser"));
            if (companyUser == null) {
                companyUser = ObjectSpace.CreateObject<ApplicationUser>();
                companyUser.UserName = "CompanyUser";
                companyUser.FirstName = "Alice";
                companyUser.LastName = "Johnson";
                companyUser.Email = "alice@acmecorp.com";
                companyUser.IsSystemAdmin = false;
                companyUser.SetPassword("Test123!");

                var userRole = ObjectSpace.FindObject<ApplicationRole>(CriteriaOperator.Parse("Name = ?", "Company User"));
                if (userRole != null) {
                    companyUser.Roles.Add(userRole);
                }
            }
        }

        #endregion

        #region Test Data Seeding

        private void SeedTestData() {
            // Create test legal firm
            var testFirm = ObjectSpace.FindObject<LegalFirm>(CriteriaOperator.Parse("Name = ?", "Test Law Associates"));
            if (testFirm == null) {
                var subscription = ObjectSpace.FindObject<SubscriptionType>(CriteriaOperator.Parse("Code = ?", "PROFESSIONAL"));

                testFirm = ObjectSpace.CreateObject<LegalFirm>();
                testFirm.Name = "Test Law Associates";
                testFirm.RegistrationNumber = "LF-2024-001";
                testFirm.Email = "info@testlawassociates.com";
                testFirm.Phone = "+254 700 123 456";
                testFirm.Address = "123 Legal Street, Westlands";
                testFirm.City = "Nairobi";
                testFirm.Country = "Kenya";
                testFirm.SubscriptionType = subscription;
                testFirm.SubscriptionStartDate = DateTime.Today.AddMonths(-6);
                testFirm.SubscriptionEndDate = DateTime.Today.AddMonths(6);
                testFirm.IsActive = true;
            }

            // Create test companies
            var companyType = ObjectSpace.FindObject<CompanyType>(CriteriaOperator.Parse("Code = ?", "PRIVATE"));
            var sector = ObjectSpace.FindObject<Sector>(CriteriaOperator.Parse("Code = ?", "TECHNOLOGY"));
            var financialSector = ObjectSpace.FindObject<Sector>(CriteriaOperator.Parse("Code = ?", "FINANCIAL"));

            // Company 1: ACME Corporation
            var acmeCorp = ObjectSpace.FindObject<Company>(CriteriaOperator.Parse("Name = ?", "ACME Corporation"));
            if (acmeCorp == null) {
                acmeCorp = ObjectSpace.CreateObject<Company>();
                acmeCorp.LegalFirm = testFirm;
                acmeCorp.Name = "ACME Corporation";
                acmeCorp.ShortName = "ACME";
                acmeCorp.RegistrationNumber = "CPR/2020/123456";
                acmeCorp.TaxPin = "P051234567A";
                acmeCorp.CompanyType = companyType;
                acmeCorp.Sector = sector;
                acmeCorp.IncorporationDate = new DateTime(2020, 1, 15);
                acmeCorp.FinancialYearEnd = new DateTime(DateTime.Now.Year, 12, 31);
                acmeCorp.Email = "info@acmecorp.com";
                acmeCorp.Phone = "+254 700 111 222";
                acmeCorp.Address = "456 Tech Park, Kilimani";
                acmeCorp.City = "Nairobi";
                acmeCorp.Country = "Kenya";
                acmeCorp.IsActive = true;

                // Create company contact
                var contact = ObjectSpace.CreateObject<CompanyContact>();
                contact.Company = acmeCorp;
                contact.FirstName = "Alice";
                contact.LastName = "Johnson";
                contact.Email = "alice@acmecorp.com";
                contact.Phone = "+254 700 111 223";
                contact.ComplianceRole = ObjectSpace.FindObject<ComplianceRole>(CriteriaOperator.Parse("Code = ?", "CO"));
                contact.IsPrimaryContact = true;
            }

            // Company 2: Beta Finance Ltd
            var betaFinance = ObjectSpace.FindObject<Company>(CriteriaOperator.Parse("Name = ?", "Beta Finance Ltd"));
            if (betaFinance == null) {
                betaFinance = ObjectSpace.CreateObject<Company>();
                betaFinance.LegalFirm = testFirm;
                betaFinance.Name = "Beta Finance Ltd";
                betaFinance.ShortName = "BFL";
                betaFinance.RegistrationNumber = "CPR/2019/789012";
                betaFinance.TaxPin = "P057654321B";
                betaFinance.CompanyType = companyType;
                betaFinance.Sector = financialSector;
                betaFinance.IncorporationDate = new DateTime(2019, 6, 1);
                betaFinance.FinancialYearEnd = new DateTime(DateTime.Now.Year, 12, 31);
                betaFinance.Email = "info@betafinance.co.ke";
                betaFinance.Phone = "+254 700 333 444";
                betaFinance.Address = "789 Finance Plaza, Upper Hill";
                betaFinance.City = "Nairobi";
                betaFinance.Country = "Kenya";
                betaFinance.IsActive = true;

                // Create company contact
                var contact = ObjectSpace.CreateObject<CompanyContact>();
                contact.Company = betaFinance;
                contact.FirstName = "Bob";
                contact.LastName = "Kamau";
                contact.Email = "bob.kamau@betafinance.co.ke";
                contact.Phone = "+254 700 333 445";
                contact.ComplianceRole = ObjectSpace.FindObject<ComplianceRole>(CriteriaOperator.Parse("Code = ?", "CFO"));
                contact.IsPrimaryContact = true;
            }

            // Add compliance requirements to Companies Act
            var companiesAct = ObjectSpace.FindObject<RegulatoryAct>(CriteriaOperator.Parse("ShortName = ?", "Companies Act"));
            if (companiesAct != null) {
                SeedComplianceRequirements(companiesAct);

                // Create applicable regulations and obligations
                SeedApplicableRegulationsAndObligations(companiesAct, acmeCorp, betaFinance);
            }
        }

        private void SeedComplianceRequirements(RegulatoryAct companiesAct) {
            var annualTimeline = ObjectSpace.FindObject<TimelineType>(CriteriaOperator.Parse("Code = ?", "ANNUAL"));
            var quarterlyTimeline = ObjectSpace.FindObject<TimelineType>(CriteriaOperator.Parse("Code = ?", "QUARTERLY"));
            var eventTimeline = ObjectSpace.FindObject<TimelineType>(CriteriaOperator.Parse("Code = ?", "EVENT"));
            var highRisk = ObjectSpace.FindObject<RiskRating>(CriteriaOperator.Parse("Code = ?", "HIGH"));
            var mediumRisk = ObjectSpace.FindObject<RiskRating>(CriteriaOperator.Parse("Code = ?", "MEDIUM"));
            var lowRisk = ObjectSpace.FindObject<RiskRating>(CriteriaOperator.Parse("Code = ?", "LOW"));

            // Annual Return Filing
            var annualReturn = ObjectSpace.FindObject<ComplianceRequirement>(CriteriaOperator.Parse("SectionReference = ?", "Section 658"));
            if (annualReturn == null) {
                annualReturn = ObjectSpace.CreateObject<ComplianceRequirement>();
                annualReturn.RegulatoryAct = companiesAct;
                annualReturn.Title = "Annual Return Filing";
                annualReturn.SectionReference = "Section 658";
                annualReturn.Description = "Every company must file an annual return with the Registrar within 42 days after the anniversary of its incorporation date.";
                annualReturn.TimelineType = annualTimeline;
                annualReturn.DueMonth = 12;
                annualReturn.DueDayOfMonth = 31;
                annualReturn.RiskRating = highRisk;
                annualReturn.PenaltyAmount = 50000;
                annualReturn.PenaltyDescription = "Fine not exceeding KES 50,000 and/or imprisonment for a term not exceeding 2 years";
                annualReturn.IsActive = true;
            }

            // AGM Requirement
            var agmRequirement = ObjectSpace.FindObject<ComplianceRequirement>(CriteriaOperator.Parse("SectionReference = ?", "Section 278"));
            if (agmRequirement == null) {
                agmRequirement = ObjectSpace.CreateObject<ComplianceRequirement>();
                agmRequirement.RegulatoryAct = companiesAct;
                agmRequirement.Title = "Annual General Meeting";
                agmRequirement.SectionReference = "Section 278";
                agmRequirement.Description = "A public company must hold an annual general meeting within six months of its financial year end.";
                agmRequirement.TimelineType = annualTimeline;
                agmRequirement.DueMonth = 6;
                agmRequirement.DueDayOfMonth = 30;
                agmRequirement.RiskRating = highRisk;
                agmRequirement.PenaltyAmount = 100000;
                agmRequirement.PenaltyDescription = "Default fine and liability for officers";
                agmRequirement.IsActive = true;
            }

            // Quarterly Board Meeting
            var boardMeeting = ObjectSpace.FindObject<ComplianceRequirement>(CriteriaOperator.Parse("Title = ?", "Quarterly Board Meeting"));
            if (boardMeeting == null) {
                boardMeeting = ObjectSpace.CreateObject<ComplianceRequirement>();
                boardMeeting.RegulatoryAct = companiesAct;
                boardMeeting.Title = "Quarterly Board Meeting";
                boardMeeting.SectionReference = "Section 141";
                boardMeeting.Description = "The board of directors should meet at least once every quarter to discharge their duties.";
                boardMeeting.TimelineType = quarterlyTimeline;
                boardMeeting.DueDayOfMonth = 15;
                boardMeeting.RiskRating = mediumRisk;
                boardMeeting.IsActive = true;
            }

            // Director Change Notification
            var directorChange = ObjectSpace.FindObject<ComplianceRequirement>(CriteriaOperator.Parse("SectionReference = ?", "Section 148"));
            if (directorChange == null) {
                directorChange = ObjectSpace.CreateObject<ComplianceRequirement>();
                directorChange.RegulatoryAct = companiesAct;
                directorChange.Title = "Director Appointment/Resignation Notification";
                directorChange.SectionReference = "Section 148";
                directorChange.Description = "Notice of change of directors must be filed with the Registrar within 14 days.";
                directorChange.TimelineType = eventTimeline;
                directorChange.DaysAfterEvent = 14;
                directorChange.TriggerEvent = "Appointment or resignation of a director";
                directorChange.RiskRating = mediumRisk;
                directorChange.PenaltyAmount = 20000;
                directorChange.PenaltyDescription = "Fine not exceeding KES 20,000";
                directorChange.IsActive = true;
            }

            // Beneficial Ownership Register
            var beneficialOwner = ObjectSpace.FindObject<ComplianceRequirement>(CriteriaOperator.Parse("SectionReference = ?", "Section 93"));
            if (beneficialOwner == null) {
                beneficialOwner = ObjectSpace.CreateObject<ComplianceRequirement>();
                beneficialOwner.RegulatoryAct = companiesAct;
                beneficialOwner.Title = "Beneficial Ownership Register Update";
                beneficialOwner.SectionReference = "Section 93";
                beneficialOwner.Description = "Companies must maintain a register of beneficial owners and report any changes.";
                beneficialOwner.TimelineType = eventTimeline;
                beneficialOwner.DaysAfterEvent = 30;
                beneficialOwner.TriggerEvent = "Change in beneficial ownership";
                beneficialOwner.RiskRating = highRisk;
                beneficialOwner.PenaltyAmount = 500000;
                beneficialOwner.PenaltyDescription = "Fine not exceeding KES 500,000 and/or imprisonment";
                beneficialOwner.IsActive = true;
            }
        }

        private void SeedApplicableRegulationsAndObligations(RegulatoryAct companiesAct, Company acmeCorp, Company betaFinance) {
            var pendingStatus = ObjectSpace.FindObject<ObligationStatus>(CriteriaOperator.Parse("Code = ?", "PENDING"));
            var inProgressStatus = ObjectSpace.FindObject<ObligationStatus>(CriteriaOperator.Parse("Code = ?", "INPROGRESS"));

            // Apply Companies Act to ACME Corporation
            var acmeRegulation = ObjectSpace.FindObject<ApplicableRegulation>(
                CriteriaOperator.And(
                    new BinaryOperator("Company.Oid", acmeCorp.Oid),
                    new BinaryOperator("RegulatoryAct.Oid", companiesAct.Oid)));

            if (acmeRegulation == null) {
                acmeRegulation = ObjectSpace.CreateObject<ApplicableRegulation>();
                acmeRegulation.Company = acmeCorp;
                acmeRegulation.RegulatoryAct = companiesAct;
                acmeRegulation.AppliedOn = DateTime.Today.AddMonths(-3);
                acmeRegulation.EffectiveFrom = new DateTime(2020, 1, 15);
                acmeRegulation.IsActive = true;
            }

            // Apply Companies Act to Beta Finance
            var betaRegulation = ObjectSpace.FindObject<ApplicableRegulation>(
                CriteriaOperator.And(
                    new BinaryOperator("Company.Oid", betaFinance.Oid),
                    new BinaryOperator("RegulatoryAct.Oid", companiesAct.Oid)));

            if (betaRegulation == null) {
                betaRegulation = ObjectSpace.CreateObject<ApplicableRegulation>();
                betaRegulation.Company = betaFinance;
                betaRegulation.RegulatoryAct = companiesAct;
                betaRegulation.AppliedOn = DateTime.Today.AddMonths(-3);
                betaRegulation.EffectiveFrom = new DateTime(2019, 6, 1);
                betaRegulation.IsActive = true;
            }

            // Create sample obligations for ACME
            var annualReturn = ObjectSpace.FindObject<ComplianceRequirement>(CriteriaOperator.Parse("SectionReference = ?", "Section 658"));
            var currentYear = DateTime.Now.Year;

            var acmeObligation = ObjectSpace.FindObject<ComplianceObligation>(
                CriteriaOperator.And(
                    new BinaryOperator("Company.Oid", acmeCorp.Oid),
                    new BinaryOperator("ComplianceRequirement.Oid", annualReturn.Oid),
                    new BinaryOperator("PeriodYear", currentYear)));

            if (acmeObligation == null) {
                var acmeContact = ObjectSpace.FindObject<CompanyContact>(
                    new BinaryOperator("Company.Oid", acmeCorp.Oid));

                acmeObligation = ObjectSpace.CreateObject<ComplianceObligation>();
                acmeObligation.Company = acmeCorp;
                acmeObligation.ComplianceRequirement = annualReturn;
                acmeObligation.PeriodYear = currentYear;
                acmeObligation.DueDate = new DateTime(currentYear, 12, 31);
                acmeObligation.Status = pendingStatus;
                acmeObligation.AssignedTo = acmeContact;

                // Create reminders for this obligation
                SeedTestReminders(acmeObligation);
            }

            // Create obligations for Beta Finance
            var betaObligation = ObjectSpace.FindObject<ComplianceObligation>(
                CriteriaOperator.And(
                    new BinaryOperator("Company.Oid", betaFinance.Oid),
                    new BinaryOperator("ComplianceRequirement.Oid", annualReturn.Oid),
                    new BinaryOperator("PeriodYear", currentYear)));

            if (betaObligation == null) {
                var betaContact = ObjectSpace.FindObject<CompanyContact>(
                    new BinaryOperator("Company.Oid", betaFinance.Oid));

                betaObligation = ObjectSpace.CreateObject<ComplianceObligation>();
                betaObligation.Company = betaFinance;
                betaObligation.ComplianceRequirement = annualReturn;
                betaObligation.PeriodYear = currentYear;
                betaObligation.DueDate = new DateTime(currentYear, 12, 31);
                betaObligation.Status = inProgressStatus;
                betaObligation.AssignedTo = betaContact;
            }

            // Create an overdue obligation for testing
            var agmRequirement = ObjectSpace.FindObject<ComplianceRequirement>(CriteriaOperator.Parse("SectionReference = ?", "Section 278"));
            var overdueObligation = ObjectSpace.FindObject<ComplianceObligation>(
                CriteriaOperator.And(
                    new BinaryOperator("Company.Oid", acmeCorp.Oid),
                    new BinaryOperator("ComplianceRequirement.Oid", agmRequirement.Oid),
                    new BinaryOperator("PeriodYear", currentYear - 1)));

            if (overdueObligation == null) {
                var overdueStatus = ObjectSpace.FindObject<ObligationStatus>(CriteriaOperator.Parse("Code = ?", "OVERDUE"));

                overdueObligation = ObjectSpace.CreateObject<ComplianceObligation>();
                overdueObligation.Company = acmeCorp;
                overdueObligation.ComplianceRequirement = agmRequirement;
                overdueObligation.PeriodYear = currentYear - 1;
                overdueObligation.DueDate = new DateTime(currentYear - 1, 6, 30);
                overdueObligation.Status = overdueStatus;
            }
        }

        private void SeedTestReminders(ComplianceObligation obligation) {
            var initialType = ObjectSpace.FindObject<ReminderType>(CriteriaOperator.Parse("Code = ?", "INITIAL"));
            var pendingDelivery = ObjectSpace.FindObject<DeliveryStatus>(CriteriaOperator.Parse("Code = ?", "PENDING"));
            var emailChannel = ObjectSpace.FindObject<NotificationChannel>(CriteriaOperator.Parse("Code = ?", "EMAIL"));

            if (obligation.DueDate.HasValue) {
                var reminder = ObjectSpace.CreateObject<ComplianceReminder>();
                reminder.ComplianceObligation = obligation;
                reminder.ReminderType = initialType;
                reminder.ScheduledDate = obligation.DueDate.Value.AddDays(-30);
                reminder.DeliveryStatus = pendingDelivery;
                reminder.NotificationChannel = emailChannel;
                reminder.Recipient = obligation.AssignedTo;
                reminder.RecipientEmail = obligation.AssignedTo?.Email ?? obligation.Company?.Email;
            }
        }

        #endregion

        public override void UpdateDatabaseBeforeUpdateSchema() {
            base.UpdateDatabaseBeforeUpdateSchema();
        }
    }
}
