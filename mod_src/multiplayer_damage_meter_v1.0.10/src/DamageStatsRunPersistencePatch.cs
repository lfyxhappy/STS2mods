using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;

namespace MultiplayerDamageMeter;

[HarmonyPatch]
public static class DamageStatsRunPersistencePatch
{
	private static readonly AccessTools.FieldRef<RunManager, long> StartTimeRef = AccessTools.FieldRefAccess<RunManager, long>("_startTime");

	[HarmonyPostfix]
	[HarmonyPatch(typeof(RunManager), nameof(RunManager.SetUpNewSinglePlayer))]
	public static void AfterSetUpNewSinglePlayer(RunManager __instance, RunState state)
	{
		DamageStatsService.PrepareForUpcomingRun(state, StartTimeRef(__instance), restoreRunTotals: false);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(RunManager), nameof(RunManager.SetUpNewMultiPlayer))]
	public static void AfterSetUpNewMultiPlayer(RunManager __instance, RunState state)
	{
		DamageStatsService.PrepareForUpcomingRun(state, StartTimeRef(__instance), restoreRunTotals: false);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(RunManager), nameof(RunManager.SetUpSavedSinglePlayer))]
	public static void AfterSetUpSavedSinglePlayer(RunManager __instance, RunState state)
	{
		DamageStatsService.PrepareForUpcomingRun(state, StartTimeRef(__instance), restoreRunTotals: true);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(RunManager), nameof(RunManager.SetUpSavedMultiPlayer))]
	public static void AfterSetUpSavedMultiPlayer(RunManager __instance, RunState state)
	{
		DamageStatsService.PrepareForUpcomingRun(state, StartTimeRef(__instance), restoreRunTotals: true);
	}
}
