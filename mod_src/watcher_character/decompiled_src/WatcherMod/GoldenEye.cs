using MegaCrit.Sts2.Core.Entities.Relics;

namespace WatcherMod;

public sealed class GoldenEye : WatcherRelic
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	public GoldenEye()
		: base("golden_eye")
	{
	}
}
