using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

[HarmonyPatch(typeof(CharacterModel), "ArmPaperTexturePath", MethodType.Getter)]
internal static class WatcherArmPaperPathPatch
{
	private static bool Prefix(CharacterModel __instance, ref string __result)
	{
		if (!(__instance is Watcher))
		{
			return true;
		}
		__result = "res://images/ui/hands/multiplayer_hand_watcher_paper.png";
		return false;
	}
}
