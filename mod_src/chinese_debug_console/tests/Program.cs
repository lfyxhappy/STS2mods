using ChineseDebugConsole;

static void AssertEqual<T>(T expected, T actual, string name)
{
	if (!EqualityComparer<T>.Default.Equals(expected, actual))
	{
		throw new InvalidOperationException($"{name}: expected '{expected}', got '{actual}'");
	}
}

static void AssertTrue(bool condition, string name)
{
	if (!condition)
	{
		throw new InvalidOperationException($"{name}: expected true");
	}
}

static void AssertFalse(bool condition, string name)
{
	if (condition)
	{
		throw new InvalidOperationException($"{name}: expected false");
	}
}

void CommandParserUnderstandsChineseShortcuts()
{
	var addCard = ChineseConsoleCommandParser.Parse("加卡 相信着你");
	AssertTrue(addCard.Success, "add card parsed");
	AssertEqual(ChineseConsoleAction.AddCard, addCard.Action, "add card action");
	AssertEqual("相信着你", addCard.Argument, "add card argument");

	var removeCard = ChineseConsoleCommandParser.Parse("删卡 打击");
	AssertTrue(removeCard.Success, "remove card parsed");
	AssertEqual(ChineseConsoleAction.RemoveCard, removeCard.Action, "remove card action");

	var addRelic = ChineseConsoleCommandParser.Parse("添加遗物 锚");
	AssertTrue(addRelic.Success, "add relic parsed");
	AssertEqual(ChineseConsoleAction.AddRelic, addRelic.Action, "add relic action");

	var removeRelic = ChineseConsoleCommandParser.Parse("移除遗物 锚");
	AssertTrue(removeRelic.Success, "remove relic parsed");
	AssertEqual(ChineseConsoleAction.RemoveRelic, removeRelic.Action, "remove relic action");
}

void CommandParserRejectsMissingArguments()
{
	var parsed = ChineseConsoleCommandParser.Parse("加卡");
	AssertFalse(parsed.Success, "missing argument rejected");
	AssertTrue(parsed.Message.Contains("需要指定"), "missing argument message");
}

void CatalogMatchesChineseEnglishAndIds()
{
	var catalog = new ChineseModelCatalog(
	[
		new ChineseModelCatalogEntry("BELIEVE_IN_YOU", "相信着你", "Believe in You"),
		new ChineseModelCatalogEntry("BASH", "重击", "Bash"),
	],
	[
		new ChineseModelCatalogEntry("ANCHOR", "锚", "Anchor"),
		new ChineseModelCatalogEntry("BAG_OF_PREPARATION", "准备背包", "Bag of Preparation"),
	]);

	AssertEqual("BELIEVE_IN_YOU", catalog.FindCard("相信").Single().Id, "card chinese fuzzy");
	AssertEqual("BELIEVE_IN_YOU", catalog.FindCard("believe").Single().Id, "card english fuzzy");
	AssertEqual("BASH", catalog.FindCard("bash").Single().Id, "card id fuzzy");
	AssertEqual("ANCHOR", catalog.FindRelic("锚").Single().Id, "relic chinese exact");
	AssertEqual("BAG_OF_PREPARATION", catalog.FindRelic("preparation").Single().Id, "relic english fuzzy");
}

void CatalogReportsAmbiguousMatches()
{
	var catalog = new ChineseModelCatalog(
	[
		new ChineseModelCatalogEntry("STRIKE_IRONCLAD", "打击", "Strike"),
		new ChineseModelCatalogEntry("STRIKE_SILENT", "打击", "Strike"),
	],
	[]);

	var matches = catalog.FindCard("打击").ToArray();
	AssertEqual(2, matches.Length, "ambiguous match count");
	AssertEqual("STRIKE_IRONCLAD", matches[0].Id, "stable ambiguous order");
}

void CatalogCollapsesDuplicateIds()
{
	var catalog = new ChineseModelCatalog(
	[
		new ChineseModelCatalogEntry("STRIKE_IRONCLAD", "打击", "Strike"),
		new ChineseModelCatalogEntry("STRIKE_IRONCLAD", "打击", "Strike"),
	],
	[]);

	AssertEqual(1, catalog.FindCard("打击").Count(), "duplicate id collapsed");
}

void CommandServiceExecutesUniqueMatches()
{
	var catalog = new ChineseModelCatalog(
	[
		new ChineseModelCatalogEntry("BELIEVE_IN_YOU", "相信着你", "Believe in You"),
	],
	[
		new ChineseModelCatalogEntry("ANCHOR", "锚", "Anchor"),
	]);
	var actions = new RecordingChineseConsoleActions();
	var service = new ChineseConsoleCommandService(catalog, actions);

	var addCard = service.ExecuteTextCommand("加卡 相信");
	AssertTrue(addCard.Success, "add card command succeeds");
	AssertEqual("AddCard:BELIEVE_IN_YOU", actions.Calls.Single(), "add card action call");

	actions.Calls.Clear();
	var removeRelic = service.ExecuteTextCommand("删遗物 锚");
	AssertTrue(removeRelic.Success, "remove relic command succeeds");
	AssertEqual("RemoveRelic:ANCHOR", actions.Calls.Single(), "remove relic action call");
}

