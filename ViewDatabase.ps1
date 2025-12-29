# View Database Information Script
# This script helps you locate and view basic information about the Factory Management database

$dbPath = "$env:LOCALAPPDATA\Factory Management\factory.db"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Factory Management Database Information" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if (Test-Path $dbPath) {
    Write-Host "Database Location:" -ForegroundColor Green
    Write-Host $dbPath
    Write-Host ""
    
    $fileInfo = Get-Item $dbPath
    Write-Host "Database File Size:" -ForegroundColor Green
    Write-Host "$([Math]::Round($fileInfo.Length / 1KB, 2)) KB"
    Write-Host ""
    
    Write-Host "Last Modified:" -ForegroundColor Green
    Write-Host $fileInfo.LastWriteTime
    Write-Host ""
    
    Write-Host "To view the database, you can:" -ForegroundColor Yellow
    Write-Host "1. Use DB Browser for SQLite (https://sqlitebrowser.org/)" -ForegroundColor White
    Write-Host "2. Use SQLite command-line tool" -ForegroundColor White
    Write-Host "3. Use VS Code SQLite extension" -ForegroundColor White
    Write-Host ""
    
    Write-Host "Opening database location in Explorer..." -ForegroundColor Green
    Start-Process "explorer.exe" -ArgumentList "/select,`"$dbPath`""
} else {
    Write-Host "Database file not found!" -ForegroundColor Red
    Write-Host "Expected location: $dbPath" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "The database will be created when you run the application for the first time." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Database Tables in this application:" -ForegroundColor Cyan
Write-Host "  - Items (products/materials)" -ForegroundColor White
Write-Host "  - Parties (customers/suppliers)" -ForegroundColor White
Write-Host "  - Users (application users)" -ForegroundColor White
Write-Host "  - Transactions (item transactions)" -ForegroundColor White
Write-Host "  - FinancialTransactions (payments/receipts)" -ForegroundColor White
Write-Host "  - LoanAccounts (loan tracking)" -ForegroundColor White
Write-Host "  - Workers (employee data)" -ForegroundColor White
Write-Host "  - WageTransactions (payroll)" -ForegroundColor White
Write-Host "  - AppSettings (application settings)" -ForegroundColor White
Write-Host ""
