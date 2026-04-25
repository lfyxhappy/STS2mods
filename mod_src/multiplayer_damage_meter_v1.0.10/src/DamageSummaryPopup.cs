using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace MultiplayerDamageMeter;

public class DamageSummaryPopup : Control, IScreenContext
{
	private static readonly string VerticalPopupScenePath = SceneHelper.GetScenePath("ui/vertical_popup");

	private readonly Callable _closeCallable;

	private readonly NVerticalPopup _verticalPopup;

	public Control? DefaultFocusedControl => _verticalPopup.YesButton;

	public DamageSummaryPopup(IReadOnlyList<PlayerDamageSnapshot> snapshots)
	{
		IReadOnlyList<PlayerDamageSnapshot> orderedSnapshots = snapshots.OrderByDescending(static item => item.CombatDamage).ThenBy(static item => item.PlayerOrder).ToList();
		int totalDamage = orderedSnapshots.Sum(static item => item.CombatDamage);
		int maxDamage = orderedSnapshots.Count == 0 ? 0 : orderedSnapshots.Max(static item => item.CombatDamage);

		SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		MouseFilter = MouseFilterEnum.Stop;
		ProcessMode = ProcessModeEnum.Always;

		ColorRect scrim = new ColorRect
		{
			Color = new Color(0.03f, 0.06f, 0.1f, 0.12f),
			MouseFilter = MouseFilterEnum.Ignore
		};
		scrim.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		AddChild(scrim);

		CenterContainer centerContainer = new CenterContainer
		{
			MouseFilter = MouseFilterEnum.Ignore
		};
		centerContainer.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		AddChild(centerContainer);

		Control popupHost = CreatePopupHost();
		centerContainer.AddChild(popupHost);

		_verticalPopup = PreloadManager.Cache.GetScene(VerticalPopupScenePath).Instantiate<NVerticalPopup>(PackedScene.GenEditState.Disabled);
		_verticalPopup.CustomMinimumSize = new Vector2(768f, 624f);
		_verticalPopup.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		_verticalPopup.SizeFlagsVertical = SizeFlags.ExpandFill;
		_verticalPopup.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		popupHost.AddChild(_verticalPopup);

		_verticalPopup.SetText("本场伤害摘要", "已计入格挡伤害，不计超额击杀伤害");
		_verticalPopup.HideNoButton();

		_closeCallable = Callable.From<NButton>(OnCloseButtonPressed);
		ConfigureCloseButton();
		ApplyPopupTheme();
		DamageStatsGlassTheme.TryStylePopupShell(_verticalPopup);
		_verticalPopup.AddChild(CreateContentHost(orderedSnapshots, totalDamage, maxDamage));
	}

	public override void _ExitTree()
	{
		if (_verticalPopup.YesButton.IsConnected(NClickableControl.SignalName.Released, _closeCallable))
		{
			_verticalPopup.YesButton.Disconnect(NClickableControl.SignalName.Released, _closeCallable);
		}
	}

	private void ConfigureCloseButton()
	{
		NPopupYesNoButton closeButton = _verticalPopup.YesButton;
		closeButton.IsYes = true;
		closeButton.SetText("关闭");
		if (!closeButton.IsConnected(NClickableControl.SignalName.Released, _closeCallable))
		{
			closeButton.Connect(NClickableControl.SignalName.Released, _closeCallable);
		}

		DamageStatsGlassTheme.TryStylePopupButton(closeButton, primary: true);
	}

	private void ApplyPopupTheme()
	{
		if (_verticalPopup.GetNodeOrNull<MegaLabel>("Header") is MegaLabel header)
		{
			DamageStatsGlassTheme.ApplyLabelStyle(header, 30, DamageStatsGlassTheme.GlassText, outlineAlpha: 0.86f);
		}

		if (_verticalPopup.GetNodeOrNull<MegaRichTextLabel>("Description") is MegaRichTextLabel description)
		{
			DamageStatsGlassTheme.ApplyRichTextStyle(description, 18, DamageStatsGlassTheme.GlassTextMuted);
		}

		DamageStatsGlassTheme.TryStylePopupButton(_verticalPopup.NoButton, primary: false);
	}

