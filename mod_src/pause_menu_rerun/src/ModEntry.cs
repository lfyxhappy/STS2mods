using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace PauseMenuRerun;

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
		_harmony = new Harmony("codex.pause_menu_rerun");
		_harmony.PatchAll(typeof(ModEntry).Assembly);
	}
}
