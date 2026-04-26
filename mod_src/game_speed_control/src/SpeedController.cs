using Godot;

namespace GameSpeedControl;

internal static class SpeedController
{
	private static double _currentSpeed = 1.0;
	private static string? _settingsPath;

	public static double CurrentSpeed => _currentSpeed;

	public static void LoadAndApply()
	{
		SpeedSettings settings = SpeedSettingsStore.Load(GetSettingsPath());
		Apply(settings.SpeedMultiplier, persist: false);
	}

	public static double Cycle()
	{
		double nextSpeed = SpeedPresetCycle.Next(_currentSpeed);
		Apply(nextSpeed, persist: true);
		return _currentSpeed;
	}

	public static void Apply(double speed, bool persist)
	{
		_currentSpeed = SpeedPresetCycle.Normalize(speed);
		Engine.TimeScale = _currentSpeed;

		if (!persist)
		{
			return;
		}

		try
		{
			SpeedSettingsStore.Save(GetSettingsPath(), new SpeedSettings
			{
				SchemaVersion = 1,
				SpeedMultiplier = _currentSpeed
			});
		}
		catch
		{
			// Failing to persist a quality-of-life setting should not break the game.
		}
	}

	private static string GetSettingsPath()
	{
		if (_settingsPath != null)
		{
			return _settingsPath;
		}

		string userRoot = ProjectSettings.GlobalizePath("user://");
		_settingsPath = Path.Combine(userRoot, "game_speed_control", "settings.json");
		return _settingsPath;
	}
}
