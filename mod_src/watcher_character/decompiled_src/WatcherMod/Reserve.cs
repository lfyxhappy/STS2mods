using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class Reserve : WatcherCard, IProphecyCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new CardsVar("MagicNumber", 5),
		new CardsVar("DrawNext", 3),
		new PowerVar<Mantra>(3m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromPower<ReserveNextTurnPower>(),
		HoverTipFactory.FromPower<Mantra>()
	});

	public Reserve()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int intValue = base.DynamicVars["MagicNumber"].IntValue;
		await WatcherCombatHelper.ScryAutoDiscard(choiceContext, base.Owner, intValue, (IReadOnlyList<CardModel> peek) => peek.Where((CardModel c) => c.Type == CardType.Skill || c.Type == CardType.Attack), this);
		ReserveNextTurnPower reserveNextTurnPower = await PowerCmd.Apply<ReserveNextTurnPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
		if (reserveNextTurnPower != null)
		{
			reserveNextTurnPower.DrawAmount = base.DynamicVars["DrawNext"].IntValue;
			reserveNextTurnPower.MantraAmount = base.DynamicVars[typeof(Mantra).Name].IntValue;
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["DrawNext"].UpgradeValueBy(1m);
		base.DynamicVars[typeof(Mantra).Name].UpgradeValueBy(1m);
	}
}
