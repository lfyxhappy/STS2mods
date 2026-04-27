using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;

namespace WatcherMod;

[HarmonyPatch(typeof(Hook), "AfterSideTurnStart")]
internal static class WatcherHpSnapshotPatch
{
	private static void Postfix(CombatState combatState, CombatSide side)
	{
		if (side != CombatSide.Player)
		{
			return;
		}
		try
		{
			foreach (Creature playerCreature in combatState.PlayerCreatures)
			{
				if (playerCreature.IsAlive)
				{
					WatcherTurnHpSnapshot.Record(playerCreature);
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Watcher] HP snapshot failed: " + ex.Message);
		}
	}
}
