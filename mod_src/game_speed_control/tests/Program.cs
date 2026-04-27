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
	AssertEqual(1.5, SpeedPresetCycle.Normalize(1.5), "Half-step speed should be a valid preset.");
	AssertEqual(2.0, SpeedPresetCycle.Normalize(2.0), "Exact valid speed should be preserved.");
	AssertEqual(1.5, SpeedPresetCycle.Next(1.0), "1x should advance to 1.5x.");
	AssertEqual(2.5, SpeedPresetCycle.Next(2.0), "2x should advance to 2.5x.");
	AssertEqual(1.0, SpeedPresetCycle.Next(4.0), "4x should wrap back to 1x.");
	AssertEqual("速度 1.5x", SpeedPresetCycle.FormatLabel(1.5), "Label should display half-step speeds.");
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

	CombatSpeedState state = new CombatSpeedState();
	state.LoadTargetSpeed(3.0);
	AssertEqual(3.0, state.TargetSpeed, "Loaded target speed should be remembered.");
	AssertEqual(1.0, state.EffectiveSpeed, "Loaded speed should not apply outside combat.");

	state.EnterCombat();
	AssertEqual(3.0, state.EffectiveSpeed, "Entering combat should apply the remembered target speed.");

	state.CycleTargetSpeed();
	AssertEqual(3.5, state.TargetSpeed, "Cycling during combat should update target speed by a half-step.");
	AssertEqual(3.5, state.EffectiveSpeed, "Cycling during combat should immediately update effective speed.");

	state.ExitCombat();
	AssertEqual(1.0, state.EffectiveSpeed, "Leaving combat should restore normal speed.");

	state.CycleTargetSpeed();
	AssertEqual(4.0, state.TargetSpeed, "Cycling after 3.5x outside combat should advance to 4x.");
	AssertEqual(1.0, state.EffectiveSpeed, "Cycling outside combat should not speed up global time.");

	state.CycleTargetSpeed();
	AssertEqual(1.0, state.TargetSpeed, "Cycling after 4x outside combat should wrap target speed to 1x.");
	AssertEqual(1.0, state.EffectiveSpeed, "Cycling outside combat should not speed up global time.");

	state.CycleTargetSpeed();
	AssertEqual(1.5, state.TargetSpeed, "Cycling outside combat should still update the next combat target speed by a half-step.");
	AssertEqual(1.0, state.EffectiveSpeed, "Outside combat should continue to use 1x even after choosing 2x.");

	state.EnterCombat();
	AssertEqual(1.5, state.EffectiveSpeed, "Next combat should use the target selected outside combat.");
}
finally
{
	Directory.Delete(tempRoot, recursive: true);
}
