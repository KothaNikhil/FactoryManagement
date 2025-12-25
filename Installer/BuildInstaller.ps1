# BuildInstaller.ps1
# Automated script to build the Factory Management MSI installer

param(
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0.0",
    [switch]$Clean,
    [switch]$Sign,
    [string]$CertificatePath = "",
    [SecureString]$CertificatePassword = (New-Object System.Security.SecureString)
)

$ErrorActionPreference = "Stop"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Factory Management Installer Builder" -ForegroundColor Cyan
Write-Host "  Version: $Version" -ForegroundColor Cyan
Write-Host "  Configuration: $Configuration" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# Navigate to installer directory
$InstallerDir = $PSScriptRoot
Set-Location $InstallerDir

# Clean previous builds if requested
if ($Clean) {
    Write-Host "`n[1/5] Cleaning previous builds..." -ForegroundColor Yellow
    if (Test-Path "bin") { Remove-Item "bin" -Recurse -Force }
    if (Test-Path "obj") { Remove-Item "obj" -Recurse -Force }
    if (Test-Path "..\FactoryManagement\bin") { Remove-Item "..\FactoryManagement\bin" -Recurse -Force }
    if (Test-Path "..\FactoryManagement\obj") { Remove-Item "..\FactoryManagement\obj" -Recurse -Force }
    Write-Host "    Cleanup complete." -ForegroundColor Green
}

# Step 1: Build the main application
Write-Host "`n[2/5] Building main application..." -ForegroundColor Yellow
Push-Location "..\FactoryManagement"
try {
    dotnet build FactoryManagement.csproj -c $Configuration
    if ($LASTEXITCODE -ne 0) {
        throw "Application build failed with exit code $LASTEXITCODE"
    }
    Write-Host "    Application build successful." -ForegroundColor Green
} finally {
    Pop-Location
}

# Step 2.5: Publish application to generate EXE (apphost)
Write-Host "`n[2.5/5] Publishing application (win-x64 apphost)..." -ForegroundColor Yellow
$PublishDir = Join-Path (Split-Path "..\FactoryManagement\FactoryManagement.csproj" -Parent) "bin\$Configuration\net8.0-windows\publish"
dotnet publish "..\FactoryManagement\FactoryManagement.csproj" -c $Configuration -r win-x64 --self-contained false -o "$PublishDir" | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "Application publish failed with exit code $LASTEXITCODE"
}
Write-Host "    Application publish successful: $PublishDir" -ForegroundColor Green

# Step 2: Restore WiX dependencies
Write-Host "`n[3/5] Restoring WiX dependencies..." -ForegroundColor Yellow
dotnet restore FactoryManagement.Installer.wixproj
if ($LASTEXITCODE -ne 0) {
    throw "WiX restore failed with exit code $LASTEXITCODE"
}
Write-Host "    Dependencies restored." -ForegroundColor Green

# Step 3: Build the installer
Write-Host "`n[4/5] Building MSI installer..." -ForegroundColor Yellow
dotnet build FactoryManagement.Installer.wixproj -c $Configuration -p:Version=$Version
if ($LASTEXITCODE -ne 0) {
    throw "Installer build failed with exit code $LASTEXITCODE"
}

# Locate the MSI file
$MsiPath = Join-Path $InstallerDir "bin\x64\$Configuration\FactoryManagementSetup.msi"

if (-not (Test-Path $MsiPath)) {
    Write-Host "`nERROR: MSI file not found at expected location:" -ForegroundColor Red
    Write-Host "  $MsiPath" -ForegroundColor Red
    exit 1
}

Write-Host "    Installer build successful." -ForegroundColor Green

# Step 4: Sign the MSI (if requested)
if ($Sign) {
    Write-Host "`n[5/5] Signing MSI installer..." -ForegroundColor Yellow
    
    if (-not $CertificatePath -or -not (Test-Path $CertificatePath)) {
        Write-Host "    ERROR: Certificate path not provided or file not found." -ForegroundColor Red
        Write-Host "    Use: -Sign -CertificatePath 'path\to\cert.pfx' -CertificatePassword 'password'" -ForegroundColor Red
        exit 1
    }

    # Check if signtool is available
    $SignTool = "signtool.exe"
    try {
        & $SignTool /? | Out-Null
    } catch {
        Write-Host "    ERROR: signtool.exe not found in PATH." -ForegroundColor Red
        Write-Host "    Install Windows SDK: https://developer.microsoft.com/windows/downloads/windows-sdk/" -ForegroundColor Red
        exit 1
    }

    # Sign the MSI
    $SignArgs = @(
        "sign",
        "/f", $CertificatePath,
        "/p", ([System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToCoTaskMemUnicode($CertificatePassword))),
        "/t", "http://timestamp.digicert.com",
        "/fd", "SHA256",
        "/v",
        $MsiPath
    )

    & $SignTool $SignArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Host "    WARNING: Signing failed. MSI is unsigned." -ForegroundColor Yellow
    } else {
        Write-Host "    MSI signed successfully." -ForegroundColor Green
    }
} else {
    Write-Host "`n[5/5] Skipping code signing (use -Sign to enable)" -ForegroundColor Gray
}

# Display results
Write-Host "`n================================================" -ForegroundColor Cyan
Write-Host "  BUILD SUCCESSFUL!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "`nInstaller Details:" -ForegroundColor White
Write-Host "  File:     $(Split-Path $MsiPath -Leaf)" -ForegroundColor White
Write-Host "  Location: $MsiPath" -ForegroundColor Cyan
Write-Host "  Size:     $([math]::Round((Get-Item $MsiPath).Length / 1MB, 2)) MB" -ForegroundColor White
Write-Host "  Version:  $Version" -ForegroundColor White

# Calculate hash for verification
$Hash = (Get-FileHash $MsiPath -Algorithm SHA256).Hash
Write-Host "  SHA256:   $Hash" -ForegroundColor Gray

Write-Host "`nInstallation Commands:" -ForegroundColor Yellow
Write-Host "  Install:   msiexec /i `"$MsiPath`" /l*v install.log" -ForegroundColor Gray
Write-Host "  Uninstall: msiexec /x `"$MsiPath`" /l*v uninstall.log" -ForegroundColor Gray
Write-Host "  Silent:    msiexec /i `"$MsiPath`" /quiet /qn /l*v install.log" -ForegroundColor Gray

Write-Host "`n================================================`n" -ForegroundColor Cyan
