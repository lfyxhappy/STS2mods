using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

[HarmonyPatch(typeof(CharacterModel), "get_CharacterSelectIcon")]
internal static class WatcherCharSelectIconPatch
{
	private static bool Prefix(CharacterModel __instance, ref Texture2D __result)
	{
		if (!(__instance is Watcher))
		{
			return true;
		}
		Texture2D texture2D = WatcherTextureHelper.LoadTexture("res://images/packed/character_select/char_select_watcher.png");
		if (texture2D != null)
		{
			__result = texture2D;
			return false;
		}
		return true;
	}
}
