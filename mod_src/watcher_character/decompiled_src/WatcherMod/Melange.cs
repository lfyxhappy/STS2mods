using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace WatcherMod;

public sealed class Melange : WatcherRelic
{
	public override RelicRarity Rarity => RelicRarity.Shop;

	public Melange()
		: base("melange")
	{
	}

	public override async Task AfterShuffle(PlayerChoiceContext choiceContext, Player shuffler)
	{
		if (shuffler == base.Owner)
		{
			Flash();
			await WatcherCombatHelper.Scry(new BlockingPlayerChoiceContext(), base.Owner, 3);
		}
	}
}
