# New Transaction Page - User Guide

## Overview
The **New Transaction** page is the primary entry point for recording all inventory movements (purchases, sales, wastage) and service transactions (processing). The page supports both creating new transactions and editing existing ones.

## Navigation
- **Keyboard Shortcut**: `Ctrl+2`
- **Menu Path**: Main Menu → New Transaction

## Transaction Types

### 1. Buy
Record material purchases from suppliers or external sources.
- **Fields**: Item, Quantity, Price Per Unit, Supplier (Party), Date, Payment Mode, Notes
- **Stock Impact**: Increases inventory for the selected item
- **Requirements**: Item and Supplier party are mandatory

### 2. Sell
Record sales or deliveries to customers.
- **Fields**: Item, Quantity, Price Per Unit, Buyer (Party), Date, Payment Mode, Notes
- **Stock Impact**: Decreases inventory for the selected item (sufficient stock required)
- **Requirements**: Item and Buyer party are mandatory

### 3. Wastage
Record material loss, spoilage, or waste.
- **Fields**: Item, Quantity, Cost Estimate, Date, Notes
- **Stock Impact**: Decreases inventory for the selected item
- **Requirements**: Item is mandatory; party is optional

### 4. Processing
Record value-added processing or transformation of materials (service-only; no inventory impact).
- **Fields**: Output Item, Output Quantity, Input Material (Item), Input Quantity, Date, Notes
- **Stock Impact**: None (service records only)
- **Requirements**: Output Item, Input Material, and both quantities are mandatory

## Form Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| **Transaction Type** | Dropdown | Yes | Buy, Sell, Wastage, Processing |
| **Item** | Searchable Dropdown | Yes | Output item for processing; primary item for others |
| **Quantity** | Decimal | Yes | Must be > 0 |
| **Price Per Unit** | Decimal | Yes | ≥ 0; auto-calculated for some types |
| **Party** | Searchable Dropdown | Conditional | Required for Buy/Sell; optional for Wastage |
| **Input Material** | Searchable Dropdown | Yes (Processing) | Only visible in Processing mode |
| **Input Quantity** | Decimal | Yes (Processing) | Only visible in Processing mode |
| **Payment Mode** | Dropdown | Yes | Cash or Bank; default is Cash |
| **Date** | Date Picker | Yes | Defaults to today |
| **Time** | Time Picker | No | Defaults to current time |
| **Notes** | Text | No | Up to 500 characters |

## Key Features

### Auto-Calculation
- **Total Amount** = Quantity × Price Per Unit (automatic on input)
- Visible in the summary section after entering quantity and price

### Searchable Dropdowns
- Item and Party fields support real-time search
- Start typing to filter dropdown options
- Works with keyboard navigation

### Edit Mode
1. Click the **Edit** button on any row in the "Recent Transactions" grid
2. Form fields populate with transaction data
3. Button changes to **UPDATE TRANSACTION**
4. Modify fields and click UPDATE to save changes
5. Stock adjustments are handled automatically

### Recent Transactions Grid
- Displays last 100 transactions in reverse chronological order
- Shows: Item, Party, Type, Quantity, Price, Amount, Date, User
- Actions: **Edit**, **Delete**, **Undo Delete**

### Form Actions
- **SAVE TRANSACTION**: Create a new transaction
- **UPDATE TRANSACTION**: Update existing transaction (in edit mode)
- **CLEAR FORM**: Reset all fields to defaults

### Stock Validation
- **Sell/Wastage**: System checks available stock before saving
- **In Edit Mode**: Calculates required stock as (new quantity - old quantity)
- **Error Message**: "Insufficient stock available" if validation fails

## Workflow Examples

### Recording a Material Purchase
1. Select **Transaction Type** → "Buy"
2. Select **Item** (e.g., Rice)
3. Enter **Quantity** (e.g., 500)
4. Enter **Price Per Unit** (e.g., 50)
5. Select **Party** (supplier name)
6. Choose **Payment Mode** (Cash/Bank)
7. Click **SAVE TRANSACTION**
8. Success message appears; form clears automatically

