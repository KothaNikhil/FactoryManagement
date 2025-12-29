# Financial Records User Guide

## Overview

The Financial Records module manages loans, payments, interest accrual, and loan accounts. It tracks both loans given to parties and loans taken from parties, with automatic interest calculations and comprehensive payment management.

### Navigation
- **Menu**: Financial Records (shortcut key available)
- **View**: Split layout with loans on left, transactions on right
- **Main Tabs**: Loans, Transactions

---

## Key Features

### 1. Loan Management

#### Creating a New Loan

**Step-by-step**:
1. Navigate to Financial Records module
2. Enter loan details in the form:
   - **Loan Type**: Select "Given" (money lent) or "Taken" (money borrowed)
   - **Party**: Select the party using searchable dropdown
   - **Original Amount**: Principal amount (required)
   - **Interest Rate**: Annual percentage rate (e.g., 5.5 for 5.5%)
   - **Start Date**: When the loan begins
   - **Start Time** (optional): Specific time of loan creation
   - **Due Date** (optional): When the loan should be fully repaid
   - **Payment Mode**: Cash or Bank
   - **Notes** (optional): Additional details
3. Click **CREATE LOAN**
4. System confirms: "✓ Loan created successfully"

**What Happens**:
- Loan account created with status "Active"
- Outstanding principal set to original amount
- Outstanding interest set to 0
- Initial financial transaction recorded
- Loan ready for payments and interest accrual

#### Viewing Loans

**List Display**:
- All active loans sorted by creation date
- Filter by party using dropdown
- Shows: Party Name, Loan Type, Principal, Outstanding, Status, Due Date

**Loan Details**:
- Click loan to view related transactions
- Right panel shows transaction history
- All payments and interest accruals visible

**Loan Status Values**:
- **Active**: Principal is fully outstanding
- **PartiallyPaid**: Some payments have been made
- **Overdue**: Due date has passed and loan still outstanding
- **Closed**: Fully paid

---

### 2. Interest Management

#### Understanding Interest Calculation

**How Interest Works**:
- **Calculation Method**: Simple interest (Principal × Rate × Time ÷ 100)
- **Time Unit**: Days (automatically calculated)
- **Formula**: (Outstanding Principal × Interest Rate × Days) ÷ (365 × 100)

**Example**:
- Principal: 10,000
- Rate: 5% per year
- Days since last calculation: 30
- Interest = (10,000 × 5 × 30) ÷ (365 × 100) = 41.10

#### Accruing Interest

**Automatic vs Manual**:
- **Automatic**: Interest automatically accrued when recording payment (if not calculated today)
- **Manual**: Click UPDATE INTEREST to force recalculation (once per day only)

**Interest Accrual Rules**:
1. Can be calculated once per day maximum
2. Only accrues if outstanding principal > 0
3. Interest rate must be > 0
4. Accrued interest is tracked separately from principal

**What Happens After Interest Accrual**:
- New financial transaction created (type: Interest Received/Paid)
- Outstanding interest amount increased
- Total outstanding updated
- Loan status may change to "Overdue" if past due date

---

### 3. Payment Recording

#### Recording a Payment

**Step-by-step**:
1. Select a loan from the list
2. Enter payment details:
   - **Payment Amount**: Must be ≤ Total Outstanding
   - **Payment Mode**: Cash or Bank
   - **Payment Notes** (optional): Memo or reference number
3. Interest automatically calculated if needed (if not done today)
4. Click **RECORD PAYMENT**
5. System confirms: "✓ Payment recorded successfully"

**Payment Allocation**:
Payments are allocated in this order:
1. **Interest First**: Any outstanding interest is paid first
2. **Principal Second**: Remaining amount reduces principal

**Example**:
- Outstanding Principal: 9,000
- Outstanding Interest: 50
- Payment: 1,000
  - Interest payment: 50 (interest now: 0)
  - Principal payment: 950 (principal now: 8,050)
  - Total outstanding now: 8,050

#### Automatic Loan Status Updates

**Status Changes** after payment:
- **Active → PartiallyPaid**: When total outstanding < original principal
- **PartiallyPaid → Closed**: When total outstanding ≤ 0
- **Active → Overdue**: When due date passed (on interest calculation)
- **Overdue → PartiallyPaid/Closed**: When payment made

---

### 4. Transaction Management

#### Transaction Types

| Type | Description | Cash Flow | Notes |
|------|-------------|-----------|-------|
| Loan Given | Initial loan disbursement | Yes | Credit to party |
| Loan Taken | Initial loan received | Yes | Debit from party |
| Loan Repayment | Payment received on loan given | Yes | Cash in |
| Loan Payment | Payment made on loan taken | Yes | Cash out |
| Interest Received | Interest accrued on loan given | No | Not yet received |
| Interest Paid | Interest accrued on loan taken | No | Not yet paid |

#### Viewing Transactions

**Transaction List**:
- Shows all transactions for selected loan
- Columns: Date, Type, Amount, Interest Amount, Notes, Status
- Most recent first

**Transaction Details**:
- Click to view full details
- Shows party, amounts, dates, and linked loan
- Edit or delete options available

