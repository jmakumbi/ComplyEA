# ComplyEA End-to-End Test Scenarios

## Test Environment Setup

### Test Users

| Username | Password | Role | Description |
|----------|----------|------|-------------|
| Admin | ComplyEA123! | System Administrator | Full system access |
| FirmAdmin | Test123! | Legal Firm Administrator | Manages firm and clients |
| ComplianceOfficer | Test123! | Compliance Officer | Manages compliance tasks |
| CompanyUser | Test123! | Company User | View-only access |

### Test Data

**Legal Firm:** Test Law Associates
- Subscription: Professional
- Status: Active

**Test Companies:**

| Company | Short Name | Type | Sector | Registration |
|---------|------------|------|--------|--------------|
| ACME Corporation | ACME | Private Limited | Technology | CPR/2020/123456 |
| Beta Finance Ltd | BFL | Private Limited | Financial Services | CPR/2019/789012 |

**Test Contacts:**

| Company | Contact | Email | Role |
|---------|---------|-------|------|
| ACME Corporation | Alice Johnson | alice@acmecorp.com | Compliance Officer |
| Beta Finance Ltd | Bob Kamau | bob.kamau@betafinance.co.ke | CFO |

---

## Test Scenarios

### Scenario 1: User Authentication

**Objective:** Verify user login and role-based access

#### Steps:
1. Navigate to https://localhost:5001
2. Login with username `Admin` and password `ComplyEA123!`
3. Verify dashboard loads with full navigation menu
4. Logout and login as `CompanyUser` with password `Test123!`
5. Verify limited menu options (read-only access)

#### Expected Results:
- Admin sees: All navigation items including Administration
- CompanyUser sees: Limited menu, no Administration section
- Invalid credentials show appropriate error message

---

### Scenario 2: Company Management

**Objective:** Verify company CRUD operations

#### Prerequisites:
- Login as `FirmAdmin`

#### Steps:
1. Navigate to **Organization > Companies**
2. Verify ACME Corporation and Beta Finance Ltd are listed
3. Click on ACME Corporation to view details
4. Edit the phone number and save
5. Create a new company "Gamma Industries"
6. Delete the test company

#### Expected Results:
- Company list shows all active companies
- Edit saves successfully with audit timestamp
- New company appears in the list
- Delete removes company from active list

---

### Scenario 3: Obligation Generation

**Objective:** Verify automatic obligation generation from requirements

#### Prerequisites:
- Login as `ComplianceOfficer`

#### Steps:
1. Navigate to **Organization > Companies**
2. Select ACME Corporation
3. Click **"Generate Obligations"** action
4. In the popup, set:
   - Year: Current year
   - Quarter: (leave empty for all)
   - Include Annual: Yes
   - Include Quarterly: Yes
5. Click OK/Accept
6. Navigate to **Compliance > Compliance Obligations**
7. Filter by Company = ACME Corporation

#### Expected Results:
- Message shows "Successfully generated X compliance obligation(s)"
- Obligations list shows new records with:
  - Correct due dates based on timeline type
  - Status = Pending
  - Period Year set correctly
- Annual obligations have year-end due dates
- Quarterly obligations have quarter-end due dates

---

### Scenario 4: Obligation Status Workflow

**Objective:** Verify obligation status transitions

#### Prerequisites:
- Login as `ComplianceOfficer`
- Have existing pending obligation

#### Steps:
1. Navigate to **Compliance > Compliance Obligations**
2. Select a Pending obligation
3. Click **"Change Status"** > **"In Progress"**
4. Verify status changes
5. Click **"Change Status"** > **"Submitted"**
6. Click **"Mark Complete"**
7. Verify CompletedDate is set

#### Expected Results:
- Status transitions: Pending → In Progress → Submitted → Completed
- CompletedDate auto-populates when marking complete
- Status badge/icon updates appropriately

---

### Scenario 5: Mark Overdue Obligations

**Objective:** Verify bulk marking of overdue obligations

#### Prerequisites:
- Login as `ComplianceOfficer`
- Have obligations with DueDate in the past

#### Steps:
1. Navigate to **Compliance > Compliance Obligations**
2. Note any obligations past due date with Pending/In Progress status
3. Click **"Mark Overdue"** action
4. Verify obligations are updated

