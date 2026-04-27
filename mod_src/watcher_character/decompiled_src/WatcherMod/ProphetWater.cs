using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace WatcherMod;

public sealed class ProphetWater : WatcherRelic
{
	public override RelicRarity Rarity => RelicRarity.Starter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
		HoverTipFactory.FromPower<KnowFatePower>()
	});

	public ProphetWater()
		: base("clean_water")
	{
	}

	public override async Task BeforeCombatStart()
	{
		if (base.Owner.Creature.CombatState != null)
		{
			Flash();
			await CardPileCmd.AddGeneratedCardToCombat(base.Owner.Creature.CombatState.CreateCard<Miracle>(base.Owner), PileType.Hand, addedByPlayer: true);
			if (!base.Owner.Creature.HasPower<WatcherStatePower>())
			{
				await PowerCmd.Apply<WatcherStatePower>(base.Owner.Creature, 1m, base.Owner.Creature, null);
			}
		}
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card is IProphecyCard)
		{
			int amountToSpend = cardPlay.Card.EnergyCost.GetAmountToSpend();
			if (amountToSpend > 0)
			{
				Flash();
				await PowerCmd.Apply<KnowFatePower>(base.Owner.Creature, amountToSpend, base.Owner.Creature, null);
			}
		}
	}
}
