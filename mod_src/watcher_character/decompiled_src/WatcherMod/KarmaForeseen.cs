using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class KarmaForeseen : WatcherCard, IProphecyCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new CardsVar("MagicNumber", 3),
		new DamageVar(4m, ValueProp.Unpowered)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromPower<KarmaForeseenPower>(),
		HoverTipFactory.FromPower<KnowFatePower>()
	});

	public KarmaForeseen()
		: base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		KarmaForeseenPower karmaForeseenPower = await PowerCmd.Apply<KarmaForeseenPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
		if (karmaForeseenPower != null)
		{
			karmaForeseenPower.DynamicVars["Threshold"].BaseValue = base.DynamicVars["MagicNumber"].BaseValue;
			karmaForeseenPower.DynamicVars["DamageBonus"].BaseValue = base.DynamicVars.Damage.BaseValue;
		}
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this
		});
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(-1m);
	}
}
