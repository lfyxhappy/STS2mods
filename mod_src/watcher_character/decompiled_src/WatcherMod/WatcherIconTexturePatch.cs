using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

[HarmonyPatch(typeof(CharacterModel), "get_IconTexture")]
internal static class WatcherIconTexturePatch
{
	private static bool Prefix(CharacterModel __instance, ref Texture2D __result)
	{
		if (!(__instance is Watcher))
		{
			return true;
		}
		Texture2D texture2D = WatcherTextureHelper.LoadTexture("res://images/ui/top_panel/character_icon_" + __instance.Id.Entry.ToLower() + ".png");
		if (texture2D != null)
		{
			__result = texture2D;
			return false;
		}
		return true;
	}
}
