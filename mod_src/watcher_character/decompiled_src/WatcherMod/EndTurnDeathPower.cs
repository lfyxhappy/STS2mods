using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class EndTurnDeathPower : PowerModel
{
	private int _appliedOnRound;

	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override bool IsVisibleInternal => true;

	public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		_appliedOnRound = base.Owner.CombatState?.RoundNumber ?? 0;
		await Task.CompletedTask;
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == base.Owner.Player && base.Owner.CombatState?.RoundNumber != _appliedOnRound)
		{
			await CreatureCmd.Damage(choiceContext, base.Owner, 99999m, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
			await PowerCmd.Remove(this);
		}
	}
}