	private static Control CreatePopupHost()
	{
		Control host = new Control
		{
			CustomMinimumSize = new Vector2(768f, 624f),
			SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
			SizeFlagsVertical = SizeFlags.ShrinkCenter,
			MouseFilter = MouseFilterEnum.Ignore
		};
		host.AddChild(DamageStatsGlassTheme.CreateGlassBackdrop(glassAlpha: 0.84f, blurRadius: 1.2f, tintMix: 0.34f, cornerRadius: 0.078f, brightnessBoost: 0.03f));
		host.AddChild(DamageStatsGlassTheme.CreateGlassHighlight(cornerRadius: 0.078f, opacity: 0.92f));

		PanelContainer frame = new PanelContainer
		{
			MouseFilter = MouseFilterEnum.Ignore
		};
		frame.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		frame.AddThemeStyleboxOverride("panel", DamageStatsGlassTheme.CreateShellStyle(backgroundAlpha: 0.04f, borderAlpha: 0.24f, radius: 24, shadowSize: 26, shadowAlpha: 0.3f));
		host.AddChild(frame);
		return host;
	}

	private static Control CreateContentHost(IReadOnlyList<PlayerDamageSnapshot> orderedSnapshots, int totalDamage, int maxDamage)
	{
		MarginContainer host = new MarginContainer
		{
			MouseFilter = MouseFilterEnum.Ignore
		};
		host.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		host.AddThemeConstantOverride(ThemeConstants.MarginContainer.MarginLeft, 50);
		host.AddThemeConstantOverride(ThemeConstants.MarginContainer.MarginRight, 50);
		host.AddThemeConstantOverride(ThemeConstants.MarginContainer.MarginTop, 152);
		host.AddThemeConstantOverride(ThemeConstants.MarginContainer.MarginBottom, 118);

		VBoxContainer content = new VBoxContainer
		{
			MouseFilter = MouseFilterEnum.Ignore
		};
		content.AddThemeConstantOverride(ThemeConstants.BoxContainer.Separation, 12);
		host.AddChild(content);

		content.AddChild(CreateDivider(DamageStatsGlassTheme.Alpha(DamageStatsGlassTheme.GlassAccentGold, 0.16f)));
		content.AddChild(CreateTotalRow(totalDamage));
		content.AddChild(CreateDivider(DamageStatsGlassTheme.GlassDivider));

		VBoxContainer list = new VBoxContainer
		{
			MouseFilter = MouseFilterEnum.Ignore
		};
		list.AddThemeConstantOverride(ThemeConstants.BoxContainer.Separation, 8);
		content.AddChild(list);

		int rank = 1;
		foreach (PlayerDamageSnapshot snapshot in orderedSnapshots)
		{
			list.AddChild(CreateRankRow(snapshot, rank, maxDamage));
			rank++;
		}

		return host;
	}

	private static Control CreateRankRow(PlayerDamageSnapshot snapshot, int rank, int maxDamage)
	{
		PopupRowTheme theme = GetRowTheme(snapshot, rank);
		PanelContainer rowPanel = new PanelContainer
		{
			CustomMinimumSize = new Vector2(0f, 72f),
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			MouseFilter = MouseFilterEnum.Ignore
		};
		rowPanel.AddThemeStyleboxOverride("panel", CreateRowPanelStyle(theme));

		MarginContainer rowMargin = new MarginContainer
		{
			MouseFilter = MouseFilterEnum.Ignore
		};
		rowMargin.AddThemeConstantOverride(ThemeConstants.MarginContainer.MarginLeft, 12);
		rowMargin.AddThemeConstantOverride(ThemeConstants.MarginContainer.MarginRight, 12);
		rowMargin.AddThemeConstantOverride(ThemeConstants.MarginContainer.MarginTop, 8);
		rowMargin.AddThemeConstantOverride(ThemeConstants.MarginContainer.MarginBottom, 8);
		rowPanel.AddChild(rowMargin);

		HBoxContainer row = new HBoxContainer
		{
			MouseFilter = MouseFilterEnum.Ignore
		};
		row.AddThemeConstantOverride(ThemeConstants.BoxContainer.Separation, 10);
		rowMargin.AddChild(row);

		row.AddChild(CreateAccentStrip(theme.AccentStripColor));
		row.AddChild(CreateFixedLabelCell($"#{rank}", 44f, 46f, 18, theme.AccentColor, HorizontalAlignment.Center));
		row.AddChild(CreateNameCell(snapshot.PlayerName, theme.PrimaryTextColor));
		row.AddChild(CreateBarCell(snapshot.CombatDamage, maxDamage, theme));
		row.AddChild(CreateValueCell(snapshot.CombatDamage, snapshot.RunDamage, theme));
		return rowPanel;
	}

