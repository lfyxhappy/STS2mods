using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

[HarmonyPatch(typeof(CardModel), "get_HasBetaPortrait")]
internal static class WatcherCardHasBetaPortraitPatch
{
	private static bool Prefix(CardModel __instance, ref bool __result)
	{
		if (!(__instance.Pool is WatcherCardPool))
		{
			return true;
		}
		__result = WatcherTextureHelper.LoadTexture(__instance.BetaPortraitPath) != null;
		return false;
	}
}
