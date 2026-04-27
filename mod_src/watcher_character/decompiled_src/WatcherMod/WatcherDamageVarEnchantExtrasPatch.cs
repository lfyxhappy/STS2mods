using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

[HarmonyPatch(typeof(DamageVar), "UpdateCardPreview")]
internal static class WatcherDamageVarEnchantExtrasPatch
{
	private static void Postfix(DamageVar __instance, CardModel card, bool runGlobalHooks)
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
			previewValue += item.EnchantDamageAdditive(previewValue, props);
			previewValue *= item.EnchantDamageMultiplicative(previewValue, props);
		}
		if (!card.IsEnchantmentPreview)
		{
			__instance.EnchantedValue = previewValue;
		}
		__instance.PreviewValue = previewValue;
	}
}
