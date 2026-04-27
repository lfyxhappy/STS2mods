using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class WatcherExtraTurnPower : PowerModel
{
	internal static bool SkipPaelsEyeConsumption;

	protected override bool IsVisibleInternal => false;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.None;

	public override bool ShouldTakeExtraTurn(Player player)
	{
		return player == base.Owner.Player;
	}

	public override async Task AfterTakingExtraTurn(Player player)
	{
		if (player == base.Owner.Player)
		{
			SkipPaelsEyeConsumption = true;
			await PowerCmd.Remove(this);
		}
	}
}
