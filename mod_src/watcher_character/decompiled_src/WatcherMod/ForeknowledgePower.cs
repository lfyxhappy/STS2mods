using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class ForeknowledgePower : PowerModel, IWatcherProphecyListener
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public async Task OnProphecy(Player owner, ProphecyContext ctx)
	{
		if (owner == base.Owner.Player)
		{
			Flash();
			await CardPileCmd.Draw(new BlockingPlayerChoiceContext(), base.Amount, owner);
		}
	}
}
