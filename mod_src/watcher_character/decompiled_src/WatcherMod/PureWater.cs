using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;

namespace WatcherMod;

public sealed class PureWater : WatcherRelic
{
	public override RelicRarity Rarity => RelicRarity.Starter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Exhaust));

	public PureWater()
		: base("clean_water")
	{
	}

	public override async Task BeforeCombatStart()
	{
		if (base.Owner.Creature.CombatState != null)
		{
			Flash();
			await CardPileCmd.AddGeneratedCardToCombat(base.Owner.Creature.CombatState.CreateCard<Miracle>(base.Owner), PileType.Hand, addedByPlayer: true);
		}
	}
}
