using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class HourglassPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public int ScryAmount { get; set; } = 2;

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != base.Owner.Player)
		{
			return;
		}
		PlayerCombatState playerCombatState = player.PlayerCombatState;
		if (playerCombatState == null)
		{
			return;
		}
		Flash();
		int effectiveScryAmount = WatcherCombatHelper.GetEffectiveScryAmount(player, ScryAmount);
		List<CardModel> source = playerCombatState.DrawPile.Cards.Take(effectiveScryAmount).ToList();
		List<CardModel> attacksInPeek = source.Where((CardModel c) => c.Type == CardType.Attack).ToList();
		await WatcherCombatHelper.Scry(choiceContext, player, ScryAmount);
		if (attacksInPeek.Count > 0)
		{
			await CardPileCmd.Draw(choiceContext, 1m, player);
			List<CardModel> list = PileType.Hand.GetPile(player).Cards.Where((CardModel c) => c.Type == CardType.Attack && c.EnergyCost.GetResolved() > 0).ToList();
			if (list.Count > 0)
			{
				player.RunState.Rng.CombatCardSelection.NextItem(list)?.EnergyCost.AddThisTurn(-1, reduceOnly: true);
			}
		}
	}
}
