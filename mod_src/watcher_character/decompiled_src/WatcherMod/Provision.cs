using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class Provision : WatcherCard, IProphecyCard
{
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new CardsVar("MagicNumber", 7),
		new BlockVar(6m, ValueProp.Move),
		new DynamicVar("BlockPerSkill", 2m)
	});

	public Provision()
		: base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		PlayerCombatState playerCombatState = base.Owner.PlayerCombatState;
		if (playerCombatState != null)
		{
			int intValue = base.DynamicVars["MagicNumber"].IntValue;
			int effectiveScryAmount = WatcherCombatHelper.GetEffectiveScryAmount(base.Owner, intValue);
			List<CardModel> source = playerCombatState.DrawPile.Cards.Take(effectiveScryAmount).ToList();
			int skillCount = source.Count((CardModel c) => c.Type == CardType.Skill);
			await WatcherCombatHelper.Scry(choiceContext, base.Owner, intValue, this);
			int num = base.DynamicVars.Block.IntValue + skillCount * base.DynamicVars["BlockPerSkill"].IntValue;
			if (num > 0)
			{
				await CreatureCmd.GainBlock(base.Owner.Creature, num, ValueProp.Move, cardPlay);
			}
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(3m);
		base.DynamicVars["BlockPerSkill"].UpgradeValueBy(1m);
	}
}
