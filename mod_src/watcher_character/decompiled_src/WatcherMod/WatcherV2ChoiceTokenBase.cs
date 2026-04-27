using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace WatcherMod;

public abstract class WatcherV2ChoiceTokenBase : WatcherCard
{
	protected override bool IsPlayable => false;

	public override bool CanBeGeneratedInCombat => false;

	public override string PortraitPath => "res://images/packed/card_portraits/watcher/_placeholder.png";

	public override string BetaPortraitPath => "res://images/packed/card_portraits/watcher/_placeholder.png";

	protected WatcherV2ChoiceTokenBase()
		: base(0, CardType.Skill, CardRarity.Token, TargetType.Self, shouldShowInCardLibrary: false)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return Task.CompletedTask;
	}
}
