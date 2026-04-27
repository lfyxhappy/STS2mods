using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class Wrath : PowerModel
{
	private Node2D? _vfx;

	private Node? _borderVfx;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override bool IsVisibleInternal => false;

	public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		_vfx = StanceVfxHelper.SpawnOnCreature(base.Owner, StanceVfxHelper.CreateWrathVfx());
		_borderVfx = StanceVfxHelper.SpawnWrathBorder();
		WatcherAudioHelper.PlayOneShot("res://audio/watcher/wrath.ogg");
		await Task.CompletedTask;
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
			return 2m;
		}
		if (target == base.Owner && flag)
		{
			return 2m;
		}
		return 1m;
	}
}
