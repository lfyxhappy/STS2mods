using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes;

namespace ChineseDebugConsole;

internal sealed class ChineseConsoleOverlay : Control
{
	private enum PickerMode
	{
		None,
		AddCard,
		RemoveCard,
		AddRelic,
		RemoveRelic
	}

	private static readonly Color BackdropColor = new(0.02f, 0.018f, 0.014f, 0.74f);
	private static readonly Color PanelColor = new(0.08f, 0.075f, 0.065f, 0.96f);
	private static readonly Color BorderColor = new(0.82f, 0.70f, 0.46f, 0.75f);
	private static readonly Color TextColor = new(0.92f, 0.88f, 0.78f);
	private static readonly Color MutedTextColor = new(0.68f, 0.62f, 0.52f);

	private readonly ChineseConsoleRuntimeCatalog _catalog = new();
	private readonly List<string> _logLines = [];

	private ChineseConsoleGameActions _actions = null!;
	private Panel _panel = null!;
	private RichTextLabel _output = null!;
	private LineEdit _commandInput = null!;
	private LineEdit _searchInput = null!;
	private Label _pickerTitle = null!;
	private VBoxContainer _pickerList = null!;
	private PickerMode _pickerMode;
	private ChineseConsoleCommandService? _commandService;

	public static ChineseConsoleOverlay? Instance { get; private set; }

	public static void EnsureMounted(NGame game)
	{
		if (Instance != null && GodotObject.IsInstanceValid(Instance))
		{
			return;
		}

		ChineseConsoleOverlay overlay = new()
		{
			Name = "ChineseDebugConsoleOverlay",
			Visible = false
		};
		game.AddChild(overlay);
		Instance = overlay;
	}

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _ExitTree()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public override void _Ready()
	{
		_actions = new ChineseConsoleGameActions(_catalog);
		SetAnchorsPreset(LayoutPreset.FullRect);
		MouseFilter = MouseFilterEnum.Stop;
		ZIndex = 4096;
		BuildUi();
		AppendLog("中文调试控制台已载入。输入“帮助”查看命令。", success: true);
	}

	public bool HandleGlobalInput(InputEvent inputEvent)
	{
		if (inputEvent is not InputEventKey { Pressed: true, Echo: false } inputEventKey)
		{
			return false;
		}

		if (IsToggleKey(inputEventKey))
		{
			Toggle();
			GetViewport().SetInputAsHandled();
			return true;
		}

		if (!Visible)
		{
			return false;
		}

		if (inputEventKey.Keycode == Key.Escape)
		{
			HideOverlay();
			GetViewport().SetInputAsHandled();
			return true;
		}

		return false;
	}

	private void BuildUi()
	{
		ColorRect backdrop = new()
		{
			Color = BackdropColor,
			MouseFilter = MouseFilterEnum.Stop
		};
		backdrop.SetAnchorsPreset(LayoutPreset.FullRect);
		AddChild(backdrop);

		_panel = new Panel
		{
			CustomMinimumSize = new Vector2(1180f, 760f),
			MouseFilter = MouseFilterEnum.Stop
		};
		_panel.SetAnchorsPreset(LayoutPreset.Center);
		_panel.OffsetLeft = -590f;
		_panel.OffsetRight = 590f;
		_panel.OffsetTop = -380f;
		_panel.OffsetBottom = 380f;
		_panel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
		AddChild(_panel);

		MarginContainer margin = new()
		{
			OffsetLeft = 24f,
			OffsetTop = 20f,
			OffsetRight = -24f,
			OffsetBottom = -20f
		};
		margin.SetAnchorsPreset(LayoutPreset.FullRect);
		_panel.AddChild(margin);

		VBoxContainer root = new()
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill
		};
		root.AddThemeConstantOverride("separation", 12);
		margin.AddChild(root);

		root.AddChild(BuildHeader());
		root.AddChild(BuildQuickActions());

