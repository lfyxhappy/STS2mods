using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace WatcherMod;

[HarmonyPatch(typeof(TouchOfOrobas), "GetUpgradedStarterRelic")]
internal static class WatcherTouchOfOrobasPatch
{
	private static void Postfix(RelicModel starterRelic, ref RelicModel __result)
	{
		if (starterRelic is PureWater || starterRelic is ProphetWater)
		{
			__result = ModelDb.Relic<HolyWater>().ToMutable();
		}
	}
}
