using System.Text.Json;
using GameSpeedControl;

static void AssertEqual<T>(T expected, T actual, string message)
{
	if (!EqualityComparer<T>.Default.Equals(expected, actual))
	{
		throw new InvalidOperationException($"{message} Expected={expected} Actual={actual}");
	}
}

static void AssertTrue(bool condition, string message)
{
	if (!condition)
	{
		throw new InvalidOperationException(message);
	}
}

string tempRoot = Path.Combine(Path.GetTempPath(), "game_speed_control_tests_" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(tempRoot);

try
{
	AssertEqual(1.0, SpeedPresetCycle.Normalize(0.25), "Values below the minimum preset should fall back to 1x.");
	AssertEqual(1.0, SpeedPresetCycle.Normalize(9.0), "Values above the maximum preset should fall back to 1x.");
	AssertEqual(2.0, SpeedPresetCycle.Normalize(2.0), "Exact valid speed should be preserved.");
	AssertEqual(3.0, SpeedPresetCycle.Next(2.0), "2x should advance to 3x.");
	AssertEqual(1.0, SpeedPresetCycle.Next(4.0), "4x should wrap back to 1x.");
	AssertEqual("速度 3x", SpeedPresetCycle.FormatLabel(3.0), "Label should use Chinese pause menu text.");

	string missingPath = Path.Combine(tempRoot, "missing.json");
	AssertEqual(1.0, SpeedSettingsStore.Load(missingPath).SpeedMultiplier, "Missing config should default to 1x.");

	string corruptPath = Path.Combine(tempRoot, "corrupt.json");
	File.WriteAllText(corruptPath, "{ not valid json");
	AssertEqual(1.0, SpeedSettingsStore.Load(corruptPath).SpeedMultiplier, "Corrupt config should default to 1x.");

	string invalidSpeedPath = Path.Combine(tempRoot, "invalid-speed.json");
	File.WriteAllText(invalidSpeedPath, JsonSerializer.Serialize(new SpeedSettings { SchemaVersion = 1, SpeedMultiplier = 99.0 }));
	AssertEqual(1.0, SpeedSettingsStore.Load(invalidSpeedPath).SpeedMultiplier, "Unsupported speed should default to 1x.");

	string validPath = Path.Combine(tempRoot, "nested", "settings.json");
	SpeedSettingsStore.Save(validPath, new SpeedSettings { SchemaVersion = 1, SpeedMultiplier = 4.0 });
	SpeedSettings loaded = SpeedSettingsStore.Load(validPath);
	AssertEqual(1, loaded.SchemaVersion, "Schema version should round-trip.");
	AssertEqual(4.0, loaded.SpeedMultiplier, "Saved speed should round-trip.");
	AssertTrue(File.Exists(validPath), "Settings file should be written.");
}
finally
{
	Directory.Delete(tempRoot, recursive: true);
}
