# TODO

## Pending Tasks

### Phase 3: Reporting & Analytics
- [ ] Create compliance dashboard with charts
- [ ] Implement obligation summary reports
- [ ] Add compliance calendar view
- [ ] Create audit trail reports
- [ ] Export functionality (PDF, Excel)

### Phase 4: Integrations
- [ ] OAuth integration for Gmail/Outlook
- [ ] Calendar integration (Google Calendar, Outlook)
- [ ] SMS notification support (Africa's Talking, Twilio)
- [ ] Document storage integration (cloud storage)

### Phase 5: Advanced Features
- [ ] Multi-tenant data isolation
- [ ] Company-specific branding
- [ ] API endpoints for external integration
- [ ] Mobile-responsive UI enhancements
- [ ] Bulk import/export of compliance data

### Technical Debt
- [ ] Upgrade target framework from netcoreapp3.1
- [ ] Add unit tests for services
- [ ] Add integration tests for controllers
- [ ] Performance optimization for large datasets
- [ ] Implement caching for lookup data

### Documentation
- [ ] API documentation
- [ ] User guide
- [ ] Administrator guide
- [ ] Deployment guide

### Bug Fixes & Improvements
- [ ] Fix unused field warnings in ComplianceDocument.cs
- [ ] Fix unused field warnings in ComplianceTemplate.cs
- [ ] Add input validation for email addresses
- [ ] Improve error messages for end users

## Implementation Phases

### White-Label
- [x] Add AboutInfoString, Company, Copyright to Application model
- [x] Hide EditModel, DiagnosticInfo, ModelDifferences actions
- [x] Add localization overrides (Sign In/Sign Out, action group renames)
- [x] Add branded loading spinner CSS and splash screen color overrides
- [x] Rename "DevExpress Themes" → "Application Themes", curate themes
- [x] Add Assembly metadata to all 3 .csproj files
- [x] Add favicon link tag and verify title in _Host.cshtml

### Design Language
- [x] Row highlighting via ConditionalAppearance (already implemented)
- [x] Dashboard 4-quadrant layout (already implemented)
- [x] Popup auto-detect period and preview count (already implemented)
- [x] Add IncludeAdhoc filter for event-driven/always timeline types
- [x] CSS theme (already implemented)

### Icon Sourcing
- [x] Download ~60 Lucide SVGs and transform for DevExpress skin-awareness
- [x] Create Images directory structure (BusinessObjects, Actions, Navigation, Status)
- [x] Add EmbeddedResource glob to ComplyEA.Module.csproj
- [x] Update ImageName attributes on all business object classes
- [x] Update navigation group icons in model
- [x] Add action icon overrides in Blazor model

### Docker Deployment
- [x] Add Npgsql 4.1.13 for PostgreSQL support
- [x] Create NuGet.Config with DevExpress feed
- [x] Create multi-stage Dockerfile
- [x] Create docker-compose.yml
- [x] Create .env.example and .dockerignore
- [x] Create appsettings.Production.json
- [x] Create GitHub Actions CI/CD workflow
- [x] Create ZimaOS deployment compose
- [x] Add Docker deployment section to README
