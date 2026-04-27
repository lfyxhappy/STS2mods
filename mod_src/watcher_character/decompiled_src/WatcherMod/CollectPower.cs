using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class CollectPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == base.Owner.Player && base.Owner.CombatState != null)
		{
			Flash();
			Miracle card = base.Owner.CombatState.CreateCard<Miracle>(player);
			CardCmd.Upgrade(card);
			await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, addedByPlayer: true);
			await PowerCmd.ModifyAmount(this, -1m, null, null, silent: true);
			if (base.Amount <= 0)
			{
				await PowerCmd.Remove(this);
			}
		}
	}
}
