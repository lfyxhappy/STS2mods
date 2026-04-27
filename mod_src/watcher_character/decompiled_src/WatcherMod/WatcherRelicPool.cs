using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class WatcherRelicPool : RelicPoolModel
{
	public override string EnergyColorName => "watcher";

	public override Color LabOutlineColor => new Color("9E68FF");

	protected override IEnumerable<RelicModel> GenerateAllRelics()
	{
		return new global::_003C_003Ez__ReadOnlyArray<RelicModel>(new RelicModel[9]
		{
			ModelDb.Relic<PureWater>(),
			ModelDb.Relic<Damaru>(),
			ModelDb.Relic<Yang>(),
			ModelDb.Relic<GoldenEye>(),
			ModelDb.Relic<Melange>(),
			ModelDb.Relic<TeardropLocket>(),
			ModelDb.Relic<VioletLotus>(),
			ModelDb.Relic<CloakClasp_P>(),
			ModelDb.Relic<CeramicFish_P>()
		});
	}
}
