using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class DeepThoughtSleepPower : PowerModel
{
	private bool _blockDepleted;

	private bool _triggered;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public int VulnerableAmount { get; set; } = 2;

	public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (_triggered)
		{
			return Task.CompletedTask;
		}
		if (target != base.Owner)
		{
			return Task.CompletedTask;
		}
		if (base.Owner.Block <= 0)
		{
			_blockDepleted = true;
		}
		return Task.CompletedTask;
	}

	public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		if (_triggered || side != CombatSide.Player || base.Owner == null || base.Owner.IsDead || base.Owner.Player == null)
		{
			return;
		}
		_triggered = true;
		Flash();
		if (!_blockDepleted)
		{
			await WatcherCombatHelper.EnterCalm(base.Owner.Player, null);
		}
		else
		{
			await WatcherCombatHelper.EnterWrath(base.Owner.Player, null);
			List<Creature> list = combatState?.Enemies.ToList() ?? new List<Creature>();
			foreach (Creature item in list)
			{
				await PowerCmd.Apply<VulnerablePower>(item, VulnerableAmount, base.Owner, null);
			}
		}
		await PowerCmd.Remove(this);
	}
}
