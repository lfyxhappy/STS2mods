using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class FollowHeaven : WatcherCard
{
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new DamageVar(8m, ValueProp.Move),
		new BlockVar(5m, ValueProp.Move),
		new CardsVar("MagicNumber", 4)
	});

	public FollowHeaven()
		: base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		Creature target = cardPlay.Target;
		bool intendsAttack = target.Monster?.NextMove?.Intents.OfType<AttackIntent>().Any() == true;
		decimal baseValue = base.DynamicVars.Damage.BaseValue;
		if (intendsAttack)
		{
			baseValue += base.DynamicVars["MagicNumber"].BaseValue;
		}
		await DamageCmd.Attack(baseValue).FromCard(this).Targeting(target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
		if (intendsAttack)
		{
			await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(3m);
		base.DynamicVars["MagicNumber"].UpgradeValueBy(2m);
		base.DynamicVars.Block.UpgradeValueBy(2m);
	}
}
