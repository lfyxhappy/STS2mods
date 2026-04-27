using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace WatcherMod;

public sealed class SwivelV2 : Swivel, IProphecyCard
{
	public override string PortraitPath => "res://images/packed/card_portraits/watcher/swivel.png";

	public override string BetaPortraitPath => "res://images/packed/card_portraits/watcher/beta/swivel.png";

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await base.OnPlay(choiceContext, cardPlay);
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this
		});
	}
}
