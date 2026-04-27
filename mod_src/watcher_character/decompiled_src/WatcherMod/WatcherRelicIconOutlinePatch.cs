using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

[HarmonyPatch(typeof(RelicModel), "get_IconOutline")]
internal static class WatcherRelicIconOutlinePatch
{
	private static bool Prefix(RelicModel __instance, ref Texture2D __result)
	{
		if (!(__instance is WatcherRelic))
		{
			return true;
		}
		Texture2D texture2D = WatcherTextureHelper.LoadTexture(__instance.PackedIconPath.Replace("/relics/", "/relics/outline/"));
		if (texture2D != null)
		{
			__result = texture2D;
			return false;
		}
		return true;
	}
}
