using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class CeramicFish_P : WatcherRelic
{
	public override RelicRarity Rarity => RelicRarity.Common;

	public CeramicFish_P()
		: base("ceramic_fish")
	{
	}

	public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
	{
		if (card.Owner == base.Owner)
		{
			CardPile? pile = card.Pile;
			if (pile != null && pile.Type == PileType.Deck && oldPileType != PileType.Deck)
			{
				base.Owner.Gold += 9;
			}
		}
		return Task.CompletedTask;
	}
}
