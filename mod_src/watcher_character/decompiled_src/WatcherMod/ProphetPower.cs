using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class ProphetPower : PowerModel, IWatcherProphecyListener
{
	private const int MantraPerInsight = 2;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<Insight>());

	public async Task OnProphecy(Player owner, ProphecyContext ctx)
	{
		if (owner == base.Owner.Player)
		{
			Flash();
			await CardPileCmd.AddGeneratedCardToCombat(await WatcherCombatHelper.CreateWatcherCard<Insight>(owner), PileType.Hand, addedByPlayer: true);
		}
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (base.Owner?.Player != null && cardPlay.Card is Insight)
		{
			Flash();
			await WatcherCombatHelper.GainMantra(base.Owner.Player, 2, cardPlay.Card);
		}
	}
}