void CommandServiceRejectsAmbiguousMatches()
{
	var catalog = new ChineseModelCatalog(
	[
		new ChineseModelCatalogEntry("STRIKE_IRONCLAD", "打击", "Strike"),
		new ChineseModelCatalogEntry("STRIKE_SILENT", "打击", "Strike"),
	],
	[]);
	var actions = new RecordingChineseConsoleActions();
	var service = new ChineseConsoleCommandService(catalog, actions);

	var result = service.ExecuteTextCommand("删卡 打击");
	AssertFalse(result.Success, "ambiguous command rejected");
	AssertTrue(result.Message.Contains("匹配到多个"), "ambiguous command message");
	AssertEqual(0, actions.Calls.Count, "ambiguous command no action");
}

void RemoveCommandsUseCurrentInventoryScope()
{
	var allCatalog = new ChineseModelCatalog(
	[
		new ChineseModelCatalogEntry("STRIKE_IRONCLAD", "打击", "Strike"),
		new ChineseModelCatalogEntry("STRIKE_SILENT", "打击", "Strike"),
	],
	[
		new ChineseModelCatalogEntry("ANCHOR", "锚", "Anchor"),
		new ChineseModelCatalogEntry("BAG_OF_PREPARATION", "准备背包", "Bag of Preparation"),
	]);
	var currentCatalog = new ChineseModelCatalog(
	[
		new ChineseModelCatalogEntry("STRIKE_IRONCLAD", "打击", "Strike"),
	],
	[
		new ChineseModelCatalogEntry("ANCHOR", "锚", "Anchor"),
	]);
	var actions = new RecordingChineseConsoleActions();
	var service = new ChineseConsoleCommandService(allCatalog, actions, () => currentCatalog);

	var removeCard = service.ExecuteTextCommand("删卡 打击");
	AssertTrue(removeCard.Success, "remove current card succeeds");
	AssertEqual("RemoveCard:STRIKE_IRONCLAD", actions.Calls.Single(), "remove current card action call");

	actions.Calls.Clear();
	var removeRelic = service.ExecuteTextCommand("删遗物 锚");
	AssertTrue(removeRelic.Success, "remove current relic succeeds");
	AssertEqual("RemoveRelic:ANCHOR", actions.Calls.Single(), "remove current relic action call");
}

void CommandServiceRejectsMissingMatches()
{
	var catalog = new ChineseModelCatalog([], []);
	var actions = new RecordingChineseConsoleActions();
	var service = new ChineseConsoleCommandService(catalog, actions);

	var result = service.ExecuteTextCommand("加卡 不存在");
	AssertFalse(result.Success, "missing command rejected");
	AssertTrue(result.Message.Contains("没有找到"), "missing command message");
	AssertEqual(0, actions.Calls.Count, "missing command no action");
}

var tests = new (string Name, Action Run)[]
{
	(nameof(CommandParserUnderstandsChineseShortcuts), CommandParserUnderstandsChineseShortcuts),
	(nameof(CommandParserRejectsMissingArguments), CommandParserRejectsMissingArguments),
	(nameof(CatalogMatchesChineseEnglishAndIds), CatalogMatchesChineseEnglishAndIds),
	(nameof(CatalogReportsAmbiguousMatches), CatalogReportsAmbiguousMatches),
	(nameof(CatalogCollapsesDuplicateIds), CatalogCollapsesDuplicateIds),
	(nameof(CommandServiceExecutesUniqueMatches), CommandServiceExecutesUniqueMatches),
	(nameof(CommandServiceRejectsAmbiguousMatches), CommandServiceRejectsAmbiguousMatches),
	(nameof(RemoveCommandsUseCurrentInventoryScope), RemoveCommandsUseCurrentInventoryScope),
	(nameof(CommandServiceRejectsMissingMatches), CommandServiceRejectsMissingMatches),
};

foreach (var test in tests)
{
	test.Run();
	Console.WriteLine($"PASS {test.Name}");
}

internal sealed class RecordingChineseConsoleActions : IChineseConsoleActions
{
	public List<string> Calls { get; } = [];

	public ChineseConsoleExecutionResult AddCard(string cardId)
	{
		Calls.Add($"AddCard:{cardId}");
		return ChineseConsoleExecutionResult.Ok($"已添加卡牌 {cardId}");
	}

	public ChineseConsoleExecutionResult RemoveCard(string cardId)
	{
		Calls.Add($"RemoveCard:{cardId}");
		return ChineseConsoleExecutionResult.Ok($"已移除卡牌 {cardId}");
	}

	public ChineseConsoleExecutionResult AddRelic(string relicId)
	{
		Calls.Add($"AddRelic:{relicId}");
		return ChineseConsoleExecutionResult.Ok($"已添加遗物 {relicId}");
	}

	public ChineseConsoleExecutionResult RemoveRelic(string relicId)
	{
		Calls.Add($"RemoveRelic:{relicId}");
		return ChineseConsoleExecutionResult.Ok($"已移除遗物 {relicId}");
	}
}
