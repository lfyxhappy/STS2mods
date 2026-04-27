using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace WatcherMod;

public sealed class ThirdEyeV2 : ThirdEye, IProphecyCard
{
	public override string PortraitPath => "res://images/packed/card_portraits/watcher/third_eye.png";

	public override string BetaPortraitPath => "res://images/packed/card_portraits/watcher/beta/third_eye.png";

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await base.OnPlay(choiceContext, cardPlay);
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this
		});
	}
}