		HSplitContainer split = new()
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill,
			SplitOffset = 500
		};
		root.AddChild(split);

		split.AddChild(BuildConsolePanel());
		split.AddChild(BuildPickerPanel());
	}

	private Control BuildHeader()
	{
		HBoxContainer header = new()
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		header.AddThemeConstantOverride("separation", 12);

		Label title = CreateLabel("中文调试控制台", 26, TextColor);
		title.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		header.AddChild(title);

		Button close = CreateButton("关闭");
		close.Pressed += HideOverlay;
		header.AddChild(close);

		return header;
	}

	private Control BuildQuickActions()
	{
		HBoxContainer row = new()
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		row.AddThemeConstantOverride("separation", 10);

		row.AddChild(CreateModeButton("添加卡牌", PickerMode.AddCard));
		row.AddChild(CreateModeButton("移除卡牌", PickerMode.RemoveCard));
		row.AddChild(CreateModeButton("添加遗物", PickerMode.AddRelic));
		row.AddChild(CreateModeButton("移除遗物", PickerMode.RemoveRelic));

		Label hint = CreateLabel("快捷键：F10 呼出或关闭", 16, MutedTextColor);
		hint.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		hint.HorizontalAlignment = HorizontalAlignment.Right;
		row.AddChild(hint);

		return row;
	}

	private Control BuildConsolePanel()
	{
		VBoxContainer box = new()
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill
		};
		box.AddThemeConstantOverride("separation", 8);

		Label title = CreateLabel("命令输出", 18, TextColor);
		box.AddChild(title);

		_output = new RichTextLabel
		{
			BbcodeEnabled = true,
			ScrollFollowing = true,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill,
			FitContent = false
		};
		_output.AddThemeColorOverride("default_color", TextColor);
		_output.AddThemeFontSizeOverride("normal_font_size", 16);
		box.AddChild(_output);

		HBoxContainer commandRow = new();
		commandRow.AddThemeConstantOverride("separation", 8);
		box.AddChild(commandRow);

		_commandInput = new LineEdit
		{
			PlaceholderText = "输入命令，例如：加卡 相信着你",
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			CaretBlink = true
		};
		_commandInput.TextSubmitted += ExecuteCommand;
		commandRow.AddChild(_commandInput);

		Button runButton = CreateButton("执行");
		runButton.Pressed += () => ExecuteCommand(_commandInput.Text);
		commandRow.AddChild(runButton);

		return box;
	}

	private Control BuildPickerPanel()
	{
		VBoxContainer box = new()
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill
		};
		box.AddThemeConstantOverride("separation", 8);

		_pickerTitle = CreateLabel("选择器", 18, TextColor);
		box.AddChild(_pickerTitle);

		_searchInput = new LineEdit
		{
			PlaceholderText = "搜索中文名 / 英文名 / ID",
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			CaretBlink = true
		};
		_searchInput.TextChanged += _ => RefreshPicker();
		box.AddChild(_searchInput);

		ScrollContainer scroll = new()
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill
		};
		box.AddChild(scroll);

		_pickerList = new VBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		_pickerList.AddThemeConstantOverride("separation", 5);
		scroll.AddChild(_pickerList);

		return box;
	}

	private Button CreateModeButton(string text, PickerMode mode)
	{
		Button button = CreateButton(text);
		button.Pressed += () =>
		{
			_pickerMode = mode;
			_searchInput.Text = string.Empty;
			RefreshPicker();
			_searchInput.GrabFocus();
		};
		return button;
	}

	private void ExecuteCommand(string command)
	{
		if (string.IsNullOrWhiteSpace(command))
		{
			return;
		}

		AppendLog("> " + command, success: true);
		ChineseConsoleExecutionResult result = CommandService.ExecuteTextCommand(command);
		if (result.Message == "__CLEAR__")
		{
			_logLines.Clear();
			_output.Text = string.Empty;
		}
		else
		{
			AppendLog(result.Message, result.Success);
		}

		_commandInput.Text = string.Empty;
		RefreshPicker();
	}

	private void RefreshPicker()
	{
		if (_pickerList == null)
		{
			return;
		}

		foreach (Node child in _pickerList.GetChildren())
		{
			child.QueueFree();
		}

		Player? player = _actions.LocalPlayer;
		IReadOnlyList<ChineseModelCatalogEntry> entries = EntriesForMode(player).ToArray();
		string query = _searchInput.Text.Trim();
		if (!string.IsNullOrWhiteSpace(query))
		{
			entries = entries.Where(entry => entry.Matches(query)).ToArray();
		}

		_pickerTitle.Text = PickerTitle(entries.Count, player);

		foreach (ChineseModelCatalogEntry entry in entries.Take(160))
		{
			Button item = CreateButton(FormatEntry(entry));
			item.Alignment = HorizontalAlignment.Left;
			item.Pressed += () => ExecuteEntry(entry);
			_pickerList.AddChild(item);
		}

		if (entries.Count == 0)
		{
			_pickerList.AddChild(CreateLabel("没有匹配项目。", 16, MutedTextColor));
		}
		else if (entries.Count > 160)
		{
			_pickerList.AddChild(CreateLabel($"还有 {entries.Count - 160} 项未显示，请继续缩小搜索。", 16, MutedTextColor));
		}
	}

	private ChineseConsoleCommandService CommandService => _commandService ??= new ChineseConsoleCommandService(
		_catalog.AllCatalog,
		_actions,
		() => _catalog.CurrentInventoryCatalog(_actions.LocalPlayer));

	private IEnumerable<ChineseModelCatalogEntry> EntriesForMode(Player? player)
	{
		return _pickerMode switch
		{
			PickerMode.AddCard => _catalog.AllCatalog.Cards,
			PickerMode.RemoveCard => player?.Deck.Cards.Select(_catalog.CardEntryFor) ?? [],
			PickerMode.AddRelic => _catalog.AllCatalog.Relics,
			PickerMode.RemoveRelic => player?.Relics.Select(_catalog.RelicEntryFor) ?? [],
			_ => []
		};
	}

	private string PickerTitle(int count, Player? player)
	{
		string playerSuffix = player == null ? "（未进入游戏）" : "";
		return _pickerMode switch
		{
			PickerMode.AddCard => $"添加卡牌：全卡池 {count} 项{playerSuffix}",
			PickerMode.RemoveCard => $"移除卡牌：当前牌组 {count} 项{playerSuffix}",
			PickerMode.AddRelic => $"添加遗物：全遗物 {count} 项{playerSuffix}",
			PickerMode.RemoveRelic => $"移除遗物：当前遗物 {count} 项{playerSuffix}",
			_ => "选择器"
		};
	}

	private void ExecuteEntry(ChineseModelCatalogEntry entry)
	{
		ChineseConsoleExecutionResult result = _pickerMode switch
		{
			PickerMode.AddCard => _actions.AddCard(entry.Id),
			PickerMode.RemoveCard => _actions.RemoveCard(entry.Id),
			PickerMode.AddRelic => _actions.AddRelic(entry.Id),
			PickerMode.RemoveRelic => _actions.RemoveRelic(entry.Id),
			_ => ChineseConsoleExecutionResult.Fail("请选择一个功能。")
		};
		AppendLog(result.Message, result.Success);
		RefreshPicker();
	}

	private void Toggle()
	{
		if (Visible)
		{
			HideOverlay();
		}
		else
		{
			ShowOverlay();
		}
	}

	private void ShowOverlay()
	{
		Visible = true;
		_pickerMode = _pickerMode == PickerMode.None ? PickerMode.AddCard : _pickerMode;
		RefreshPicker();
		_commandInput.GrabFocus();
	}

	private void HideOverlay()
	{
		Visible = false;
	}

	private void AppendLog(string message, bool success)
	{
		string color = success ? "79F093" : "FF7A6B";
		_logLines.Add($"[color=#{color}]{EscapeBbcode(message)}[/color]");
		while (_logLines.Count > 80)
		{
			_logLines.RemoveAt(0);
		}
		_output.Text = string.Join("\n", _logLines);
	}

	private static bool IsToggleKey(InputEventKey key)
	{
		return key.Keycode == Key.F10 || key.PhysicalKeycode == Key.F10;
	}

	private static Label CreateLabel(string text, int fontSize, Color color)
	{
		Label label = new()
		{
			Text = text,
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
			SizeFlagsHorizontal = SizeFlags.ExpandFill
		};
		label.AddThemeColorOverride("font_color", color);
		label.AddThemeFontSizeOverride("font_size", fontSize);
		return label;
	}

	private static Button CreateButton(string text)
	{
		Button button = new()
		{
			Text = text,
			CustomMinimumSize = new Vector2(120f, 38f),
			MouseFilter = MouseFilterEnum.Stop
		};
		button.AddThemeFontSizeOverride("font_size", 16);
		return button;
	}

	private static StyleBoxFlat CreatePanelStyle()
	{
		StyleBoxFlat style = new()
		{
			BgColor = PanelColor,
			BorderColor = BorderColor,
			BorderWidthBottom = 2,
			BorderWidthLeft = 2,
			BorderWidthRight = 2,
			BorderWidthTop = 2,
			CornerRadiusBottomLeft = 8,
			CornerRadiusBottomRight = 8,
			CornerRadiusTopLeft = 8,
			CornerRadiusTopRight = 8
		};
		return style;
	}

	private static string FormatEntry(ChineseModelCatalogEntry entry)
	{
		string title = !string.IsNullOrWhiteSpace(entry.ChineseTitle) ? entry.ChineseTitle : entry.EnglishTitle;
		string english = !string.IsNullOrWhiteSpace(entry.EnglishTitle) && entry.EnglishTitle != title ? $" / {entry.EnglishTitle}" : "";
		return $"{title}{english}    {entry.Id}";
	}

	private static string EscapeBbcode(string text)
	{
		return text.Replace("[", "[lb]", StringComparison.Ordinal).Replace("]", "[rb]", StringComparison.Ordinal);
	}
}
