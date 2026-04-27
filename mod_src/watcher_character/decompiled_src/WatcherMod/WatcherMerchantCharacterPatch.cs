using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace WatcherMod;

[HarmonyPatch(typeof(NMerchantCharacter), "_Ready")]
internal static class WatcherMerchantCharacterPatch
{
	private static bool Prefix(NMerchantCharacter __instance)
	{
		Node node = ((__instance.GetChildCount() > 0) ? __instance.GetChild(0) : null);
		if (!(node is Sprite2D sprite2D))
		{
			if (node == null || node.GetClass() != "SpineSprite")
			{
				return true;
			}
			MegaSprite megaSprite = new MegaSprite(node);
			if (megaSprite.HasAnimation("relaxed_loop"))
			{
				return true;
			}
			if (node is Node2D node2D)
			{
				WatcherSkeletonHelper.ApplySkeletonVariant(megaSprite);
				megaSprite.GetAnimationState().SetAnimation("Idle");
				node2D.Scale = new Vector2(1.3f, 1.3f);
				node2D.SetDeferred("scale", new Vector2(1.3f, 1.3f));
			}
			return false;
		}
		Texture2D texture2D = WatcherTextureHelper.LoadTexture("res://images/characters/watcher/watcher_idle.png");
		if (texture2D != null)
		{
			sprite2D.Texture = texture2D;
		}
		return false;
	}
}
