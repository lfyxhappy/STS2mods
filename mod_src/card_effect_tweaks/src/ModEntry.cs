using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace CardEffectTweaks;

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
		_harmony = new Harmony("codex.card_effect_tweaks");
		_harmony.PatchAll(typeof(ModEntry).Assembly);
	}
}
