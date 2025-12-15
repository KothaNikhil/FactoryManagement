# 🚀 SETUP INSTRUCTIONS

Follow these steps to get the Factory Management System up and running.

## Prerequisites Check

Before starting, ensure you have:
- [ ] Windows 10 or Windows 11
- [ ] .NET 8.0 SDK or later
- [ ] PowerShell (pre-installed on Windows)
- [ ] (Optional) Visual Studio 2022 or VS Code

## Step-by-Step Setup

### 1️⃣ Check .NET Installation

Open PowerShell and run:
```powershell
dotnet --version
```

**Expected Output**: `8.0.x` or higher

**If not installed**:
1. Download from: https://dotnet.microsoft.com/download/dotnet/8.0
2. Install the SDK (not just the runtime)
3. Restart PowerShell and verify again

### 2️⃣ Navigate to Project Directory

```powershell
cd C:\FactoryManagement
```

### 3️⃣ Restore NuGet Packages

```powershell
dotnet restore
```

**Expected**: All packages download successfully (takes 1-2 minutes first time)

### 4️⃣ Build the Project

```powershell
dotnet build
```

**Expected**: "Build succeeded" message

### 5️⃣ Run the Application

**Option A - Quick Start (Recommended)**:
```powershell
.\QuickStart.ps1
```

**Option B - Full Build**:
```powershell
.\Build-And-Run.ps1
```

**Option C - Manual**:
```powershell
dotnet run --project FactoryManagement\FactoryManagement.csproj
```

### 6️⃣ First Launch

When the application starts:
1. ✓ Database is automatically created
2. ✓ Sample data is seeded
3. ✓ Main window opens with Dashboard

**You should see**:
- Summary cards with initial values
- Navigation menu on the left
- "Factory Management System" title

## 🎯 Quick Test

To verify everything works:

1. **Check Dashboard**
   - See summary cards
   - View recent transactions (should be empty initially)

2. **Add an Item**
   - Click "Items Master" from menu
   - Click "NEW"
   - Enter: Name="Test Rice", Stock=100, Unit="Kg"
   - Click "SAVE ITEM"
   - Success message appears

3. **Add a Transaction**
   - Click "Transaction Entry"
   - Select Type: "Buy"
   - Select Item: "Test Rice"
   - Select Party: Any from dropdown
   - Enter Quantity: 50
   - Enter Price: 100
   - Click "SAVE TRANSACTION"
   - Success!

4. **View Report**
   - Click "Reports"
   - See your transaction in the grid
   - Try "Export to Excel"

## 🐛 Troubleshooting

### Problem: "dotnet: command not found"
**Solution**: Install .NET 8.0 SDK and restart PowerShell

### Problem: "Build failed"
**Solution**: 
```powershell
dotnet clean
dotnet restore
dotnet build
```

### Problem: "Database error"
**Solution**:
1. Close the application
2. Delete: `FactoryManagement\bin\Debug\net8.0-windows\factory.db`
3. Run the application again

### Problem: "Application won't start"
**Solution**:
1. Run PowerShell as Administrator
2. Try: `dotnet run --project FactoryManagement\FactoryManagement.csproj`
3. Check logs in `logs\app.log`

### Problem: "Missing packages"
**Solution**:
```powershell
dotnet restore --force
dotnet build
```

## 📁 Important Files

| File | Location | Purpose |
|------|----------|---------|
| Database | `bin\Debug\net8.0-windows\factory.db` | SQLite database |
| Logs | `logs\app.log` | Error logs |
| Config | `App.xaml` | Application settings |

## 🔄 Updating the Application

To update after code changes:
```powershell
dotnet clean
dotnet build
dotnet run --project FactoryManagement\FactoryManagement.csproj
```

## 💾 Backup

To backup your data:
1. Copy `factory.db` file
2. Store in a safe location
3. Restore by copying back to original location

## 🎓 Next Steps

1. Read **USER_GUIDE.md** for detailed usage instructions
2. Check **QUICK_REFERENCE.md** for quick tips
3. Explore all features in the application
4. Add your actual items and parties
5. Start recording transactions!

## 📞 Need Help?

1. Check **README.md** for comprehensive documentation
2. Review **PROJECT_SUMMARY.md** for technical details
3. Check logs in `logs\app.log` for errors
4. Verify all prerequisites are met

## ✅ Verification Checklist

After setup, you should be able to:
- [x] Start the application
- [x] See the Dashboard
- [x] Navigate using the menu
- [x] Add items in Items Master
- [x] Add parties in Parties Master
- [x] Record transactions
- [x] View reports
- [x] Export to Excel/CSV

## 🎉 Success!

If you can complete the Quick Test above, your Factory Management System is ready to use!

---

**Installation Time**: ~5-10 minutes  
**Difficulty**: Easy ⭐  
**Support**: Check documentation files

**Enjoy your Factory Management System!** 🏭📊
