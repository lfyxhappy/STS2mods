using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

[HarmonyPatch(typeof(CardModel), "get_Portrait")]
internal static class WatcherCardModelPortraitPatch
{
	private static bool Prefix(CardModel __instance, ref Texture2D __result)
	{
		if (!(__instance.Pool is WatcherCardPool))
		{
			return true;
		}
		Texture2D texture2D = WatcherTextureHelper.LoadTexture(WatcherCardArtSettings.GetEffectivePortraitPath(__instance));
		if (texture2D != null)
		{
			__result = texture2D;
			return false;
		}
		return true;
	}
}
