# Clean Build Artifacts for MassoToolEditor
# PowerShell script to clean all build outputs, publish folders, and release packages

param(
    [switch]$KeepRelease,
    [switch]$Verbose
)

Write-Host "Cleaning MassoToolEditor build artifacts..." -ForegroundColor Green

# Function to safely remove directory with feedback
function Remove-DirectorySafe {
    param(
        [string]$Path,
        [string]$Description
    )
    
    if (Test-Path $Path) {
        try {
            if ($Verbose) {
                Write-Host "  Removing $Description at: $Path" -ForegroundColor Yellow
            }
            Remove-Item $Path -Recurse -Force
            Write-Host "  ✓ Cleaned $Description" -ForegroundColor Green
        }
        catch {
            Write-Host "  ✗ Failed to remove $Description`: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    else {
        if ($Verbose) {
            Write-Host "  ○ $Description not found (already clean)" -ForegroundColor Gray
        }
    }
}

# Run dotnet clean
Write-Host ""
Write-Host "Running dotnet clean..." -ForegroundColor Yellow
try {
    $cleanResult = & dotnet clean --verbosity minimal 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ dotnet clean completed successfully" -ForegroundColor Green
    }
    else {
        Write-Host "✗ dotnet clean failed:" -ForegroundColor Red
        Write-Host $cleanResult -ForegroundColor Red
    }
}
catch {
    Write-Host "✗ Error running dotnet clean: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Cleaning additional folders..." -ForegroundColor Yellow

# Clean publish folder
Remove-DirectorySafe -Path "publish" -Description "publish folder"

# Clean release folder (unless -KeepRelease is specified)
if (-not $KeepRelease) {
    Remove-DirectorySafe -Path "release" -Description "release folder"
}
else {
    Write-Host "  ○ Keeping release folder (use without -KeepRelease to remove)" -ForegroundColor Gray
}

# Clean additional build artifacts that dotnet clean might miss
Remove-DirectorySafe -Path "bin" -Description "bin folder"
Remove-DirectorySafe -Path "obj" -Description "obj folder"

# Clean any temporary files
$tempFiles = @("*.tmp", "*.bak", "*~")
foreach ($pattern in $tempFiles) {
    $files = Get-ChildItem -Path "." -Filter $pattern -Recurse -ErrorAction SilentlyContinue
    if ($files) {
        foreach ($file in $files) {
            try {
                Remove-Item $file.FullName -Force
                if ($Verbose) {
                    Write-Host "  Removed temp file: $($file.Name)" -ForegroundColor Yellow
                }
            }
            catch {
                Write-Host "  ✗ Failed to remove temp file $($file.Name): $($_.Exception.Message)" -ForegroundColor Red
            }
        }
        Write-Host "  ✓ Cleaned temporary files ($($files.Count) files)" -ForegroundColor Green
    }
    else {
        if ($Verbose) {
            Write-Host "  ○ No temporary files found ($pattern)" -ForegroundColor Gray
        }
    }
}

Write-Host ""
Write-Host "SUCCESS: Clean completed!" -ForegroundColor Green
Write-Host ""

# Show disk space summary
try {
    $currentDir = Get-Location
    $totalSize = (Get-ChildItem -Path $currentDir -Recurse -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
    $sizeInMB = [math]::Round($totalSize / 1MB, 2)
    Write-Host "📊 Current workspace size: $sizeInMB MB" -ForegroundColor Cyan
}
catch {
    # Ignore errors in size calculation
}

Write-Host ""
Write-Host "💡 Usage tips:" -ForegroundColor Cyan
Write-Host "  clean.ps1                 - Clean everything" -ForegroundColor White
Write-Host "  clean.ps1 -KeepRelease    - Clean but keep release folder" -ForegroundColor White
Write-Host "  clean.ps1 -Verbose        - Show detailed output" -ForegroundColor White
Write-Host ""