	private static Control CreateTotalRow(int totalDamage)
	{
		PanelContainer panel = new PanelContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			MouseFilter = MouseFilterEnum.Ignore
		};
		panel.AddThemeStyleboxOverride("panel", CreateMetaPanelStyle());

		MarginContainer margin = new MarginContainer
		{
			MouseFilter = MouseFilterEnum.Ignore
		};
		margin.AddThemeConstantOverride(ThemeConstants.MarginContainer.MarginLeft, 14);
		margin.AddThemeConstantOverride(ThemeConstants.MarginContainer.MarginRight, 14);
		margin.AddThemeConstantOverride(ThemeConstants.MarginContainer.MarginTop, 10);
		margin.AddThemeConstantOverride(ThemeConstants.MarginContainer.MarginBottom, 10);
		panel.AddChild(margin);

		HBoxContainer row = new HBoxContainer
		{
			MouseFilter = MouseFilterEnum.Ignore
		};
		row.AddThemeConstantOverride(ThemeConstants.BoxContainer.Separation, 10);
		margin.AddChild(row);
		row.AddChild(CreateText("总伤害", 15, DamageStatsGlassTheme.GlassTextMuted, outline: true, alignment: HorizontalAlignment.Left));

		Control spacer = new Control
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			MouseFilter = MouseFilterEnum.Ignore
		};
		row.AddChild(spacer);
		row.AddChild(CreateText(totalDamage.ToString(), 28, DamageStatsGlassTheme.GlassAccentGold, outline: true, alignment: HorizontalAlignment.Right));
		return panel;
	}

	private static Control CreateAccentStrip(Color color)
	{
		return new ColorRect
		{
			Color = color,
			CustomMinimumSize = new Vector2(5f, 46f),
			MouseFilter = MouseFilterEnum.Ignore
		};
	}

	private static Control CreateDivider(Color color)
	{
		return new ColorRect
		{
			Color = color,
			CustomMinimumSize = new Vector2(0f, 1f),
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			MouseFilter = MouseFilterEnum.Ignore
		};
	}

	private static Control CreateNameCell(string playerName, Color textColor)
	{
		Control cell = CreateFixedCell(178f, 46f);
		MegaLabel label = new MegaLabel
		{
			AutoSizeEnabled = false,
			Text = playerName,
			AutowrapMode = TextServer.AutowrapMode.Off,
			TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Center,
			MouseFilter = MouseFilterEnum.Ignore
		};
		DamageStatsGlassTheme.ApplyLabelStyle(label, 18, textColor, outlineAlpha: 0.82f);
		AttachFullRectChild(cell, label);
		return cell;
	}

	private static Control CreateBarCell(int combatDamage, int maxDamage, PopupRowTheme theme)
	{
		Control cell = new Control
		{
			CustomMinimumSize = new Vector2(0f, 46f),
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			MouseFilter = MouseFilterEnum.Ignore
		};

		ProgressBar bar = new ProgressBar
		{
			MinValue = 0,
			MaxValue = maxDamage <= 0 ? 1 : maxDamage,
			Value = combatDamage,
			ShowPercentage = false,
			CustomMinimumSize = new Vector2(0f, 10f),
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			MouseFilter = MouseFilterEnum.Ignore
		};
		bar.AddThemeStyleboxOverride("fill", CreateBarFillStyle(theme));
		bar.AddThemeStyleboxOverride("background", CreateBarBackgroundStyle(theme));
		bar.AnchorLeft = 0f;
		bar.AnchorRight = 1f;
		bar.AnchorTop = 0.5f;
		bar.AnchorBottom = 0.5f;
		bar.OffsetTop = -5f;
		bar.OffsetBottom = 5f;
		cell.AddChild(bar);
		return cell;
	}

	private static Control CreateValueCell(int combatDamage, int runDamage, PopupRowTheme theme)
	{
		Control cell = CreateFixedCell(108f, 46f);
		VBoxContainer values = new VBoxContainer
		{
			MouseFilter = MouseFilterEnum.Ignore
		};
		values.AddThemeConstantOverride(ThemeConstants.BoxContainer.Separation, -4);
		values.Alignment = BoxContainer.AlignmentMode.Center;

		MegaLabel combatLabel = new MegaLabel
		{
			AutoSizeEnabled = false,
			Text = combatDamage.ToString(),
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Center,
			MouseFilter = MouseFilterEnum.Ignore
		};
		DamageStatsGlassTheme.ApplyLabelStyle(combatLabel, 22, theme.AccentColor, outlineAlpha: 0.9f);

		MegaLabel runLabel = new MegaLabel
		{
			AutoSizeEnabled = false,
			Text = $"累计 {runDamage}",
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Center,
			MouseFilter = MouseFilterEnum.Ignore
		};
		DamageStatsGlassTheme.ApplyLabelStyle(runLabel, 12, theme.SecondaryTextColor, outlineAlpha: 0.76f);

		values.AddChild(combatLabel);
		values.AddChild(runLabel);
		AttachFullRectChild(cell, values);
		return cell;
	}

	private static Control CreateFixedLabelCell(string text, float width, float height, int fontSize, Color primaryColor, HorizontalAlignment alignment)
	{
		Control cell = CreateFixedCell(width, height);
		MegaLabel label = new MegaLabel
		{
			AutoSizeEnabled = false,
			Text = text,
			HorizontalAlignment = alignment,
			VerticalAlignment = VerticalAlignment.Center,
			MouseFilter = MouseFilterEnum.Ignore
		};
		DamageStatsGlassTheme.ApplyLabelStyle(label, fontSize, primaryColor, outlineAlpha: 0.84f);
		AttachFullRectChild(cell, label);
		return cell;
	}

	private static Control CreateFixedCell(float width, float height)
	{
		return new Control
		{
			CustomMinimumSize = new Vector2(width, height),
			SizeFlagsHorizontal = SizeFlags.ShrinkBegin,
			MouseFilter = MouseFilterEnum.Ignore
		};
	}

	private static void AttachFullRectChild(Control parent, Control child)
	{
		child.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		parent.AddChild(child);
	}

	private static PopupRowTheme GetRowTheme(PlayerDamageSnapshot snapshot, int rank)
	{
		if (snapshot.CombatDamage <= 0)
		{
			return new PopupRowTheme(new Color(0.72f, 0.78f, 0.86f, 1f), DamageStatsGlassTheme.Alpha(DamageStatsGlassTheme.GlassTextMuted, 0.72f), new Color(0.68f, 0.74f, 0.82f, 0.84f), new Color(0.74f, 0.82f, 0.9f, 0.08f), new Color(1f, 1f, 1f, 0.08f), new Color(0.76f, 0.82f, 0.88f, 0.32f), new Color(1f, 1f, 1f, 0.08f), new Color(0.82f, 0.88f, 0.94f, 0.42f));
		}

		return rank switch
		{
			1 => new PopupRowTheme(DamageStatsGlassTheme.GlassAccentGold, DamageStatsGlassTheme.GlassText, DamageStatsGlassTheme.Alpha(DamageStatsGlassTheme.GlassTextMuted, 0.92f), new Color(0.90f, 0.80f, 0.58f, 0.14f), new Color(0.97f, 0.90f, 0.72f, 0.22f), new Color(0.96f, 0.84f, 0.56f, 0.9f), new Color(1f, 1f, 1f, 0.08f), new Color(0.96f, 0.84f, 0.56f, 0.72f)),
			2 => new PopupRowTheme(DamageStatsGlassTheme.GlassAccentSilver, DamageStatsGlassTheme.GlassText, DamageStatsGlassTheme.Alpha(DamageStatsGlassTheme.GlassTextMuted, 0.9f), new Color(0.82f, 0.88f, 0.96f, 0.13f), new Color(0.92f, 0.96f, 1f, 0.2f), new Color(0.85f, 0.90f, 0.97f, 0.88f), new Color(1f, 1f, 1f, 0.08f), new Color(0.85f, 0.90f, 0.97f, 0.66f)),
			3 => new PopupRowTheme(DamageStatsGlassTheme.GlassAccentBronze, DamageStatsGlassTheme.GlassText, DamageStatsGlassTheme.Alpha(DamageStatsGlassTheme.GlassTextMuted, 0.88f), new Color(0.88f, 0.76f, 0.58f, 0.13f), new Color(0.95f, 0.82f, 0.64f, 0.22f), new Color(0.93f, 0.73f, 0.48f, 0.88f), new Color(1f, 1f, 1f, 0.08f), new Color(0.93f, 0.73f, 0.48f, 0.68f)),
			_ => new PopupRowTheme(DamageStatsGlassTheme.GlassAccentNeutral, DamageStatsGlassTheme.GlassText, DamageStatsGlassTheme.Alpha(DamageStatsGlassTheme.GlassTextMuted, 0.86f), new Color(0.78f, 0.84f, 0.92f, 0.11f), new Color(1f, 1f, 1f, 0.12f), new Color(0.9f, 0.95f, 1f, 0.82f), new Color(1f, 1f, 1f, 0.08f), new Color(0.96f, 0.84f, 0.56f, 0.36f))
		};
	}

	private static StyleBoxFlat CreateMetaPanelStyle()
	{
		return DamageStatsGlassTheme.CreateGlassPanelStyle(DamageStatsGlassTheme.GlassAccentGold, backgroundAlpha: 0.12f, borderAlpha: 0.18f, radius: 16, shadowSize: 6);
	}

	private static StyleBoxFlat CreateRowPanelStyle(PopupRowTheme theme)
	{
		StyleBoxFlat style = DamageStatsGlassTheme.CreateGlassPanelStyle(theme.AccentColor, theme.BackgroundColor.A, theme.BorderColor.A, radius: 16, shadowSize: 5);
		style.BgColor = theme.BackgroundColor;
		style.BorderColor = theme.BorderColor;
		return style;
	}

	private static StyleBoxFlat CreateBarBackgroundStyle(PopupRowTheme theme)
	{
		StyleBoxFlat style = DamageStatsGlassTheme.CreateGlassBarTrackStyle(theme.AccentColor);
		style.BgColor = theme.BarBackgroundColor;
		return style;
	}

	private static StyleBoxFlat CreateBarFillStyle(PopupRowTheme theme)
	{
		StyleBoxFlat style = DamageStatsGlassTheme.CreateGlassBarFillStyle(theme.AccentColor);
		style.BgColor = theme.BarFillColor;
		return style;
	}

	private static MegaLabel CreateText(string text, int fontSize, Color color, bool outline, HorizontalAlignment alignment)
	{
		MegaLabel label = new MegaLabel
		{
			AutoSizeEnabled = false,
			Text = text,
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
			HorizontalAlignment = alignment,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			MouseFilter = MouseFilterEnum.Ignore
		};
		DamageStatsGlassTheme.ApplyLabelStyle(label, fontSize, color, outline ? 0.86f : 0.68f);
		return label;
	}

	private void OnCloseButtonPressed(NButton _)
	{
		Close();
	}

	private void Close()
	{
		NModalContainer.Instance?.Clear();
	}

	private sealed record PopupRowTheme(Color AccentColor, Color PrimaryTextColor, Color SecondaryTextColor, Color BackgroundColor, Color BorderColor, Color BarFillColor, Color BarBackgroundColor, Color AccentStripColor);
}
