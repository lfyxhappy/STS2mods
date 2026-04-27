using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

[HarmonyPatch(typeof(CardModel), "GetEnchantedReplayCount")]
internal static class WatcherCardEnchantedReplayCountExtrasPatch
{
	private static void Postfix(CardModel __instance, ref int __result)
	{
		List<EnchantmentModel> extras = WatcherEnchantStack.GetExtras(__instance);
		if (extras == null || extras.Count == 0)
		{
			return;
		}
		int num = __result;
		foreach (EnchantmentModel item in extras)
		{
			num = item.EnchantPlayCount(num);
		}
		__result = num;
	}
}
