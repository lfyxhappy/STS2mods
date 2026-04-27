using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace WatcherMod;

public sealed class TeardropLocket : WatcherRelic
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public TeardropLocket()
		: base("tear_drop_locket")
	{
	}

	public override async Task BeforeCombatStart()
	{
		Flash();
		await WatcherCombatHelper.EnterCalm(base.Owner, null);
	}
}
