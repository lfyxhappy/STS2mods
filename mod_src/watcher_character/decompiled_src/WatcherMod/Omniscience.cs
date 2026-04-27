using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class Omniscience : WatcherCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	public Omniscience()
		: base(4, CardType.Skill, CardRarity.Rare, TargetType.None)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		List<CardModel> list = PileType.Draw.GetPile(base.Owner).Cards.ToList();
		if (list.Count == 0)
		{
			return;
		}
		CardSelectorPrefs prefs = WatcherCombatHelper.SetCancelable(new CardSelectorPrefs(new LocString("card_selection", "CHOOSE_A_CARD"), 1), value: false);
		CardModel chosenCard = (await CardSelectCmd.FromSimpleGrid(choiceContext, list, base.Owner, prefs)).FirstOrDefault();
		if (chosenCard == null)
		{
			return;
		}
		await PowerCmd.Apply<OmniscienceDoublePower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
		await CardCmd.AutoPlay(choiceContext, chosenCard, null);
		if (chosenCard.CombatState != null)
		{
			CardPile? pile = chosenCard.Pile;
			if (pile == null || pile.Type != PileType.Exhaust)
			{
				await CardCmd.Exhaust(choiceContext, chosenCard);
			}
		}
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}
}
