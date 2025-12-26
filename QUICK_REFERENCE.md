# Factory Management System - Quick Reference

## 🚀 Quick Start
```powershell
cd C:\FactoryManagement
.\QuickStart.ps1
```

## 📊 Main Features

### Dashboard
- Total Purchases, Sales, Wastage
- Recent Transactions (Last 10)
- Low Stock Alerts

### Transaction Entry
1. Select Type: Buy/Sell/Wastage
2. Choose Item & Party
3. Enter Quantity & Price
4. Auto-calculated Total
5. Save Transaction

### Reports
- Filter by: Item, Party, Date Range
- Export to: Excel (.xlsx), CSV
- View transaction history

### Master Data
- **Items**: Add/Edit/Delete products
- **Parties**: Manage buyers/sellers

## 🎯 Common Tasks

### Record a Purchase
Transaction Entry → Buy → Select Item & Seller → Enter Details → Save

### Record a Sale
Transaction Entry → Sell → Select Item & Buyer → Enter Details → Save

### View Item History
Reports → Filter by Item → Select Item → Apply

### Export Monthly Report
Reports → Set Date Range → Filter by Date → Export to Excel

### Add New Item
Items Management → NEW → Enter Details → Save

### Add New Party
Parties Management → NEW → Enter Details → Save

## ⚡ Quick Tips

✓ Dashboard shows real-time overview
✓ Stock updates automatically
✓ Party optional for wastage
✓ Search boxes in all master screens
✓ Export for backups
✓ Check low stock daily

## 🔧 Validation Rules

| Field | Rule |
|-------|------|
| Item | Required |
| Party | Required (except Wastage) |
| Quantity | Must be > 0 |
| Price | Cannot be negative |
| Stock | Must be sufficient for Sell/Wastage |

## 📁 Files & Locations

| Item | Location |
|------|----------|
| Database | `bin/Debug/net8.0-windows/factory.db` |
| Logs | `logs/app.log` |
| Exports | User-selected location |

## 🎨 UI Elements

| Icon | Meaning |
|------|---------|
| ☰ | Menu |
| ✏️ | Edit |
| 🗑️ | Delete |
| 📊 | Dashboard |
| 🛒 | Transaction Entry |
| 📈 | Reports |
| 📦 | Items Management |
| 👥 | Parties Management |

## ⌨️ Keyboard Shortcuts

- **Tab**: Navigate fields
- **Escape**: Clear form (in master screens)
- **Enter**: Submit (when on button)

## 🎓 Default Users

- Admin (Administrator)
- Manager (Manager)
- Operator (Operator)

## 📞 Transaction Types

1. **Buy**: Purchase from supplier (Stock ↑)
2. **Sell**: Sale to customer (Stock ↓)
3. **Wastage**: Loss/damage (Stock ↓)

## 💾 Backup Strategy

Daily: Export transaction reports
Weekly: Copy `factory.db` file
Monthly: Full database backup

Note: The app auto-maintains `DefaultBackup.json` (read-only) in Documents\FactoryManagement\Backups and shows it in Backup & Restore, but it cannot be deleted from the app.

---

**Quick Help**: Press F1 in any screen (planned feature)
**Version**: 1.0.0
