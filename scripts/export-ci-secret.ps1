<#
.SYNOPSIS
    Exports STS2 game assemblies into a Base64-encoded zip string for GitHub Actions CI.
.DESCRIPTION
    Packs GodotSharp.dll, sts2.dll, 0Harmony.dll, and BaseLib.dll from your local
    Slay the Spire 2 installation into a zip archive and converts it to Base64.
    Saves the output to 'ci-secret.txt' so you can add it as the STS2_LIBS_BASE64 secret in GitHub.
#>

param(
    [string]$SteamPath = "C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2"
)

$ErrorActionPreference = "Stop"

Write-Host "Locating Slay the Spire 2 game assemblies at '$SteamPath'..." -ForegroundColor Cyan

$dlls = @(
    "$SteamPath\data_sts2_windows_x86_64\GodotSharp.dll",
    "$SteamPath\data_sts2_windows_x86_64\sts2.dll",
    "$SteamPath\data_sts2_windows_x86_64\0Harmony.dll",
    "$SteamPath\mods\BaseLib\BaseLib.dll"
)

foreach ($dll in $dlls) {
    if (-not (Test-Path $dll)) {
        Write-Error "Could not find required assembly: $dll. Verify your SteamPath."
        exit 1
    }
}

$tempDir = Join-Path ([System.IO.Path]::GetTempPath()) ("sts2_ci_" + [System.Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Force -Path $tempDir | Out-Null

try {
    foreach ($dll in $dlls) {
        Copy-Item -Path $dll -Destination $tempDir
    }

    $zipPath = Join-Path ([System.IO.Path]::GetTempPath()) ("sts2_ci_" + [System.Guid]::NewGuid().ToString("N") + ".zip")
    Compress-Archive -Path "$tempDir\*" -DestinationPath $zipPath -Force

    $bytes = [System.IO.File]::ReadAllBytes($zipPath)
    $base64 = [Convert]::ToBase64String($bytes)

    $outputFile = Join-Path $PSScriptRoot "..\ci-secret.txt"
    [System.IO.File]::WriteAllText($outputFile, $base64)

    Write-Host "`nSuccess! Game assemblies packed and encoded." -ForegroundColor Green
    Write-Host "Base64 secret saved to: $outputFile" -ForegroundColor Yellow
    Write-Host "`nNext steps to enable GitHub Actions builds:" -ForegroundColor Cyan
    Write-Host "1. Go to your GitHub repository -> Settings -> Secrets and variables -> Actions"
    Write-Host "2. Click 'New repository secret'"
    Write-Host "3. Secret name: STS2_LIBS_BASE64"
    Write-Host "4. Secret value: Paste contents of ci-secret.txt"
}
finally {
    if (Test-Path $tempDir) { Remove-Item -Path $tempDir -Recurse -Force }
    if (Test-Path $zipPath) { Remove-Item -Path $zipPath -Force }
}
