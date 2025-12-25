# Factory Management System - User Guide

## Table of Contents
1. [Getting Started](#getting-started)
2. [Dashboard Overview](#dashboard-overview)
3. [Transaction Entry](#transaction-entry)
4. [Reports & Analytics](#reports--analytics)
5. [Master Data Management](#master-data-management)
6. [Tips & Best Practices](#tips--best-practices)

---

## Getting Started

### First Launch
When you first launch the application:
1. The database will be automatically created
2. Sample data (items, parties, users) will be seeded
3. You'll see the Dashboard with initial statistics

### Navigation
Use the **hamburger menu** (☰) on the top-left to access different sections:
- **Dashboard**: Overview and quick stats
- **Transaction Entry**: Record new transactions
- **Reports**: View and export transaction data
- **Items Management**: Manage inventory items
- **Parties Management**: Manage buyers and sellers

---

## Dashboard Overview

The Dashboard provides a quick overview of your business:

### Summary Cards
1. **Total Purchases**: Sum of all buying transactions
2. **Total Sales**: Sum of all selling transactions
3. **Total Wastage**: Sum of all wastage recorded
4. **Transaction Count**: Total number of transactions

### Recent Transactions
- Shows the last 10 transactions
- Displays date, type, item, party, quantity, and amount
- Updates automatically when new transactions are added

### Low Stock Alert
- Lists items with stock below 100 units
- Sorted by current stock (lowest first)
- Helps prevent stock-outs

**Tip**: Check the dashboard daily to monitor business health!

---

## Transaction Entry

### Recording a Buy Transaction
1. Click **Transaction Entry** from the menu
2. Select **"Buy"** from Transaction Type dropdown
3. Choose the **item** you're purchasing
   - The dropdown shows current stock levels
4. Select the **party** (seller)
   - Shows name, place, and mobile number
5. Enter **quantity** and **price per unit**
   - Total amount is calculated automatically
6. Select **transaction date** (defaults to today)
7. Choose the **user** entering the data
8. Add **notes** if needed (optional)
9. Click **"SAVE TRANSACTION"**

**Result**: Stock increases, transaction is recorded

### Recording a Sell Transaction
1. Follow steps above, but select **"Sell"**
2. Choose a **buyer** as the party
3. System validates that sufficient stock is available
4. Click **"SAVE TRANSACTION"**

**Result**: Stock decreases, transaction is recorded

### Recording Wastage
1. Select **"Wastage"** as transaction type
2. **Party selection is optional** for wastage
3. Enter quantity and estimated value
4. Click **"SAVE TRANSACTION"**

**Result**: Stock decreases, wastage is tracked

### Validation Rules
- ✓ Item must be selected
- ✓ Party required for Buy/Sell (optional for Wastage)
- ✓ Quantity must be > 0
- ✓ Price cannot be negative
- ✓ Sufficient stock required for Sell/Wastage

### Quick Actions
- **CLEAR**: Reset the form
- **SAVE TRANSACTION**: Record the transaction

**Success Message**: Appears at the bottom when saved successfully

---

## Reports & Analytics

### Viewing All Transactions
1. Navigate to **Reports** from the menu
2. Default view shows all transactions
3. Total amount and count displayed at the top

### Filter by Item
1. Select an item from the **"Filter by Item"** dropdown
2. Click **"Apply"**
3. View all transactions for that specific item
4. Report title updates to show current filter

### Filter by Party
1. Select a party from the **"Filter by Party"** dropdown
2. Click **"Apply"**
3. View all transactions with that buyer/seller
4. Useful for party-wise analysis

### Filter by Date Range
1. Select **Start Date** and **End Date**
2. Click **"Filter by Date"**
3. View transactions within that period
4. Great for monthly/quarterly reports

### Reset Filters
Click **"Show All"** to return to viewing all transactions

### Export to Excel
1. Apply any filters you want (or view all)
2. Click **"Export to Excel"** button
3. Choose save location and filename
4. Excel file (.xlsx) is created with:
   - Transaction ID
   - Item Name
   - Party Name
   - Transaction Type
   - Quantity
   - Price Per Unit
   - Total Amount
   - Transaction Date
   - Entered By
   - Notes

### Export to CSV
1. Click **"Export to CSV"** button
2. Choose save location
3. CSV file is created with same data as Excel
4. Can be opened in Excel, Google Sheets, or any spreadsheet software

**Tip**: Export data regularly for backup and analysis in Excel!

---

## Master Data Management

### Items Management

#### Adding a New Item
1. Go to **Items Management**
2. Click **"NEW"** button (top-right of form)
3. Enter **Item Name** (e.g., "Basmati Rice")
4. Enter **Current Stock** (e.g., 1500)
5. Select or enter **Unit** (e.g., "Kg", "Quintal", "Ton")
6. Click **"SAVE ITEM"**

#### Editing an Item
1. Find the item in the list
2. Click the **pencil (✏️) icon**
3. Modify the details
4. Click **"SAVE ITEM"**

**Note**: Stock updates automatically through transactions

#### Deleting an Item
1. Click the **delete (🗑️) icon** next to the item
2. Confirm deletion
3. Item is removed from the system

**Warning**: Delete only if no transactions exist for this item!

#### Searching Items
- Use the search box at the top
- Search by name or unit
- Results update instantly as you type

### Parties Management

#### Adding a New Party
1. Go to **Parties Management**
2. Click **"NEW"** button
3. Enter **Party Name** (required)
4. Enter **Mobile Number** (optional)
5. Enter **Place** (location/city) (optional)
6. Select **Party Type**:
   - **Buyer**: Customer who purchases from you
   - **Seller**: Supplier who sells to you
   - **Both**: Party that both buys and sells
7. Click **"SAVE PARTY"**

#### Editing a Party
1. Find the party in the list
2. Click the **pencil icon**
3. Update details
4. Click **"SAVE PARTY"**

#### Deleting a Party
1. Click the **delete icon**
2. Confirm deletion

**Warning**: Delete only if no transactions exist for this party!

#### Searching Parties
- Search by name, place, or mobile number
- Instant results

---

## Tips & Best Practices

### Daily Operations
1. ✓ Start your day by checking the **Dashboard**
2. ✓ Record transactions as they happen
3. ✓ Review **Low Stock Alert** to plan purchases
4. ✓ Add notes to transactions for reference

### Weekly Tasks
1. ✓ Export transaction data for backup
2. ✓ Review party-wise reports
3. ✓ Verify stock levels match physical inventory
4. ✓ Clean up old or duplicate entries

### Monthly Tasks
1. ✓ Generate date-range reports for the month
2. ✓ Analyze buying and selling patterns
3. ✓ Update party information if needed
4. ✓ Backup database file

### Data Entry Best Practices
- **Be consistent** with naming (e.g., always "Rice" not "rice" or "RICE")
- **Add notes** for unusual transactions
- **Verify quantities** before saving
- **Select correct party** to avoid confusion
- **Choose correct transaction type** (Buy/Sell/Wastage)

### Stock Management
- Monitor low stock items daily
- Plan purchases before stock-outs
- Record wastage immediately for accurate inventory
- Perform physical stock verification monthly

### Reporting Tips
- Export data before making bulk changes
- Use date filters for period analysis
- Filter by item to track specific product performance
- Filter by party to manage relationships

### System Maintenance
- Keep application updated
- Backup database regularly (copy `factory.db` file)
- Clean up test/duplicate data periodically
- Log out of the application properly

---

## Keyboard Shortcuts

- **Escape**: Clear form in master data screens
- **Tab**: Navigate between form fields
- **Enter**: Submit forms (when focused on buttons)

---

## Error Messages

### "Please select an item"
- You haven't selected an item from the dropdown
- Solution: Choose an item before saving

### "Please select a party"
- Party is required for Buy/Sell transactions
- Solution: Choose a party from dropdown

### "Quantity must be greater than 0"
- Invalid quantity entered
- Solution: Enter a positive number

### "Insufficient stock available"
- You're trying to sell/waste more than available stock
- Solution: Check current stock and adjust quantity

### "Item name is required"
- Missing item name in Items Management
- Solution: Enter a name before saving

### "Party name is required"
- Missing party name in Parties Management
- Solution: Enter a name before saving

---

## Troubleshooting

### Application Won't Start
1. Check .NET 8.0 SDK is installed
2. Run as Administrator
3. Check logs folder for error details

### Database Errors
1. Close application completely
2. Check `factory.db` file isn't locked by another program
3. Try restarting the application

### Data Not Showing
1. Check filters in Reports section
2. Click "Show All" to reset filters
3. Verify data was saved (check Dashboard counts)

### Export Not Working
1. Ensure you have write permissions to the save location
2. Close the file if it's already open in Excel
3. Try saving to a different location

---

## Contact & Support

For technical issues or questions:
- Check the README.md file
- Review error logs in the `logs` folder
- Contact your system administrator

---

**Version**: 1.0.0  
**Last Updated**: December 2025

---

**Remember**: Regular backups and data verification are key to maintaining accurate inventory records!
