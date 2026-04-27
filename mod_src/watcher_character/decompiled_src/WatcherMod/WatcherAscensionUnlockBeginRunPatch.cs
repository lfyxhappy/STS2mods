using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;

namespace WatcherMod;

[HarmonyPatch(typeof(StartRunLobby), "BeginRunLocally")]
internal static class WatcherAscensionUnlockBeginRunPatch
{
	private static void Prefix(StartRunLobby __instance)
	{
		if (__instance?.Players == null)
		{
			return;
		}
		foreach (LobbyPlayer player in __instance.Players)
		{
			WatcherAscensionUnlockPatch.EnsureWatcherAscensionUnlocked(player.character?.Id);
		}
	}
}
