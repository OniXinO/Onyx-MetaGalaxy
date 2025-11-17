Param(
    [string]$ActiveProfile
)

$ErrorActionPreference = 'Stop'

function Get-ActiveProfileName {
    param([string]$settingsPath)
    if (-not (Test-Path $settingsPath)) { return 'default' }
    $lines = Get-Content -LiteralPath $settingsPath
    foreach ($line in $lines) {
        if ($line -match 'activeProfile\s*=\s*(\S+)') { return $Matches[1] }
    }
    return 'default'
}

function Get-StrictMode {
    param([string]$settingsPath)
    if (-not (Test-Path $settingsPath)) { return $true }
    $lines = Get-Content -LiteralPath $settingsPath
    foreach ($line in $lines) {
        if ($line -match 'strictMarkers\s*=\s*(\S+)') {
            $val = $Matches[1].ToLower()
            return ($val -in @('true','1','yes'))
        }
    }
    return $true
}

$root = Split-Path $PSScriptRoot -Parent
$gameData = Join-Path $root 'GameData'
$modVendor = Join-Path $gameData 'OniXinO'
$modRoot = Join-Path $modVendor 'OMG'
$profilesDir = Join-Path $modRoot 'Profiles'
$settingsPath = Join-Path $modRoot 'OMGSettings.cfg'

if (-not $ActiveProfile) {
    $ActiveProfile = Get-ActiveProfileName -settingsPath $settingsPath
}
$strict = Get-StrictMode -settingsPath $settingsPath

if (-not (Test-Path $profilesDir)) {
    Write-Host "Profiles directory not found: $profilesDir" -ForegroundColor Red
    exit 1
}

$profilePath = Join-Path $profilesDir ("{0}.cfg" -f $ActiveProfile)
if (-not (Test-Path $profilePath)) {
    Write-Host "Profile '$ActiveProfile' not found: $profilePath" -ForegroundColor Red
    exit 1
}

Write-Host "Activating profile: $ActiveProfile" -ForegroundColor Cyan

# Parse Packs from profile .cfg
$packs = @()
$inPack = $false
$current = @{ id = $null; enabled = $false }

foreach ($raw in Get-Content -LiteralPath $profilePath) {
    $line = $raw.Trim()
    # Початок блоку Pack (підтримка форматів: "Pack {" та на наступному рядку "{")
    if (-not $inPack -and ($line -match '^Pack(\s*\{)?$')) {
        $inPack = $true
        $current = @{ id = $null; enabled = $false }
        continue
    }
    if ($inPack -and $line -match '^\{\s*$') { continue }
    if ($inPack -and $line -match '^id\s*=') {
        $current.id = ($line -replace 'id\s*=\s*','').Trim()
        continue
    }
    if ($inPack -and $line -match '^enabled\s*=') {
        $val = ($line -replace 'enabled\s*=\s*','').Trim().ToLower()
        $current.enabled = ($val -in @('true','1','yes'))
        continue
    }
    if ($inPack -and $line -match '^\}\s*$') {
        if ($current.id) { $packs += [pscustomobject]$current }
        $inPack = $false
        $current = @{ id = $null; enabled = $false }
        continue
    }
}

if ($packs.Count -eq 0) {
    Write-Host "No packs defined in profile: $profilePath" -ForegroundColor Yellow
}

foreach ($p in $packs) {
    $markerFolderName = "OMG_Enable_$($p.id)"
    # Нове розташування маркерів у вендорній теці
    $markerPath = Join-Path $modVendor $markerFolderName
    # Легасі-шлях (раніше у корені GameData) — при потребі приберемо
    $legacyMarkerPath = Join-Path $gameData $markerFolderName
    $packPath = Join-Path $gameData $p.id
    $packExists = Test-Path $packPath
    if ($p.enabled -and -not $packExists) {
        Write-Host "WARNING: Pack '$($p.id)' enabled but not found in GameData ($packPath)" -ForegroundColor Yellow
        if ($strict) {
            Write-Host "Strict mode active: skipping marker for '$($p.id)'" -ForegroundColor Yellow
            continue
        }
    }
    if ($p.enabled) {
        if (-not (Test-Path $markerPath)) { New-Item -ItemType Directory -Path $markerPath | Out-Null }
        $mmPatchPath = Join-Path $markerPath 'MM_Marker.cfg'
        $modName = $markerFolderName
        $content = "@OMG:FOR[$modName]`n{`n    // Marker for ModuleManager :NEEDS[$modName]`n}"
        Set-Content -LiteralPath $mmPatchPath -Value $content -Encoding UTF8
        Write-Host "Enabled marker: $markerFolderName" -ForegroundColor Green
        # Приберемо легасі-маркер у корені, щоб уникнути дублювань
        if ((Test-Path $legacyMarkerPath) -and ($legacyMarkerPath -ne $markerPath)) {
            Remove-Item -Recurse -Force -LiteralPath $legacyMarkerPath
            Write-Host "Removed legacy marker: $legacyMarkerPath" -ForegroundColor Yellow
        }
    } else {
        foreach ($path in @($markerPath, $legacyMarkerPath)) {
            if (Test-Path $path) {
                Remove-Item -Recurse -Force -LiteralPath $path
                Write-Host "Removed marker: $path" -ForegroundColor Yellow
            }
        }
    }
}

Write-Host "Activation complete. Launch KSP to apply '$ActiveProfile'." -ForegroundColor Cyan