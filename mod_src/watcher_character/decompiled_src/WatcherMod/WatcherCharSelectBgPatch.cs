using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace WatcherMod;

[HarmonyPatch(typeof(NCharacterSelectScreen), "SelectCharacter")]
internal static class WatcherCharSelectBgPatch
{
	private static readonly AccessTools.FieldRef<NCharacterSelectScreen, Control> BgContainerRef = AccessTools.FieldRefAccess<NCharacterSelectScreen, Control>("_bgContainer");

	private static void Postfix(NCharacterSelectScreen __instance, NCharacterSelectButton charSelectButton, CharacterModel characterModel)
	{
		if (!(characterModel is Watcher))
		{
			return;
		}
		Control control = BgContainerRef(__instance);
		if (control == null)
		{
			return;
		}
		Node nodeOrNull = control.GetNodeOrNull(characterModel.Id.Entry + "_bg");
		if (nodeOrNull == null)
		{
			return;
		}
		TextureRect nodeOrNull2 = nodeOrNull.GetNodeOrNull<TextureRect>("Portrait");
		if (nodeOrNull2 != null)
		{
			Texture2D texture2D = WatcherTextureHelper.LoadTexture((characterModel is WatcherV2) ? "res://images/ui/charSelect/watcherPortrait_v2.jpg" : "res://images/ui/charSelect/watcherPortrait.jpg");
			if (texture2D != null)
			{
				nodeOrNull2.Texture = texture2D;
			}
		}
		WatcherAudioHelper.PlayOneShot("res://audio/watcher/select.ogg");
	}
}
