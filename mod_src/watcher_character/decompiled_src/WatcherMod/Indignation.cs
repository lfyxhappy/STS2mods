using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace WatcherMod;

public sealed class Indignation : WatcherCard
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[3]
	{
		WatcherHoverTips.Stance,
		HoverTipFactory.FromPower<Wrath>(),
		HoverTipFactory.FromPower<VulnerablePower>()
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("MagicNumber", 3m));

	public Indignation()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (WatcherCombatHelper.IsInStance<Wrath>(base.Owner.Creature))
		{
			await PowerCmd.Apply<VulnerablePower>(base.CombatState.HittableEnemies, base.DynamicVars["MagicNumber"].BaseValue, base.Owner.Creature, this);
		}
		else
		{
			await WatcherCombatHelper.EnterWrath(base.Owner, this);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(2m);
	}
}
