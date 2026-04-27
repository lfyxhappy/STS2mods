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

public sealed class Verdict : WatcherCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new DamageVar(12m, ValueProp.Move),
		new CardsVar("MagicNumber", 5),
		new DynamicVar("DamagePerStack", 3m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<KnowFatePower>());

	public Verdict()
		: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		int stacks = base.Owner.Creature.GetPowerAmount<KnowFatePower>();
		int intValue = base.DynamicVars["MagicNumber"].IntValue;
		int consumed = Math.Min(stacks, intValue);
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue + (decimal)consumed * base.DynamicVars["DamagePerStack"].BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
		if (consumed <= 0)
		{
			return;
		}
		KnowFatePower power = base.Owner.Creature.GetPower<KnowFatePower>();
		if (power != null)
		{
			if (consumed >= stacks)
			{
				await PowerCmd.Remove(power);
			}
			else
			{
				await PowerCmd.ModifyAmount(power, -consumed, base.Owner.Creature, this);
			}
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(4m);
		base.DynamicVars["MagicNumber"].UpgradeValueBy(2m);
	}
}
