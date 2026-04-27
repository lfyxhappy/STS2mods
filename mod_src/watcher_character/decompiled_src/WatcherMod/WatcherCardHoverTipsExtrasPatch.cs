using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

[HarmonyPatch(typeof(CardModel), "HoverTips", MethodType.Getter)]
internal static class WatcherCardHoverTipsExtrasPatch
{
	private static void Postfix(CardModel __instance, ref IEnumerable<IHoverTip> __result)
	{
		List<EnchantmentModel> extras = WatcherEnchantStack.GetExtras(__instance);
		bool flag = extras != null && extras.Count > 0;
		bool flag2 = __instance is IProphecyCard;
		bool flag3 = NeedsScryTip(__instance);
		if (!flag && !flag2 && !flag3)
		{
			return;
		}
		List<IHoverTip> list = __result.ToList();
		if (flag2 && !list.Contains(WatcherHoverTips.Prophecy))
		{
			list.Insert(0, WatcherHoverTips.Prophecy);
		}
		if (flag3 && !list.Contains(WatcherHoverTips.Scry))
		{
			list.Add(WatcherHoverTips.Scry);
		}
		if (flag)
		{
			foreach (EnchantmentModel item in extras)
			{
				list.AddRange(item.HoverTips);
			}
		}
		__result = list;
	}

	private static bool NeedsScryTip(CardModel card)
	{
		try
		{
			string text = card.Description?.GetRawText() ?? string.Empty;
			return text.Contains("预见") || text.Contains("Scry");
		}
		catch
		{
			return false;
		}
	}
}
