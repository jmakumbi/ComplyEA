# Re-download all Lucide SVG icons fresh (unmodified)
# Mapping: our filename -> Lucide icon name
$icons = @{
    # Business Objects
    'ComplianceObligation' = 'clipboard-list'
    'ComplianceReminder' = 'bell'
    'ComplianceDocument' = 'file-text'
    'ComplianceTemplate' = 'file-plus'
    'LegalFirm' = 'landmark'
    'Company' = 'briefcase'
    'CompanyContact' = 'user'
    'RegulatoryAct' = 'book-open'
    'ComplianceRequirement' = 'file-check'
    'ApplicableRegulation' = 'link'
    'ActAcronym' = 'hash'
    'TemplateCategory' = 'folder'
    'SystemConfiguration' = 'settings'
    'EmailTemplate' = 'mail'
    'CompanyReminderSettings' = 'bell-plus'
    'CompanyCalendarSettings' = 'calendar'
    'ApplicationUser' = 'circle-user-round'
    'ApplicationRole' = 'shield'
    'ObligationGenerationParameters' = 'zap'
    # Lookup types
    'BaseLookup' = 'list'
    'ActType' = 'bookmark'
    'CompanyType' = 'building'
    'ComplianceRole' = 'user-check'
    'DocumentType' = 'file'
    'DeliveryStatus' = 'send'
    'EmailProvider' = 'at-sign'
    'SMSProvider' = 'smartphone'
    'CalendarProvider' = 'calendar-days'
    'NotificationChannel' = 'bell-ring'
    'FileFormat' = 'file-type'
    'ObligationStatus' = 'activity'
    'RegulationScope' = 'globe'
    'ReminderType' = 'clock'
    'RiskRating' = 'gauge'
    'Sector' = 'layers'
    'SubscriptionType' = 'credit-card'
    'TimelineType' = 'git-branch'
    # Navigation
    'Nav_Organization' = 'building-2'
    'Nav_Regulatory' = 'scale'
    'Nav_Compliance' = 'shield-check'
    'Nav_Configuration' = 'settings'
    'Nav_Administration' = 'lock'
    # Actions
    'Action_New' = 'circle-plus'
    'Action_Save' = 'save'
    'Action_SaveAndClose' = 'square-check-big'
    'Action_Edit' = 'pencil'
    'Action_Delete' = 'trash-2'
    'Action_Refresh' = 'refresh-cw'
    'Action_Cancel' = 'x'
    'Action_Close' = 'log-out'
    'Action_Filter' = 'list-filter'
    'Action_Search' = 'search'
    'Action_Export' = 'download'
    'Action_Print' = 'printer'
    # Status
    'Status_Overdue' = 'circle-alert'
    'Status_Upcoming' = 'calendar-clock'
    'Status_Completed' = 'circle-check-big'
    'Status_Pending' = 'hourglass'
    'Status_Failed' = 'circle-x'
}

$dest = Join-Path $PSScriptRoot '..\ComplyEA.Module\Images'
$baseUrl = 'https://raw.githubusercontent.com/lucide-icons/lucide/main/icons'
$success = 0
$failed = @()

foreach ($entry in $icons.GetEnumerator()) {
    $fileName = $entry.Key
    $lucideName = $entry.Value
    $url = "$baseUrl/$lucideName.svg"
    $outFile = Join-Path $dest "$fileName.svg"
    try {
        Invoke-WebRequest -Uri $url -OutFile $outFile -ErrorAction Stop
        $success++
        Write-Host "OK: $fileName <- $lucideName"
    } catch {
        $failed += "$fileName ($lucideName): $($_.Exception.Message)"
        Write-Host "FAIL: $fileName <- $lucideName"
    }
}

Write-Host "`nDownloaded $success / $($icons.Count) icons"
if ($failed.Count -gt 0) {
    Write-Host "`nFailed downloads:"
    $failed | ForEach-Object { Write-Host "  $_" }
}
