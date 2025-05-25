# Build GitHub Release Package for MassoToolEditor
# PowerShell script to publish, package, and create a zip release with checksums
# This script automatically handles the entire release process from build to packaging

param(
    [string]$OutputDir = "release"
)

# Function to get version from MSBuild properties
function Get-ProjectVersion {
    try {
        $versionOutput = & dotnet msbuild MassoToolEditor.csproj -getProperty:Version -verbosity:quiet 2>&1
        if ($LASTEXITCODE -eq 0 -and $versionOutput) {
            return $versionOutput.Trim()
        }
        
        # Fallback: read from Directory.Build.props
        $propsFile = "Directory.Build.props"
        if (Test-Path $propsFile) {
            [xml]$props = Get-Content $propsFile
            $major = $props.Project.PropertyGroup.MajorVersion
            $minor = $props.Project.PropertyGroup.MinorVersion
            $patch = $props.Project.PropertyGroup.PatchVersion
            if ($major -and $minor -and $patch) {
                return "$major.$minor.$patch"
            }
        }
        
        throw "Could not determine version"
    }
    catch {
        Write-Host "Warning: Could not read version automatically, using 1.0.0" -ForegroundColor Yellow
        return "1.0.0"
    }
}

$Version = Get-ProjectVersion
Write-Host "Building GitHub release package for MassoToolEditor v$Version..." -ForegroundColor Green

# Clean and create output directory
if (Test-Path $OutputDir) {
    Remove-Item $OutputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputDir | Out-Null

# Clean and publish the application
Write-Host "Publishing application..." -ForegroundColor Yellow
if (Test-Path "publish") {
    Remove-Item "publish" -Recurse -Force
}

try {
    $publishResult = & dotnet publish -c Release -r win-x64 --self-contained true -o publish 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error: Publish failed" -ForegroundColor Red
        Write-Host $publishResult -ForegroundColor Red
        Read-Host "Press Enter to exit"
        exit 1
    }
    Write-Host "Publish completed successfully" -ForegroundColor Green
}
catch {
    Write-Host "Error: Failed to publish application: $($_.Exception.Message)" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

try {
    # Create release folder structure
    $releaseFolder = Join-Path $OutputDir "MassoToolEditor-v$Version"
    New-Item -ItemType Directory -Path $releaseFolder | Out-Null
    
    Write-Host "Copying application files..." -ForegroundColor Yellow
    
    # Copy published application files
    Copy-Item "publish\*" -Destination $releaseFolder -Recurse
    
    # Copy documentation and license files
    Copy-Item "README.md" -Destination $releaseFolder
    Copy-Item "LICENSE.md" -Destination $releaseFolder
    
    # Copy icon file
    if (Test-Path "Masso_Tool_Editor.ico") {
        Copy-Item "Masso_Tool_Editor.ico" -Destination $releaseFolder
    }
    
    Write-Host "Creating zip archive..." -ForegroundColor Yellow
    
    # Create zip file
    $zipPath = Join-Path $OutputDir "MassoToolEditor-v$Version-win-x64.zip"
    Compress-Archive -Path $releaseFolder -DestinationPath $zipPath -CompressionLevel Optimal
    
    Write-Host "Generating checksums..." -ForegroundColor Yellow
    
    # Generate checksums
    $zipFile = Get-Item $zipPath
    $md5Hash = (Get-FileHash $zipPath -Algorithm MD5).Hash
    $sha256Hash = (Get-FileHash $zipPath -Algorithm SHA256).Hash
    $sha512Hash = (Get-FileHash $zipPath -Algorithm SHA512).Hash
    
    # Create checksums file
    $checksumFile = Join-Path $OutputDir "MassoToolEditor-v$Version-checksums.txt"
    $checksumContent = @"
MassoToolEditor v$Version - Release Checksums
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC")

File: $(Split-Path $zipPath -Leaf)
Size: $([math]::Round($zipFile.Length / 1MB, 2)) MB

MD5:    $md5Hash
SHA256: $sha256Hash
SHA512: $sha512Hash

Verification:
- Windows: certutil -hashfile "$(Split-Path $zipPath -Leaf)" SHA256
- PowerShell: (Get-FileHash "$(Split-Path $zipPath -Leaf)" -Algorithm SHA256).Hash
- Linux/Mac: sha256sum "$(Split-Path $zipPath -Leaf)"
"@
    
    Set-Content -Path $checksumFile -Value $checksumContent -Encoding UTF8
      # Create release notes template
    $releaseNotesFile = Join-Path $OutputDir "release-notes-v$Version.md"
    $releaseNotesContent = @"
# MassoToolEditor v$Version

## Installation

1. Download the zip file: ``MassoToolEditor-v$Version-win-x64.zip``
2. Extract to a folder of your choice
3. Run ``MassoToolEditor.exe``

## File Verification

Verify the download integrity using the checksums in ``MassoToolEditor-v$Version-checksums.txt``:

````
SHA256: $sha256Hash
````

## Requirements

- Windows 10 or later (x64)
- .NET 8.0 Runtime (included in package)

No additional software installation required - the application is self-contained.

## What's New

<!-- Add your release notes here -->
- Feature: Unit selection dialog at startup
- Improvement: Removed CRC field from UI (handled internally)
- Improvement: Better Record 0 handling
- Fix: CSV export precision and unit conversion issues
- Fix: Display precision now matches Masso controller (5 decimal places)

## Bug Fixes

<!-- Add bug fixes here -->

## Breaking Changes

<!-- Add any breaking changes here -->

## Full Changelog

<!-- Link to full changelog or commit history -->
See the [commit history](https://github.com/yourusername/MassoToolEditor/commits/main) for detailed changes.

---

**Important Disclaimer**

Please back up your Masso settings before using this tool. You assume all risk if this application causes any problems with your Masso controller.
"@
    
    Set-Content -Path $releaseNotesFile -Value $releaseNotesContent -Encoding UTF8
      # Clean up temporary folder
    Remove-Item $releaseFolder -Recurse -Force
    
    Write-Host ""
    Write-Host "SUCCESS: Release package created!" -ForegroundColor Green
    Write-Host ""
    Write-Host "[PACKAGE] Files created:" -ForegroundColor Cyan
    Write-Host "  - $zipPath" -ForegroundColor White
    Write-Host "  - $checksumFile" -ForegroundColor White
    Write-Host "  - $releaseNotesFile" -ForegroundColor White
    Write-Host ""
    Write-Host "[INFO] Package Info:" -ForegroundColor Cyan
    Write-Host "  Size: $([math]::Round($zipFile.Length / 1MB, 2)) MB" -ForegroundColor White
    Write-Host "  SHA256: $sha256Hash" -ForegroundColor White
    Write-Host ""
    Write-Host "[NEXT] Next Steps:" -ForegroundColor Cyan
    Write-Host "  1. Test the extracted application" -ForegroundColor White
    Write-Host "  2. Update the release notes in: $releaseNotesFile" -ForegroundColor White
    Write-Host "  3. Create a GitHub release and upload the files" -ForegroundColor White
    Write-Host "  4. Include the checksums in the release description" -ForegroundColor White
    Write-Host ""
    Write-Host "[TIP] To create future releases:" -ForegroundColor Cyan
    Write-Host "  1. Update version in Directory.Build.props" -ForegroundColor White
    Write-Host "  2. Run this script: .\build-release.ps1" -ForegroundColor White
    Write-Host "  3. Upload to GitHub" -ForegroundColor White
    
}
catch {
    Write-Host "Error occurred: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

