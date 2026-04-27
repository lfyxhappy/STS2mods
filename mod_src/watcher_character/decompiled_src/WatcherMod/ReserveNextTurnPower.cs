using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class ReserveNextTurnPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public int DrawAmount { get; set; } = 3;

	public int MantraAmount { get; set; } = 3;

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == base.Owner.Player)
		{
			Flash();
			if (DrawAmount > 0)
			{
				await CardPileCmd.Draw(choiceContext, DrawAmount, player);
			}
			if (MantraAmount > 0)
			{
				await WatcherCombatHelper.GainMantra(player, MantraAmount, null);
			}
			await PowerCmd.Remove(this);
		}
	}
}
