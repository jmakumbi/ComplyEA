# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Run Commands

```bash
# Build the solution
dotnet build ComplyEA.sln

# Run the Blazor Server application
dotnet run --project ComplyEA.Blazor.Server
# App runs at https://localhost:5001

# Build specific configuration
dotnet build ComplyEA.sln -c Release
dotnet build ComplyEA.sln -c EasyTest
```

## Architecture Overview

This is a **DevExpress XAF (eXpressApp Framework) Blazor Server application** using XPO (eXpress Persistent Objects) for data persistence.

### Project Structure

- **ComplyEA.Module** (netstandard2.1) - Platform-agnostic business logic module containing:
  - Domain/business objects (XPO persistent classes) in `BusinessObjects/`
  - Controllers and actions in `Controllers/`
  - Business logic services in `Services/`
  - Database migration logic in `DatabaseUpdate/Updater.cs`
  - Application model customizations (`Model.DesignedDiffs.xafml`)

- **ComplyEA.Module.Blazor** (netstandard2.1) - Blazor-specific UI module containing:
  - Blazor-specific editors and property editors
  - Blazor-specific controllers
  - Platform-specific model customizations

- **ComplyEA.Blazor.Server** (netcoreapp3.1) - Blazor Server host application containing:
  - `Startup.cs` - ASP.NET Core service configuration and DI registration
  - `BlazorApplication.cs` - XAF application setup, connection string handling
  - `Program.cs` - Application entry point
  - `Services/BackgroundJobs/` - Background processing services

### Key XAF Concepts

- **Modules**: Self-contained units that can be added to XAF applications. Each module class (inheriting from `ModuleBase`) registers its components via `InitializeComponent()`.
- **ObjectSpace**: XAF's abstraction over data access (similar to DbContext). Created via `XPObjectSpaceProvider` for XPO persistence.
- **ModuleUpdater**: Handles database schema migrations in `DatabaseUpdate/Updater.cs`.
- **Application Model**: UI metadata stored in `.xafml` files that configures views, navigation, and layout.
- **Controllers**: Inherit from `ViewController<ViewType>` or `ObjectViewController<ViewType, ObjectType>` to add actions to views.

### Database Configuration

Connection strings are in `ComplyEA.Blazor.Server/appsettings.json`:
- Default: LocalDB SQL Server instance with database `ComplyEA`
- EasyTest: Separate database `ComplyEAEasyTest` for automated UI testing

Database schema updates are automatic when debugging (see `BlazorApplication.cs:27-29`).

## DevExpress Version

DevExpress XAF v20.2.13 - When looking up documentation or APIs, reference this version.

## Implemented Features

### Services (ComplyEA.Module/Services/)
- `IObligationGenerationService` / `ObligationGenerationService` - Generate obligations from requirements
- `IReminderGenerationService` / `ReminderGenerationService` - Create reminder schedules
- `INotificationService` / `NotificationService` - Process and send reminders
- `IEmailService` / `SmtpEmailService` - SMTP email delivery

### Controllers (ComplyEA.Module/Controllers/Compliance/)
- `ObligationGenerationController` - Generate Obligations actions
- `ObligationStatusController` - Status workflow actions
- `ReminderGenerationController` - Generate/Regenerate Reminders
- `NotificationController` - Send/Resend/Retry reminders
- `ComplianceDashboardController` - Dashboard navigation actions

### Background Jobs (ComplyEA.Blazor.Server/Services/BackgroundJobs/)
- `ReminderProcessingJob` - Automatic reminder processing (15 min interval)

## Test Users

| Username | Password | Role |
|----------|----------|------|
| Admin | ComplyEA123! | System Administrator |
| FirmAdmin | Test123! | Legal Firm Administrator |
| ComplianceOfficer | Test123! | Compliance Officer |
| CompanyUser | Test123! | Company User |

## Test Data

- **Legal Firm**: Test Law Associates
- **Companies**: ACME Corporation, Beta Finance Ltd
- **Regulatory Act**: Companies Act 2015 (Kenya)
- **Requirements**: Annual Return, AGM, Quarterly Board Meeting, Director Changes, Beneficial Ownership

## Code Patterns

### Creating a New Service
```csharp
// Interface in Services/
public interface IMyService {
    void DoSomething(IObjectSpace os, ...);
}

// Implementation in Services/
public class MyService : IMyService {
    public void DoSomething(IObjectSpace os, ...) {
        // Use os.FindObject<T>, os.CreateObject<T>, os.GetObjects<T>
    }
}

// Register in Startup.cs
services.AddScoped<IMyService, MyService>();

// Usage in controllers - instantiate directly (services are stateless)
var service = new MyService();
service.DoSomething(os, ...);
```

