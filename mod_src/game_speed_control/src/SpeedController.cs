using Godot;

namespace GameSpeedControl;

internal static class SpeedController
{
	private static readonly CombatSpeedState State = new();

	private static string? _settingsPath;

	public static double CurrentSpeed => State.TargetSpeed;

	public static void LoadAndApply()
	{
		SpeedSettings settings = SpeedSettingsStore.Load(GetSettingsPath());
		ApplyTargetSpeed(settings.SpeedMultiplier, persist: false);
	}

	public static double Cycle()
	{
		double targetSpeed = State.CycleTargetSpeed();
		ApplyEffectiveSpeed();
		PersistTargetSpeed();
		return targetSpeed;
	}

	public static void EnterCombat()
	{
		State.EnterCombat();
		ApplyEffectiveSpeed();
	}

	public static void ExitCombat()
	{
		State.ExitCombat();
		ApplyEffectiveSpeed();
	}

	private static void ApplyTargetSpeed(double speed, bool persist)
	{
		State.LoadTargetSpeed(speed);
		ApplyEffectiveSpeed();
		if (persist)
		{
			PersistTargetSpeed();
		}
	}

	private static void PersistTargetSpeed()
	{
		try
		{
			SpeedSettingsStore.Save(GetSettingsPath(), new SpeedSettings
			{
				SchemaVersion = 1,
				SpeedMultiplier = State.TargetSpeed
			});
		}
		catch
		{
			// Failing to persist a quality-of-life setting should not break the game.
		}
	}

	private static void ApplyEffectiveSpeed()
	{
		Engine.TimeScale = State.EffectiveSpeed;
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
