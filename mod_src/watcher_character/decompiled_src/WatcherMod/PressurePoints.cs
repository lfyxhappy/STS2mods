using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class PressurePoints : WatcherCard
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<MarkPower>());

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("MagicNumber", 8m));

	public PressurePoints()
		: base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await PowerCmd.Apply<MarkPower>(cardPlay.Target, base.DynamicVars["MagicNumber"].BaseValue, base.Owner.Creature, this);
		foreach (Creature item in base.CombatState.HittableEnemies.ToList())
		{
			int powerAmount = item.GetPowerAmount<MarkPower>();
			if (powerAmount > 0)
			{
				await CreatureCmd.Damage(choiceContext, item, powerAmount, ValueProp.Unblockable | ValueProp.Unpowered, base.Owner.Creature, this);
			}
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(3m);
	}
}
