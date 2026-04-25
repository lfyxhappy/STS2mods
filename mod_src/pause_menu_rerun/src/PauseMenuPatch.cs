using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Screens.PauseMenu;
using MegaCrit.Sts2.Core.Runs;

namespace PauseMenuRerun;

[HarmonyPatch(typeof(NPauseMenu), nameof(NPauseMenu._Ready))]
internal static class PauseMenuPatch
{
	private static void Postfix(NPauseMenu __instance)
	{
		if (!RunManager.Instance.IsInProgress || RunManager.Instance.IsGameOver)
		{
			return;
		}

		if (RunManager.Instance.NetService.Type != NetGameType.Singleplayer)
		{
			return;
		}

		RerunCoordinator.AddRerunButton(__instance);
	}
}
