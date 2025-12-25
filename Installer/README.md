# Factory Management Installer

This folder contains the WiX Toolset configuration to build an MSI installer for the Factory Management application.

## Prerequisites

1. **Install WiX Toolset v4**
   ```powershell
   dotnet tool install --global wix --version 4.0.5
   ```

2. **Visual Studio 2022** (optional, for GUI development)
   - Install the "WiX Toolset Build Tools" extension if editing in VS

## Building the Installer

### Option 1: Command Line (Recommended)

```powershell
# Navigate to the installer directory
cd C:\GitRepo\Personal\FactoryManagement\Installer

# Restore WiX dependencies
dotnet restore

# Build the MSI
dotnet build -c Release

# Output will be in: bin\Release\net8.0\en-US\FactoryManagementSetup.msi
```

### Option 2: Visual Studio

1. Add the `Installer` project to your solution:
   ```powershell
   dotnet sln add Installer/FactoryManagement.Installer.wixproj
   ```

2. Right-click the Installer project → Build

### Option 3: Build Script

Create a `BuildInstaller.ps1` script:

```powershell
# BuildInstaller.ps1
param(
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0.0"
)

Write-Host "Building Factory Management v$Version..." -ForegroundColor Green

# Build the main application first
dotnet build ..\FactoryManagement\FactoryManagement.csproj -c $Configuration

# Build the installer
dotnet build FactoryManagement.Installer.wixproj -c $Configuration -p:Version=$Version

$msiPath = "bin\$Configuration\en-US\FactoryManagementSetup.msi"
if (Test-Path $msiPath) {
    Write-Host "`nInstaller created successfully!" -ForegroundColor Green
    Write-Host "Location: $(Resolve-Path $msiPath)" -ForegroundColor Cyan
} else {
    Write-Host "`nInstaller build failed!" -ForegroundColor Red
}
```

Run it:
```powershell
.\BuildInstaller.ps1 -Version "1.2.3.0"
```

## Customization

### Update Product Information

Edit `Product.wxs`:
- **Line 8**: `Manufacturer` - Your company name
- **Line 9**: `Version` - Update for each release (e.g., "1.2.3.0")
- **Line 10**: `UpgradeCode` - Generate once, NEVER change (ensures upgrades work)
- **Line 15**: `Description` and `Manufacturer`

### Generate a New UpgradeCode

```powershell
[guid]::NewGuid().ToString()
```
Copy this GUID to the `UpgradeCode` attribute and keep it forever.

### Add Custom Icons

1. Create `Banner.bmp` (493 x 58 pixels) for the installer banner
2. Create `Dialog.bmp` (493 x 312 pixels) for the welcome dialog
3. Place them in the `Installer` folder

### Include Additional Files

In `Product.wxs`, add more `<File>` elements:

```xml
<Component Id="ConfigFiles" Guid="NEW-GUID-HERE">
  <File Source="..\FactoryManagement\appsettings.json" />
  <File Source="..\FactoryManagement\README.txt" />
</Component>
```

### Add Prerequisites (e.g., .NET Runtime)

Add to `Product.wxs` before `</Package>`:

```xml
<Property Id="DOTNET_RUNTIME_INSTALLED">
  <RegistrySearch Id="DotNetRuntime"
                  Root="HKLM"
                  Key="SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedhost"
                  Name="Version"
                  Type="raw" />
</Property>

<Launch Condition="DOTNET_RUNTIME_INSTALLED"
        Message="This application requires .NET 8.0 Runtime. Please install it from https://dotnet.microsoft.com/download" />
```

## Testing the Installer

1. **Install**:
   ```powershell
   msiexec /i bin\Release\en-US\FactoryManagementSetup.msi /l*v install.log
   ```

2. **Uninstall**:
   ```powershell
   msiexec /x bin\Release\en-US\FactoryManagementSetup.msi /l*v uninstall.log
   ```

3. **Silent Install** (for deployment):
   ```powershell
   msiexec /i FactoryManagementSetup.msi /quiet /qn /l*v install.log
   ```

## Signing the MSI (Production)

For production releases, sign the MSI with a code signing certificate:

```powershell
signtool sign /f "YourCertificate.pfx" /p "password" /t http://timestamp.digicert.com "FactoryManagementSetup.msi"
```

## Troubleshooting

### "WiX Toolset not found"
```powershell
dotnet tool install --global wix --version 4.0.5
```

### "File not found" errors
- Ensure the main project is built first: `dotnet build ..\FactoryManagement\FactoryManagement.csproj`
- Check that `$(var.FactoryManagement.TargetPath)` points to the correct output

### Large MSI size
- The MSI includes all dependencies. For .NET 8 self-contained apps, this is normal (100-200 MB)
- For framework-dependent deployments, exclude runtime files

### Upgrade not working
- Never change the `UpgradeCode`
- Increment the `Version` number
- Ensure `MajorUpgrade` element is present

## Advanced: Creating Framework-Dependent Installer

If you want a smaller installer that requires .NET 8 to be pre-installed:

1. Publish as framework-dependent:
   ```powershell
   dotnet publish ..\FactoryManagement\FactoryManagement.csproj -c Release --no-self-contained
   ```

2. Update `Product.wxs` to reference publish output:
   ```xml
   Source="$(var.FactoryManagement.PublishDir)FactoryManagement.exe"
   ```

## Resources

- [WiX Toolset Documentation](https://wixtoolset.org/docs/)
- [WiX v4 Tutorial](https://wixtoolset.org/docs/intro/)
- [MSI Best Practices](https://docs.microsoft.com/windows/win32/msi/windows-installer-best-practices)
