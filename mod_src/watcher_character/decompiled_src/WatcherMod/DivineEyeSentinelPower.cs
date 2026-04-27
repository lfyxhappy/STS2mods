using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class DivineEyeSentinelPower : PowerModel, IWatcherProphecyListener
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public int BlockPerScry { get; set; } = 4;

	public async Task OnProphecy(Player owner, ProphecyContext ctx)
	{
		if (owner == base.Owner.Player && ctx.FromScry && BlockPerScry > 0)
		{
			Flash();
			await CreatureCmd.GainBlock(base.Owner, BlockPerScry, ValueProp.Unpowered, null);
		}
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == base.Owner.Side)
		{
			await PowerCmd.Remove(this);
		}
	}
}
