# STS2mods / 杀戮尖塔2 Mod 合集

`Slay the Spire 2 Mods made by 良风有幸(Mr.Huang)`

这是 lfyxhappy 的《杀戮尖塔 2》（Slay the Spire 2 / STS2）Mod 集合仓库，包含可直接安装的 `mods/` 成品包和对应的 `mod_src/` 源码工程。适合想要多人伤害统计、游戏速度调节、暂停菜单重打、中文调试控制台、观者角色等功能的玩家。

## 下载

- 最新 Release：<https://github.com/lfyxhappy/STS2mods/releases/latest>
- 下载 `STS2mods-v1.0.16.zip`
- 解压后把压缩包里的 `mods` 文件夹复制到《杀戮尖塔 2》游戏目录

最终目录结构应类似这样：

```text
Slay the Spire 2/
  mods/
    multiplayer_damage_meter_v1.0.11/
    card_effect_tweaks/
    pause_menu_rerun/
    game_speed_control/
    chinese_debug_console/
    watcher_character/
```

如果你直接 clone 本仓库，也可以把仓库里的 `mods/` 文件夹复制到游戏目录。

## 已包含的 Mod

### 多人伤害统计

- 在多人模式中显示每名玩家的伤害统计。
- 统计普通伤害和被格挡伤害。
- 战斗结束后显示本场战斗伤害摘要。
- HUD 中分开显示“之前战斗”的累计伤害和“本场战斗”的当前伤害。
- 如果在战斗中 save/load，本场战斗伤害会重置为 `0`，避免当前战斗伤害被重复计算。

### 暂停菜单重打

- 在单人模式暂停菜单中添加绿色的“重打”按钮。
- 按钮会先执行游戏原本的“保存并退出”，回到主菜单后自动执行原本的“继续游戏”。
- 仅在单人模式显示，避免影响多人同步流程。

### 游戏速度调节

- 在暂停菜单中添加“速度”按钮。
- 倍率在 `1x -> 1.5x -> 2x -> 2.5x -> 3x -> 3.5x -> 4x -> 1x` 之间循环切换。
- 战斗中通过 Godot 全局 `Engine.TimeScale` 调整速度。
- 多人模式中以房主速度为准，客户端跟随房主速度。

### 观者角色

- 为《杀戮尖塔 2》加入观者角色。
- 包含 `Watcher.dll` 逻辑和 `Watcher.pck` 资源包。
- 本地目录名保持 `watcher_character`，manifest id 保持 `Watcher`，以兼容游戏 Mod loader 对 `Watcher.dll` / `Watcher.pck` 的加载规则。

### 中文调试控制台

- 游戏内按 `F10` 呼出或关闭中文覆盖层调试控制台。
- 顶部提供 `添加卡牌`、`移除卡牌`、`添加遗物`、`移除遗物` 四个快捷按钮。
- 支持按中文名、英文名或内部 ID 搜索卡牌/遗物。
- 支持中文命令，例如 `加卡 相信着你`、`删卡 打击`、`加遗物 锚`、`删遗物 锚`。

### Card Effect Tweaks

这是一个独立的 C# Harmony Mod 模板/脚手架，用来后续编写卡牌或效果调整类 Mod。

## 从源码编译

源码工程使用 `.NET 9.0`，并引用《杀戮尖塔 2》游戏目录中的运行时 DLL。游戏运行库 DLL 不会放进本仓库。

常见编译命令：

```powershell
dotnet build "mod_src\card_effect_tweaks\card_effect_tweaks.csproj" -c Release --no-restore
dotnet build "mod_src\pause_menu_rerun\pause_menu_rerun.csproj" -c Release --no-restore
dotnet build "mod_src\game_speed_control\game_speed_control.csproj" -c Release --no-restore
dotnet build "mod_src\chinese_debug_console\chinese_debug_console.csproj" -c Release --no-restore
```

编译观者反编译工程：

```powershell
.\mod_src\watcher_character\tools\build_decompiled_watcher.ps1 -GameRuntimeDir "C:\算法\小应用\Slay the Spire 2\data_sts2_windows_x86_64"
```

验证观者安装形态：

```powershell
.\mod_src\watcher_character\tools\verify_watcher_package.ps1
```

运行 `game_speed_control` 的核心逻辑测试：

```powershell
dotnet run --project "mod_src\game_speed_control\tests\GameSpeedControl.Tests.csproj"
```

## 存档进度同步

如果你需要同步官方档和 Modded 档的 `progress.save`，可以使用我的另一个工具：

[STS2-Save-Sync-Tool](https://github.com/lfyxhappy/STS2-Save-Sync-Tool)

## 注意事项

- 本仓库内容面向《杀戮尖塔 2》。
- 安装或更新 Mod 前，建议先备份游戏存档。
- 发布新版本时，应同步更新 Mod 文件夹名、`manifest.json` 版本号、源码版本号和 Release tag。
- `mod_src/` 中包含源码和部分构建产物，主要用于记录当前可用版本的完整状态。

## 关键词

Slay the Spire 2, STS2, STS2 mods, Slay the Spire 2 mods, Harmony mod, modded, multiplayer damage meter, game speed control, Watcher, Chinese debug console, WPF, .NET, Godot
