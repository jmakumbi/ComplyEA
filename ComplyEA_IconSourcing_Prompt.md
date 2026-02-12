# ComplyEA — Icon Sourcing, Theming & Application Prompt

> **Usage:** Place this file in the ComplyEA project root alongside `README.md`, `design.md`, and `ComplyEA_DesignLanguage.md`. Run with Claude Code from the project root.

---

## Prerequisites

Read the following files **in full** before taking any action:

1. `README.md` — project name, platform target
2. `design.md` — full domain model, navigation structure, business objects, controllers, user roles
3. `ComplyEA_DesignLanguage.md` — icon style rules, skin-awareness specifications, recommended icon mapping, colour palette

Extract and confirm these values from the design language file before proceeding:

```
ICON_STYLE         = Line-art (outline)
ICON_STROKE_SMALL  = 1.5px (for 16×16 icons)
ICON_STROKE_LARGE  = 2px (for 32×32 icons — only if needed)
ICON_CANVAS        = 16×16 (navigation/grid), 32×32 (dashboard/detail headers)
ICON_CORNER_RADIUS = 2px for rectangles, fully rounded for circles
ICON_PADDING       = 1px clear zone inside canvas edge
COLOUR_LIMIT       = Maximum 2 semantic colours per icon
SVG_CLASS_METHOD   = DevExpress named CSS classes (.White, .Black, .Green, .Red, .Yellow, .Blue)
```

---

## Step 1 — Select an Open-Source Icon Set

Search for and select a **free, open-source, SVG icon set** that meets ALL of these criteria:

1. **License:** MIT, Apache 2.0, or ISC — must permit commercial use and modification without attribution requirements in the compiled application (attribution in source/docs is fine)
2. **Style match:** Line-art / outline icons with consistent stroke weights — matching the "Professional, Trustworthy, Organized" tone described in the design language
3. **Domain coverage:** Must have icons for legal/compliance/business concepts: documents, buildings, briefcases, bells, clocks, calendars, shields, gears, envelopes, users, charts, clipboards, books, scales
4. **Format:** Individual SVG files downloadable per-icon (not a single sprite sheet)
5. **Size:** Must include or render cleanly at 16×16 and 32×32

**Recommended icon sets to evaluate (in priority order):**

| Icon Set | License | Style | URL Pattern for Individual SVGs |
|----------|---------|-------|---------------------------------|
| **Lucide** | ISC | Line-art, 24px base, 2px stroke | `https://raw.githubusercontent.com/lucide-icons/lucide/main/icons/{name}.svg` |
| **Tabler Icons** | MIT | Line-art, 24px base, 2px stroke | `https://raw.githubusercontent.com/tabler/tabler-icons/main/icons/outline/{name}.svg` |
| **Phosphor Icons** | MIT | Line-art (thin variant), 256px base | `https://raw.githubusercontent.com/phosphor-icons/core/main/assets/regular/{name}.svg` |
| **Heroicons** | MIT | Outline variant, 24px base | `https://raw.githubusercontent.com/tailwindlabs/heroicons/master/optimized/24/outline/{name}.svg` |

Pick **one** icon set for the entire application to ensure visual consistency. Confirm your selection and the license before downloading.

---

## Step 2 — Map Business Objects to Icons

Using the domain model from `design.md` and the recommended icon mapping from `ComplyEA_DesignLanguage.md`, create a mapping of every business object and navigation item to a specific icon from the chosen set.

**Required mappings (from design.md domain model):**

### Organisation Layer
| Business Object | Icon Name (search term) | Primary SVG Class | Accent SVG Class |
|-----------------|------------------------|-------------------|-----------------|
| LegalFirm | building, landmark, building-2 | `.Black` | `.Blue` |
| Company | briefcase, building-office | `.Black` | — |
| CompanyContact | user, contact, user-circle | `.Black` | `.Blue` |

### Regulatory Framework
| Business Object | Icon Name (search term) | Primary SVG Class | Accent SVG Class |
|-----------------|------------------------|-------------------|-----------------|
| RegulatoryAct | book-open, book, library | `.Black` | `.Blue` |
| ComplianceRequirement | file-check, clipboard-check | `.Black` | `.Green` |
| ApplicableRegulation | link, file-symlink, git-merge | `.Black` | `.Blue` |
| ActAcronym | hash, tag, bookmark | `.Black` | — |

### Compliance Workflow
| Business Object | Icon Name (search term) | Primary SVG Class | Accent SVG Class |
|-----------------|------------------------|-------------------|-----------------|
| ComplianceObligation | clipboard-list, list-checks, task | `.Black` | `.Green` |
| ComplianceReminder | bell, bell-ring, alarm-clock | `.Black` | `.Yellow` |
| ComplianceDocument | file-text, file, document | `.Black` | — |

