using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace WatcherMod;

public sealed class PrescienceShacklesPower : TemporaryStrengthPower
{
	public override AbstractModel OriginModel => ModelDb.Card<Prescience>();

	protected override bool IsPositive => false;
}
