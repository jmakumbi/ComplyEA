# White-Label XAF Application — Claude Code Prompt

> **Usage:** Place this file in the root of any XAF project folder alongside the required reference files. Run with Claude Code from the project root.

---

## Prerequisites

Before executing, confirm the following files exist in the project directory (or a `/docs` subfolder):

| File | Purpose | Required |
|------|---------|----------|
| `README.md` | Project name, description, tagline, company name, support contact, target platform(s) | **Yes** |
| `*_DesignLanguage.md` | Colour palette, typography, iconography rules, component-specific design guidance | **Yes** |
| `design.md` | Application design document — user personas, navigation structure, domain model, brand assets inventory | **Yes** |

Read **all three files in full** before making any changes. Extract the following values and use them consistently throughout every modification:

```
From README.md:
  APP_NAME          → The official application name (used in title bars, splash screens, about dialogs)
  APP_TAGLINE       → The catchphrase or tagline (used beneath the app name on login/splash)
  COMPANY_NAME      → The owning company name (replaces all "DevExpress" references)
  SUPPORT_CONTACT   → Support email or URL (used in About Info and error messages)
  APP_VERSION        → Current version string (if present; omit from title bar unless README says otherwise)

From *_DesignLanguage.md:
  PRIMARY_ACTION     → Primary Action Color hex (for Save buttons, active selections)
  SECONDARY_NAV      → Secondary/Navigation Color hex (sidebar, header backgrounds)
  BILLABLE_DARK      → Navigation sidebar and header background colour
  BILLABLE_ORANGE    → Primary action button colour
  BILLABLE_CREAM     → Detail view and card surface colour
  SURFACE_WHITE      → Main content background
  TEXT_PRIMARY        → Body text colour
  TEXT_SECONDARY      → Label/caption colour
  SEMANTIC_SUCCESS    → Success state hex
  SEMANTIC_WARNING    → Warning state hex
  SEMANTIC_DANGER     → Danger state hex
  SEMANTIC_INFO       → Info state hex
  FONT_FAMILY         → Main interface font name
  ICON_STYLE          → Icon style (Line-art / Solid-fill / Duotone)
  ICON_STROKE         → Stroke weight for custom SVGs
  ICON_CANVAS_SMALL   → Small icon canvas size (e.g., 16×16)
  ICON_CANVAS_LARGE   → Large icon canvas size (e.g., 32×32)

From design.md:
  PLATFORM_TARGET    → WinForms, Blazor, or both
  NAV_STYLE          → Accordion sidebar, Ribbon tabs, etc.
  WINDOW_STYLE       → Ribbon + TabbedMDI, classic menu, etc.
  EXISTING_ASSETS    → Inventory of logos, icons, splash screens already in the project
  USER_ROLES         → List of roles (determines which stock actions to hide per role)
  RECOMMENDED_SKIN   → DevExpress skin name for light mode
```

---

## Task: Comprehensive White-Labeling

Execute a comprehensive white-labeling of this XAF application to remove **all** default "DevExpress" and "ExpressApp Framework" identifiers, replacing them with the bespoke brand assets and linguistics defined in the reference files above.

Work through each section below in order. After completing each section, report what was changed.

---

### 1. Global Identity Overrides (Model Editor)

Update the `Model.xafml` (and `Model.DesignedDiffs.xafml` if present) to reflect the new identity:

**Application Node:**
- Set the `Title` property to `APP_NAME` extracted from `README.md`
- Remove any version numbers from the title bar unless `README.md` explicitly requires them
- Set `Logo` to the project's primary SVG brand mark (check the `EXISTING_ASSETS` inventory in `design.md` for the current logo file path; if none exists, note this as a gap requiring a new asset)

**About Info:**
- Customise the `AboutInfoString` property
- Replace every mention of "DevExpress", "Developer Express", and "ExpressApp Framework" with `COMPANY_NAME` and `APP_NAME`
- Include `SUPPORT_CONTACT` from `README.md`
- Include `APP_VERSION` if present in `README.md`

**Company and Copyright:**
- Set `Company` to `COMPANY_NAME`
- Update copyright strings to `© {CURRENT_YEAR} {COMPANY_NAME}. All rights reserved.`

---

### 2. Splash Screen & Login Customisation

