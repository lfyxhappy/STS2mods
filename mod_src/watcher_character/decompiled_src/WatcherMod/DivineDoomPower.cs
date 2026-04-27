using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class DivineDoomPower : PowerModel
{
	private bool _usedThisTurn;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<DoomPower>());

	public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == base.Owner.Player)
		{
			_usedThisTurn = false;
		}
		return Task.CompletedTask;
	}

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		if (dealer == base.Owner && props.HasFlag(ValueProp.Move) && !props.HasFlag(ValueProp.Unpowered) && result.UnblockedDamage > 0 && !_usedThisTurn && base.Owner.HasPower<Divinity>())
		{
			_usedThisTurn = true;
			int num = (int)Math.Ceiling((decimal)target.MaxHp * 0.05m);
			if (num > 0 && base.Owner.CombatState != null)
			{
				await PowerCmd.Apply<DoomPower>(base.Owner.CombatState.HittableEnemies, num, base.Owner, null);
			}
		}
	}
}
