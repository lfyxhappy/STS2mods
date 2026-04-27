using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

[HarmonyPatch(typeof(CharacterModel), "ArmPointingTexturePath", MethodType.Getter)]
internal static class WatcherArmPointingPathPatch
{
	private static bool Prefix(CharacterModel __instance, ref string __result)
	{
		if (!(__instance is Watcher))
		{
			return true;
		}
		__result = "res://images/ui/hands/multiplayer_hand_watcher_point.png";
		return false;
	}
}
