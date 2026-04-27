namespace ChineseDebugConsole;

public sealed record ChineseModelCatalogEntry(string Id, string ChineseTitle, string EnglishTitle)
{
	public bool Matches(string query)
	{
		string normalized = Normalize(query);
		if (normalized.Length == 0)
		{
			return false;
		}

		return Normalize(Id).Contains(normalized, StringComparison.OrdinalIgnoreCase)
			|| Normalize(ChineseTitle).Contains(normalized, StringComparison.OrdinalIgnoreCase)
			|| Normalize(EnglishTitle).Contains(normalized, StringComparison.OrdinalIgnoreCase);
	}

	private static string Normalize(string value)
	{
		return value.Trim().Replace(" ", "", StringComparison.Ordinal).Replace("_", "", StringComparison.Ordinal).ToLowerInvariant();
	}
}
