using MegaCrit.Sts2.Core.Entities.Relics;

namespace WatcherMod;

public sealed class VioletLotus : WatcherRelic
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	public VioletLotus()
		: base("violet_lotus")
	{
	}
}