### Configuration & Templates
| Business Object | Icon Name (search term) | Primary SVG Class | Accent SVG Class |
|-----------------|------------------------|-------------------|-----------------|
| SystemConfiguration | settings, sliders, cog | `.Black` | — |
| EmailTemplate | mail, envelope, at-sign | `.Black` | — |
| CompanyReminderSettings | bell-plus, timer, clock-plus | `.Black` | `.Yellow` |
| ComplianceTemplate | file-plus, template, copy | `.Black` | — |

### Navigation Groups
| Navigation Group | Icon Name (search term) | Primary SVG Class | Accent SVG Class |
|-----------------|------------------------|-------------------|-----------------|
| Organization | building-2, landmark | `.White` | — |
| Regulatory | book-open, scale | `.White` | — |
| Compliance | shield-check, clipboard | `.White` | — |
| Configuration | settings, wrench | `.White` | — |
| Administration | lock, shield, key | `.White` | — |

### Dashboard / Status Icons
| Usage | Icon Name (search term) | Primary SVG Class | Accent SVG Class |
|-------|------------------------|-------------------|-----------------|
| Overdue indicator | alert-circle, clock-alert | `.Red` | — |
| Upcoming indicator | calendar-clock, calendar-arrow | `.Yellow` | — |
| Completed indicator | check-circle, circle-check | `.Green` | — |
| Pending indicator | clock, hourglass, loader | `.Blue` | — |
| Failed delivery | x-circle, alert-triangle | `.Red` | — |

### Standard XAF Actions
| Action | Icon Name (search term) | Primary SVG Class | Accent SVG Class |
|--------|------------------------|-------------------|-----------------|
| New / Create | plus-circle, file-plus | `.Black` | `.Green` |
| Save | save, check, floppy-disk | `.Black` | `.Green` |
| Save and Close | check-square, log-out | `.Black` | `.Green` |
| Edit | pencil, edit, edit-2 | `.Black` | `.Blue` |
| Delete | trash, trash-2 | `.Black` | `.Red` |
| Refresh | refresh-cw, rotate-ccw | `.Black` | `.Blue` |
| Cancel | x, x-circle | `.Black` | `.Red` |
| Close | log-out, door-open | `.Black` | — |
| Filter | filter, funnel | `.Black` | — |
| Search | search, magnifying-glass | `.Black` | — |
| Export | download, file-down | `.Black` | `.Blue` |
| Print | printer | `.Black` | — |

If the chosen icon set does not have an exact match for a business object, find the closest conceptual equivalent. Document any substitutions.

---

## Step 3 — Download the Icons

For each mapped icon:

1. Download the SVG file using `curl` or `wget` from the icon set's raw GitHub URL
2. Save to a temporary working directory: `./icons_temp/`
3. Name each file according to the business object: `{BusinessObjectName}.svg` (e.g., `ComplianceObligation.svg`, `LegalFirm.svg`)
4. Also download action icons named: `Action_{ActionName}.svg` (e.g., `Action_New.svg`, `Action_Save.svg`)
5. Also download navigation icons named: `Nav_{GroupName}.svg` (e.g., `Nav_Organization.svg`, `Nav_Compliance.svg`)
6. Also download status icons named: `Status_{State}.svg` (e.g., `Status_Overdue.svg`, `Status_Completed.svg`)

Verify each download succeeded (non-empty file, valid XML).

---

## Step 4 — Theme the Icons for DevExpress Skin-Awareness

For **every** downloaded SVG, perform the following transformation:

### 4a. Normalise the SVG Structure

- Set `viewBox="0 0 16 16"` (or retain original if already 16×16 or 32×32; rescale viewBox proportionally if source is 24×24)
- Remove any `width` and `height` attributes from the root `<svg>` element (let XAF control sizing)
- Remove any `xmlns:xlink` if not used
- Strip any `<title>`, `<desc>`, `<!-- comments -->`, and metadata elements

### 4b. Replace Colours with DevExpress CSS Classes

This is the critical transformation. DevExpress XAF recolours SVG icons based on the active skin by mapping named CSS classes to skin palette colours.

**Replacement rules:**

For each element that has a `fill`, `stroke`, or `style` attribute containing a colour value:

1. Identify whether this element is the **primary shape** or a **semantic accent** based on the mapping table in Step 2
2. Replace the colour with the appropriate CSS class:

```
Primary shape elements:
  - Remove: fill="#000000", fill="currentColor", fill="none" (keep none), stroke="#000000"
  - Add: class="Black"  (for icons on light backgrounds)
  - Add: class="White"  (for navigation icons on the dark sidebar)

Accent shape elements:
  - Remove: any fill/stroke colour
  - Add: class="{AccentClass}" from the mapping table (Green, Red, Yellow, Blue)
```

**Transformation example:**

Before (raw Lucide icon):
```xml
<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24"
     fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"
     stroke-linejoin="round">
  <path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2"/>
  <path d="m9 14 2 2 4-4"/>
  <rect x="8" y="2" width="8" height="4" rx="1" ry="1"/>
</svg>
```

