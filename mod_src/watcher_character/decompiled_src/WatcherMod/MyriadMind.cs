using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class MyriadMind : WatcherCard, IProphecyCard
{
	protected override bool HasEnergyCostX => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new DamageVar(5m, ValueProp.Move),
		new CardsVar("MagicNumber", 3),
		new DynamicVar("KnowFateBonus", 5m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<KnowFatePower>());

	public MyriadMind()
		: base(-1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(base.CombatState, "CombatState");
		int x = ResolveEnergyXValue();
		int num = x + 2;
		PlayerCombatState playerCombatState = base.Owner.PlayerCombatState;
		bool sawAttack = false;
		if (playerCombatState != null && num > 0)
		{
			List<CardModel> source = playerCombatState.DrawPile.Cards.Take(num).ToList();
			sawAttack = source.Any((CardModel c) => c.Type == CardType.Attack);
		}
		await WatcherCombatHelper.Scry(choiceContext, base.Owner, num, this);
		int kfStacks = base.Owner.Creature.GetPowerAmount<KnowFatePower>();
		decimal num2 = (decimal)x * base.DynamicVars.Damage.BaseValue;
		if (sawAttack)
		{
			num2 += (decimal)x * base.DynamicVars["MagicNumber"].BaseValue;
		}
		num2 += (decimal)kfStacks * base.DynamicVars["KnowFateBonus"].BaseValue;
		if (num2 > 0m)
		{
			await DamageCmd.Attack(num2).FromCard(this).TargetingAllOpponents(base.CombatState)
				.WithHitFx("vfx/vfx_attack_slash")
				.SpawningHitVfxOnEachCreature()
				.Execute(choiceContext);
		}
		if (kfStacks > 0)
		{
			KnowFatePower power = base.Owner.Creature.GetPower<KnowFatePower>();
			if (power != null)
			{
				await PowerCmd.Remove(power);
			}
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(1m);
		base.DynamicVars["MagicNumber"].UpgradeValueBy(1m);
		base.DynamicVars["KnowFateBonus"].UpgradeValueBy(1m);
	}
}
