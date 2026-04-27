namespace ChineseDebugConsole;

public interface IChineseConsoleActions
{
	ChineseConsoleExecutionResult AddCard(string cardId);

	ChineseConsoleExecutionResult RemoveCard(string cardId);

	ChineseConsoleExecutionResult AddRelic(string relicId);

	ChineseConsoleExecutionResult RemoveRelic(string relicId);
}
