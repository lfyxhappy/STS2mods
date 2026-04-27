using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

[HarmonyPatch(typeof(CardModel), "get_HasPortrait")]
internal static class WatcherCardHasPortraitPatch
{
	private static bool Prefix(CardModel __instance, ref bool __result)
	{
		if (!(__instance.Pool is WatcherCardPool))
		{
			return true;
		}
		__result = WatcherTextureHelper.LoadTexture(__instance.PortraitPath) != null || WatcherTextureHelper.LoadTexture("res://images/packed/card_portraits/watcher/_placeholder.png") != null;
		return false;
	}
}