#### Expected Results:
- All past-due, non-terminal obligations change to "Overdue" status
- Message shows count of updated obligations
- Overdue obligations visible in dashboard "Overdue Obligations" view

---

### Scenario 6: Reminder Generation

**Objective:** Verify reminder scheduling for obligations

#### Prerequisites:
- Login as `ComplianceOfficer`
- Have pending obligation with future due date

#### Steps:
1. Navigate to **Compliance > Compliance Obligations**
2. Select an obligation with due date 30+ days in future
3. Click **"Generate Reminders"**
4. Navigate to **Compliance > Compliance Reminders**
5. Filter by the selected obligation

#### Expected Results:
- Reminders created for each reminder type:
  - Initial: 30 days before due
  - First Follow-up: 14 days before
  - Second Follow-up: 7 days before
  - Final Notice: 3 days before
  - Escalation: 1 day before (if enabled)
- All reminders have:
  - DeliveryStatus = Pending
  - ScheduledDate correctly calculated
  - RecipientEmail populated

---

### Scenario 7: Regenerate Reminders

**Objective:** Verify reminder regeneration when due date changes

#### Prerequisites:
- Login as `ComplianceOfficer`
- Have obligation with existing reminders

#### Steps:
1. Navigate to obligation detail view
2. Change the DueDate to a later date
3. Save the obligation
4. Click **"Regenerate Reminders"**
5. Confirm the action
6. Check reminders list

#### Expected Results:
- Old unsent reminders are deleted
- New reminders created with updated schedule
- Already sent reminders are preserved

---

### Scenario 8: Send Reminder (Manual)

**Objective:** Verify manual reminder sending

#### Prerequisites:
- Login as `ComplianceOfficer`
- Have pending reminder with valid email
- SMTP configured in System Configuration

#### Steps:
1. Navigate to **Compliance > Compliance Reminders**
2. Select a pending reminder
3. Click **"Preview Message"**
4. Verify MessageSubject and MessageBody are populated
5. Click **"Send Now"**
6. Check delivery status

#### Expected Results:
- Preview populates message with placeholders replaced
- Send attempts email delivery
- Success: DeliveryStatus = Sent, SentDate populated
- Failure: DeliveryStatus = Failed, ErrorMessage shows reason

---

### Scenario 9: Process Due Reminders (Bulk)

**Objective:** Verify bulk processing of due reminders

#### Prerequisites:
- Login as `ComplianceOfficer`
- Have multiple reminders with ScheduledDate <= today

#### Steps:
1. Navigate to **Compliance > Compliance Reminders**
2. Click **"Send Due Reminders"**
3. Wait for processing
4. Review results message

#### Expected Results:
- Message shows: "Processed X reminders: Y sent, Z failed"
- Due reminders attempted
- Statuses updated appropriately
- RetryCount incremented for failures

---

### Scenario 10: Retry Failed Reminders

**Objective:** Verify retry mechanism for failed reminders

#### Prerequisites:
- Have failed reminders with RetryCount < 3

#### Steps:
1. Navigate to **Compliance > Compliance Reminders**
2. Filter by DeliveryStatus = Failed
3. Click **"Retry Failed"**
4. Check updated statuses

#### Expected Results:
- Failed reminders re-attempted
- RetryCount incremented
- Reminders exceeding MaxRetries not retried
- Success message shows count

---

### Scenario 11: Dashboard Navigation

**Objective:** Verify dashboard quick-access actions

#### Prerequisites:
- Login as `ComplianceOfficer`
- Have various obligations in different states

#### Steps:
1. From any view, access the Dashboard menu
2. Click **"Overdue Obligations"**
3. Verify filtered view shows only overdue items
4. Click **"Upcoming Deadlines"**
5. Verify shows obligations due in next 30 days
6. Click **"Pending Reminders"**
7. Verify shows unsent reminders
8. Click **"All Obligations"**
9. Verify unfiltered list

#### Expected Results:
- Each action navigates to correctly filtered list view
- Captions reflect the filter applied
- Back navigation works correctly

---

### Scenario 12: Email Template Processing

