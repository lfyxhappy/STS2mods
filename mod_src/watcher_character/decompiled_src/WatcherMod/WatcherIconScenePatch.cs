using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

[HarmonyPatch(typeof(CharacterModel), "get_Icon")]
internal static class WatcherIconScenePatch
{
	private static bool Prefix(CharacterModel __instance, ref Control __result)
	{
		if (!(__instance is Watcher))
		{
			return true;
		}
		Texture2D texture2D = WatcherTextureHelper.LoadTexture("res://images/ui/top_panel/character_icon_" + __instance.Id.Entry.ToLower() + ".png");
		if (texture2D != null)
		{
			TextureRect textureRect = new TextureRect
			{
				Texture = texture2D,
				ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				AnchorRight = 1f,
				AnchorBottom = 1f,
				GrowHorizontal = Control.GrowDirection.Both,
				GrowVertical = Control.GrowDirection.Both,
				MouseFilter = Control.MouseFilterEnum.Ignore
			};
			__result = textureRect;
			return false;
		}
		return true;
	}
}
