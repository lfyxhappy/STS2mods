using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class Meditate : WatcherCard
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		WatcherHoverTips.Stance,
		HoverTipFactory.FromPower<Calm>()
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar("MagicNumber", 1));

	public Meditate()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		foreach (CardModel card in await WatcherSimpleBHelper.TakeFromDiscard(choiceContext, base.Owner, base.DynamicVars["MagicNumber"].IntValue))
		{
			card.GiveSingleTurnRetain();
			await CardPileCmd.Add(card, PileType.Hand);
			CardPile? pile = card.Pile;
			if (pile == null || pile.Type != PileType.Hand)
			{
				WatcherCombatHelper.DeferRetainCard(card);
			}
		}
		await WatcherCombatHelper.EnterCalm(base.Owner, this);
		if (cardPlay.IsLastInSeries)
		{
			PlayerCmd.EndTurn(base.Owner, canBackOut: false);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(1m);
	}
}
