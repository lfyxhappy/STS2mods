using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace ChineseDebugConsole;

internal sealed class ChineseConsoleGameActions : IChineseConsoleActions
{
	private readonly ChineseConsoleRuntimeCatalog _catalog;

	public ChineseConsoleGameActions(ChineseConsoleRuntimeCatalog catalog)
	{
		_catalog = catalog;
	}

	public Player? LocalPlayer => LocalContext.GetMe(RunManager.Instance.DebugOnlyGetState());

	public ChineseConsoleExecutionResult AddCard(string cardId)
	{
		Player? player = LocalPlayer;
		if (player == null)
		{
			return ChineseConsoleExecutionResult.Fail("当前没有本地玩家。请先进入一局游戏。");
		}

		CardModel? canonicalCard = FindCard(cardId);
		if (canonicalCard == null)
		{
			return ChineseConsoleExecutionResult.Fail($"没有找到卡牌 ID：{cardId}");
		}

		try
		{
			CardModel card = player.RunState.CreateCard(canonicalCard, player);
			TaskHelper.RunSafely(CardPileCmd.Add(card, PileType.Deck));
			return ChineseConsoleExecutionResult.Ok($"已添加卡牌：{Format(_catalog.CardEntryFor(canonicalCard))}");
		}
		catch (Exception ex)
		{
			return ChineseConsoleExecutionResult.Fail($"添加卡牌失败：{ex.Message}");
		}
	}

	public ChineseConsoleExecutionResult RemoveCard(string cardId)
	{
		Player? player = LocalPlayer;
		if (player == null)
		{
			return ChineseConsoleExecutionResult.Fail("当前没有本地玩家。请先进入一局游戏。");
		}

		CardModel? card = player.Deck.Cards.FirstOrDefault(c => string.Equals(c.Id.Entry, cardId, StringComparison.OrdinalIgnoreCase));
		if (card == null)
		{
			return ChineseConsoleExecutionResult.Fail($"当前牌组中没有这张卡：{cardId}");
		}

		try
		{
			TaskHelper.RunSafely(CardPileCmd.RemoveFromDeck(card));
			return ChineseConsoleExecutionResult.Ok($"已移除卡牌：{Format(_catalog.CardEntryFor(card))}");
		}
		catch (Exception ex)
		{
			return ChineseConsoleExecutionResult.Fail($"移除卡牌失败：{ex.Message}");
		}
	}

	public ChineseConsoleExecutionResult AddRelic(string relicId)
	{
		Player? player = LocalPlayer;
		if (player == null)
		{
			return ChineseConsoleExecutionResult.Fail("当前没有本地玩家。请先进入一局游戏。");
		}

		RelicModel? canonicalRelic = FindRelic(relicId);
		if (canonicalRelic == null)
		{
			return ChineseConsoleExecutionResult.Fail($"没有找到遗物 ID：{relicId}");
		}

		try
		{
			TaskHelper.RunSafely(RelicCmd.Obtain(canonicalRelic.ToMutable(), player));
			return ChineseConsoleExecutionResult.Ok($"已添加遗物：{Format(_catalog.RelicEntryFor(canonicalRelic))}");
		}
		catch (Exception ex)
		{
			return ChineseConsoleExecutionResult.Fail($"添加遗物失败：{ex.Message}");
		}
	}

	public ChineseConsoleExecutionResult RemoveRelic(string relicId)
	{
		Player? player = LocalPlayer;
		if (player == null)
		{
			return ChineseConsoleExecutionResult.Fail("当前没有本地玩家。请先进入一局游戏。");
		}

		RelicModel? relic = player.Relics.FirstOrDefault(r => string.Equals(r.Id.Entry, relicId, StringComparison.OrdinalIgnoreCase));
		if (relic == null)
		{
			return ChineseConsoleExecutionResult.Fail($"当前遗物中没有这个遗物：{relicId}");
		}

		try
		{
			TaskHelper.RunSafely(RelicCmd.Remove(relic));
			return ChineseConsoleExecutionResult.Ok($"已移除遗物：{Format(_catalog.RelicEntryFor(relic))}");
		}
		catch (Exception ex)
		{
			return ChineseConsoleExecutionResult.Fail($"移除遗物失败：{ex.Message}");
		}
	}

	private static CardModel? FindCard(string id)
	{
		return ModelDb.AllCards.FirstOrDefault(card => string.Equals(card.Id.Entry, id, StringComparison.OrdinalIgnoreCase));
	}

	private static RelicModel? FindRelic(string id)
	{
		return ModelDb.AllRelics.FirstOrDefault(relic => string.Equals(relic.Id.Entry, id, StringComparison.OrdinalIgnoreCase));
	}

	private static string Format(ChineseModelCatalogEntry entry)
	{
		string title = !string.IsNullOrWhiteSpace(entry.ChineseTitle) ? entry.ChineseTitle : entry.EnglishTitle;
		return $"{title} ({entry.Id})";
	}
}
