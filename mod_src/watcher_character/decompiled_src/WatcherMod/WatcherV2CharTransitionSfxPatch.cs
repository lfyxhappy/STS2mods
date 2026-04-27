using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

[HarmonyPatch(typeof(CharacterModel), "get_CharacterTransitionSfx")]
internal static class WatcherV2CharTransitionSfxPatch
{
	private static void Postfix(CharacterModel __instance, ref string __result)
	{
		WatcherV2PathRedirect.Apply(__instance, ref __result);
	}
}
