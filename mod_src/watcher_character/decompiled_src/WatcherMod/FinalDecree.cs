using System;
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

public sealed class FinalDecree : WatcherCard, IProphecyCard
{
	private const int _vulnThreshold = 5;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new DamageVar(10m, ValueProp.Move),
		new DynamicVar("DamagePerStack", 2m),
		new PowerVar<VulnerablePower>(3m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromPower<KnowFatePower>(),
		HoverTipFactory.FromPower<VulnerablePower>()
	});

	public FinalDecree()
		: base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		int stacks = base.Owner.Creature.GetPowerAmount<KnowFatePower>();
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue + (decimal)stacks * base.DynamicVars["DamagePerStack"].BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
		if (stacks > 0)
		{
			KnowFatePower power = base.Owner.Creature.GetPower<KnowFatePower>();
			if (power != null)
			{
				await PowerCmd.Remove(power);
			}
		}
		if (stacks >= 5 && cardPlay.Target.IsAlive)
		{
			await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, base.DynamicVars[typeof(VulnerablePower).Name].BaseValue, base.Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(4m);
		base.DynamicVars["DamagePerStack"].UpgradeValueBy(1m);
		base.DynamicVars[typeof(VulnerablePower).Name].UpgradeValueBy(1m);
	}
}
