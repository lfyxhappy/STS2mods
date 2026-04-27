using System.Text.Json;
using Godot;

namespace ChineseDebugConsole;

internal static class ChineseLocalizationCatalog
{
	public static Dictionary<string, string> LoadTitleMap(string language, string tableName)
	{
		string path = $"res://localization/{language}/{tableName}.json";
		if (!Godot.FileAccess.FileExists(path))
		{
			return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		}

		try
		{
			using Godot.FileAccess? file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
			string json = file?.GetAsText() ?? string.Empty;
			Dictionary<string, string>? values = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
			if (values == null)
			{
				return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			}

			Dictionary<string, string> result = new(StringComparer.OrdinalIgnoreCase);
			foreach ((string key, string value) in values)
			{
				if (!key.EndsWith(".title", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				string id = key[..^".title".Length];
				result[id] = value;
			}

			return result;
		}
		catch
		{
			return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		}
	}
}
