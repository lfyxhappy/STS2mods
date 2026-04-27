using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class Calm : PowerModel
{
	private Node2D? _vfx;

	private Node? _borderVfx;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override bool IsVisibleInternal => false;

	public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		_vfx = StanceVfxHelper.SpawnOnCreature(base.Owner, StanceVfxHelper.CreateCalmVfx());
		_borderVfx = StanceVfxHelper.SpawnCalmBorder();
		WatcherAudioHelper.PlayOneShot("res://audio/watcher/calm.ogg");
		await Task.CompletedTask;
	}

	public override async Task AfterRemoved(Creature oldOwner)
	{
		StanceVfxHelper.Remove(ref _vfx);
		StanceVfxHelper.RemoveBorder(ref _borderVfx);
		if (CombatManager.Instance.IsInProgress && oldOwner.Player != null)
		{
			await PlayerCmd.GainEnergy(2m, oldOwner.Player);
		}
	}
}
