using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace MultiplayerDamageMeter;

public sealed class DamageStatsHudWindow : PanelContainer
{
	private const float HudMinWidth = 228f;

	private const float TitleBarMinHeight = 40f;

	private const float CardMinWidth = 212f;

	private const float CardMinHeight = 96f;

	private const float HudDefaultLeftMargin = 18f;

	private const float HudDefaultMinimumTopMargin = 120f;

	private const float HudBackgroundAlpha = 0.66f;

	private const float TitleBarBackgroundAlpha = 0.74f;

	private const float CardBackgroundAlpha = 0.62f;

	private const string UiConfigFileName = "damage_stats_ui.json";

	private const string UiConfigDirectoryName = "ui";

	private const string UiStorageDirectoryName = "multiplayer_damage_meter";

	private const int CurrentUiLayoutVersion = 2;

	private static readonly object UiConfigLock = new object();

	private static PersistedDamageStatsUiConfig? _cachedUiConfig;

	private readonly VBoxContainer _rootContainer;

	private readonly PanelContainer _titleBar;

	private readonly MegaLabel _titleLabel;

	private readonly GridContainer _cardGrid;

	private readonly Dictionary<ulong, DamageStatsHudCardView> _cardsByPlayerId = new Dictionary<ulong, DamageStatsHudCardView>();

	private IReadOnlyList<MegaCrit.Sts2.Core.Entities.Players.Player> _players = Array.Empty<MegaCrit.Sts2.Core.Entities.Players.Player>();

	private Control? _anchorControl;

	private bool _isDragging;

	private Vector2 _dragPointerOffset;

	private bool _placementQueued;

	public DamageStatsHudWindow()
	{
		MouseFilter = MouseFilterEnum.Stop;
		MouseDefaultCursorShape = CursorShape.Move;
		ZIndex = 20;
		CustomMinimumSize = new Vector2(HudMinWidth, 0f);
		AddThemeStyleboxOverride("panel", DamageStatsGlassTheme.CreateShellStyle(backgroundAlpha: 0.05f, borderAlpha: 0.18f, radius: 22, shadowSize: 18, shadowAlpha: 0.22f));

		Control surface = new Control
		{
			MouseFilter = MouseFilterEnum.Ignore
		};
		surface.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		AddChild(surface);

		surface.AddChild(DamageStatsGlassTheme.CreateGlassBackdrop(glassAlpha: 0.82f, blurRadius: 1.12f, tintMix: 0.3f, cornerRadius: 0.105f, brightnessBoost: 0.035f));
		surface.AddChild(DamageStatsGlassTheme.CreateGlassHighlight(cornerRadius: 0.105f, opacity: 0.84f));

		MarginContainer outerMargin = new MarginContainer
		{
			MouseFilter = MouseFilterEnum.Ignore
		};
		outerMargin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		outerMargin.AddThemeConstantOverride("margin_left", 12);
		outerMargin.AddThemeConstantOverride("margin_top", 12);
		outerMargin.AddThemeConstantOverride("margin_right", 12);
		outerMargin.AddThemeConstantOverride("margin_bottom", 12);
		surface.AddChild(outerMargin);

		_rootContainer = new VBoxContainer
		{
			MouseFilter = MouseFilterEnum.Ignore
		};
		_rootContainer.AddThemeConstantOverride("separation", 9);
		outerMargin.AddChild(_rootContainer);

		_titleBar = new PanelContainer
		{
			MouseFilter = MouseFilterEnum.Ignore,
			CustomMinimumSize = new Vector2(0f, TitleBarMinHeight)
		};
		_titleBar.AddThemeStyleboxOverride("panel", DamageStatsGlassTheme.CreateGlassPanelStyle(DamageStatsGlassTheme.GlassAccentGold, backgroundAlpha: 0.16f, borderAlpha: 0.24f, radius: 18, shadowSize: 8));
		_rootContainer.AddChild(_titleBar);

		MarginContainer titleMargin = new MarginContainer
		{
			MouseFilter = MouseFilterEnum.Ignore
		};
		titleMargin.AddThemeConstantOverride("margin_left", 12);
		titleMargin.AddThemeConstantOverride("margin_top", 7);
		titleMargin.AddThemeConstantOverride("margin_right", 12);
		titleMargin.AddThemeConstantOverride("margin_bottom", 7);
		_titleBar.AddChild(titleMargin);

		_titleLabel = new MegaLabel
		{
			AutoSizeEnabled = false,
			MouseFilter = MouseFilterEnum.Ignore,
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Center,
			Text = "伤害统计"
		};
		DamageStatsGlassTheme.ApplyLabelStyle(_titleLabel, 20, DamageStatsGlassTheme.GlassText, outlineAlpha: 0.82f);
		titleMargin.AddChild(_titleLabel);

		_cardGrid = new GridContainer
		{
			MouseFilter = MouseFilterEnum.Ignore,
			Columns = 1
		};
		_cardGrid.AddThemeConstantOverride("h_separation", 10);
		_cardGrid.AddThemeConstantOverride("v_separation", 10);
		_rootContainer.AddChild(_cardGrid);
	}

