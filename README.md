# STS2mods

Slay the Spire 2 mod collection by lfyxhappy.

This repository contains ready-to-use mod builds in `mods/` and matching C# source projects in `mod_src/`.

## Included Mods

### Multiplayer Damage Meter

- Folder: `mods/multiplayer_damage_meter_v1.0.11`
- Source: `mod_src/multiplayer_damage_meter_v1.0.10`
- Version: `1.0.11`
- Adds a multiplayer damage HUD and combat-end summary.
- Counts blocked damage and normal damage.
- Separates previous-combat total damage from current-combat damage.
- After save/load during an active combat, current-combat damage is reset to `0` to avoid duplicate counting.

### Card Effect Tweaks

- Folder: `mods/card_effect_tweaks`
- Source: `mod_src/card_effect_tweaks`
- Standalone C# Harmony mod scaffold for card/effect tweaks.

## Installation

1. Download `STS2mods-v1.0.11.zip` from the latest GitHub Release.
2. Extract the archive.
3. Copy the extracted `mods` folder into your Slay the Spire 2 game folder.
4. The final structure should look like this:

```text
Slay the Spire 2/
  mods/
    multiplayer_damage_meter_v1.0.11/
      manifest.json
      multiplayer_damage_meter.dll
    card_effect_tweaks/
      manifest.json
      card_effect_tweaks.dll
```

If you clone this repository instead of using the Release zip, copy the repository's `mods/` folder into the game folder.

## Build From Source

The source projects target `.NET 9.0` and reference the Slay the Spire 2 runtime DLLs from the game folder.

Build the multiplayer damage meter:

```powershell
cd "mod_src\multiplayer_damage_meter_v1.0.10"
dotnet build -c Release --no-restore
```

Build card effect tweaks:

```powershell
cd "mod_src\card_effect_tweaks"
dotnet build -c Release --no-restore
```

After building, copy the generated DLL from `bin\Release\netcoreapp9.0\` into the matching folder under `mods/`.

## Save Progress Sync

For syncing Slay the Spire 2 save progress across machines, see my related tool:

[STS2-Save-Sync-Tool](https://github.com/lfyxhappy/STS2-Save-Sync-Tool)

## Notes

- These mods are for Slay the Spire 2.
- Keep mod folder names and `manifest.json` versions aligned when releasing updates.
- The `mod_src/` folder is included for transparency and further modification.
