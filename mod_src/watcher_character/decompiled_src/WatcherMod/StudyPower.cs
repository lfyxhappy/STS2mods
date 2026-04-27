using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class StudyPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == base.Owner.Side && base.Owner.Player != null)
		{
			for (int i = 0; i < base.Amount; i++)
			{
				await CardPileCmd.AddGeneratedCardToCombat(await WatcherCombatHelper.CreateWatcherCard<Insight>(base.Owner.Player), PileType.Draw, addedByPlayer: true, CardPilePosition.Random);
			}
		}
	}
}
