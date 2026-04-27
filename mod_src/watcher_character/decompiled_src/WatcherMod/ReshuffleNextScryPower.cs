using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace WatcherMod;

public sealed class ReshuffleNextScryPower : PowerModel, IWatcherProphecyListener
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public async Task OnProphecy(Player owner, ProphecyContext ctx)
	{
		if (owner != base.Owner.Player || !ctx.FromScry || ctx.Source is Reshuffle)
		{
			return;
		}
		Flash();
		if (owner.PlayerCombatState == null)
		{
			await PowerCmd.Remove(this);
			return;
		}
		List<CardModel> list = ctx.PeekedCards.Where((CardModel c) => c.Type == CardType.Attack).ToList();
		if (list.Count == 0 || WatcherEnchantStack.RandomPool.Count == 0)
		{
			await PowerCmd.Remove(this);
			return;
		}
		Rng combatCardSelection = owner.RunState.Rng.CombatCardSelection;
		int num = Math.Min(base.Amount, list.Count);
		for (int num2 = 0; num2 < num; num2++)
		{
			CardModel cardModel = combatCardSelection.NextItem(list);
			if (cardModel == null)
			{
				break;
			}
			list.Remove(cardModel);
			EnchantmentModel enchantmentModel = combatCardSelection.NextItem(WatcherEnchantStack.RandomPool);
			if (enchantmentModel == null)
			{
				break;
			}
			WatcherEnchantStack.ApplyTempEnchantment(cardModel, enchantmentModel);
		}
		await PowerCmd.Remove(this);
	}
}