**Splash Screen (WinForms — if `PLATFORM_TARGET` includes WinForms):**
- Locate the splash screen class (typically `XafSplashScreen.cs` or `DXSplashScreen` override in the `.Win` project)
- Replace background colour with `BILLABLE_DARK` from the design language
- Set the application name label to `APP_NAME` using `FONT_FAMILY` at Bold 24px in `BILLABLE_CREAM` colour
- Set the subtitle/tagline label to `APP_TAGLINE` from `README.md` using `FONT_FAMILY` at Regular 12px
- Set the company/copyright label to `© {CURRENT_YEAR} {COMPANY_NAME}`
- Replace "Loading..." text colour with `BILLABLE_ORANGE`
- Set progress bar colour to `BILLABLE_ORANGE`
- **Remove the "DevExpress" watermark/logo entirely** — replace with the project's own logo SVG if one exists in `EXISTING_ASSETS`
- If using `SvgImage` for the splash logo, load it via `SvgImage.FromResources()` from the module's embedded resources

**Splash Screen (Blazor — if `PLATFORM_TARGET` includes Blazor):**
- Update `wwwroot/css/site.css` to apply brand colours to the loading screen
- Replace any DevExpress loading spinner with a branded alternative using `BILLABLE_ORANGE`
- Ensure the Blazor loading text shows `APP_NAME` not "Loading..."

**Login / Logon View:**
- Locate the authentication configuration (typically `AuthenticationStandard` or `AuthenticationActiveDirectory`)
- In the Model Editor or via code, apply these text overrides:
  - Replace "Log On" / "Logon" → **"Sign In"**
  - Replace "Log Off" / "Logoff" → **"Sign Out"**
- Add `APP_TAGLINE` as sub-text beneath the application name on the login form
- Replace the default "Key" or "User" icons with custom SVGs following the `ICON_STYLE` and skin-awareness rules from the design language (use `.White` and `.Blue` CSS classes for skin compatibility)
- Set the login form background to `SURFACE_WHITE` with a `BILLABLE_DARK` header strip containing the `APP_NAME` in `BILLABLE_CREAM`

---

### 3. Linguistic "De-Stocking" (Localisation Overrides)

Perform search-and-replace within the Localisation node of the Application Model and any `.resx` or hardcoded strings:

**Framework Term Replacements:**

| Find | Replace With |
|------|-------------|
| `ExpressApp Framework` | `{APP_NAME} Core` |
| `eXpress App Framework` | `{APP_NAME} Core` |
| `XAF` (in user-visible strings only) | `{APP_NAME}` |
| `DevExpress` | `{COMPANY_NAME}` |
| `Developer Express Inc.` | `{COMPANY_NAME}` |

**Navigation Group Renaming:**

| Default Name | Replacement | Condition |
|-------------|-------------|-----------|
| `Default` | `Workspace` or `Dashboard` | If the group contains dashboard views |
| `Default` | `Home` | If the group is a general landing area |

Cross-reference the navigation structure in `design.md` — if custom navigation groups are already defined there, use those names exactly. Only rename groups that still carry default framework names.

**Error and Validation Message Softening:**

| Technical Message | User-Friendly Replacement |
|------------------|--------------------------|
| `User cannot be null` | `Please provide a valid user account to continue.` |
| `Access denied` | `You do not have permission to perform this action. Please contact your administrator.` |
| `Object not found` | `The requested record could not be found. It may have been deleted or moved.` |
| `Validation failed` | `Please review the highlighted fields and correct any errors before saving.` |
| `Session expired` | `Your session has expired. Please sign in again to continue.` |
| `Unexpected error` | `Something went wrong. Please try again or contact {SUPPORT_CONTACT} if the issue persists.` |

**Tooltip and Label Refinements:**
- Replace "Record" with the domain-appropriate term from `design.md` (e.g., "Obligation", "Shipment", "Case", "Contract")
- Replace "List View" / "Detail View" labels visible to users with plain language equivalents (e.g., "List", "Details")

---

### 4. Ribbon & Action Bar Sanitisation

**Icon Mapping:**

Map custom skin-aware SVGs to all standard XAF Actions. Follow the `ICON_STYLE`, `ICON_STROKE`, and `ICON_CANVAS_SMALL` / `ICON_CANVAS_LARGE` specifications from the design language file.

All custom SVGs **must** use DevExpress named CSS colour classes for skin awareness:

```xml
<!-- Skin-aware SVG template -->
<svg viewBox="0 0 {ICON_CANVAS_SMALL}" xmlns="http://www.w3.org/2000/svg">
  <path d="..." class="Black"/>   <!-- Primary shape — adapts to skin -->
  <path d="..." class="Green"/>   <!-- Semantic accent — adapts to skin -->
</svg>
```

