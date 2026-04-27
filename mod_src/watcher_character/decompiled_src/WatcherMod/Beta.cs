using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace WatcherMod;

public sealed class Beta : WatcherCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	public Beta()
		: base(2, CardType.Skill, CardRarity.Token, TargetType.None, shouldShowInCardLibrary: false)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CardPileCmd.AddGeneratedCardToCombat(base.CombatState.CreateCard<Omega>(base.Owner), PileType.Draw, addedByPlayer: true, CardPilePosition.Random);
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}
}
