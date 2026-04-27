using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace WatcherMod;

public sealed class BestowFate : WatcherCard, IProphecyCard
{
	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new PowerVar<VigorPower>(1m),
		new CardsVar("MagicNumber", 1)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromPower<KnowFatePower>(),
		HoverTipFactory.FromPower<VigorPower>()
	});

	public BestowFate()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.AnyAlly)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		int powerAmount = base.Owner.Creature.GetPowerAmount<KnowFatePower>();
		if (powerAmount > 0)
		{
			int intValue = base.DynamicVars["MagicNumber"].IntValue;
			int num = powerAmount * intValue;
			await PowerCmd.Apply<VigorPower>(cardPlay.Target, num, base.Owner.Creature, this);
			KnowFatePower power = base.Owner.Creature.GetPower<KnowFatePower>();
			if (power != null)
			{
				await PowerCmd.Remove(power);
			}
		}
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this
		});
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(1m);
	}
}
