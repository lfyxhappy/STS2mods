using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.PauseMenu;

namespace GameSpeedControl;

[HarmonyPatch(typeof(NPauseMenu), nameof(NPauseMenu._Ready))]
internal static class PauseMenuSpeedPatch
{
	private static void Postfix(NPauseMenu __instance)
	{
		PauseMenuSpeedUi.AddSpeedButton(__instance);
	}
}