After (DevExpress skin-aware):
```xml
<?xml version="1.0" encoding="utf-8"?>
<svg version="1.1" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
  <path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2"
        fill="none" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
        class="Black"/>
  <path d="m9 14 2 2 4-4"
        fill="none" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
        class="Green"/>
  <rect x="8" y="2" width="8" height="4" rx="1" ry="1"
        fill="none" stroke-width="2"
        class="Black"/>
</svg>
```

**Key rules:**
- `stroke="currentColor"` must be **removed** — the CSS class handles colouring
- `fill="none"` must be **kept** on outline/line-art icons (it prevents fill)
- `stroke-width`, `stroke-linecap`, `stroke-linejoin` should be **kept** (they define the line style)
- Each element gets its colour **only** from the `class` attribute
- For icons where ALL elements share one colour, add the class to each element individually (not to the root `<svg>`)

### 4c. Create Sidebar Variants

For navigation group icons that appear on the dark `#450f00` sidebar, create a second variant where primary elements use `.White` instead of `.Black`:

- Copy `Nav_{GroupName}.svg` → `Nav_{GroupName}_Dark.svg`
- Replace `class="Black"` → `class="White"` on primary shape elements
- Keep accent classes unchanged (DevExpress handles skin adaptation)

**Note:** In many DevExpress skins, the accordion/navigation panel automatically inverts icon colours. Test whether the `.Black` class icons are automatically recoloured on the dark sidebar before creating separate variants. If auto-recolouring works, skip the `_Dark` variants.

---

## Step 5 — Embed Icons in the XAF Project

### 5a. Create the Images Directory

```
{Module Project}/Images/
  ├── BusinessObjects/
  │   ├── ComplianceObligation.svg
  │   ├── ComplianceReminder.svg
  │   ├── LegalFirm.svg
  │   └── ... (all business object icons)
  ├── Actions/
  │   ├── Action_New.svg
  │   ├── Action_Save.svg
  │   └── ... (all action icons)
  ├── Navigation/
  │   ├── Nav_Organization.svg
  │   ├── Nav_Compliance.svg
  │   └── ... (all navigation group icons)
  └── Status/
      ├── Status_Overdue.svg
      ├── Status_Completed.svg
      └── ... (all status icons)
```

Place icons in the **platform-agnostic module project** (e.g., `ComplyEA.Module`) so they are available to both WinForms and Blazor platforms.

### 5b. Set Build Action

For every SVG file added, set the **Build Action** to `Embedded Resource` in the `.csproj`:

```xml
<ItemGroup>
  <EmbeddedResource Include="Images\BusinessObjects\ComplianceObligation.svg" />
  <EmbeddedResource Include="Images\BusinessObjects\ComplianceReminder.svg" />
  <!-- ... all SVGs ... -->
</ItemGroup>
```

Or use a glob pattern:
```xml
<ItemGroup>
  <EmbeddedResource Include="Images\**\*.svg" />
</ItemGroup>
```

### 5c. Apply ImageName to Business Objects

For each XPO business object class, add or update the `ImageName` attribute. The `ImageName` value should be the SVG filename **without the extension and without the folder path**:

```csharp
[ImageName("ComplianceObligation")]
public class ComplianceObligation : BaseObject { ... }
```

If the business object already has an `[ImageName]` attribute, replace the value. If it does not have one, add it.

**Do not include the `.svg` extension or folder path in the ImageName value.** XAF resolves embedded resource images by name automatically.

### 5d. Apply Icons to Navigation Groups

In `Model.xafml` or `Model.DesignedDiffs.xafml`, update each navigation item's `ImageName`:

```xml
<NavigationItems>
  <Items>
    <Item Id="Organization" ImageName="Nav_Organization" ...>
    <Item Id="Regulatory" ImageName="Nav_Regulatory" ...>
    <Item Id="Compliance" ImageName="Nav_Compliance" ...>
    <!-- etc. -->
  </Items>
</NavigationItems>
```

Also ensure `ShowImages="True"` is set on the NavigationItems node if it was previously `False`.

### 5e. Apply Icons to Actions

In the Application Model or via Controller code, map action icons:

```xml
<ActionDesign>
  <Actions>
    <Action Id="New" ImageName="Action_New" />
    <Action Id="Save" ImageName="Action_Save" />
    <Action Id="Delete" ImageName="Action_Delete" />
    <!-- etc. -->
  </Actions>
</ActionDesign>
```

---

## Step 6 — Verify and Report

1. **List all icons downloaded** with source URL and target filename
2. **Confirm license** — print the icon set name and license type
3. **List all SVGs transformed** and confirm each has DevExpress CSS classes applied (no inline colours remaining)
4. **List all business objects updated** with their `[ImageName]` values
5. **List all navigation items updated** in Model.xafml
6. **List any gaps** — business objects or navigation items where no suitable icon was found
7. **Build the project** with `dotnet build` and report any errors

---

*Prompt version 1.0 — February 2026*
*Part of the Billable Design System for EpiTent Limited enterprise XAF applications*
