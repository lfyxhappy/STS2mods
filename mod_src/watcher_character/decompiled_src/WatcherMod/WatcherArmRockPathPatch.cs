using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

[HarmonyPatch(typeof(CharacterModel), "ArmRockTexturePath", MethodType.Getter)]
internal static class WatcherArmRockPathPatch
{
	private static bool Prefix(CharacterModel __instance, ref string __result)
	{
		if (!(__instance is Watcher))
		{
			return true;
		}
		__result = "res://images/ui/hands/multiplayer_hand_watcher_rock.png";
		return false;
	}
}
