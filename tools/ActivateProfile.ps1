# Activate OMG Profile and manage ModuleManager markers
param(
  [string]$ActiveProfile,
  [string]$GameDataPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Read-Setting {
  param([string]$file, [string]$key, [string]$default)
  if (!(Test-Path $file)) { return $default }
  foreach ($raw in Get-Content -LiteralPath $file) {
    $t = $raw.Trim()
    if ($t -like "$key*") {
      $parts = $t.Split('=')
      if ($parts.Length -ge 2) { return $parts[1].Trim() }
    }
  }
  return $default
}

function Read-BoolSetting {
  param([string]$file, [string]$key, [bool]$default)
  $val = Read-Setting -file $file -key $key -default ($null)
  if ($null -eq $val) { return $default }
  $v = $val.ToLowerInvariant()
  return ($v -eq 'true' -or $v -eq '1' -or $v -eq 'yes')
}

function Parse-Packs {
  param([string]$profilePath)
  $result = @()
  if (!(Test-Path $profilePath)) { return $result }
  $inPack = $false; $id = $null; $enabled = $false
  foreach ($raw in Get-Content -LiteralPath $profilePath) {
    $line = $raw.Trim()
    if (!$inPack -and ($line -eq 'Pack' -or $line.StartsWith('Pack{'))) { $inPack = $true; $id = $null; $enabled = $false; continue }
    if ($inPack -and $line -eq '{') { continue }
    if ($inPack -and $line -like 'id*') { $id = ($line.Split('=')[1]).Trim(); continue }
    if ($inPack -and $line -like 'enabled*') { $enabled = (($line.Split('=')[1]).Trim().ToLowerInvariant() -in @('true','1','yes')); continue }
    if ($inPack -and $line -eq '}') { if ($id) { $result += [pscustomobject]@{ id=$id; enabled=$enabled } }; $inPack = $false }
  }
  return $result
}

function Ensure-Marker {
  param([string]$omgRoot, [string]$packId)
  $markerDir = Join-Path $omgRoot ("OMG_Enable_" + $packId)
  if (!(Test-Path $markerDir)) { New-Item -ItemType Directory -Path $markerDir | Out-Null }
  $markerFile = Join-Path $markerDir 'MM_Marker.cfg'
  $content = @(
    "OMG:FOR[OMG_Enable_$packId]",
    "{",
    "}"
  )
  Set-Content -LiteralPath $markerFile -Value $content -Encoding UTF8
}

function Clean-OldMarkers {
  param([string]$omgRoot)
  Get-ChildItem -LiteralPath $omgRoot -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -like 'OMG_Enable_*' } |
    ForEach-Object { Remove-Item -LiteralPath $_.FullName -Recurse -Force -ErrorAction SilentlyContinue }
}

# Resolve paths
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $GameDataPath) { $GameDataPath = Join-Path $repoRoot 'GameData' }
$omgRoot = Join-Path $GameDataPath 'OniXinO\OMG'
$settingsPath = Join-Path $omgRoot 'OMGSettings.cfg'

if (-not $ActiveProfile -or $ActiveProfile.Trim() -eq '') {
  $ActiveProfile = Read-Setting -file $settingsPath -key 'activeProfile' -default 'default'
}

$strictMarkers = Read-BoolSetting -file $settingsPath -key 'strictMarkers' -default $true
$profilePath = Join-Path $omgRoot ('Profiles\' + $ActiveProfile + '.cfg')
$packs = Parse-Packs -profilePath $profilePath
Write-Host "[OMG] ActiveProfile=$ActiveProfile, packs=$($packs.Count), strictMarkers=$strictMarkers"

Clean-OldMarkers -omgRoot $omgRoot

foreach ($p in $packs) {
  if (-not $p.enabled) { continue }
  if ($strictMarkers) {
    $packDir = Join-Path $GameDataPath $p.id
    if (!(Test-Path $packDir)) {
      Write-Warning "[OMG] Skipping marker for '$($p.id)' — pack folder not found (strict mode)."
      continue
    }
  }
  Ensure-Marker -omgRoot $omgRoot -packId $p.id
  Write-Host "[OMG] Marker created: OMG_Enable_$($p.id)"
}

Write-Host '[OMG] Done.'