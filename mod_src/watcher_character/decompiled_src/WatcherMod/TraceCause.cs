using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class TraceCause : WatcherCard, IProphecyCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new DamageVar(6m, ValueProp.Move),
		new CardsVar("MagicNumber", 3),
		new DynamicVar("DamagePerDiscard", 3m)
	});

	public TraceCause()
		: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		PlayerCombatState combat = base.Owner.PlayerCombatState;
		if (combat != null)
		{
			int beforeCount = combat.DiscardPile.Cards.Count;
			await WatcherCombatHelper.Scry(choiceContext, base.Owner, base.DynamicVars["MagicNumber"].IntValue, this);
			int num = combat.DiscardPile.Cards.Count - beforeCount;
			await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue + (decimal)num * base.DynamicVars["DamagePerDiscard"].BaseValue).FromCard(this).Targeting(cardPlay.Target)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(2m);
		base.DynamicVars["MagicNumber"].UpgradeValueBy(1m);
		base.DynamicVars["DamagePerDiscard"].UpgradeValueBy(1m);
	}
}