#### Deleting Transactions

**Allowed Deletions**:
- Any transaction can be deleted (with confirmation)
- Cannot delete loan itself if it has transactions (use individual deletion)

**What Happens When Deleted**:
- **Interest Transaction**: Interest amount reversed from outstanding
- **Payment Transaction**: Payment amount re-added to outstanding principal
- Loan status recalculated based on new totals

**Undo Feature**:
- "Undo Delete" appears in snackbar after deletion
- Click within 4 seconds to restore deleted transaction

---

## Workflows

### Complete Loan Workflow

**Scenario**: Give loan to party, accrue interest, receive partial payment, close loan

**Steps**:
1. **Create Loan**
   - Party: "ABC Trading"
   - Type: "Given"
   - Amount: 50,000
   - Rate: 6%
   - Due Date: 6 months from now

2. **Wait Period** (e.g., 30 days)
   - Interest accrues automatically when payment is recorded
   - Or manually via UPDATE INTEREST

3. **First Payment** (Day 30)
   - Pay: 10,000
   - Interest calculated: (50,000 × 6 × 30) ÷ (365 × 100) = 247.95
   - Payment allocated: 247.95 interest + 9,752.05 principal
   - Outstanding: 40,247.95
   - Status: PartiallyPaid

4. **Second Payment** (Day 60)
   - Interest calculated since day 30: (40,247.95 × 6 × 30) ÷ (365 × 100) = 198.45
   - Pay: 20,000
   - Payment allocated: 198.45 interest + 19,801.55 principal
   - Outstanding: 20,446.40
   - Status: PartiallyPaid

5. **Final Payment** (Day 90)
   - Interest calculated: (20,446.40 × 6 × 30) ÷ (365 × 100) = 100.42
   - Pay: 20,546.82 (principal + interest)
   - Outstanding: 0
   - Status: Closed

---

### Interest-Only Loan Workflow

**Scenario**: Receive interest payments before principal repayment

**Steps**:
1. Create loan taken from party
2. Update interest calculation (monthly)
3. Record interest payment (without principal reduction)
4. Later: Record principal payment
5. Finally: Close loan

---

## Validation Rules

### Loan Creation Validation
- ✓ Party must be selected
- ✓ Original amount must be > 0
- ✓ Interest rate can be 0 (interest-free loans allowed)
- ✓ Due date can be empty (no due date)
- ✓ Start date required

### Payment Recording Validation
- ✓ Loan must exist and not be closed
- ✓ Payment amount must be > 0
- ✓ Payment amount must be ≤ total outstanding
- ✓ Cannot exceed total outstanding (interest + principal)

### Interest Calculation Validation
- ✓ Loan must not be closed
- ✓ Can only be calculated once per day
- ✓ Outstanding principal must be > 0
- ✓ Interest rate must be > 0

---

## Common Scenarios & Troubleshooting

### "Cannot record payment for closed loan"
**Cause**: Loan has been fully paid and closed
**Solution**: Create a new loan if additional credit needed

### "Payment amount exceeds outstanding amount"
**Cause**: Payment > Total Outstanding
**Solution**: Reduce payment amount to ≤ total outstanding, or pay exact final amount

### "No interest to calculate. Interest can only be calculated once per day"
**Cause**: Interest already calculated today
**Solution**: Try again tomorrow, or wait if recording payment (auto-accrues)

### "Outstanding principal is zero or interest rate is zero"
**Cause**: No principal remaining, or interest rate = 0
**Solution**: For zero principal: loan already closed. For zero rate: no interest to accrue (normal for interest-free loans)

### Loan Status Not Updating
**Cause**: Payment not fully recorded
**Solution**: Refresh list (click refresh button or select different loan)

### Transaction Appears to Show Duplicate Interest
**Cause**: Interest shown both in transaction AND in outstanding interest total
**Solution**: This is correct - interest amount is both accrued AND awaiting payment

---

## Tips & Best Practices

### Interest Rate Entry
- Enter rates as whole numbers (5 for 5%, not 0.05)
- Rates can include decimals (5.5 for 5.5%)
- Zero rate allowed for interest-free loans

### Due Dates
- Optionally set due date for accountability
- System marks loan "Overdue" when past due date
- Helps track loan terms

### Payment Modes
- Select based on actual receipt: "Cash" or "Bank"
- Used for financial reporting and categorization
- Can filter/analyze by mode

### Notes Field
- Include reference numbers (check number, transfer ID)
- Add reminders (next review date, special terms)
- Note any partial or advance payments

### Periodic Reviews
- Review loans monthly for overdue status
- Calculate interest regularly to keep balances current
- Monitor outstanding amounts to predict cash flow

---

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+F | Filter loans by party |
| Ctrl+R | Refresh loan list |
| Ctrl+I | Update interest for selected loan |
| Del | Delete selected transaction |
| Enter | Record payment (when form focused) |

---

## Related Information

- See [USER_GUIDE.md](USER_GUIDE.md) for general application overview
- See [PROJECT_SUMMARY.md](PROJECT_SUMMARY.md) for feature status and architecture

