using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Saves;

namespace WatcherMod;

[HarmonyPatch(typeof(StartRunLobby), "SetSingleplayerAscensionAfterCharacterChanged")]
internal static class WatcherAscensionUnlockPatch
{
	private static void Prefix(ModelId characterId)
	{
		EnsureWatcherAscensionUnlocked(characterId);
	}

	internal static void EnsureWatcherAscensionUnlocked(ModelId? characterId)
	{
		if (characterId == null || (characterId != ModelDb.GetId(typeof(Watcher)) && characterId != ModelDb.GetId(typeof(WatcherV2))))
		{
			return;
		}
		ProgressState progressState = SaveManager.Instance?.Progress;
		if (progressState != null)
		{
			CharacterStats orCreateCharacterStats = progressState.GetOrCreateCharacterStats(characterId);
			if (orCreateCharacterStats != null && orCreateCharacterStats.MaxAscension < 10)
			{
				orCreateCharacterStats.MaxAscension = 10;
			}
		}
	}
}
