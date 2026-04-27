using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;

namespace WatcherMod;

public sealed class Formation : WatcherCard, IProphecyCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			if (!base.IsUpgraded)
			{
				return new CardKeyword[1] { CardKeyword.Exhaust };
			}
			return Array.Empty<CardKeyword>();
		}
	}

	public Formation()
		: base(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CombatState combatState = base.Owner.Creature.CombatState;
		if (combatState == null)
		{
			return;
		}
		List<Creature> liveEnemies = combatState.Enemies.Where((Creature c) => c.IsAlive).ToList();
		if (liveEnemies.Count == 0)
		{
			return;
		}
		if (liveEnemies.Count == 1)
		{
			WatcherProphecy.StunEnemy(liveEnemies[0]);
			await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
			{
				Source = this,
				AffectedEnemy = liveEnemies[0],
				ChangedIntent = true
			});
			return;
		}
		List<Creature> list = await WatcherIntentSelector.PickOrder(choiceContext, base.Owner, liveEnemies, new LocString("card_selection", "TO_DISCARD"));
		if (list.Count != liveEnemies.Count)
		{
			return;
		}
		var list2 = list.Select((Creature e) => new
		{
			Owner = e,
			Move = e.Monster?.NextMove
		}).ToList();
		bool anyChanged = false;
		for (int num = 0; num < liveEnemies.Count; num++)
		{
			Creature creature = liveEnemies[num];
			var anon = list2[num];
			if (anon.Move == null)
			{
				continue;
			}
			if (anon.Owner != creature && !anon.Move.CanTransitionAway)
			{
				WatcherProphecy.StunEnemy(creature);
				anyChanged = true;
			}
			else if (anon.Owner != creature)
			{
				try
				{
					creature.Monster?.SetMoveImmediate(anon.Move, forceTransition: true);
					WatcherProphecy.RefreshIntents(creature);
					anyChanged = true;
				}
				catch (Exception ex)
				{
					Log.Error("[Watcher] Formation reassign failed: " + ex.Message);
				}
			}
		}
		if (anyChanged)
		{
			foreach (Creature item in liveEnemies)
			{
				await WatcherProphecy.ApplyConfusion(item, base.Owner.Creature, this);
			}
		}
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this,
			AffectedEnemy = liveEnemies[0],
			ChangedIntent = anyChanged
		});
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
		RemoveKeyword(CardKeyword.Exhaust);
	}
}
