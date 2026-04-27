using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class GlimpseFuture : WatcherCard, IProphecyCard
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<Mantra>());

	public GlimpseFuture()
		: base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		PlayerCombatState playerCombatState = base.Owner.PlayerCombatState;
		if (playerCombatState == null)
		{
			return;
		}
		List<CardModel> prophecyCards = playerCombatState.Hand.Cards.Where((CardModel c) => c is IProphecyCard && c != this).ToList();
		if (prophecyCards.Count != 0)
		{
			GlimpseFuturePower glimpseFuturePower = await PowerCmd.Apply<GlimpseFuturePower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
			if (glimpseFuturePower != null)
			{
				glimpseFuturePower.MantraBonus = (base.IsUpgraded ? 1 : 0);
				glimpseFuturePower.AssignLabels(prophecyCards);
			}
		}
	}

	protected override void OnUpgrade()
	{
	}
}
