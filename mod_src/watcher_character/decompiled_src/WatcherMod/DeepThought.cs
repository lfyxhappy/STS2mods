using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class DeepThought : WatcherCard, IProphecyCard
{
	public override bool GainsBlock => true;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new BlockVar(30m, ValueProp.Move),
		new PowerVar<VulnerablePower>(2m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[4]
	{
		HoverTipFactory.FromPower<DeepThoughtSleepPower>(),
		HoverTipFactory.FromPower<Wrath>(),
		HoverTipFactory.FromPower<VulnerablePower>(),
		WatcherHoverTips.Stance
	});

	public DeepThought()
		: base(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int intValue = base.DynamicVars.Block.IntValue;
		await CreatureCmd.GainBlock(base.Owner.Creature, intValue, ValueProp.Move, cardPlay);
		DeepThoughtSleepPower deepThoughtSleepPower = await PowerCmd.Apply<DeepThoughtSleepPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
		if (deepThoughtSleepPower != null)
		{
			deepThoughtSleepPower.VulnerableAmount = base.DynamicVars[typeof(VulnerablePower).Name].IntValue;
		}
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this
		});
		PlayerCmd.EndTurn(base.Owner, canBackOut: false);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(10m);
		base.DynamicVars[typeof(VulnerablePower).Name].UpgradeValueBy(1m);
		AddKeyword(CardKeyword.Retain);
	}
}
