# ComplyEA — Design Language Specification

> Cohesive design language for the ComplyEA regulatory compliance platform
> DevExpress XAF 20.2.13 | .NET Core 3.1 | Blazor Server

---

## 1. Color Palette

### Billable Foundation Palette

All four enterprise applications in this suite share a common Billable palette as the brand anchor. ComplyEA interprets this palette to express **urgency, trust, and clarity** — qualities essential for deadline-driven compliance work.

| Swatch | Name | Hex | Pantone | Role in ComplyEA |
|--------|------|-----|---------|-----------------|
| ██ | Billable Dark | `#450f00` | 498C | Navigation sidebar background, header bars, footer bars |
| ██ | Billable Orange | `#eb9130` | 157C | Primary action color (Save, Generate Obligations), active tab indicator, progress bars |
| ██ | Billable Cream | `#e8e1d1` | 7527C | Detail View backgrounds, card surfaces, alternating row tint in grids |

### Extended Application Palette

| Swatch | Name | Hex | Usage |
|--------|------|-----|-------|
| ██ | Surface White | `#FAFAF8` | Main content area background, grid canvas |
| ██ | Text Primary | `#2D2018` | Body text, grid cell values (warm dark derived from Billable Dark) |
| ██ | Text Secondary | `#7A6E62` | Labels, captions, helper text |
| ██ | Border Subtle | `#D4CEC4` | Grid lines, dividers, input field borders |
| ██ | Orange Hover | `#D47E28` | Hover/pressed state for primary action buttons |
| ██ | Orange Light | `#FDECD6` | Active navigation item background tint, selected row highlight |

### Semantic Colors

| State | Hex | Usage | Contrast Note |
|-------|-----|-------|---------------|
| Success | `#2E7D32` | Completed obligations, delivered reminders | WCAG AA on white and cream |
| Warning | `#E65100` | Upcoming deadlines (30-day window), pending reminders | Distinct from Billable Orange — deeper, redder |
| Danger | `#C62828` | Overdue obligations, failed deliveries, escalation reminders | Strong contrast on all light surfaces |
| Info | `#1565C0` | Informational badges, hyperlinks, total counts | Complements the warm palette without clashing |

### Dark / Light Mode Strategy

| Aspect | Light Mode (Default) | Dark Mode |
|--------|---------------------|-----------|
| Recommended Skin | **Office White** (current) or custom variation | **Office Black** or **WXI Dark** |
| Sidebar Background | `#450f00` (Billable Dark) | `#2A0900` (deeper variant) |
| Content Background | `#FAFAF8` | `#1E1E1E` |
| Card/Detail Surfaces | `#e8e1d1` (Billable Cream) | `#2D2D2D` |
| Primary Action | `#eb9130` (unchanged) | `#eb9130` (unchanged — orange reads well on dark) |
| Grid Text | `#2D2018` | `#E8E1D1` (Billable Cream as text) |
| Semantic Colors | As above | Lighten by ~15% for legibility on dark backgrounds |

**Implementation Note:** In XAF 20.2.13 Blazor, theme switching is handled via `appsettings.json` theme list. The 13 configured themes should be curated to a recommended pair (one light, one dark) rather than exposing all options to end users.

---

## 2. Typography & Hierarchy

### Main Interface Font

**Segoe UI** — the recommended typeface for all ComplyEA interfaces.

Rationale: Segoe UI is the Windows system font, renders crisply at small sizes in high-density grids, and is universally available on the office desktops where ComplyEA is deployed. It pairs naturally with DevExpress's built-in Blazor rendering pipeline and requires no custom font loading.

### Hierarchy Scale

| Element | Font | Weight | Size | Letter Spacing | Example |
|---------|------|--------|------|---------------|---------|
| Navigation Group Header | Segoe UI | **Semibold (600)** | 13px | 0.02em | `COMPLIANCE` |
| Navigation Item | Segoe UI | Regular (400) | 12px | Normal | `Obligations` |
| Detail View Header | Segoe UI | **Semibold (600)** | 16px | Normal | `ACME Corporation — Annual Return` |
| Layout Group Caption | Segoe UI | **Semibold (600)** | 13px | 0.01em | `Obligation Details` |
| Grid Column Header | Segoe UI | **Semibold (600)** | 11px | 0.03em (uppercase) | `DUE DATE` |
| Grid Cell Value | Segoe UI | Regular (400) | 12px | Normal | `2026-03-31` |
| Grid Cell — Overdue | Segoe UI | **Bold (700)** | 12px | Normal | `OVERDUE` (with Danger BackColor) |
| Dashboard KPI Label | Segoe UI | Regular (400) | 11px | 0.04em (uppercase) | `OVERDUE OBLIGATIONS` |
| Dashboard KPI Value | Segoe UI | **Bold (700)** | 28px | Normal | `14` |
| Button Label | Segoe UI | **Semibold (600)** | 12px | 0.02em | `Generate Obligations` |
| Form Field Label | Segoe UI | Regular (400) | 11px | Normal | `Company Name` |
| Form Field Value | Segoe UI | Regular (400) | 12px | Normal | `ACME Corporation` |

