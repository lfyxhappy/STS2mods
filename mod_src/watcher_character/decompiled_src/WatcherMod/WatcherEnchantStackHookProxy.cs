using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class WatcherEnchantStackHookProxy : AbstractModel
{
	public static WatcherEnchantStackHookProxy? Instance { get; private set; }

	public override bool ShouldReceiveCombatHooks => true;

	public WatcherEnchantStackHookProxy()
	{
		Instance = this;
	}

	public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
	{
		if (cardSource == null)
		{
			return 0m;
		}
		List<EnchantmentModel> extras = WatcherEnchantStack.GetExtras(cardSource);
		if (extras == null || extras.Count == 0)
		{
			return 0m;
		}
		decimal num = block;
		foreach (EnchantmentModel item in extras)
		{
			num += item.EnchantBlockAdditive(num, props);
			num *= item.EnchantBlockMultiplicative(num, props);
		}
		return num - block;
	}

	public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (cardSource == null)
		{
			return 0m;
		}
		List<EnchantmentModel> extras = WatcherEnchantStack.GetExtras(cardSource);
		if (extras == null || extras.Count == 0)
		{
			return 0m;
		}
		decimal num = amount;
		foreach (EnchantmentModel item in extras)
		{
			num += item.EnchantDamageAdditive(num, props);
			num *= item.EnchantDamageMultiplicative(num, props);
		}
		return num - amount;
	}
}
