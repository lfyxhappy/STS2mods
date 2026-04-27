using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace ChineseDebugConsole;

internal sealed class ChineseConsoleRuntimeCatalog
{
	private readonly Dictionary<string, string> _cardChineseTitles;
	private readonly Dictionary<string, string> _cardEnglishTitles;
	private readonly Dictionary<string, string> _relicChineseTitles;
	private readonly Dictionary<string, string> _relicEnglishTitles;

	private ChineseModelCatalog? _allCatalog;

	public ChineseConsoleRuntimeCatalog()
	{
		_cardChineseTitles = ChineseLocalizationCatalog.LoadTitleMap("zhs", "cards");
		_cardEnglishTitles = ChineseLocalizationCatalog.LoadTitleMap("eng", "cards");
		_relicChineseTitles = ChineseLocalizationCatalog.LoadTitleMap("zhs", "relics");
		_relicEnglishTitles = ChineseLocalizationCatalog.LoadTitleMap("eng", "relics");
	}

	public ChineseModelCatalog AllCatalog => _allCatalog ??= BuildAllCatalog();

	public ChineseModelCatalog CurrentInventoryCatalog(Player? player)
	{
		if (player == null)
		{
			return new ChineseModelCatalog([], []);
		}

		return new ChineseModelCatalog(
			player.Deck.Cards.Select(CardEntryFor),
			player.Relics.Select(RelicEntryFor));
	}

	public ChineseModelCatalogEntry CardEntryFor(CardModel card)
	{
		string id = card.Id.Entry;
		return new ChineseModelCatalogEntry(id, Lookup(_cardChineseTitles, id, card.Title), Lookup(_cardEnglishTitles, id, id));
	}

	public ChineseModelCatalogEntry RelicEntryFor(RelicModel relic)
	{
		string id = relic.Id.Entry;
		return new ChineseModelCatalogEntry(id, Lookup(_relicChineseTitles, id, relic.Title.GetFormattedText()), Lookup(_relicEnglishTitles, id, id));
	}

	private ChineseModelCatalog BuildAllCatalog()
	{
		return new ChineseModelCatalog(
			ModelDb.AllCards.Select(CardEntryFor),
			ModelDb.AllRelics.Select(RelicEntryFor));
	}

	private static string Lookup(IReadOnlyDictionary<string, string> titles, string id, string fallback)
	{
		return titles.TryGetValue(id, out string? title) && !string.IsNullOrWhiteSpace(title)
			? title
			: fallback;
	}
}
