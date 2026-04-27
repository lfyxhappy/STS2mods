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

public sealed class GospelPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromPower<Mantra>(),
		HoverTipFactory.FromPower<DoomPower>()
	});

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		if (dealer != base.Owner || !props.HasFlag(ValueProp.Move) || props.HasFlag(ValueProp.Unpowered) || result.UnblockedDamage <= 0)
		{
			return;
		}
		Player player = base.Owner.Player;
		await WatcherCombatHelper.GainMantra(player, base.Amount, null);
		if ((base.Owner.CombatState?.Players.Count ?? 1) > 1)
		{
			int mantraGainedThisTurn = WatcherCombatHelper.GetMantraGainedThisTurn(player);
			if (mantraGainedThisTurn > 0 && base.Owner.CombatState != null)
			{
				await PowerCmd.Apply<DoomPower>(base.Owner.CombatState.HittableEnemies, mantraGainedThisTurn, base.Owner, null);
			}
		}
	}
}
