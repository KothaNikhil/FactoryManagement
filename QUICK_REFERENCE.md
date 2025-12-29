# Factory Management System - Quick Reference

## 2025-12-29 Quick Updates
- Login required on startup; select your user after login if needed.
- Database location is `%LocalAppData%\\Factory Management\\factory.db` (not under bin).
- Reports include a unified "All" view combining Inventory, Financial, and Wages, with pagination.
- Dropdowns support type-to-search via `SearchableComboBoxBehavior`.
- Backup files are JSON stored under `Documents\\FactoryManagement\\Backups` (see Data Backup).
- Keyboard: Ctrl+1..9 for screens, Ctrl+S Save, Ctrl+N New, Escape to clear/close.

## 🚀 Quick Start
```powershell
cd C:\FactoryManagement
.\QuickStart.ps1
```

## 📊 Main Features

### Dashboard
- Purchases, Sales, Wastage summaries
- Unified recent transactions
- Low stock alerts

### New Transaction
1. Select Type: Buy/Sell/Wastage
2. Choose Item & Party
3. Enter Quantity & Price
4. Auto-calculated Total
5. Save Transaction

### Reports
- Views: All (Unified), Inventory, Financial, Wages
- Filters: Item, Party/Worker, User/Name, Date Range
- Pagination, totals, export Excel/CSV

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
| Database | `%LocalAppData%\\Factory Management\\factory.db` |
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

## ⌨️ Keyboard Shortcuts

- **Ctrl+1..9**: Navigate screens
- **Ctrl+S**: Save (when applicable)
- **Ctrl+N**: New record (when applicable)
- **Tab/Shift+Tab**: Navigate fields
- **Enter**: Activate focused button/select
- **Escape**: Clear/close dialogs

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
