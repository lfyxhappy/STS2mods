using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace WatcherMod;

public sealed class ThreeEyes : WatcherRelic, IWatcherProphecyListener
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Mantra", 1m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<Mantra>());

	public ThreeEyes()
		: base("three_eyes")
	{
	}

	public async Task OnProphecy(Player owner, ProphecyContext ctx)
	{
		if (owner == base.Owner)
		{
			Flash();
			await WatcherCombatHelper.GainMantra(owner, base.DynamicVars["Mantra"].IntValue, ctx.Source);
		}
	}
}
