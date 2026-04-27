using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

[HarmonyPatch(typeof(BlockVar), "UpdateCardPreview")]
internal static class WatcherBlockVarEnchantExtrasPatch
{
	private static void Postfix(BlockVar __instance, CardModel card, bool runGlobalHooks)
	{
		if (runGlobalHooks)
		{
			return;
		}
		List<EnchantmentModel> extras = WatcherEnchantStack.GetExtras(card);
		if (extras == null || extras.Count == 0)
		{
			return;
		}
		decimal previewValue = __instance.PreviewValue;
		ValueProp props = __instance.Props;
		foreach (EnchantmentModel item in extras)
		{
			previewValue += item.EnchantBlockAdditive(previewValue, props);
			previewValue *= item.EnchantBlockMultiplicative(previewValue, props);
		}
		if (!card.IsEnchantmentPreview)
		{
			__instance.EnchantedValue = previewValue;
		}
		__instance.PreviewValue = previewValue;
	}
}
