using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

[HarmonyPatch(typeof(CharacterModel), "get_CharacterSelectSfx")]
internal static class WatcherV2CharSelectSfxPatch
{
	private static void Postfix(CharacterModel __instance, ref string __result)
	{
		WatcherV2PathRedirect.Apply(__instance, ref __result);
	}
}
