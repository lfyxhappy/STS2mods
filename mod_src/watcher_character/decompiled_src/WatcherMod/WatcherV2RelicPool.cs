using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class WatcherV2RelicPool : RelicPoolModel
{
	public override string EnergyColorName => "watcher";

	public override Color LabOutlineColor => new Color("9E68FF");

	protected override IEnumerable<RelicModel> GenerateAllRelics()
	{
		return ModelDb.RelicPool<WatcherRelicPool>().AllRelics.Concat(new RelicModel[4]
		{
			ModelDb.Relic<ProphetWater>(),
			ModelDb.Relic<ThreeEyes>(),
			ModelDb.Relic<Doctrine>(),
			ModelDb.Relic<Lightbulb>()
		});
	}
}
