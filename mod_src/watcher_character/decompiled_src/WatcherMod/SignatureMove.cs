using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class SignatureMove : WatcherCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(30m, ValueProp.Move));

	protected override bool IsPlayable
	{
		get
		{
			if (base.Owner == null)
			{
				return true;
			}
			return PileType.Hand.GetPile(base.Owner).Cards.Count((CardModel c) => c.Type == CardType.Attack) <= 1;
		}
	}

	protected override bool ShouldGlowGoldInternal
	{
		get
		{
			if (base.Owner == null)
			{
				return false;
			}
			return PileType.Hand.GetPile(base.Owner).Cards.Count((CardModel c) => c.Type == CardType.Attack) <= 1;
		}
	}

	public SignatureMove()
		: base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(10m);
	}
}
