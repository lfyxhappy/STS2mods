using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class SpiritShield : WatcherCard
{
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("MagicNumber", 3m));

	public SpiritShield()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		decimal num = (decimal)PileType.Hand.GetPile(base.Owner).Cards.Count * base.DynamicVars["MagicNumber"].BaseValue;
		if (num > 0m)
		{
			await CreatureCmd.GainBlock(base.Owner.Creature, num, ValueProp.Move, cardPlay);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(1m);
	}
}
