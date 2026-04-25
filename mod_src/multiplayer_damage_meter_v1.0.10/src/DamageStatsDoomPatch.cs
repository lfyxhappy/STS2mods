using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MultiplayerDamageMeter;

[HarmonyPatch(typeof(DoomPower), nameof(DoomPower.DoomKill))]
public static class DamageStatsDoomPatch
{
	[HarmonyPrefix]
	public static void BeforeDoomKill(IReadOnlyList<Creature> creatures)
	{
		DamageStatsService.CapturePendingDoomKills(creatures);
	}
}
