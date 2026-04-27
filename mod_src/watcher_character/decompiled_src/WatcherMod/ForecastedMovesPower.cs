using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace WatcherMod;

public sealed class ForecastedMovesPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override bool IsVisibleInternal => false;

	public Queue<MoveState?> Queue { get; } = new Queue<MoveState>();

	public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		if (side != CombatSide.Player || base.Owner == null || base.Owner.IsDead)
		{
			return;
		}
		if (Queue.Count == 0)
		{
			await PowerCmd.Remove(this);
			return;
		}
		MoveState moveState = Queue.Dequeue();
		try
		{
			if (moveState == null)
			{
				WatcherProphecy.ForceStunEnemy(base.Owner);
			}
			else
			{
				base.Owner.Monster?.SetMoveImmediate(moveState, forceTransition: true);
				WatcherProphecy.RefreshIntents(base.Owner);
			}
		}
		catch
		{
		}
		if (Queue.Count == 0)
		{
			await PowerCmd.Remove(this);
		}
	}
}
