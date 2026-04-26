using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace GameSpeedControl;

[ModInitializer("Init")]
public static class ModEntry
{
	private static bool _initialized;
	private static Harmony? _harmony;

	public static void Init()
	{
		if (_initialized)
		{
			return;
		}

		_initialized = true;
		SpeedController.LoadAndApply();
		_harmony = new Harmony("codex.game_speed_control");
		_harmony.PatchAll(typeof(ModEntry).Assembly);
	}
}
