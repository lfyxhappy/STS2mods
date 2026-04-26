# STS2mods

这是 lfyxhappy 的《杀戮尖塔 2》（Slay the Spire 2）Mod 集合仓库。

仓库里包含两个部分：

- `mods/`：已经编译好的 Mod，可以直接复制到游戏目录使用。
- `mod_src/`：对应的 C# 源码工程，方便查看、修改和重新编译。

## 已包含的 Mod

### 多人伤害统计

- 安装目录：`mods/multiplayer_damage_meter_v1.0.11`
- 源码目录：`mod_src/multiplayer_damage_meter_v1.0.10`
- 当前版本：`1.0.11`

功能说明：

- 在多人模式中显示每名玩家的伤害统计。
- 统计普通伤害和被格挡伤害。
- 战斗结束后显示本场战斗伤害摘要。
- HUD 中分开显示“之前战斗”的累计伤害和“本场战斗”的当前伤害。
- 如果在战斗中 save/load，本场战斗伤害会重置为 `0`，避免当前战斗伤害被重复计算。

### 暂停菜单重打

- 安装目录：`mods/pause_menu_rerun`
- 源码目录：`mod_src/pause_menu_rerun`
- 当前版本：`v1.0`

功能说明：

- 在单人模式暂停菜单中添加绿色的“重打”按钮。
- 按钮会先执行游戏原本的“保存并退出”，回到主菜单后自动执行原本的“继续游戏”。
- 仅在单人模式显示，避免影响多人同步流程。

### 游戏速度调节

- 安装目录：`mods/game_speed_control`
- 源码目录：`mod_src/game_speed_control`
- 当前版本：`1.0.0`

功能说明：

- 在暂停菜单中添加“速度”按钮。
- 按钮会在 `1x -> 2x -> 3x -> 4x -> 1x` 之间循环切换。
- 使用 Godot 全局 `Engine.TimeScale` 调整本机游戏速度。
- 会把上次选择的速度保存到独立配置文件，下次启动游戏时自动恢复。
- 多人模式中也会显示按钮，但倍率只影响本机体验，不会同步给其他玩家。

### Card Effect Tweaks

- 安装目录：`mods/card_effect_tweaks`
- 源码目录：`mod_src/card_effect_tweaks`

这是一个独立的 C# Harmony Mod 模板/脚手架，用来后续编写卡牌或效果调整类 Mod。

## 安装方式

推荐使用 Release 里的压缩包安装：

1. 打开本仓库的 GitHub Release 页面。
2. 下载 `STS2mods-v1.0.13.zip`。
3. 解压压缩包。
4. 把解压出来的 `mods` 文件夹复制到《杀戮尖塔 2》的游戏目录下。

最终目录结构应类似这样：

```text
Slay the Spire 2/
  mods/
    multiplayer_damage_meter_v1.0.11/
      manifest.json
      multiplayer_damage_meter.dll
    card_effect_tweaks/
      manifest.json
      card_effect_tweaks.dll
    pause_menu_rerun/
      manifest.json
      pause_menu_rerun.dll
    game_speed_control/
      manifest.json
      game_speed_control.dll
```

如果你是直接 clone 本仓库，也可以直接把仓库里的 `mods/` 文件夹复制到游戏目录。

## 从源码编译

源码工程使用 `.NET 9.0`，并引用《杀戮尖塔 2》游戏目录中的运行时 DLL。

编译“多人伤害统计”：

```powershell
cd "mod_src\multiplayer_damage_meter_v1.0.10"
dotnet build -c Release --no-restore
```

编译 `card_effect_tweaks`：

```powershell
cd "mod_src\card_effect_tweaks"
dotnet build -c Release --no-restore
```

编译 `pause_menu_rerun`：

```powershell
cd "mod_src\pause_menu_rerun"
dotnet build -c Release --no-restore
```

编译 `game_speed_control`：

```powershell
cd "mod_src\game_speed_control"
dotnet build -c Release --no-restore
```

运行 `game_speed_control` 的核心逻辑测试：

```powershell
dotnet run --project "mod_src\game_speed_control\tests\GameSpeedControl.Tests.csproj"
```

编译完成后，把生成的 DLL 从：

```text
bin\Release\netcoreapp9.0\
```

复制到 `mods/` 下对应的 Mod 文件夹里。

## 存档进度同步

如果你需要在多台电脑之间同步《杀戮尖塔 2》的存档进度，可以使用我的另一个工具：

[STS2-Save-Sync-Tool](https://github.com/lfyxhappy/STS2-Save-Sync-Tool)

这个工具用于同步存档进度，和本仓库里的 Mod 可以配合使用。

## 注意事项

- 本仓库内容面向《杀戮尖塔 2》。
- 发布新版本时，应同步更新 Mod 文件夹名、`manifest.json` 版本号、源码版本号和 Release tag。
- `mod_src/` 中包含源码和部分构建产物，主要用于记录当前可用版本的完整状态。
