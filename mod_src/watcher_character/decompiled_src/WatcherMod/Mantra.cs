using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class Mantra : PowerModel
{
	private bool _isResolving;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (_isResolving || power != this || base.Owner.Player == null || base.Amount < 10)
		{
			return;
		}
		_isResolving = true;
		try
		{
			WatcherAudioHelper.PlayOneShot("res://audio/watcher/mantra.ogg");
			await PowerCmd.ModifyAmount(this, -10m, applier, cardSource, silent: true);
			await WatcherCombatHelper.EnterDivinity(base.Owner.Player, cardSource);
		}
		finally
		{
			_isResolving = false;
		}
	}
}
