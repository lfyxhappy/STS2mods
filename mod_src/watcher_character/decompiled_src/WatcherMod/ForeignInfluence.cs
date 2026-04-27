using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace WatcherMod;

public sealed class ForeignInfluence : WatcherCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	public ForeignInfluence()
		: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		bool hasNecrobinder = base.Owner.RunState.Players.Any((Player p) => p.Character.CardPool is NecrobinderCardPool);
		bool isSinglePlayer = base.Owner.RunState.Players.Count <= 1;
		List<CardModel> options = (from card in (from _ in CardFactory.FilterForCombat(ModelDb.AllCards)
				where _.Type == CardType.Attack && _.Id != base.Id && _.Rarity != CardRarity.Token && (hasNecrobinder || !(_.Pool is NecrobinderCardPool)) && (!isSinglePlayer || _.MultiplayerConstraint != CardMultiplayerConstraint.MultiplayerOnly) && (isSinglePlayer || _.MultiplayerConstraint != CardMultiplayerConstraint.SingleplayerOnly)
				orderby base.Owner.RunState.Rng.Niche.NextInt()
				select _).Take(3)
			select base.CombatState.CreateCard(card, base.Owner)).ToList();
		CardModel cardModel = await WatcherCombatHelper.ChooseOne(base.Owner, options, new LocString("cards", "FOREIGN_INFLUENCE.selectionScreenPrompt"), cancelable: true);
		if (cardModel != null)
		{
			if (base.IsUpgraded)
			{
				cardModel.SetToFreeThisTurn();
			}
			await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, addedByPlayer: true);
		}
	}
}
