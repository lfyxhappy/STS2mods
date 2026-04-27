using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class Divinity : PowerModel
{
	private Node2D? _vfx;

	private Node? _borderVfx;

	private int _appliedOnRound;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override bool IsVisibleInternal => false;

	public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		_appliedOnRound = base.Owner.CombatState?.RoundNumber ?? 0;
		_vfx = StanceVfxHelper.SpawnOnCreature(base.Owner, StanceVfxHelper.CreateDivinityVfx());
		_borderVfx = StanceVfxHelper.SpawnDivinityBorder();
		WatcherAudioHelper.PlayOneShot("res://audio/watcher/divinity.ogg");
		if (base.Owner.Player != null)
		{
			await PlayerCmd.GainEnergy(3m, base.Owner.Player);
		}
	}

	public override async Task AfterRemoved(Creature oldOwner)
	{
		StanceVfxHelper.Remove(ref _vfx);
		StanceVfxHelper.RemoveBorder(ref _borderVfx);
		await Task.CompletedTask;
	}

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		bool flag = props.HasFlag(ValueProp.Move) && !props.HasFlag(ValueProp.Unpowered);
		if (dealer == base.Owner && flag)
		{
			return 3m;
		}
		return 1m;
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == base.Owner.Player && base.Owner.CombatState?.RoundNumber != _appliedOnRound)
		{
			await WatcherCombatHelper.ExitStance(player);
		}
	}
}
