using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

[HarmonyPatch(typeof(CharacterModel), "get_DeathSfx")]
internal static class WatcherDeathSfxPatch
{
	private static void Postfix(CharacterModel __instance, ref string __result)
	{
		if (__instance is Watcher)
		{
			__result = "event:/sfx/characters/necrobinder/necrobinder_die";
		}
		else if (__instance is WatcherV2)
		{
			WatcherV2PathRedirect.Apply(__instance, ref __result);
		}
	}
}
