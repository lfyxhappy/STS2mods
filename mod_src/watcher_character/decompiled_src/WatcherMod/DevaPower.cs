using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class DevaPower : PowerModel
{
	private int _energyGainAmount = 1;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new EnergyVar("GainEnergy", 1));

	public override async Task AfterEnergyReset(Player player)
	{
		if (player == base.Owner.Player)
		{
			await PlayerCmd.GainEnergy(_energyGainAmount, player);
			_energyGainAmount += base.Amount;
		}
	}
}
