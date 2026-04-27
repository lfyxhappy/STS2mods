# Watcher Character Local Mod

This local folder is now based on the upstream `Watcher` 0.5.7 package from:

`C:\baidunetdiskdownload\【0.99 最后支持】观者-0.5.7.zip`

## Installed Shape

The game loader recursively scans `mods` for any `.json` manifest, then loads files named after `manifest.id`.

Current installed files:

- `mods\watcher_character\manifest.json`
- `mods\watcher_character\Watcher.dll`
- `mods\watcher_character\Watcher.pck`

The directory remains `watcher_character` for local organization, but the manifest id remains `Watcher` so the original DLL, PCK, and `res://Watcher/...` resource paths stay compatible.

## Editable Source

- `decompiled_src\` is a buildable ILSpy decompile of `Watcher.dll`.
- `upstream_package\` keeps upstream metadata/DLL copies for comparison and recovery. The large upstream PCK is installed under `mods\watcher_character\Watcher.pck`; GitHub mirrors may avoid duplicating it in this source folder.
- The previous local 0.1.0 implementation was backed up under `.codex-tools\backups\watcher_character_pre_0_5_7_*`.

Build the decompiled source:

```powershell
.\mod_src\watcher_character\tools\build_decompiled_watcher.ps1 -GameRuntimeDir "C:\算法\小应用\Slay the Spire 2\data_sts2_windows_x86_64"
```

Build and deploy the edited DLL into the local mod:

```powershell
.\mod_src\watcher_character\tools\build_decompiled_watcher.ps1 -GameRuntimeDir "C:\算法\小应用\Slay the Spire 2\data_sts2_windows_x86_64" -Deploy
```

If your game install is elsewhere, point `-GameRuntimeDir` or `STS2_GAME_RUNTIME_DIR` at the folder that contains `sts2.dll`, `0Harmony.dll`, and `GodotSharp.dll`. Those game runtime DLLs are intentionally not stored in this repository.

Verify the installed package shape:

```powershell
.\mod_src\watcher_character\tools\verify_watcher_package.ps1
```

## Resource Notes

`Watcher.pck` is still the upstream 0.5.7 resource pack. It contains resources under `res://Watcher/...`, so renaming the manifest id to `watcher_character` would break the loader/resource relationship unless the PCK and DLL references are migrated together.
