using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class AnswerQuestion : WatcherCard, IProphecyCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar("MagicNumber", 3));

	public AnswerQuestion()
		: base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		PlayerCombatState playerCombatState = base.Owner.PlayerCombatState;
		if (playerCombatState == null)
		{
			return;
		}
		int intValue = base.DynamicVars["MagicNumber"].IntValue;
		int playCount = ((!base.IsUpgraded) ? 1 : 2);
		List<CardModel> topCards = playerCombatState.DrawPile.Cards.Take(intValue).ToList();
		if (topCards.Count == 0)
		{
			await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
			{
				Source = this
			});
			return;
		}
		int playedCount = 0;
		for (int i = 0; i < playCount; i++)
		{
			List<CardModel> list = topCards.Where(delegate(CardModel c)
			{
				CardPile? pile = c.Pile;
				return pile != null && pile.Type == PileType.Draw;
			}).ToList();
			if (list.Count == 0)
			{
				break;
			}
			CardModel chosen = await WatcherCombatHelper.ChooseOne(base.Owner, list, new LocString("card_selection", "TO_PLAY"), cancelable: true);
			if (chosen == null)
			{
				break;
			}
			await CardPileCmd.Add(chosen, PileType.Play);
			await CardCmd.AutoPlay(choiceContext, chosen, null);
			playedCount++;
			if (base.Owner.Creature.IsDead)
			{
				break;
			}
		}
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this,
			CardsDiscarded = playedCount
		});
	}
}
