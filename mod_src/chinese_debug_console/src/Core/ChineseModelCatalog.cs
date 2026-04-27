namespace ChineseDebugConsole;

public sealed class ChineseModelCatalog
{
	private readonly IReadOnlyList<ChineseModelCatalogEntry> _cards;
	private readonly IReadOnlyList<ChineseModelCatalogEntry> _relics;

	public ChineseModelCatalog(
		IEnumerable<ChineseModelCatalogEntry> cards,
		IEnumerable<ChineseModelCatalogEntry> relics)
	{
		_cards = NormalizeEntries(cards);
		_relics = NormalizeEntries(relics);
	}

	public IReadOnlyList<ChineseModelCatalogEntry> Cards => _cards;

	public IReadOnlyList<ChineseModelCatalogEntry> Relics => _relics;

	public IEnumerable<ChineseModelCatalogEntry> FindCard(string query)
	{
		return Find(_cards, query);
	}

	public IEnumerable<ChineseModelCatalogEntry> FindRelic(string query)
	{
		return Find(_relics, query);
	}

	private static IEnumerable<ChineseModelCatalogEntry> Find(IEnumerable<ChineseModelCatalogEntry> entries, string query)
	{
		string normalized = query.Trim();
		if (normalized.Length == 0)
		{
			return [];
		}

		return entries.Where(entry => entry.Matches(normalized));
	}

	private static IReadOnlyList<ChineseModelCatalogEntry> NormalizeEntries(IEnumerable<ChineseModelCatalogEntry> entries)
	{
		return entries
			.Where(entry => !string.IsNullOrWhiteSpace(entry.Id))
			.GroupBy(entry => entry.Id, StringComparer.OrdinalIgnoreCase)
			.Select(group => group.First())
			.OrderBy(entry => entry.Id, StringComparer.OrdinalIgnoreCase)
			.ToArray();
	}
}
