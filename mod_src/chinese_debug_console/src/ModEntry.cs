using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace ChineseDebugConsole;

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
		_harmony = new Harmony("codex.chinese_debug_console");
		_harmony.PatchAll(typeof(ModEntry).Assembly);
	}
}
