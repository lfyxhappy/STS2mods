using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace WatcherMod;

public sealed class ConjureBlade : WatcherCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	protected override bool HasEnergyCostX => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<Expunger>());

	public ConjureBlade()
		: base(-1, CardType.Skill, CardRarity.Rare, TargetType.None)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int num = ResolveEnergyXValue();
		if (base.IsUpgraded)
		{
			num++;
		}
		if (num > 0)
		{
			Expunger expunger = base.CombatState.CreateCard<Expunger>(base.Owner);
			if (expunger is Expunger expunger2)
			{
				expunger2.HitCount = num;
			}
			await CardPileCmd.AddGeneratedCardToCombat(expunger, PileType.Draw, addedByPlayer: true, CardPilePosition.Random);
		}
	}

	protected override void OnUpgrade()
	{
	}
}
