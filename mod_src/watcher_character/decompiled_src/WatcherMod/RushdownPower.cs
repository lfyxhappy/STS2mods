using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class RushdownPower : PowerModel
{
	internal int PendingDraws;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
	{
		if (PendingDraws > 0 && oldPileType == PileType.Play && card.Owner == base.Owner.Player)
		{
			int pendingDraws = PendingDraws;
			PendingDraws = 0;
			await CardPileCmd.Draw(new BlockingPlayerChoiceContext(), pendingDraws, base.Owner.Player);
		}
	}
}