	public override void _Input(InputEvent inputEvent)
	{
		base._Input(inputEvent);
		if (inputEvent is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
		{
			if (mouseButton.Pressed && IsMouseWithinWindow())
			{
				MoveToFront();
				_isDragging = true;
				_dragPointerOffset = GetGlobalMousePosition() - GlobalPosition;
				GetViewport().SetInputAsHandled();
			}
			else if (_isDragging)
			{
				_isDragging = false;
				Position = ClampLocalPosition(GlobalPositionToLocalPosition(GlobalPosition));
				SaveUiPosition(Position);
				GetViewport().SetInputAsHandled();
			}
		}

		if (inputEvent is InputEventMouseMotion inputEventMouseMotion && _isDragging)
		{
			Vector2 desiredGlobalPosition = GetGlobalMousePosition() - _dragPointerOffset;
			Position = ClampLocalPosition(GlobalPositionToLocalPosition(desiredGlobalPosition));
			GetViewport().SetInputAsHandled();
		}
	}

	public void Configure(RunState runState, Control? anchorControl)
	{
		_players = runState.Players;
		_anchorControl = anchorControl;
		_cardGrid.Columns = (_players.Count <= 1) ? 1 : 2;
		RebuildCards();
	}

	public void QueueInitialPlacement()
	{
		if (_placementQueued)
		{
			return;
		}

		_placementQueued = true;
		ApplyInitialPlacementAfterFrame();
	}

	public void RefreshAll()
	{
		bool isInCombat = CombatManager.Instance != null && CombatManager.Instance.IsInProgress;
		for (int i = 0; i < _players.Count; i++)
		{
			RefreshCard(_players[i].NetId, isInCombat);
		}
	}

	public void RefreshPlayer(ulong playerId)
	{
		RefreshCard(playerId, CombatManager.Instance != null && CombatManager.Instance.IsInProgress);
	}

	private async void ApplyInitialPlacementAfterFrame()
	{
		SceneTree? tree = GetTree();
		if (tree == null)
		{
			return;
		}

		await ToSignal(tree, SceneTree.SignalName.ProcessFrame);
		if (!GodotObject.IsInstanceValid(this))
		{
			return;
		}

		bool hasSavedPosition = TryLoadSavedPosition(out Vector2 savedPosition);
		Vector2 position = hasSavedPosition ? savedPosition : GetDefaultLocalPosition();
		Position = ClampLocalPosition(position);
		if (!hasSavedPosition)
		{
			SaveUiPosition(Position);
		}
	}

	private void RebuildCards()
	{
		foreach (Node child in _cardGrid.GetChildren())
		{
			child.QueueFree();
		}

		_cardsByPlayerId.Clear();
		for (int i = 0; i < _players.Count; i++)
		{
			MegaCrit.Sts2.Core.Entities.Players.Player player = _players[i];
			DamageStatsHudCardView view = CreateCardView();
			string playerName = ResolvePlayerDisplayName(player);
			view.NameLabel.Text = playerName;
			_cardGrid.AddChild(view.Container);
			_cardsByPlayerId[player.NetId] = view;
		}
	}

	private DamageStatsHudCardView CreateCardView()
	{
		PanelContainer container = new PanelContainer
		{
			MouseFilter = MouseFilterEnum.Ignore,
			CustomMinimumSize = new Vector2(CardMinWidth, CardMinHeight),
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		container.AddThemeStyleboxOverride("panel", DamageStatsGlassTheme.CreateGlassPanelStyle(DamageStatsGlassTheme.GlassAccentNeutral, backgroundAlpha: 0.14f, borderAlpha: 0.16f, radius: 18, shadowSize: 7));

		MarginContainer margin = new MarginContainer
		{
			MouseFilter = MouseFilterEnum.Ignore
		};
		margin.AddThemeConstantOverride("margin_left", 12);
		margin.AddThemeConstantOverride("margin_top", 10);
		margin.AddThemeConstantOverride("margin_right", 12);
		margin.AddThemeConstantOverride("margin_bottom", 10);
		container.AddChild(margin);

		VBoxContainer content = new VBoxContainer
		{
			MouseFilter = MouseFilterEnum.Ignore
		};
		content.AddThemeConstantOverride("separation", 4);
		margin.AddChild(content);

		MegaLabel nameLabel = new MegaLabel
		{
			AutoSizeEnabled = false,
			MouseFilter = MouseFilterEnum.Ignore,
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Center,
			ClipText = true,
			TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,
			CustomMinimumSize = new Vector2(188f, 22f)
		};
		DamageStatsGlassTheme.ApplyLabelStyle(nameLabel, 16, DamageStatsGlassTheme.GlassText, outlineAlpha: 0.8f);
		content.AddChild(nameLabel);

		MegaLabel combatLabel = new MegaLabel
		{
			AutoSizeEnabled = false,
			MouseFilter = MouseFilterEnum.Ignore,
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Center,
			CustomMinimumSize = new Vector2(188f, 28f)
		};
		DamageStatsGlassTheme.ApplyLabelStyle(combatLabel, 20, DamageStatsGlassTheme.GlassAccentGold, outlineAlpha: 0.92f);
		content.AddChild(combatLabel);

		MegaLabel runLabel = new MegaLabel
		{
			AutoSizeEnabled = false,
			MouseFilter = MouseFilterEnum.Ignore,
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Center,
			CustomMinimumSize = new Vector2(188f, 22f)
		};
		DamageStatsGlassTheme.ApplyLabelStyle(runLabel, 15, DamageStatsGlassTheme.GlassTextMuted, outlineAlpha: 0.8f);
		content.AddChild(runLabel);

		return new DamageStatsHudCardView(container, nameLabel, combatLabel, runLabel);
	}

	private void RefreshCard(ulong playerId, bool isInCombat)
	{
		if (!_cardsByPlayerId.TryGetValue(playerId, out DamageStatsHudCardView? view))
		{
			return;
		}

		view.CombatLabel.SetTextAutoSize($"本场战斗 {DamageStatsService.GetCombatTotal(playerId)}");
		view.CombatLabel.Visible = isInCombat;
		view.RunLabel.SetTextAutoSize($"之前战斗 {DamageStatsService.GetRunTotal(playerId)}");
	}

	private Vector2 GetDefaultLocalPosition()
	{
		Control? parentControl = GetParent<Control>();
		if (parentControl == null)
		{
			return new Vector2(HudDefaultLeftMargin, 240f);
		}

		Vector2 viewportSize = GetViewportRect().Size;
		Vector2 parentGlobalPosition = parentControl.GlobalPosition;
		float windowHeight = EstimateHudHeight();
		float desiredGlobalY = Mathf.Max(HudDefaultMinimumTopMargin, viewportSize.Y * 0.5f - windowHeight * 0.5f);
		return new Vector2(HudDefaultLeftMargin - parentGlobalPosition.X, desiredGlobalY - parentGlobalPosition.Y);
	}

	private Vector2 ClampLocalPosition(Vector2 desiredLocalPosition)
	{
		Control? parentControl = GetParent<Control>();
		if (parentControl == null)
		{
			return desiredLocalPosition;
		}

		const float padding = 8f;
		Vector2 parentGlobalPosition = parentControl.GlobalPosition;
		Vector2 desiredGlobalPosition = parentGlobalPosition + desiredLocalPosition;
		Vector2 viewportSize = GetViewportRect().Size;
		float maxX = Mathf.Max(padding, viewportSize.X - Size.X - padding);
		float maxY = Mathf.Max(padding, viewportSize.Y - Size.Y - padding);
		Vector2 clampedGlobalPosition = new Vector2(Mathf.Clamp(desiredGlobalPosition.X, padding, maxX), Mathf.Clamp(desiredGlobalPosition.Y, padding, maxY));
		return clampedGlobalPosition - parentGlobalPosition;
	}

	private bool IsMouseWithinWindow()
	{
		Rect2 globalRect = GetGlobalRect();
		return globalRect.HasArea() && globalRect.HasPoint(GetGlobalMousePosition());
	}

	private Vector2 GlobalPositionToLocalPosition(Vector2 globalPosition)
	{
		Control? parentControl = GetParent<Control>();
		return parentControl == null ? globalPosition : globalPosition - parentControl.GlobalPosition;
	}

	private static bool TryLoadSavedPosition(out Vector2 position)
	{
		lock (UiConfigLock)
		{
			PersistedDamageStatsUiConfig config = GetPersistedUiConfig();
			if (config.LayoutVersion != CurrentUiLayoutVersion || !config.HudLocalPositionX.HasValue || !config.HudLocalPositionY.HasValue)
			{
				position = default;
				return false;
			}

			position = new Vector2(config.HudLocalPositionX.Value, config.HudLocalPositionY.Value);
			return true;
		}
	}

	private static void SaveUiPosition(Vector2 position)
	{
		lock (UiConfigLock)
		{
			PersistedDamageStatsUiConfig config = GetPersistedUiConfig();
			config.LayoutVersion = CurrentUiLayoutVersion;
			config.HudLocalPositionX = position.X;
			config.HudLocalPositionY = position.Y;
			SavePersistedUiConfig(config);
		}
	}

	private float EstimateHudHeight()
	{
		if (Size.Y > 0f)
		{
			return Size.Y;
		}

		int rows = Math.Max(1, (_players.Count + _cardGrid.Columns - 1) / _cardGrid.Columns);
		return 86f + rows * (CardMinHeight + 10f) + 24f;
	}

	private static PersistedDamageStatsUiConfig GetPersistedUiConfig()
	{
		if (_cachedUiConfig != null)
		{
			return _cachedUiConfig;
		}

		string configPath = GetUiConfigPath();
		if (TryReadUiConfig(configPath, out PersistedDamageStatsUiConfig? config))
		{
			_cachedUiConfig = config!;
			return _cachedUiConfig;
		}

		string legacyConfigPath = GetLegacyUiConfigPath();
		if (TryReadUiConfig(legacyConfigPath, out config))
		{
			_cachedUiConfig = config!;
			return _cachedUiConfig;
		}

		_cachedUiConfig = new PersistedDamageStatsUiConfig();
		return _cachedUiConfig;
	}

	private static bool TryReadUiConfig(string configPath, out PersistedDamageStatsUiConfig? config)
	{
		config = null;
		if (!File.Exists(configPath))
		{
			return false;
		}

		try
		{
			string json = File.ReadAllText(configPath);
			config = JsonSerializer.Deserialize<PersistedDamageStatsUiConfig>(json) ?? new PersistedDamageStatsUiConfig();
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static void SavePersistedUiConfig(PersistedDamageStatsUiConfig config)
	{
		string configPath = GetUiConfigPath();
		string? directoryPath = Path.GetDirectoryName(configPath);
		if (!string.IsNullOrEmpty(directoryPath))
		{
			Directory.CreateDirectory(directoryPath);
		}

		string tempPath = configPath + ".tmp";
		string json = JsonSerializer.Serialize(config);
		File.WriteAllText(tempPath, json);
		File.Copy(tempPath, configPath, overwrite: true);
		File.Delete(tempPath);
		_cachedUiConfig = config;
	}

	private static string GetUiConfigPath()
	{
		if (SaveManager.Instance == null)
		{
			return GetLegacyUiConfigPath();
		}

		string relativePath = Path.Combine(UserDataPathProvider.SavesDir, UiStorageDirectoryName, UiConfigDirectoryName, UiConfigFileName).Replace('\\', '/');
		string godotPath = SaveManager.Instance.GetProfileScopedPath(relativePath);
		return ProjectSettings.GlobalizePath(godotPath);
	}

	private static string GetLegacyUiConfigPath()
	{
		string baseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppContext.BaseDirectory;
		return Path.Combine(baseDirectory, UiConfigFileName);
	}

	private static string ResolvePlayerDisplayName(MegaCrit.Sts2.Core.Entities.Players.Player player)
	{
		RunManager? runManager = RunManager.Instance;
		if (runManager == null || runManager.IsSinglePlayerOrFakeMultiplayer)
		{
			return player.Character.Title.GetFormattedText();
		}

		string name = PlatformUtil.GetPlayerName(runManager.NetService.Platform, player.NetId);
		if (string.IsNullOrWhiteSpace(name) || string.Equals(name, player.NetId.ToString(), StringComparison.Ordinal))
		{
			return player.Character.Title.GetFormattedText();
		}

		return name;
	}
}

internal sealed record DamageStatsHudCardView(PanelContainer Container, MegaLabel NameLabel, MegaLabel CombatLabel, MegaLabel RunLabel);

internal sealed class PersistedDamageStatsUiConfig
{
	public int LayoutVersion { get; set; }

	public float? HudLocalPositionX { get; set; }

	public float? HudLocalPositionY { get; set; }
}
