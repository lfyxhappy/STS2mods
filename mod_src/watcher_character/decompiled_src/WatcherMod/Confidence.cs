using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class Confidence : WatcherCard, IProphecyCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new DamageVar(3m, ValueProp.Move),
		new CardsVar("MagicNumber", 5),
		new DynamicVar("DamagePerDiscard", 3m)
	});

	public Confidence()
		: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(base.CombatState, "CombatState");
		int beforeCount = base.Owner.PlayerCombatState.DiscardPile.Cards.Count;
		await WatcherCombatHelper.Scry(choiceContext, base.Owner, base.DynamicVars["MagicNumber"].IntValue, this);
		int num = base.Owner.PlayerCombatState.DiscardPile.Cards.Count - beforeCount;
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue + base.DynamicVars["DamagePerDiscard"].BaseValue * (decimal)num).FromCard(this).TargetingAllOpponents(base.CombatState)
			.WithHitFx("vfx/vfx_attack_blunt")
			.SpawningHitVfxOnEachCreature()
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(2m);
		base.DynamicVars["DamagePerDiscard"].UpgradeValueBy(1m);
	}
}
