param(
  [string]$WorkspaceRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path,
  [string]$ExpectedId = 'Watcher',
  [string]$ExpectedVersion = '0.5.7'
)

$ErrorActionPreference = 'Stop'

$modDir = Join-Path $WorkspaceRoot 'mods\watcher_character'
$manifestPath = Join-Path $modDir 'manifest.json'
$dllPath = Join-Path $modDir "$ExpectedId.dll"
$pckPath = Join-Path $modDir "$ExpectedId.pck"
$sourceRoot = Join-Path $WorkspaceRoot 'mod_src\watcher_character'
$decompiledProject = Join-Path $sourceRoot 'decompiled_src\Watcher.csproj'

function Assert-True([bool]$condition, [string]$message) {
  if (-not $condition) {
    throw $message
  }
}

Assert-True (Test-Path -LiteralPath $manifestPath) "Missing manifest: $manifestPath"
$manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
Assert-True ($manifest.id -eq $ExpectedId) "Expected manifest id '$ExpectedId', got '$($manifest.id)'"
Assert-True ($manifest.version -eq $ExpectedVersion) "Expected manifest version '$ExpectedVersion', got '$($manifest.version)'"
Assert-True ($manifest.has_dll -eq $true) 'Manifest must declare has_dll=true'
Assert-True ($manifest.has_pck -eq $true) 'Manifest must declare has_pck=true'

Assert-True (Test-Path -LiteralPath $dllPath) "Missing DLL matching manifest id: $dllPath"
Assert-True (Test-Path -LiteralPath $pckPath) "Missing PCK matching manifest id: $pckPath"

$dll = Get-Item -LiteralPath $dllPath
$pck = Get-Item -LiteralPath $pckPath
Assert-True ($dll.Length -gt 300KB) "DLL looks too small for upstream Watcher 0.5.7: $($dll.Length) bytes"
Assert-True ($pck.Length -gt 80MB) "PCK looks too small for upstream Watcher 0.5.7: $($pck.Length) bytes"

Assert-True (Test-Path -LiteralPath $decompiledProject) "Missing editable decompiled project: $decompiledProject"

Write-Host "Watcher package verification passed."
Write-Host "Mod dir: $modDir"
Write-Host "Manifest: $($manifest.id) $($manifest.version)"
Write-Host "DLL: $($dll.Name) $($dll.Length) bytes"
Write-Host "PCK: $($pck.Name) $($pck.Length) bytes"
