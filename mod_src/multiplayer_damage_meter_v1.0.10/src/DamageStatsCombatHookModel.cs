using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace MultiplayerDamageMeter;

public class DamageStatsCombatHookModel : AbstractModel
{
	public override bool ShouldReceiveCombatHooks => true;

	public static DamageStatsCombatHookModel GetCanonical()
	{
		return ModelDb.GetById<DamageStatsCombatHookModel>(ModelDb.GetId<DamageStatsCombatHookModel>());
	}

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
	{
		DamageStatsService.CapturePendingPoisonTicks(side, combatState);
		return Task.CompletedTask;
	}

	public override Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		DamageStatsService.FinalizePendingPoisonTicks(side);
		return Task.CompletedTask;
	}

	public override Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		DamageStatsService.RegisterPowerAmountChanged(power, amount, applier, cardSource);
		return Task.CompletedTask;
	}

	public override Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		DamageStatsService.RegisterDamage(dealer, cardSource, result, props, target);
		return Task.CompletedTask;
	}

	public override Task AfterDiedToDoom(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> creatures)
	{
		DamageStatsService.RegisterDoomKill(creatures);
		return Task.CompletedTask;
	}

	public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
	{
		if (!wasRemovalPrevented)
		{
			DamageStatsService.ClearTargetLedgers(creature);
		}

		return Task.CompletedTask;
	}
}
