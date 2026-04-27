using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class Sanctity : WatcherCard
{
	public override bool GainsBlock => true;

	protected override bool ShouldGlowGoldInternal => WatcherSimpleAHelper.GetPreviousPlayedCardType(this) == CardType.Skill;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new BlockVar(6m, ValueProp.Move),
		new CardsVar("MagicNumber", 2)
	});

	public Sanctity()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
		if (WatcherSimpleAHelper.GetPreviousPlayedCardType(this) == CardType.Skill)
		{
			await CardPileCmd.Draw(choiceContext, base.DynamicVars["MagicNumber"].BaseValue, base.Owner);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(3m);
	}
}
