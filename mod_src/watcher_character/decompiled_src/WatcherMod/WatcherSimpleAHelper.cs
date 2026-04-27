using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

internal static class WatcherSimpleAHelper
{
	public static async Task Scry(PlayerChoiceContext choiceContext, Player owner, int amount)
	{
		List<CardModel> list = PileType.Draw.GetPile(owner).Cards.Take(amount).ToList();
		if (list.Count == 0)
		{
			return;
		}
		CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 0, list.Count);
		foreach (CardModel item in (await CardSelectCmd.FromSimpleGrid(choiceContext, list, owner, prefs)).ToList())
		{
			await CardPileCmd.Add(item, PileType.Discard);
		}
	}

	public static CardType? GetPreviousPlayedCardType(CardModel currentCard)
	{
		if (currentCard.CombatState == null)
		{
			return null;
		}
		return CombatManager.Instance.History.CardPlaysStarted.LastOrDefault((CardPlayStartedEntry entry) => entry.CardPlay.Card.Owner == currentCard.Owner && entry.CardPlay.Card != currentCard)?.CardPlay.Card.Type;
	}

	public static bool IsTargetAttacking(Creature? target)
	{
		return target?.Monster?.IntendsToAttack == true;
	}
}
