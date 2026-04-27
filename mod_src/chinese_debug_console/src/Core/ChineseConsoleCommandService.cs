namespace ChineseDebugConsole;

public sealed class ChineseConsoleCommandService
{
	private readonly ChineseModelCatalog _catalog;
	private readonly Func<ChineseModelCatalog>? _currentCatalogFactory;
	private readonly IChineseConsoleActions _actions;

	public ChineseConsoleCommandService(
		ChineseModelCatalog catalog,
		IChineseConsoleActions actions,
		Func<ChineseModelCatalog>? currentCatalogFactory = null)
	{
		_catalog = catalog;
		_currentCatalogFactory = currentCatalogFactory;
		_actions = actions;
	}

	public ChineseConsoleExecutionResult ExecuteTextCommand(string input)
	{
		ChineseConsoleParseResult parsed = ChineseConsoleCommandParser.Parse(input);
		if (!parsed.Success)
		{
			return ChineseConsoleExecutionResult.Fail(parsed.Message);
		}

		return parsed.Action switch
		{
			ChineseConsoleAction.Help => ChineseConsoleExecutionResult.Ok(BuildHelpText()),
			ChineseConsoleAction.Clear => ChineseConsoleExecutionResult.Ok("__CLEAR__"),
			ChineseConsoleAction.AddCard => ExecuteCardCommand(parsed.Argument, _actions.AddCard, "卡牌"),
			ChineseConsoleAction.RemoveCard => ExecuteCardCommand(parsed.Argument, _actions.RemoveCard, "当前卡组内卡牌", CurrentCatalog),
			ChineseConsoleAction.AddRelic => ExecuteRelicCommand(parsed.Argument, _actions.AddRelic, "遗物"),
			ChineseConsoleAction.RemoveRelic => ExecuteRelicCommand(parsed.Argument, _actions.RemoveRelic, "当前遗物", CurrentCatalog),
			_ => ChineseConsoleExecutionResult.Fail("这个命令需要通过界面按钮打开。")
		};
	}

	private ChineseConsoleExecutionResult ExecuteCardCommand(
		string query,
		Func<string, ChineseConsoleExecutionResult> execute,
		string targetLabel,
		ChineseModelCatalog? catalog = null)
	{
		ChineseModelCatalogEntry[] matches = (catalog ?? _catalog).FindCard(query).Take(8).ToArray();
		return ExecuteMatchedCommand(query, matches, execute, targetLabel);
	}

	private ChineseConsoleExecutionResult ExecuteRelicCommand(
		string query,
		Func<string, ChineseConsoleExecutionResult> execute,
		string targetLabel,
		ChineseModelCatalog? catalog = null)
	{
		ChineseModelCatalogEntry[] matches = (catalog ?? _catalog).FindRelic(query).Take(8).ToArray();
		return ExecuteMatchedCommand(query, matches, execute, targetLabel);
	}

	private ChineseModelCatalog CurrentCatalog => _currentCatalogFactory?.Invoke() ?? _catalog;

	private static ChineseConsoleExecutionResult ExecuteMatchedCommand(
		string query,
		IReadOnlyList<ChineseModelCatalogEntry> matches,
		Func<string, ChineseConsoleExecutionResult> execute,
		string targetLabel)
	{
		if (matches.Count == 0)
		{
			return ChineseConsoleExecutionResult.Fail($"没有找到匹配“{query}”的{targetLabel}。");
		}

		if (matches.Count > 1)
		{
			string candidates = string.Join("、", matches.Select(FormatEntry));
			return ChineseConsoleExecutionResult.Fail($"匹配到多个{targetLabel}，请再输入得更具体：{candidates}");
		}

		return execute(matches[0].Id);
	}

	public static string BuildHelpText()
	{
		string[] lines =
		[
			"可用命令：",
			"加卡 <中文名/英文名/ID> - 添加卡牌到当前牌组",
			"删卡 <中文名/英文名/ID> - 从当前牌组移除卡牌",
			"加遗物 <中文名/英文名/ID> - 添加遗物",
			"删遗物 <中文名/英文名/ID> - 移除当前遗物",
			"清屏 - 清空输出",
			"也可以使用按钮打开卡牌/遗物选择器。"
		];

		return string.Join("\n", lines);
	}

	private static string FormatEntry(ChineseModelCatalogEntry entry)
	{
		string title = !string.IsNullOrWhiteSpace(entry.ChineseTitle) ? entry.ChineseTitle : entry.EnglishTitle;
		return $"{title}({entry.Id})";
	}
}
