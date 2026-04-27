using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class EstablishmentPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override Task AfterCardRetained(CardModel card)
	{
		if (card.Owner == base.Owner.Player)
		{
			card.EnergyCost.AddThisCombat(-base.Amount, reduceOnly: true);
		}
		return Task.CompletedTask;
	}

	public override Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side != base.Owner.Side || base.Owner.CombatState == null)
		{
			return Task.CompletedTask;
		}
		Player player = base.Owner.Player;
		if (player == null)
		{
			return Task.CompletedTask;
		}
		if (Hook.ShouldFlush(base.Owner.CombatState, player))
		{
			return Task.CompletedTask;
		}
		foreach (CardModel card in PileType.Hand.GetPile(player).Cards)
		{
			if (!card.ShouldRetainThisTurn)
			{
				card.EnergyCost.AddThisCombat(-base.Amount, reduceOnly: true);
			}
		}
		return Task.CompletedTask;
	}
}
