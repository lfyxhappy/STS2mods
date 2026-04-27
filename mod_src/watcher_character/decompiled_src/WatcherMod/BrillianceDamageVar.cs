using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

internal sealed class BrillianceDamageVar : DamageVar
{
	public BrillianceDamageVar(decimal damage, ValueProp props)
		: base(damage, props)
	{
	}

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		decimal num = ((decimal?)card.Owner?.Creature?.GetPower<WatcherStatePower>()?.TotalMantraGainedThisCombat) ?? 0m;
		decimal num2 = base.BaseValue + num;
		decimal num3 = num2;
		EnchantmentModel enchantment = card.Enchantment;
		if (enchantment != null)
		{
			num3 += enchantment.EnchantDamageAdditive(num3, base.Props);
			num3 *= enchantment.EnchantDamageMultiplicative(num3, base.Props);
			if (!card.IsEnchantmentPreview)
			{
				base.EnchantedValue = num3;
			}
		}
		if (runGlobalHooks)
		{
			num3 = Hook.ModifyDamage(card.Owner.RunState, card.CombatState, target, card.Owner.Creature, num2, base.Props, card, ModifyDamageHookType.All, previewMode, out IEnumerable<AbstractModel> _);
		}
		else
		{
			List<EnchantmentModel> extras = WatcherEnchantStack.GetExtras(card);
			if (extras != null && extras.Count > 0)
			{
				foreach (EnchantmentModel item in extras)
				{
					num3 += item.EnchantDamageAdditive(num3, base.Props);
					num3 *= item.EnchantDamageMultiplicative(num3, base.Props);
				}
				if (!card.IsEnchantmentPreview)
				{
					base.EnchantedValue = num3;
				}
			}
		}
		base.PreviewValue = num3;
	}
}
