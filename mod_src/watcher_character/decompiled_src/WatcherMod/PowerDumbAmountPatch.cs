using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

[HarmonyPatch(typeof(PowerModel), "AddDumbVariablesToDescription")]
internal static class PowerDumbAmountPatch
{
	private static void Postfix(PowerModel __instance, LocString description)
	{
		description.Add("Amount", __instance.Amount);
	}
}
