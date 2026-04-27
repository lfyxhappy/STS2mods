using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Runs;

namespace WatcherMod;

[HarmonyPatch(typeof(TheArchitect), "WinRun")]
internal static class WatcherArchitectWinRunPatch
{
	private static bool Prefix(TheArchitect __instance, ref Task __result)
	{
		if (AccessTools.Field(typeof(TheArchitect), "_dialogue")?.GetValue(__instance) != null)
		{
			return true;
		}
		if (LocalContext.IsMe(__instance.Owner))
		{
			RunManager.Instance.ActChangeSynchronizer.SetLocalPlayerReady();
		}
		__result = Task.CompletedTask;
		return false;
	}
}
