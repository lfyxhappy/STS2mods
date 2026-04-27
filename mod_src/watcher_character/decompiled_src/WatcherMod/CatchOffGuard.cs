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

public sealed class CatchOffGuard : WatcherCard, IProphecyCard
{
	private const int _kfThreshold = 5;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new CardsVar("MagicNumber", 3),
		new DamageVar(4m, ValueProp.Move),
		new DynamicVar("KnowFateBonus", 10m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<KnowFatePower>());

	public CatchOffGuard()
		: base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		PlayerCombatState playerCombatState = base.Owner.PlayerCombatState;
		if (playerCombatState == null)
		{
			return;
		}
		int intValue = base.DynamicVars["MagicNumber"].IntValue;
		int effectiveScryAmount = WatcherCombatHelper.GetEffectiveScryAmount(base.Owner, intValue);
		List<CardModel> source = playerCombatState.DrawPile.Cards.Take(effectiveScryAmount).ToList();
		int prophecyCount = source.Count((CardModel c) => c is IProphecyCard);
		await WatcherCombatHelper.Scry(choiceContext, base.Owner, intValue, this);
		if (cardPlay.Target.IsDead)
		{
			return;
		}
		decimal damage = (decimal)prophecyCount * base.DynamicVars.Damage.BaseValue;
		int powerAmount = base.Owner.Creature.GetPowerAmount<KnowFatePower>();
		if (powerAmount >= 5)
		{
			damage += base.DynamicVars["KnowFateBonus"].BaseValue;
			KnowFatePower power = base.Owner.Creature.GetPower<KnowFatePower>();
			if (power != null)
			{
				if (5 < powerAmount)
				{
					await PowerCmd.ModifyAmount(power, -5m, base.Owner.Creature, this);
				}
				else
				{
					await PowerCmd.Remove(power);
				}
			}
		}
		if (damage > 0m)
		{
			await DamageCmd.Attack(damage).FromCard(this).Targeting(cardPlay.Target)
				.WithHitFx("vfx/vfx_attack_slash")
				.Execute(choiceContext);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(1m);
		base.DynamicVars["KnowFateBonus"].UpgradeValueBy(2m);
	}
}