### Compact Density Specifications

Given ComplyEA's grid-heavy compliance triage workflow:

| Metric | Value |
|--------|-------|
| Grid Row Height | 24–28px |
| Grid Header Height | 30px |
| Detail View Field Spacing | 6px vertical gap between fields |
| Navigation Item Height | 28px |
| Dashboard Card Padding | 12px internal |
| Layout Group Margin | 8px |

---

## 3. Iconography Style

### Style Recommendation

**Line-art (outline)** icons with a 1.5px stroke weight.

Rationale: ComplyEA's visual tone is Professional, Trustworthy, and Organized. Line-art icons convey precision and clarity without visual heaviness — appropriate for a compliance tool where the data (grids, dates, statuses) must dominate the visual hierarchy, not the chrome.

### Skin-Awareness Rules for Custom SVGs

All custom SVG icons must use DevExpress's named CSS colour classes to enable automatic skin-based recolouring:

| SVG CSS Class | Mapped Colour (Light) | Mapped Colour (Dark) | Usage |
|---------------|----------------------|---------------------|-------|
| `.White` | White | Adapted by skin | Icon primary fill on dark backgrounds |
| `.Black` | Dark grey | Light grey | Icon primary stroke on light backgrounds |
| `.Green` | Success green | Lightened green | Completed/success state icons |
| `.Red` | Danger red | Lightened red | Overdue/failed state icons |
| `.Yellow` | Warning amber | Lightened amber | Pending/attention state icons |
| `.Blue` | Info blue | Lightened blue | Informational/link state icons |

**Alternatively**, use CSS variable syntax for XAF 20.2+ compatibility:

```xml
<circle cx="16" cy="16" r="14" style="fill: var(--dxds-icon-color-green)"/>
```

### Consistency Guide

| Rule | Specification |
|------|--------------|
| Canvas Size | 16×16px (navigation, grid inline), 32×32px (dashboard, detail headers) |
| Stroke Weight | 1.5px (16px icons), 2px (32px icons) |
| Corner Radius | 2px for rounded rectangles, fully rounded for circles |
| Padding | 1px clear zone inside canvas edge |
| Style | Open paths preferred (outlines, not filled shapes) |
| Colour Limit | Maximum 2 semantic colours per icon |
| File Naming | `{ObjectName}.svg` (e.g., `Obligation.svg`, `Reminder.svg`) |
| Build Action | Embedded Resource in `ComplyEA.Module` assembly |

### Recommended Icon Mapping

| Navigation / Entity | Icon Concept | SVG Classes Used |
|---------------------|-------------|-----------------|
| Legal Firms | Building with columns | `.Black` + `.Blue` |
| Companies | Briefcase | `.Black` |
| Obligations | Clipboard with checkmark | `.Black` + `.Green` |
| Reminders | Bell | `.Black` + `.Yellow` |
| Documents | Document with arrow | `.Black` |
| Regulatory Acts | Book with scales | `.Black` + `.Blue` |
| Dashboard — Overdue | Clock with exclamation | `.Red` |
| Dashboard — Upcoming | Calendar with arrow | `.Yellow` |
| Email Templates | Envelope with gear | `.Black` |
| System Config | Gear | `.Black` |

---

## 4. UX Feature Enhancements

### Enhancement 1 — Conditional Appearance Row Highlighting for Obligation Status

The Compliance Officer's primary workflow is scanning the obligation grid and triaging by urgency. Implement `ConditionalAppearanceModule` rules directly on the `ComplianceObligation` business object to visually encode status:

