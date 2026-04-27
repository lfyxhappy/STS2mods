using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class DeusExMachina : WatcherCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlyArray<CardKeyword>(new CardKeyword[2]
	{
		CardKeyword.Unplayable,
		CardKeyword.Exhaust
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<Miracle>());

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("MagicNumber", 2m));

	public DeusExMachina()
		: base(-1, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card != this || base.CombatState == null)
		{
			return;
		}
		CardPile? pile = base.Pile;
		if (pile == null || pile.Type != PileType.Hand)
		{
			return;
		}
		await CardCmd.AutoPlay(choiceContext, this, null);
		CardPile? pile2 = base.Pile;
		if (pile2 != null && pile2.Type == PileType.Exhaust)
		{
			for (int index = 0; index < base.DynamicVars["MagicNumber"].IntValue; index++)
			{
				await CardPileCmd.AddGeneratedCardToCombat(base.CombatState.CreateCard<Miracle>(base.Owner), PileType.Hand, addedByPlayer: true);
			}
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await Task.CompletedTask;
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(1m);
	}
}
