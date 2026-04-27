using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class Prescience : WatcherCard, IProphecyCard
{
	public override bool GainsBlock => true;

	protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Defend };

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new BlockVar(5m, ValueProp.Move),
		new CardsVar("MagicNumber", 2)
	});

	public Prescience()
		: base(1, CardType.Skill, CardRarity.Basic, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
		int reduction = base.DynamicVars["MagicNumber"].IntValue;
		CombatState combatState = base.Owner.Creature.CombatState;
		if (combatState != null && reduction > 0)
		{
			IEnumerable<Creature> enumerable = (base.IsUpgraded ? combatState.Enemies.Where((Creature c) => c.IsAlive).ToList() : ((cardPlay.Target == null || !cardPlay.Target.IsAlive) ? ((IEnumerable<Creature>)Array.Empty<Creature>()) : ((IEnumerable<Creature>)new global::_003C_003Ez__ReadOnlySingleElementList<Creature>(cardPlay.Target))));
			foreach (Creature enemy in enumerable)
			{
				if (enemy.Monster?.NextMove?.Intents.OfType<AttackIntent>().Any() == true)
				{
					try
					{
						await PowerCmd.Apply<PrescienceShacklesPower>(enemy, reduction, base.Owner.Creature, this);
						await WatcherProphecy.ApplyConfusion(enemy, base.Owner.Creature, this, 1);
						WatcherProphecy.RefreshIntents(enemy);
					}
					catch (Exception ex)
					{
						Log.Warn("[Watcher] Prescience shackles on " + enemy.Monster?.Id.Entry + " failed: " + ex.Message);
					}
				}
			}
		}
		int num = (base.IsUpgraded ? 4 : 3);
		await PowerCmd.Apply<KnowFatePower>(base.Owner.Creature, num, base.Owner.Creature, this);
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this
		});
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(3m);
	}
}
