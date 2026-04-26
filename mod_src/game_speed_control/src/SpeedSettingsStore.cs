using System.Text;
using System.Text.Json;

namespace GameSpeedControl;

internal static class SpeedSettingsStore
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		WriteIndented = true
	};

	public static SpeedSettings Load(string path)
	{
		try
		{
			if (!File.Exists(path))
			{
				return CreateDefault();
			}

			string json = File.ReadAllText(path, Encoding.UTF8);
			SpeedSettings? settings = JsonSerializer.Deserialize<SpeedSettings>(json, JsonOptions);
			if (settings?.SchemaVersion != 1)
			{
				return CreateDefault();
			}

			settings.SpeedMultiplier = SpeedPresetCycle.Normalize(settings.SpeedMultiplier);
			return settings;
		}
		catch
		{
			return CreateDefault();
		}
	}

	public static void Save(string path, SpeedSettings settings)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
		settings.SchemaVersion = 1;
		settings.SpeedMultiplier = SpeedPresetCycle.Normalize(settings.SpeedMultiplier);
		string json = JsonSerializer.Serialize(settings, JsonOptions);
		File.WriteAllText(path, json, Encoding.UTF8);
	}

	private static SpeedSettings CreateDefault()
	{
		return new SpeedSettings
		{
			SchemaVersion = 1,
			SpeedMultiplier = 1.0
		};
	}
}
