using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class PlaceStone : WatcherCard, IProphecyCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new DamageVar(6m, ValueProp.Move),
		new DynamicVar("BonusDamage", 5m),
		new PowerVar<KnowFatePower>(2m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<KnowFatePower>());

	public PlaceStone()
		: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		int priorScryPlays = WatcherCombatHelper.GetScryPlaysThisTurn(base.Owner);
		decimal baseValue = base.DynamicVars.Damage.BaseValue;
		if (priorScryPlays > 0)
		{
			baseValue += base.DynamicVars["BonusDamage"].BaseValue;
		}
		await DamageCmd.Attack(baseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
		if (priorScryPlays > 0)
		{
			await PowerCmd.Apply<KnowFatePower>(base.Owner.Creature, base.DynamicVars[typeof(KnowFatePower).Name].BaseValue, base.Owner.Creature, this);
		}
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this,
			AffectedEnemy = cardPlay.Target
		});
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(2m);
		base.DynamicVars["BonusDamage"].UpgradeValueBy(2m);
		base.DynamicVars[typeof(KnowFatePower).Name].UpgradeValueBy(1m);
	}
}
