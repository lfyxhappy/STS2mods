using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class OmegaPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == base.Owner.Side && base.Owner.CombatState != null)
		{
			Flash();
			await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), base.Owner.CombatState.HittableEnemies, base.Amount, ValueProp.Unpowered, base.Owner, null);
		}
	}
}
