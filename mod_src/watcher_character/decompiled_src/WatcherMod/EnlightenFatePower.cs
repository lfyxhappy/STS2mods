using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class EnlightenFatePower : PowerModel
{
	private int _peakThisTurn;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public int MantraYield { get; set; } = 3;

	public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != base.Owner.Player)
		{
			return Task.CompletedTask;
		}
		_peakThisTurn = base.Owner.GetPowerAmount<KnowFatePower>();
		return Task.CompletedTask;
	}

	public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != base.Owner.Player)
		{
			return Task.CompletedTask;
		}
		int powerAmount = base.Owner.GetPowerAmount<KnowFatePower>();
		if (powerAmount > _peakThisTurn)
		{
			_peakThisTurn = powerAmount;
		}
		return Task.CompletedTask;
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == base.Owner.Side)
		{
			int current = base.Owner.GetPowerAmount<KnowFatePower>();
			if (current < _peakThisTurn)
			{
				Flash();
				await WatcherCombatHelper.GainMantra(base.Owner.Player, MantraYield, null);
			}
			_peakThisTurn = current;
		}
	}
}
