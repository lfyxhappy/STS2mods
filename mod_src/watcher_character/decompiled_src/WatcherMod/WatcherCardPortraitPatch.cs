using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace WatcherMod;

[HarmonyPatch(typeof(NCard), "Reload")]
internal static class WatcherCardPortraitPatch
{
	private static readonly AccessTools.FieldRef<NCard, TextureRect> PortraitRef = AccessTools.FieldRefAccess<NCard, TextureRect>("_portrait");

	private static void Postfix(NCard __instance)
	{
		CardModel model = __instance.Model;
		if (model?.Pool is WatcherCardPool)
		{
			Texture2D texture2D = WatcherTextureHelper.LoadTexture(WatcherCardArtSettings.GetEffectivePortraitPath(model));
			TextureRect textureRect = PortraitRef(__instance);
			if (texture2D != null && textureRect != null)
			{
				textureRect.Texture = texture2D;
			}
		}
	}
}
