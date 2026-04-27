using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.HoverTips;

namespace WatcherMod;

public sealed class WishV2 : Wish_P
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<KnowFatePower>());

	protected override async Task<bool> TryConsumeKnowFateBoost()
	{
		if (base.Owner.Creature.GetPowerAmount<KnowFatePower>() <= 12)
		{
			return false;
		}
		KnowFatePower power = base.Owner.Creature.GetPower<KnowFatePower>();
		if (power != null)
		{
			await PowerCmd.Remove(power);
		}
		return true;
	}
}
