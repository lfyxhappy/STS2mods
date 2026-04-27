using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace WatcherMod;

public sealed class Abscond : WatcherCard, IProphecyCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new CardsVar("MagicNumber", 6),
		new DynamicVar("EnergyGain", 2m)
	});

	public Abscond()
		: base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await WatcherCombatHelper.Scry(choiceContext, base.Owner, base.DynamicVars["MagicNumber"].IntValue, this);
		await PlayerCmd.GainEnergy(base.DynamicVars["EnergyGain"].BaseValue, base.Owner);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(2m);
		base.DynamicVars["EnergyGain"].UpgradeValueBy(1m);
	}
}