**Objective:** Verify placeholder replacement in email templates

#### Prerequisites:
- Have reminder with associated obligation, company, requirement

#### Steps:
1. Navigate to a reminder detail view
2. Click **"Preview Message"**
3. Review MessageSubject and MessageBody

#### Expected Results:
Placeholders replaced correctly:
- `{{CompanyName}}` → "ACME Corporation"
- `{{RequirementTitle}}` → "Annual Return Filing"
- `{{DueDate}}` → "December 31, 2024" (formatted)
- `{{DaysUntilDue}}` → Calculated days
- `{{RecipientFirstName}}` → Contact first name
- `{{RegulatoryAct}}` → "Companies Act"
- `{{ObligationStatus}}` → Current status name

---

### Scenario 13: Company Reminder Settings

**Objective:** Verify custom reminder settings per company

#### Prerequisites:
- Login as `FirmAdmin`

#### Steps:
1. Navigate to **Configuration > Company Reminder Settings**
2. Create new settings for ACME Corporation:
   - InitialReminderDays: 45 (instead of 30)
   - FirstReminderDays: 21 (instead of 14)
   - EscalateToManager: Yes
   - EscalationContact: Select contact
3. Save settings
4. Generate reminders for an ACME obligation
5. Verify reminder schedule uses custom settings

#### Expected Results:
- Custom settings override defaults
- Initial reminder scheduled 45 days before
- First reminder scheduled 21 days before
- Escalation reminder created when enabled

---

### Scenario 14: System Configuration

**Objective:** Verify system configuration management

#### Prerequisites:
- Login as `Admin`

#### Steps:
1. Navigate to **Administration > System Configuration**
2. Update Email.Smtp.Host to your SMTP server
3. Update Email.Smtp.Username and Password
4. Update Email.From.Address
5. Save changes
6. Navigate to a reminder and test send

#### Expected Results:
- Configuration changes saved
- Email sending uses new configuration
- Encrypted fields (password) not displayed in plain text

---

### Scenario 15: Background Job Processing

**Objective:** Verify automatic reminder processing

#### Prerequisites:
- Application running
- Due reminders exist
- SMTP configured

#### Steps:
1. Check application logs for "Reminder Processing Job started"
2. Create a reminder with ScheduledDate = today
3. Wait for processing interval (default 15 minutes)
4. Check reminder status

#### Expected Results:
- Job runs automatically at configured interval
- Due reminders processed
- Log shows processing results
- Job respects Reminders.Processing.Enabled setting

---

## Error Scenarios

### Scenario E1: Invalid Email Configuration

**Steps:**
1. Set invalid SMTP host in configuration
2. Attempt to send reminder

**Expected:** Reminder marked as Failed with appropriate error message

### Scenario E2: Missing Recipient Email

**Steps:**
1. Create obligation with no AssignedTo contact
2. Company has no email
3. Generate and send reminder

**Expected:** Reminder marked as Failed with "No recipient email address configured"

### Scenario E3: Duplicate Obligation Prevention

**Steps:**
1. Generate obligations for a period
2. Run generation again for same period

**Expected:** No duplicates created, message indicates 0 new obligations

---

## Performance Scenarios

### Scenario P1: Bulk Obligation Generation

**Steps:**
1. Create 50 test companies
2. Apply Companies Act to all
3. Generate obligations for full year

**Expected:**
- Process completes within reasonable time
- All obligations created correctly
- No timeout errors

### Scenario P2: Large Reminder Batch

**Steps:**
1. Create 100+ pending reminders
2. Run "Send Due Reminders"

**Expected:**
- All reminders processed
- Accurate count in result message
- Database commits successful

---

## Verification Checklist

- [ ] All test users can login successfully
- [ ] Role-based access control works correctly
- [ ] Obligation generation creates correct records
- [ ] Due date calculation accurate for all timeline types
- [ ] Status transitions work correctly
- [ ] Reminder generation follows settings
- [ ] Email templates render correctly
- [ ] SMTP sending works (when configured)
- [ ] Background job runs automatically
- [ ] Dashboard filters show correct data
- [ ] Error handling provides useful messages
- [ ] Audit fields (CreatedOn, LastModifiedOn) populate correctly
