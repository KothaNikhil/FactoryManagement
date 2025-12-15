# Factory Management System - Build and Run Script
# This script builds the application and runs it

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Factory Management System - Build Script" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Set the project directory
$projectDir = "C:\FactoryManagement"
$projectFile = "$projectDir\FactoryManagement\FactoryManagement.csproj"

# Check if .NET SDK is installed
Write-Host "Checking .NET SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ .NET SDK $dotnetVersion found" -ForegroundColor Green
} else {
    Write-Host "✗ .NET SDK not found. Please install .NET 8.0 SDK or later." -ForegroundColor Red
    exit 1
}

# Navigate to project directory
Write-Host ""
Write-Host "Navigating to project directory..." -ForegroundColor Yellow
Set-Location $projectDir

# Restore NuGet packages
Write-Host ""
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Failed to restore packages" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Packages restored successfully" -ForegroundColor Green

# Clean the project
Write-Host ""
Write-Host "Cleaning project..." -ForegroundColor Yellow
dotnet clean
Write-Host "✓ Project cleaned" -ForegroundColor Green

# Build the project
Write-Host ""
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Build completed successfully" -ForegroundColor Green

# Ask user if they want to run the application
Write-Host ""
$runApp = Read-Host "Do you want to run the application now? (Y/N)"
if ($runApp -eq "Y" -or $runApp -eq "y") {
    Write-Host ""
    Write-Host "Starting Factory Management System..." -ForegroundColor Yellow
    Write-Host ""
    dotnet run --project $projectFile --configuration Release
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Script completed!" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
