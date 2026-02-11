# ComplyEA Implementation Plans

## 4-Phase Implementation Plan

Executed sequentially: white-labeling, design language enhancements, icon sourcing, and Docker deployment.

### Phase 1: XAF White-Label

**Goal:** Remove all DevExpress/XAF branding; apply ComplyEA/EpiTent identity.

- Brand: APP_NAME=ComplyEA, COMPANY_NAME=EpiTent Limited
- Support contact: GitHub Issues
- Recommended skin: Office White
- Files modified: Model.DesignedDiffs.xafml, Model.xafml, site.css, appsettings.json, 3x .csproj, _Host.cshtml

### Phase 2: Design Language Enhancements

**Goal:** Complete the 3 UX enhancements from design language spec.

- Enhancement 1 (row highlighting): Already implemented
- Enhancement 2 (dashboard layout): Already implemented
- Enhancement 3 (popup improvements): Added IncludeAdhoc filter
- Files modified: ObligationGenerationParameters.cs, ObligationGenerationService.cs, ObligationGenerationController.cs

### Phase 3: Icon Sourcing & Theming

**Goal:** Replace built-in XAF icon references with custom skin-aware SVGs from Lucide Icons.

- ~60 SVGs downloaded and transformed for DevExpress skin-awareness
- Directory: ComplyEA.Module/Images/{BusinessObjects,Actions,Navigation,Status}/
- Updated ImageName attributes on all 36+ business object classes
- Updated navigation and action icons in model files

### Phase 4: Docker Deployment

**Goal:** Migrate to PostgreSQL, Dockerize, set up CI/CD, prepare ZimaOS deployment.

- PostgreSQL via Npgsql 4.1.13
- Multi-stage Dockerfile (sdk:3.1 build → aspnet:3.1 runtime)
- GitHub Actions CI/CD workflow
- ZimaOS deployment compose
- Files created: NuGet.Config, Dockerfile, docker-compose.yml, .env.example, .dockerignore, appsettings.Production.json, scripts/, .github/workflows/, deploy/zimaos/
