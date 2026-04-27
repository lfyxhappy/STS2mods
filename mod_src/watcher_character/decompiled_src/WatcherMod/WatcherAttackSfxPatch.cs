using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

[HarmonyPatch(typeof(CharacterModel), "get_AttackSfx")]
internal static class WatcherAttackSfxPatch
{
	private static void Postfix(CharacterModel __instance, ref string __result)
	{
		if (__instance is Watcher)
		{
			__result = "event:/sfx/characters/necrobinder/necrobinder_attack";
		}
		else if (__instance is WatcherV2)
		{
			WatcherV2PathRedirect.Apply(__instance, ref __result);
		}
	}
}
