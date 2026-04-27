namespace ChineseDebugConsole;

public readonly record struct ChineseConsoleParseResult(
	bool Success,
	ChineseConsoleAction Action,
	string Argument,
	string Message)
{
	public static ChineseConsoleParseResult Ok(ChineseConsoleAction action, string argument = "")
	{
		return new ChineseConsoleParseResult(true, action, argument, "");
	}

	public static ChineseConsoleParseResult Fail(string message)
	{
		return new ChineseConsoleParseResult(false, ChineseConsoleAction.None, "", message);
	}
}
