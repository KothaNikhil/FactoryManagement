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

### New Transaction
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
- **Inventory**: Add/Edit/Delete products
- **Contacts**: Manage buyers/sellers
- **Users**: Manage system users and access

## 🎯 Common Tasks

### Record a Purchase
New Transaction → Buy → Select Item & Seller → Enter Details → Save

### Record a Sale
New Transaction → Sell → Select Item & Buyer → Enter Details → Save

### View Item History
Reports → Filter by Item → Select Item → Apply

### Export Monthly Report
Reports → Set Date Range → Filter by Date → Export to Excel

### Add New Item
Inventory → NEW → Enter Details → Save

### Add New Party
Contacts → NEW → Enter Details → Save

### Add New User
Users → NEW USER → Enter Username & Role → Save

### Filter by User in Reports
Reports → Select "Entered By" dropdown → Choose User → View

## ⚡ Quick Tips

✓ **Always select your user from header dropdown before entering data**
✓ Dashboard shows real-time overview
✓ Stock updates automatically
✓ Party optional for wastage
✓ Search boxes in all master screens
✓ Export for backups
✓ Check low stock daily
✓ Filter reports by "Entered By" to track user activity

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
| 🛒 | New Transaction |
| 📈 | Reports |
| 📦 | Inventory |
| 👥 | Contacts |
| 👤 | Users |
| 👨‍💼 | User Selection (Header) |

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

---

**Quick Help**: Press F1 in any screen (planned feature)
**Version**: 1.0.0
