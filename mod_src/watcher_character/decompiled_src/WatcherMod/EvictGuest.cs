using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class EvictGuest : WatcherCard, IProphecyCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Retain);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DamageVar(8m, ValueProp.Move),
		new CardsVar("MagicNumber", 6)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<EvictionPower>());

	public EvictGuest()
		: base(4, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side != base.Owner.Creature.Side)
		{
			return;
		}
		CardPile? pile = base.Pile;
		if (pile != null && pile.Type == PileType.Hand)
		{
			if (base.EnergyCost.GetResolved() > 0)
			{
				base.EnergyCost.AddThisCombat(-1, reduceOnly: true);
			}
			await Task.CompletedTask;
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(choiceContext);
		await PowerCmd.Apply<EvictionPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
		int intValue = base.DynamicVars["MagicNumber"].IntValue;
		if (base.Owner.Creature.GetPowerAmount<EvictionPower>() < intValue)
		{
			return;
		}
		await PlayerCmd.GainGold(base.IsUpgraded ? 50 : 30, base.Owner);
		try
		{
			Creature target = cardPlay.Target;
			if (target.IsAlive)
			{
				await CreatureCmd.Escape(target);
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Watcher] EvictGuest force-flee failed: " + ex.Message);
		}
		EvictionPower power = base.Owner.Creature.GetPower<EvictionPower>();
		if (power != null)
		{
			await PowerCmd.Remove(power);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(-1m);
	}
}
