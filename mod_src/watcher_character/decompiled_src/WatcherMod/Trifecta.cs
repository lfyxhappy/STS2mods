using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class Trifecta : WatcherCard, IProphecyCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new CardsVar("MagicNumber", 5),
		new DynamicVar("EnergyGain", 1m),
		new PowerVar<ConfusionPower>(3m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<ConfusionPower>());

	public Trifecta()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int intValue = base.DynamicVars["MagicNumber"].IntValue;
		if ((await WatcherCombatHelper.Scry(choiceContext, base.Owner, intValue, this)).Select((CardModel c) => c.Type).Distinct().Count() < 3)
		{
			return;
		}
		await PlayerCmd.GainEnergy(base.DynamicVars["EnergyGain"].BaseValue, base.Owner);
		if (base.CombatState == null)
		{
			return;
		}
		int confusion = base.DynamicVars[typeof(ConfusionPower).Name].IntValue;
		foreach (Creature item in base.CombatState.Enemies.Where((Creature c) => c.IsAlive).ToList())
		{
			await WatcherProphecy.ApplyConfusion(item, base.Owner.Creature, this, confusion);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(2m);
	}
}
