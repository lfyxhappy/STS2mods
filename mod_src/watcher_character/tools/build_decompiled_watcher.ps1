param(
  [string]$WorkspaceRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path,
  [string]$GameRuntimeDir = $env:STS2_GAME_RUNTIME_DIR,
  [switch]$Deploy
)

$ErrorActionPreference = 'Stop'

$sourceRoot = Join-Path $WorkspaceRoot 'mod_src\watcher_character'
$project = Join-Path $sourceRoot 'decompiled_src\Watcher.csproj'
$modDir = Join-Path $WorkspaceRoot 'mods\watcher_character'
$builtDll = Join-Path $sourceRoot 'decompiled_src\bin\Release\netcoreapp9.0\Watcher.dll'
$deployDll = Join-Path $modDir 'Watcher.dll'
$requiredRuntimeDlls = @('sts2.dll', '0Harmony.dll', 'GodotSharp.dll')

if (!(Test-Path -LiteralPath $project)) {
  throw "Missing decompiled project: $project"
}

if ([string]::IsNullOrWhiteSpace($GameRuntimeDir)) {
  $siblingRuntimeDir = Join-Path (Split-Path -Parent $WorkspaceRoot) 'Slay the Spire 2\data_sts2_windows_x86_64'
  if (Test-Path -LiteralPath $siblingRuntimeDir) {
    $GameRuntimeDir = (Resolve-Path -LiteralPath $siblingRuntimeDir).Path
  }
}

if ([string]::IsNullOrWhiteSpace($GameRuntimeDir) -or !(Test-Path -LiteralPath $GameRuntimeDir)) {
  throw "Missing STS2 runtime directory. Pass -GameRuntimeDir or set STS2_GAME_RUNTIME_DIR to the folder containing sts2.dll, 0Harmony.dll, and GodotSharp.dll."
}

$GameRuntimeDir = (Resolve-Path -LiteralPath $GameRuntimeDir).Path
foreach ($dll in $requiredRuntimeDlls) {
  $dllPath = Join-Path $GameRuntimeDir $dll
  if (!(Test-Path -LiteralPath $dllPath)) {
    throw "Missing required runtime DLL: $dllPath"
  }
}

dotnet build $project -c Release -v:q -clp:ErrorsOnly "/p:Sts2RuntimeDir=$GameRuntimeDir"
if ($LASTEXITCODE -ne 0) {
  throw "Watcher decompiled source build failed with exit code $LASTEXITCODE"
}

if ($Deploy) {
  if (!(Test-Path -LiteralPath $builtDll)) {
    throw "Build succeeded but DLL was not found: $builtDll"
  }
  New-Item -ItemType Directory -Path $modDir -Force | Out-Null
  Copy-Item -LiteralPath $builtDll -Destination $deployDll -Force
  Write-Host "Deployed $deployDll"
}
else {
  Write-Host "Built $builtDll"
  Write-Host "Use -Deploy to copy it to $deployDll"
}
