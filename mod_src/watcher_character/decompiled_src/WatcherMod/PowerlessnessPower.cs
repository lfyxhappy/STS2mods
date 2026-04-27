using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class PowerlessnessPower : PowerModel, IWatcherProphecyListener
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public async Task OnProphecy(Player owner, ProphecyContext ctx)
	{
		if (owner == base.Owner.Player && ctx.CardsDiscarded > 0)
		{
			Flash();
			await CreatureCmd.GainBlock(owner.Creature, base.Amount * ctx.CardsDiscarded, ValueProp.Unpowered, null);
		}
	}
}
