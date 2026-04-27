using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace WatcherMod;

[HarmonyPatch(typeof(NCreatureVisuals), "_Ready")]
internal static class WatcherCreatureVisualsPatch
{
	private static void Postfix(NCreatureVisuals __instance)
	{
		if (!(__instance.Name != (StringName)"Watcher") && __instance.GetNodeOrNull<Node2D>("%Visuals") is Sprite2D sprite2D)
		{
			Texture2D texture2D = WatcherTextureHelper.LoadTexture("res://images/characters/watcher/watcher_idle.png");
			if (texture2D != null)
			{
				sprite2D.Texture = texture2D;
			}
		}
	}
}
