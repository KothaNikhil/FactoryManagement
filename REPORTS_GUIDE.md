# Reports Module User Guide

## Overview

The Reports module provides comprehensive reporting capabilities for Factory Management. Users can generate, filter, and export transaction data across four distinct report types: All Transactions, Inventory, Financial, and Wages.

---

## Report Types

### 1. All Transactions Report
**Purpose**: Combined view of all transactions across the entire system (Inventory, Financial, and Wages).

**Available Filters**:
- **Date Range**: Filter by transaction date (Start Date - End Date)
- **User**: Filter by who entered the transaction (dropdown, "All Users" = no filter)
- **Name**: Search by unified name (Party/Worker names, "All Names" = no filter)

**Fields Displayed**:
- Transaction ID
- Category (Inventory/Financial/Wages)
- Transaction Type (Buy, Sell, Processing, etc.)
- Item Name (Inventory only)
- Party Name (Inventory/Financial only)
- Worker Name (Wages only)
- Quantity / Days/Hours Worked
- Rate
- Amount
- Debit/Credit Amount
- Transaction Date
- Notes
- Entered By

**Use Case**: Management dashboard overview, cross-functional reporting, audit trails

---

### 2. Inventory Report
**Purpose**: Detailed view of all inventory transactions (purchases, sales, processing).

**Available Filters**:
- **Date Range**: Filter by transaction date (Start Date - End Date)
- **Item**: Filter by specific item (dropdown, "All Items" = no filter)
- **Party**: Filter by supplier/customer (dropdown, "All Parties" = no filter)
- **User**: Filter by who entered the transaction (dropdown, "All Users" = no filter)

**Fields Displayed**:
- Transaction ID
- Transaction Type (Buy, Sell, Processing)
- Item Name
- Party Name (Supplier/Customer)
- Quantity
- Price Per Unit (Rate)
- Total Amount
- Debit/Credit Amount
- Transaction Date
- Notes
- Entered By

**Calculated Fields**:
- **Debit Amount**: Amount for Buy/Processing transactions
- **Credit Amount**: Amount for Sell transactions

**Use Case**:
- Inventory flow tracking
- Supplier/Customer analysis
- Stock movement verification
- Purchase/Sales reconciliation

---

### 3. Financial Report
**Purpose**: Tracking of all financial transactions (loans, payments, interest).

**Available Filters**:
- **Date Range**: Filter by transaction date (Start Date - End Date)
- **Party**: Filter by borrower/lender (dropdown, "All Parties" = no filter)
- **User**: Filter by who entered the transaction (dropdown, "All Users" = no filter)

**Fields Displayed**:
- Transaction ID
- Transaction Type (LoanGiven, LoanTaken, LoanPayment, LoanRepayment, InterestPaid, InterestReceived)
- Party Name (Borrower/Lender)
- Amount
- Interest Rate (%)
- Interest Amount
- Due Date
- Debit/Credit Amount
- Transaction Date
- Notes
- Entered By

**Calculated Fields**:
- **Debit Amount**: Amount for Loan-related debits
- **Credit Amount**: Amount for Loan-related credits
- **Interest Calculations**: Accrued based on transaction type and loan terms

**Use Case**:
- Loan tracking and management
- Interest accrual monitoring
- Payment schedule verification
- Financial health assessment

**Business Rules**:
- LoanGiven/LoanPayment: Debit (money flowing out)
- LoanTaken/LoanRepayment: Credit (money flowing in)
- InterestPaid: Debit
- InterestReceived: Credit

---

### 4. Wages Report
**Purpose**: Comprehensive tracking of all wage transactions (salaries, advances, adjustments).

**Available Filters**:
- **Date Range**: Filter by transaction date (Start Date - End Date)
- **Worker**: Filter by specific worker (dropdown, "All Workers" = no filter)
- **User**: Filter by who entered the transaction (dropdown, "All Users" = no filter)

**Fields Displayed**:
- Transaction ID
- Transaction Type (MonthlyWage, AdvanceGiven, AdvanceAdjustment, BonusGiven, OtherDeduction, OtherEarning)
- Worker Name
- Days Worked / Hours Worked (whichever is available)
- Rate (per day/hour)
- Net Amount
- Debit/Credit Amount
- Transaction Date
- Notes
- Entered By

