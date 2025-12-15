# Quick Start Script - Run this to quickly start the application

Write-Host "Starting Factory Management System..." -ForegroundColor Cyan
Write-Host ""

Set-Location "C:\FactoryManagement"
dotnet run --project FactoryManagement\FactoryManagement.csproj

Write-Host ""
Write-Host "Application closed." -ForegroundColor Yellow
