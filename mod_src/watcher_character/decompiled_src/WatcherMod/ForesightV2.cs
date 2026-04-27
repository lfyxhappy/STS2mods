using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace WatcherMod;

public sealed class ForesightV2 : Foresight, IProphecyCard
{
	public override string PortraitPath => "res://images/packed/card_portraits/watcher/foresight.png";

	public override string BetaPortraitPath => "res://images/packed/card_portraits/watcher/beta/foresight.png";

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await base.OnPlay(choiceContext, cardPlay);
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this
		});
	}
}