**Calculated Fields**:
- **Quantity**: DaysWorked (fallback to HoursWorked if not available)
- **Debit Amount**: Outgoing wages (MonthlyWage, AdvanceGiven, etc.)
- **Credit Amount**: Incoming adjustments (AdvanceAdjustment reversal, other credits)

**Use Case**:
- Payroll management
- Worker attendance tracking
- Advance balance monitoring
- Labor cost analysis

---

## Filter Behavior

### Date Range Filter
- **Start Date**: Inclusive (transactions on this date are included)
- **End Date**: Inclusive (transactions on this date are included)
- **Default**: Current month (1st to last day)
- **Precision**: Full day (00:00:00 to 23:59:59)

### Selection Filters (Item, Party, Worker, User)
- **All [Type]** = No filter applied (all records shown)
- **Specific Selection** = Only matching records shown
- **None Selected** = All records shown (same as "All [Type]")

### Name Filter (All Transactions Only)
- **Unified Name**: Searches across all Party and Worker names
- **All Names** = No filter applied
- **Partial Match**: Not supported (exact match required)

---

## Pagination

All reports display **13 records per page** for optimal viewing.

**Navigation**:
- **First Page Button**: Jump to page 1
- **Previous Page**: Go back one page (disabled on page 1)
- **Page Number Input**: Enter specific page to jump to
- **Next Page**: Go forward one page (disabled on last page)
- **Last Page Button**: Jump to final page

**Display**:
- Current page shown in format: "Page X of Y"
- Total records count displayed
- "No records found" message if filter results are empty

---

## Export Functionality

### Export Formats

**Excel (.xlsx)**
- Native Excel spreadsheet format
- Formatted header row
- Data types preserved (dates, numbers)
- File naming: `Report_[Type]_[Date].xlsx`
- Recommended for: Detailed analysis, data archiving, sharing

**CSV (.csv)**
- Comma-separated values (plain text)
- Compatible with Excel, Google Sheets, databases
- All field values included
- File naming: `Report_[Type]_[Date].csv`
- Recommended for: System integration, data import, universal compatibility

### Export Behavior
- **Records Exported**: All filtered records (respects current filters and sort order)
- **Sort Order**: By Transaction Date (newest first)
- **Date/Time Fields**: ISO 8601 format (YYYY-MM-DD HH:MM:SS)
- **Numeric Fields**: Full precision preserved
- **Null Values**: Exported as empty strings (no null indicators)

### Export Naming Convention
```
Report_[ReportType]_[YYYYMMDD_HHMMSS].[Extension]
Example: Report_Inventory_20250115_143022.xlsx
```

---

## Common Workflows

### Workflow 1: Monthly Sales Review

1. **Select Report Type**: Inventory
2. **Set Date Range**: First to last day of month
3. **Apply Filters**:
   - Party: Leave as "All Parties"
   - Item: Filter to specific product line (optional)
   - User: All Users
4. **Review On Screen**: Check totals for sales (Credit Amount)
5. **Export**: Excel format for presentation

---

### Workflow 2: Loan Payment Tracking

1. **Select Report Type**: Financial
2. **Set Date Range**: Desired period
3. **Apply Filters**:
   - Party: Select specific borrower
   - User: All Users
4. **Review On Screen**: 
   - Check LoanPayment transactions
   - Verify Interest Paid amounts
   - Confirm Due Dates
5. **Export**: CSV for accounting system integration

---

### Workflow 3: Payroll Verification

1. **Select Report Type**: Wages
2. **Set Date Range**: Payment month
3. **Apply Filters**:
   - Worker: Specific worker (or all for batch report)
   - User: All Users
4. **Review On Screen**: Verify MonthlyWage entries and advances
5. **Take Action**: Correct any discrepancies
6. **Export**: Excel for payroll processing

---

### Workflow 4: Audit Trail Search

1. **Select Report Type**: All Transactions
2. **Set Date Range**: Investigation period
3. **Apply Filters**:
   - User: Filter by specific person
   - Name: Optional (if person-specific)
4. **Review On Screen**: All transactions entered by selected user
5. **Export**: CSV for audit documentation

---

## Best Practices

### For Accurate Reporting
1. **Set Consistent Dates**: Use full calendar months or fiscal periods
2. **Verify Filters**: Double-check filters before export (filter state shown at top)
3. **Check Totals**: Review calculated totals before relying on data
4. **Reconcile**: Compare report totals with source system before finalizing

### For Performance
1. **Narrow Filters**: Use specific filters (Item, Party, Worker) for faster loading
2. **Smaller Date Ranges**: Avoid exporting year-long data unnecessarily
3. **Clear Cache**: Refresh page if data seems stale

