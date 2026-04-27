using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace WatcherMod;

public sealed class JustLuckyV2 : JustLucky, IProphecyCard
{
	public override string PortraitPath => "res://images/packed/card_portraits/watcher/just_lucky.png";

	public override string BetaPortraitPath => "res://images/packed/card_portraits/watcher/beta/just_lucky.png";

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await base.OnPlay(choiceContext, cardPlay);
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this
		});
	}
}
