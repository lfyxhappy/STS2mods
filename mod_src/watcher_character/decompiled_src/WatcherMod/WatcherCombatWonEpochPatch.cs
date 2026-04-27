using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Saves;

namespace WatcherMod;

[HarmonyPatch(typeof(SaveManager), "UpdateProgressAfterCombatWon")]
internal static class WatcherCombatWonEpochPatch
{
	private static Exception? Finalizer(Exception? __exception, Player localPlayer)
	{
		if (__exception != null && localPlayer.Character is Watcher)
		{
			Log.Warn("[Watcher] Suppressed epoch exception in UpdateAfterCombatWon: " + __exception.Message);
			return null;
		}
		return __exception;
	}
}
