using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace WatcherMod;

[HarmonyPatch(typeof(NCharacterSelectButton), "Init")]
internal static class WatcherV2CharSelectShimmerPatch
{
	private static readonly FieldInfo? IconField = AccessTools.Field(typeof(NCharacterSelectButton), "_icon");

	private static void Postfix(NCharacterSelectButton __instance, CharacterModel character)
	{
		if (character is WatcherV2 && IconField?.GetValue(__instance) is TextureRect target)
		{
			WatcherShimmerOverlay.AttachTo(target);
		}
	}
}
