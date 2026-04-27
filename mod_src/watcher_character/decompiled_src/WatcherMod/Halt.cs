using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class Halt : WatcherCard
{
	public override bool GainsBlock => true;

	protected override bool ShouldGlowGoldInternal
	{
		get
		{
			if (base.Owner != null)
			{
				return WatcherCombatHelper.IsInStance<Wrath>(base.Owner.Creature);
			}
			return false;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[2]
	{
		new BlockVar(3m, ValueProp.Move),
		new DynamicVar("MagicNumber", 9m)
	};

	public Halt()
		: base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		decimal baseValue = base.DynamicVars.Block.BaseValue;
		if (WatcherCombatHelper.IsInStance<Wrath>(base.Owner.Creature))
		{
			baseValue += base.DynamicVars["MagicNumber"].BaseValue;
		}
		await CreatureCmd.GainBlock(base.Owner.Creature, baseValue, ValueProp.Move, cardPlay);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(1m);
		base.DynamicVars["MagicNumber"].UpgradeValueBy(4m);
	}
}
