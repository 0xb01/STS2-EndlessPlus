<#
.SYNOPSIS
    One-click cross-platform build & package script for EndlessPlus.
.DESCRIPTION
    Compiles the project in Release configuration, packages the mod distribution,
    and creates ready-to-deploy zip packages in the /build/ directory for Windows, Linux, and macOS.
#>

[CmdletBinding()]
param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  BNKZ.STS2.EndlessPlus - One-Click Build & Package" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Read mod version from JSON manifest
$manifestPath = Join-Path $repoRoot "BNKZ.STS2.EndlessPlus.json"
if (-not (Test-Path $manifestPath)) {
    Write-Error "Mod manifest not found at: $manifestPath"
    exit 1
}

$manifest = Get-Content -Raw $manifestPath | ConvertFrom-Json
$version = $manifest.version
Write-Host "Target Mod Version: v$version" -ForegroundColor Green

# 2. Prepare /build/ directory
$buildDir = Join-Path $repoRoot "build"
if (Test-Path $buildDir) {
    Write-Host "Cleaning existing /build/ directory..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $buildDir
}
New-Item -ItemType Directory -Force -Path $buildDir | Out-Null

# 3. Compile solution using dotnet build
Write-Host "`nCompiling .NET 9.0 Release assembly..." -ForegroundColor Cyan
$buildResult = & dotnet build (Join-Path $repoRoot "BNKZ.STS2.EndlessPlus.csproj") -c $Configuration --nologo
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

# 4. Stage distribution files
$stagingDir = Join-Path $buildDir "_staging"
$modDistDir = Join-Path $stagingDir "EndlessPlus"
New-Item -ItemType Directory -Force -Path $modDistDir | Out-Null

$binDir = Join-Path $repoRoot "bin\$Configuration"
$dllPath = Join-Path $binDir "BNKZ.STS2.EndlessPlus.dll"
$xmlPath = Join-Path $binDir "BNKZ.STS2.EndlessPlus.xml"

if (-not (Test-Path $dllPath)) {
    Write-Error "Compiled DLL not found at: $dllPath"
    exit 1
}

Copy-Item $dllPath -Destination $modDistDir
Copy-Item (Join-Path $repoRoot "BNKZ.STS2.EndlessPlus.json") -Destination $modDistDir
Copy-Item (Join-Path $repoRoot "BNKZ.STS2.EndlessPlus.config") -Destination $modDistDir
Copy-Item (Join-Path $repoRoot "README.md") -Destination $modDistDir

if (Test-Path $xmlPath) {
    Copy-Item $xmlPath -Destination $modDistDir
}

if (Test-Path (Join-Path $repoRoot "docs")) {
    Copy-Item (Join-Path $repoRoot "docs") -Destination $modDistDir -Recurse
}

# 5. Create cross-platform installer helper scripts inside staging
$winInstallScript = @'
@echo off
echo Installing EndlessPlus to Slay the Spire 2 mods folder...
set "DEST=%PROGRAMFILES(X86)%\Steam\steamapps\common\Slay the Spire 2\mods\EndlessPlus"
if not exist "%DEST%" mkdir "%DEST%"
xcopy /E /Y /I "%~dp0EndlessPlus" "%DEST%"
echo Installation complete!
pause
'@
[System.IO.File]::WriteAllText((Join-Path $stagingDir "install-windows.bat"), $winInstallScript)

$linuxInstallScript = @'
#!/usr/bin/env bash
set -e
echo "Installing EndlessPlus to Slay the Spire 2 mods folder..."
TARGET_DIR="${HOME}/.steam/steam/steamapps/common/Slay the Spire 2/mods/EndlessPlus"
mkdir -p "${TARGET_DIR}"
cp -rf "$(dirname "$0")/EndlessPlus/"* "${TARGET_DIR}/"
echo "Installation complete!"
'@
[System.IO.File]::WriteAllText((Join-Path $stagingDir "install-linux.sh"), $linuxInstallScript.Replace("`r`n", "`n"))

$macInstallScript = @'
#!/usr/bin/env bash
set -e
echo "Installing EndlessPlus to Slay the Spire 2 mods folder..."
TARGET_DIR="${HOME}/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/mods/EndlessPlus"
mkdir -p "${TARGET_DIR}"
cp -rf "$(dirname "$0")/EndlessPlus/"* "${TARGET_DIR}/"
echo "Installation complete!"
'@
[System.IO.File]::WriteAllText((Join-Path $stagingDir "install-mac.sh"), $macInstallScript.Replace("`r`n", "`n"))

# 6. Generate the zipped release packages
# User specified name: STS2-EndlessPlus_v{version}.zip
$universalZipName = "STS2-EndlessPlus_v$version.zip"
$universalZipPath = Join-Path $buildDir $universalZipName

Write-Host "`nPackaging release archives..." -ForegroundColor Cyan

# Package primary universal zip (Mac, Linux, and Windows)
Compress-Archive -Path "$stagingDir\*" -DestinationPath $universalZipPath -Force
Write-Host "Created Universal (Mac/Linux/Windows): $universalZipName" -ForegroundColor Green

# Package platform-specific named zips as well
$winZipName = "STS2-EndlessPlus_v${version}_Windows.zip"
$linuxZipName = "STS2-EndlessPlus_v${version}_Linux.zip"
$macZipName = "STS2-EndlessPlus_v${version}_macOS.zip"

Copy-Item $universalZipPath (Join-Path $buildDir $winZipName) -Force
Copy-Item $universalZipPath (Join-Path $buildDir $linuxZipName) -Force
Copy-Item $universalZipPath (Join-Path $buildDir $macZipName) -Force

# 7. Clean up staging folder
Remove-Item -Recurse -Force $stagingDir

Write-Host "`nBuild succeeded! Output files in /build/:" -ForegroundColor Green
Get-ChildItem $buildDir | ForEach-Object {
    Write-Host "  - $($_.Name) ($([math]::Round($_.Length / 1KB, 1)) KB)" -ForegroundColor Cyan
}
