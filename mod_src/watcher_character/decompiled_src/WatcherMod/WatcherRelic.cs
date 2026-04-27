using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public abstract class WatcherRelic(string assetName) : RelicModel()
{
	public override string PackedIconPath => "res://images/relics/" + assetName + ".png";

	protected override string PackedIconOutlinePath => "res://images/relics/outline/" + assetName + ".png";

	protected override string BigIconPath => PackedIconPath;
}
