using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class EnergyDownPower : PowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterEnergyReset(Player player)
	{
		if (player == base.Owner.Player)
		{
			await PlayerCmd.LoseEnergy(base.Amount, player);
		}
	}
}