| Condition | BackColor | FontColor | FontStyle | Target |
|-----------|-----------|-----------|-----------|--------|
| `Status = 'Overdue'` | `#FFCDD2` (light red) | `#C62828` (danger) | **Bold** | All ViewItems in ListView |
| `Status = 'Pending'` AND `DueDate < AddDays(Now(), 7)` | `#FFF3E0` (light amber) | `#E65100` (warning) | Normal | All ViewItems in ListView |
| `Status = 'Completed'` | `#E8F5E9` (light green) | `#2E7D32` (success) | Normal | All ViewItems in ListView |
| `Status = 'Waived'` | `#F5F5F5` (light grey) | `#9E9E9E` (grey) | *Italic* | All ViewItems in ListView |
| `Status = 'InProgress'` | `#E3F2FD` (light blue) | `#1565C0` (info) | Normal | All ViewItems in ListView |

This eliminates the need for users to read the Status column text — the entire row's colour communicates urgency at a glance.

### Enhancement 2 — Dashboard Layout Configuration with 4-Quadrant Compliance Cockpit

Replace the default list-based dashboard with a structured 4-quadrant layout using XAF's DashboardView or a custom DashboardController:

```
┌──────────────────────────┬──────────────────────────┐
│  🔴 OVERDUE              │  🟡 UPCOMING (30 days)   │
│  Filtered ListView       │  Filtered ListView       │
│  Sorted by days overdue  │  Sorted by due date ASC  │
│  (desc — worst first)    │                          │
├──────────────────────────┼──────────────────────────┤
│  🔔 PENDING REMINDERS    │  📊 STATUS SUMMARY       │
│  Filtered ListView       │  Pie chart: Obligation   │
│  Grouped by reminder     │  counts by status        │
│  type (Initial/Final/    │  Bar chart: Obligations  │
│  Escalation)             │  by company              │
└──────────────────────────┴──────────────────────────┘
```

Each quadrant is a ViewItem within a DashboardView, enabling the Compliance Officer to see the full triage landscape on a single screen. The top-left quadrant (Overdue) has visual prominence with the Danger background tint.

### Enhancement 3 — Obligation Generation Popup with Period Pre-Selection

Enhance the existing `ObligationGenerationController` popup dialog with intelligent defaults:

- **Auto-detect current period**: Pre-select the current year and quarter based on system date
- **Company filter with multi-select**: Allow generating obligations for selected companies or all companies in a single batch
- **Preview count**: Before executing, display a count of obligations that will be generated (with duplicate detection feedback)
- **Timeline type filter**: Allow generating only specific timeline types (e.g., Annual obligations for year-end, Monthly for routine)

This reduces the Compliance Officer's decision overhead from "what do I generate?" to "confirm and go."

---

## 5. Component-Specific Design Guidance

### Navigation Accordion

| Property | Value |
|----------|-------|
| Background | `#450f00` (Billable Dark) |
| Group Header Text | `#e8e1d1` (Billable Cream), Semibold 13px, UPPERCASE |
| Group Header Background (active) | `#5A1800` (slightly lighter than Billable Dark) |
| Item Text | `#e8e1d1` (Billable Cream), Regular 12px |
| Item Hover | Background `#5A1800`, text unchanged |
| Item Active/Selected | Left border accent 3px `#eb9130`, background `#6B2200` |
| Collapse/Expand Icon | `.White` SVG class |

### Grid Headers

| Property | Value |
|----------|-------|
| Background | `#450f00` (Billable Dark) |
| Text | `#e8e1d1` (Billable Cream), Semibold 11px, UPPERCASE |
| Sort Indicator | `#eb9130` arrow SVGs |
| Filter Row Background | `#F5F0E8` (warm off-white) |
| Alternating Row | `#F5F0E8` / `#FAFAF8` |

### Detail View Layout Groups

| Property | Value |
|----------|-------|
| Group Header Background | `#e8e1d1` (Billable Cream) |
| Group Header Text | `#450f00` (Billable Dark), Semibold 13px |
| Group Border | 1px `#D4CEC4` |
| Internal Padding | 12px |
| Field Label | `#7A6E62` (Text Secondary), Regular 11px |
| Field Value | `#2D2018` (Text Primary), Regular 12px |

### Dashboard Cards

| Property | Value |
|----------|-------|
| Card Background | `#FFFFFF` |
| Card Border | 1px `#D4CEC4`, radius 4px |
| Card Header | `#450f00` (Billable Dark), Semibold 13px |
| KPI Value | `#eb9130` (Billable Orange), Bold 28px |
| KPI Label | `#7A6E62` (Text Secondary), Regular 11px, UPPERCASE |
| Card Shadow | `0 1px 3px rgba(69, 15, 0, 0.08)` |

---

*Document version 1.0 — February 2026*
*Part of the Billable Design System for EpiTent Limited enterprise XAF applications*