### Creating a New Controller with SimpleAction
```csharp
public class MyController : ObjectViewController<ListView, MyObject>
{
    private SimpleAction myAction;

    public MyController()
    {
        myAction = new SimpleAction(this, "MyAction", PredefinedCategory.RecordEdit)
        {
            Caption = "My Action",
            ImageName = "Action_Grant",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        myAction.Execute += MyAction_Execute;
    }

    private void MyAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        using (var os = Application.CreateObjectSpace(typeof(MyObject)))
        {
            var obj = os.GetObject(View.CurrentObject as MyObject);
            // Do work...
            os.CommitChanges();
        }
        Application.ShowViewStrategy.ShowMessage("Done!");
        View.ObjectSpace.Refresh();
    }
}
```

### Creating a Popup Dialog Controller
```csharp
public class MyDialogController : ViewController<ListView>
{
    private PopupWindowShowAction showDialogAction;

    public MyDialogController()
    {
        TargetObjectType = typeof(MyObject);

        showDialogAction = new PopupWindowShowAction(this, "ShowDialog", PredefinedCategory.RecordEdit)
        {
            Caption = "Open Dialog",
            ImageName = "BO_Task"
        };
        showDialogAction.CustomizePopupWindowParams += ShowDialog_CustomizePopupWindowParams;
        showDialogAction.Execute += ShowDialog_Execute;
    }

    private void ShowDialog_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
    {
        var os = Application.CreateObjectSpace(typeof(MyDialogParameters));
        var parameters = os.CreateObject<MyDialogParameters>();
        e.View = Application.CreateDetailView(os, parameters);
        e.DialogController.SaveOnAccept = false;
    }

    private void ShowDialog_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
    {
        var parameters = e.PopupWindowViewCurrentObject as MyDialogParameters;
        // Process parameters...
    }
}
```

### XPO Query Patterns
```csharp
// Find single object by code
var obj = os.FindObject<MyType>(new BinaryOperator("Code", "VALUE"));

// Find multiple objects with AND criteria
var objects = os.GetObjects<MyType>(CriteriaOperator.And(
    new BinaryOperator("IsActive", true),
    new BinaryOperator("Status.Code", "PENDING")
));

// Check for null field
var criteria = CriteriaOperator.And(criteria, new NullOperator("OptionalField"));

// Create new object
var newObj = os.CreateObject<MyType>();
newObj.Property = value;

// Get object in different ObjectSpace (required when crossing ObjectSpace boundaries)
var objInNewOs = os.GetObject(existingObject);

// Commit changes
os.CommitChanges();
```

### Lookup Seeding Pattern (in Updater.cs)
```csharp
private void CreateLookup<T>(string code, string name, string description, int sortOrder, Action<T> configure = null)
    where T : BaseLookup
{
    var existing = ObjectSpace.FindObject<T>(CriteriaOperator.Parse("Code = ?", code));
    if (existing == null)
    {
        var lookup = ObjectSpace.CreateObject<T>();
        lookup.Code = code;
        lookup.Name = name;
        lookup.Description = description;
        lookup.SortOrder = sortOrder;
        lookup.IsActive = true;
        configure?.Invoke(lookup);
    }
}
```

### Safe Date Creation (for month-end handling)
```csharp
private DateTime CreateDateSafe(int year, int month, int day)
{
    int daysInMonth = DateTime.DaysInMonth(year, month);
    int safeDay = Math.Min(day, daysInMonth);
    return new DateTime(year, month, safeDay);
}
```

## Important Notes

1. **Always use database lookup tables, never enums** for extensible reference data
2. **Services are stateless** - instantiate directly in controllers (`new MyService()`) or resolve from DI in background jobs
3. **Services use IObjectSpace, not Session directly** for proper XAF integration
4. **Always use `os.GetObject()` when crossing ObjectSpace boundaries** - objects from one ObjectSpace cannot be used directly in another
5. **Controllers inherit from ViewController or ObjectViewController** to integrate with XAF
6. **Background jobs create their own ObjectSpace instances** via XPObjectSpaceProvider
7. **Template placeholders use `{{Name}}` format** for email template processing
8. **Call `View.ObjectSpace.Refresh()` after committing changes** in a different ObjectSpace to update the UI
