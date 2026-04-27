namespace ChineseDebugConsole;

public static class ChineseConsoleCommandParser
{
	private static readonly Dictionary<string, ChineseConsoleAction> CommandAliases = new(StringComparer.OrdinalIgnoreCase)
	{
		["help"] = ChineseConsoleAction.Help,
		["帮助"] = ChineseConsoleAction.Help,
		["?"] = ChineseConsoleAction.Help,
		["clear"] = ChineseConsoleAction.Clear,
		["清屏"] = ChineseConsoleAction.Clear,
		["加卡"] = ChineseConsoleAction.AddCard,
		["添加卡牌"] = ChineseConsoleAction.AddCard,
		["addcard"] = ChineseConsoleAction.AddCard,
		["add_card"] = ChineseConsoleAction.AddCard,
		["删卡"] = ChineseConsoleAction.RemoveCard,
		["移除卡牌"] = ChineseConsoleAction.RemoveCard,
		["removecard"] = ChineseConsoleAction.RemoveCard,
		["remove_card"] = ChineseConsoleAction.RemoveCard,
		["加遗物"] = ChineseConsoleAction.AddRelic,
		["添加遗物"] = ChineseConsoleAction.AddRelic,
		["addrelic"] = ChineseConsoleAction.AddRelic,
		["relic"] = ChineseConsoleAction.AddRelic,
		["删遗物"] = ChineseConsoleAction.RemoveRelic,
		["移除遗物"] = ChineseConsoleAction.RemoveRelic,
		["removerelic"] = ChineseConsoleAction.RemoveRelic,
		["选卡"] = ChineseConsoleAction.OpenAddCardPicker,
		["打开加卡"] = ChineseConsoleAction.OpenAddCardPicker,
		["选删卡"] = ChineseConsoleAction.OpenRemoveCardPicker,
		["打开删卡"] = ChineseConsoleAction.OpenRemoveCardPicker,
		["选遗物"] = ChineseConsoleAction.OpenAddRelicPicker,
		["打开加遗物"] = ChineseConsoleAction.OpenAddRelicPicker,
		["选删遗物"] = ChineseConsoleAction.OpenRemoveRelicPicker,
		["打开删遗物"] = ChineseConsoleAction.OpenRemoveRelicPicker
	};

	private static readonly HashSet<ChineseConsoleAction> RequiresArgument = new()
	{
		ChineseConsoleAction.AddCard,
		ChineseConsoleAction.RemoveCard,
		ChineseConsoleAction.AddRelic,
		ChineseConsoleAction.RemoveRelic
	};

	public static ChineseConsoleParseResult Parse(string? input)
	{
		string text = (input ?? "").Trim();
		if (string.IsNullOrWhiteSpace(text))
		{
			return ChineseConsoleParseResult.Fail("请输入命令。输入“帮助”查看可用命令。");
		}

		string command;
		string argument;
		int separator = text.IndexOfAny([' ', '\t']);
		if (separator < 0)
		{
			command = text;
			argument = "";
		}
		else
		{
			command = text[..separator].Trim();
			argument = text[(separator + 1)..].Trim();
		}

		if (!CommandAliases.TryGetValue(command, out ChineseConsoleAction action))
		{
			return ChineseConsoleParseResult.Fail($"未知命令：{command}。输入“帮助”查看可用命令。");
		}

		if (RequiresArgument.Contains(action) && string.IsNullOrWhiteSpace(argument))
		{
			return ChineseConsoleParseResult.Fail($"{command} 需要指定卡牌或遗物名称。");
		}

		return ChineseConsoleParseResult.Ok(action, argument);
	}
}
