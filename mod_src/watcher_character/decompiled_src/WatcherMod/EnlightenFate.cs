using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace WatcherMod;

public sealed class EnlightenFate : WatcherCard, IProphecyCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new PowerVar<Mantra>(1m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[3]
	{
		HoverTipFactory.FromPower<EnlightenFatePower>(),
		HoverTipFactory.FromPower<KnowFatePower>(),
		HoverTipFactory.FromPower<Mantra>()
	});

	public EnlightenFate()
		: base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		EnlightenFatePower enlightenFatePower = await PowerCmd.Apply<EnlightenFatePower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
		if (enlightenFatePower != null)
		{
			enlightenFatePower.MantraYield = base.DynamicVars[typeof(Mantra).Name].IntValue;
		}
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this
		});
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars[typeof(Mantra).Name].UpgradeValueBy(1m);
	}
}
