namespace ChineseDebugConsole;

public readonly record struct ChineseConsoleExecutionResult(bool Success, string Message)
{
	public static ChineseConsoleExecutionResult Ok(string message)
	{
		return new ChineseConsoleExecutionResult(true, message);
	}

	public static ChineseConsoleExecutionResult Fail(string message)
	{
		return new ChineseConsoleExecutionResult(false, message);
	}
}
