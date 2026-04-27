using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class AvoidPeril : WatcherCard, IProphecyCard
{
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new BlockVar(6m, ValueProp.Move),
		new PowerVar<KnowFatePower>(3m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<KnowFatePower>());

	public AvoidPeril()
		: base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
		MonsterModel? monster = cardPlay.Target.Monster;
		if (monster != null && monster.NextMove?.Intents.OfType<AttackIntent>().Any() == true)
		{
			await PowerCmd.Apply<KnowFatePower>(base.Owner.Creature, base.DynamicVars[typeof(KnowFatePower).Name].BaseValue, base.Owner.Creature, this);
		}
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this
		});
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(2m);
		base.DynamicVars[typeof(KnowFatePower).Name].UpgradeValueBy(1m);
	}
}