Supported CSS classes: `.White`, `.Black`, `.Green`, `.Red`, `.Yellow`, `.Blue`, `.Purple`

Alternatively, use CSS variable syntax:
```xml
<circle style="fill: var(--dxds-icon-color-green)"/>
```

**Standard Action Icon Assignments:**

| Action | Icon Concept | Primary Class | Accent Class |
|--------|-------------|---------------|-------------|
| New | Document with plus | `.Black` | `.Green` |
| Save | Floppy disk / checkmark | `.Black` | `.Green` |
| Save and Close | Checkmark with door | `.Black` | `.Green` |
| Edit | Pencil | `.Black` | `.Blue` |
| Delete | Trash bin | `.Black` | `.Red` |
| Refresh | Circular arrows | `.Black` | `.Blue` |
| Cancel | X mark | `.Black` | `.Red` |
| Close | Door with arrow | `.Black` | — |
| Filter | Funnel | `.Black` | — |
| Search | Magnifying glass | `.Black` | — |
| Export | Document with down-arrow | `.Black` | `.Blue` |
| Print | Printer | `.Black` | — |

Set `ImageName` properties in the Application Model to point to the embedded SVG resource names.

**Stock Action Visibility:**

Hide framework-level actions that are not relevant to the target users defined in `design.md`:

| Action | Hide For | Keep For |
|--------|----------|----------|
| `EditModel` | All non-admin roles | System Administrator only |
| `Diagnostic Info` | All non-admin roles | System Administrator only |
| `Model Differences` | All non-admin roles | System Administrator only |
| `About` (if it still shows DevExpress info) | — | All (after rebranding content) |

Reference `USER_ROLES` from `design.md` to determine the admin role name (it may be "System Administrator", "Admin", "Administrator", etc.).

**Action Group Renaming:**

| Default Group Name | Replacement |
|-------------------|-------------|
| `View` | `Interface` or `Appearance` |
| `Tools` | `Utilities` |
| `RecordEdit` | `Actions` |
| `Edit` | `Record` |
| `ObjectsCreation` | `New` |
| `Diagnostic` | `System` (admin only) |
| `Reports` | `Reports` (keep — already clear) |

Only rename groups that use default framework names. If `design.md` defines custom group names or a Ribbon structure, defer to those specifications.

---

### 5. Technical Verification

After completing all changes above, verify the following:

**Application Icon / Favicon:**
- **WinForms:** Check that the `.ico` file in the `.Win` project is **not** the default DevExpress "X" icon. If it is, flag this as a gap: "Application icon needs replacement with a custom .ico file using brand colours."
- **Blazor:** Check that `wwwroot/favicon.ico` is not the default. Also verify that `wwwroot/images/` contains the project's logo SVG referenced in `site.css`.

**Browser Tab Title (Blazor only):**
- Ensure the page title follows the format: `{Page Name} | {APP_NAME}` with no framework suffixes
- Check `_Host.cshtml` or `_Layout.cshtml` for hardcoded "DevExpress" or "XAF" in `<title>` tags

**Title Bar (WinForms only):**
- Verify the main form title shows `APP_NAME` without "DevExpress", "XAF", or version numbers (unless README requires versioning)

**Assembly Info:**
- Check `AssemblyInfo.cs` files across all projects for `AssemblyCompany`, `AssemblyProduct`, `AssemblyCopyright`, and `AssemblyDescription`
- Replace any "DevExpress" or placeholder values with `COMPANY_NAME`, `APP_NAME`, and current copyright

**Theme / Skin Setting:**
- Verify the application starts with `RECOMMENDED_SKIN` from the design language, not the DevExpress default
- **WinForms:** Check `Program.cs` or `WinApplication.cs` for `UserLookAndFeel.Default.SkinName`
- **Blazor:** Check `appsettings.json` theme configuration and ensure the default theme matches the design language recommendation

---

## Output

After completing all sections, provide a summary report listing:

1. **Files Modified** — full paths of every file changed
2. **Values Applied** — the extracted APP_NAME, COMPANY_NAME, colours, and other tokens actually used
3. **Gaps Identified** — any missing assets (logos, icons, .ico files) that need to be created manually
4. **Verification Results** — pass/fail for each item in Section 5

---

*Prompt version 1.0 — February 2026*
*Part of the Billable Design System for EpiTent Limited enterprise XAF applications*
