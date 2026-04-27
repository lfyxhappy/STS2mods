using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace WatcherMod;

public sealed class Yang : WatcherRelic
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public Yang()
		: base("duality")
	{
	}

	public override async Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == base.Owner && cardPlay.Card.Type == CardType.Attack)
		{
			Flash();
			await PowerCmd.Apply<YangDexterityPower>(base.Owner.Creature, 1m, base.Owner.Creature, cardPlay.Card);
		}
	}
}
