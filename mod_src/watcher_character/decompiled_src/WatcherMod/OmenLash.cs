using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class OmenLash : WatcherCard, IProphecyCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new DamageVar(7m, ValueProp.Move),
		new PowerVar<VulnerablePower>(1m),
		new CardsVar("MagicNumber", 1)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<VulnerablePower>());

	public OmenLash()
		: base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
		if (cardPlay.Target.IsAlive)
		{
			int intValue = base.DynamicVars[typeof(VulnerablePower).Name].IntValue;
			MonsterModel? monster = cardPlay.Target.Monster;
			if (monster == null || monster.NextMove?.Intents.OfType<AttackIntent>().Any() != true)
			{
				await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, base.DynamicVars[typeof(VulnerablePower).Name].BaseValue, base.Owner.Creature, this);
			}
			else
			{
				await WatcherProphecy.ApplyConfusion(cardPlay.Target, base.Owner.Creature, this, intValue);
			}
		}
		int intValue2 = base.DynamicVars["MagicNumber"].IntValue;
		await WatcherCombatHelper.Scry(choiceContext, base.Owner, intValue2, this);
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this,
			AffectedEnemy = cardPlay.Target
		});
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(2m);
		base.DynamicVars[typeof(VulnerablePower).Name].UpgradeValueBy(1m);
		base.DynamicVars["MagicNumber"].UpgradeValueBy(1m);
	}
}
