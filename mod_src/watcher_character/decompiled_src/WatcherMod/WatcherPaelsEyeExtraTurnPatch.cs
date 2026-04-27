using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace WatcherMod;

[HarmonyPatch(typeof(PaelsEye), "AfterTakingExtraTurn")]
internal static class WatcherPaelsEyeExtraTurnPatch
{
	private static bool Prefix(ref Task __result)
	{
		if (WatcherExtraTurnPower.SkipPaelsEyeConsumption)
		{
			WatcherExtraTurnPower.SkipPaelsEyeConsumption = false;
			__result = Task.CompletedTask;
			return false;
		}
		return true;
	}
}