### Recording a Sale
1. Select **Transaction Type** → "Sell"
2. Select **Item** (e.g., Rice)
3. Enter **Quantity** (must not exceed available stock)
4. Enter **Price Per Unit**
5. Select **Party** (customer name)
6. (Optional) Add **Notes** (e.g., Invoice #12345)
7. Click **SAVE TRANSACTION**

### Recording Material Loss/Wastage
1. Select **Transaction Type** → "Wastage"
2. Select **Item**
3. Enter **Quantity** lost
4. Enter **Cost Estimate** (value of loss)
5. Leave **Party** empty (optional for wastage)
6. Click **SAVE TRANSACTION**

### Recording Processing
1. Select **Transaction Type** → "Processing"
2. Form expands to show processing fields
3. Select **Input Material** (e.g., Raw Rice)
4. Enter **Input Quantity** (e.g., 100 kg)
5. Select **Output Item** (e.g., Milled Rice)
6. Enter **Output Quantity** (e.g., 85 kg)
7. Add notes (e.g., milling loss 15 kg)
8. Click **SAVE TRANSACTION**

### Editing a Transaction
1. Find transaction in the "Recent Transactions" grid
2. Click **Edit** button
3. Form title changes to "Edit Transaction"
4. Modify desired fields
5. Button changes to **UPDATE TRANSACTION**
6. Click UPDATE
7. System automatically adjusts stock for old and new values

### Deleting a Transaction
1. Find transaction in the "Recent Transactions" grid
2. Click **Delete** button
3. Confirm deletion in dialog
4. Snackbar message appears with **UNDO** option
5. Click UNDO within timeout period to restore transaction
6. Stock is automatically reversed on delete/undo

## Validation Rules

| Condition | Error Message |
|-----------|---------------|
| No item selected | "Please select an item" |
| Party required but not selected | "Please select a party" |
| Quantity ≤ 0 | "Quantity must be greater than 0" |
| Price < 0 | "Price per unit cannot be negative" |
| Insufficient stock for Sell/Wastage | "Insufficient stock available" |
| No user selected (header) | "Please select a user from the header" |
| Processing: No input material | "Please select input material" |
| Processing: Input quantity ≤ 0 | "Input quantity must be greater than zero" |
| Processing: Input & output same | "Input and output items must be different" |

## Keyboard Navigation

| Shortcut | Action |
|----------|--------|
| `Ctrl+2` | Open New Transaction |
| `Tab` | Navigate to next field |
| `Shift+Tab` | Navigate to previous field |
| `Enter` | Submit form (when in button focus) |
| `Escape` | Clear focus from dropdown |

## Stock Management

### How Stock Is Updated
1. **Buy**: Stock increases immediately upon save
2. **Sell**: Stock decreases immediately upon save (with validation)
3. **Wastage**: Stock decreases immediately upon save
4. **Processing**: Stock is unchanged (service-only record)

### Editing Impact
- **Reversing Old**: If quantity decreases, stock is restored; if increases, stock is reduced further
- **Stock Safety**: Transaction only saves if stock is sufficient (for Sell/Wastage)

### Undo Mechanism
- **Delete**: Creates reverse transaction; undo re-applies original transaction
- **Stock Restoration**: Stock adjustments are automatically reversed on delete

## Payment Modes
- **Cash**: Physical cash payment
- **Bank**: Bank transfer or check payment
- Stored for reporting and cash flow analysis

## Error Handling
- **Network Errors**: "Error saving transaction" with detailed message
- **Data Validation**: Field-level errors shown inline
- **Stock Issues**: "Insufficient stock available" prevents save
- **Missing Transaction**: "Transaction not found" if record is deleted during edit

## Tips & Best Practices
1. **Always verify stock** before recording sales/wastage
2. **Use meaningful notes** for future reference (invoice #, lot #, etc.)
3. **Select correct payment mode** for accurate cash flow tracking
4. **Check dates** especially when entering backdated transactions
5. **Undo available briefly** if you delete by mistake; use UNDO quickly
6. **Use searchable dropdowns** to quickly find items/parties
7. **For processing**, ensure input/output items are different and meaningful

## Troubleshooting

### Form Not Saving
- **Check**: All required fields are filled
- **Check**: Stock is sufficient for Sell/Wastage
- **Check**: A user is selected in the header
- **Check**: Network connection is active

### Stock Number Doesn't Match
- **Issue**: Multiple transactions recorded in same session
- **Solution**: Refresh Recent Transactions grid (LoadData)
- **Check**: Edit/Delete operations were applied correctly

### Cannot Find Item/Party
- **Solution**: Use Searchable Dropdown to search by name
- **Check**: Item/Party exists in the system
- **Add New**: Contact admin to create missing items/parties

### Edit Button Not Working
- **Issue**: Transaction may have been deleted
- **Solution**: Refresh the page (press F5 or reload)
- **Check**: Recent Transactions grid is fully loaded

## Related Topics
- [Inventory Management](INVENTORY_MANAGEMENT.md)
- [Reports & Analytics](USER_GUIDE.md#Reports)
- [Processing Feature](PROCESSING_FEATURE_DESIGN.md)
- [Stock Tracking](USER_GUIDE.md#Inventory)