### For Data Integrity
1. **Use "Entered By" Filter**: Track who made entries for accountability
2. **Cross-Check Dates**: Verify transaction dates match intended period
3. **Verify Amounts**: Ensure debit/credit amounts are correctly calculated
4. **Document Exports**: Keep records of exported reports for audit trails

---

## Troubleshooting

### No Records Found
- **Check Filters**: Verify at least one filter permits expected records
- **Verify Date Range**: Ensure transactions fall within selected dates
- **Check Report Type**: Confirm you're on the correct report type
- **Clear Filters**: Try "All [Type]" selection to see if data exists

### Incorrect Totals
- **Verify Filters**: Some records may be filtered out
- **Check Debit/Credit**: Ensure you're viewing correct columns
- **Date Precision**: Note that times are included in date filtering

### Export Fails
- **Verify Filters**: Some filters may return no data (→ empty export)
- **Check Disk Space**: Ensure sufficient space for export file
- **Try CSV Format**: CSV is lighter-weight if Excel export fails
- **File Location**: Check Downloads folder or specified location

### Data Appears Stale
- **Refresh Page**: Press F5 or reload Reports tab
- **Reload Report**: Switch report type and return
- **Clear Application Cache**: Restart application if needed

---

## Technical Notes

### Report Type Mapping

| Report Type | Primary Source | Key Collections | Filter Options |
|---|---|---|---|
| All | UnifiedTransactionService | Unified transactions | Date, User, Name |
| Inventory | TransactionService | Inventory transactions | Date, Item, Party, User |
| Financial | FinancialTransactionService | Financial transactions | Date, Party, User |
| Wages | WageService | Wage transactions | Date, Worker, User |

### Data Consolidation
The All Transactions report consolidates data from multiple sources:
- Inventory transactions (purchases, sales, processing)
- Financial transactions (loans, payments, interest)
- Wage transactions (salaries, advances, adjustments)

Each transaction type retains its specific fields while displaying common fields (Date, Amount, Notes).

### Calculated Fields
- **Debit Amount**: Derived from transaction type and DebitCredit property
- **Credit Amount**: Inverse of Debit Amount (only one populated per transaction)
- **Quantity**: DaysWorked for wages, units for inventory
- **Rate**: PricePerUnit for inventory, daily/hourly for wages

---

## Version History

- **v1.0** (January 2025): Initial release with four report types, filtering, pagination, and export functionality
- **Service Extraction** (Phase 2): IReportExportBuilder service consolidates all export logic
- **Comprehensive Testing** (Phase 3): 13 dedicated unit tests for export mappings

---

## Support & Feedback

For issues or feature requests, please contact the development team with:
1. Report type being used
2. Filters applied
3. Date range
4. Expected vs. actual results
5. Export format (if applicable)

---

## Appendix: Field Definitions

### Common Fields

| Field | Definition | Format |
|---|---|---|
| Transaction ID | Unique identifier for the transaction | Integer or String |
| Transaction Type | Classification of transaction | Enum (Buy, Sell, LoanGiven, etc.) |
| Transaction Date | Date transaction was recorded | YYYY-MM-DD HH:MM:SS |
| Amount | Transaction total | Decimal (2 places) |
| Debit Amount | Money going out | Decimal (2 places) or null |
| Credit Amount | Money coming in | Decimal (2 places) or null |
| Notes | Transaction description | Text (max 500 chars) |
| Entered By | User who recorded transaction | Username |

### Inventory-Specific

| Field | Definition | Format |
|---|---|---|
| Item Name | Product name | Text |
| Party Name | Supplier/Customer | Text |
| Quantity | Units purchased/sold | Integer |
| Rate | Price per unit | Decimal (2 places) |

### Financial-Specific

| Field | Definition | Format |
|---|---|---|
| Party Name | Borrower/Lender | Text |
| Interest Rate | Annual percentage rate | Decimal (2 places) |
| Interest Amount | Accrued interest | Decimal (2 places) |
| Due Date | Loan payment due date | YYYY-MM-DD |

### Wages-Specific

| Field | Definition | Format |
|---|---|---|
| Worker Name | Employee name | Text |
| Days Worked | Days in pay period | Integer or null |
| Hours Worked | Hours in pay period | Integer or null |
| Rate | Daily or hourly wage rate | Decimal (2 places) |

---
