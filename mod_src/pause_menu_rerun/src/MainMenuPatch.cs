using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace PauseMenuRerun;

[HarmonyPatch(typeof(NMainMenu), nameof(NMainMenu._Ready))]
internal static class MainMenuPatch
{
	private static void Postfix(NMainMenu __instance)
	{
		RerunCoordinator.TryContinueFromMainMenu(__instance);
	}
}
