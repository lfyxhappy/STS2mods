using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace WatcherMod;

public sealed class ControlFuture : WatcherCard, IProphecyCard
{
	private const int ForecastSize = 5;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	public ControlFuture()
		: base(4, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CombatState combatState = base.Owner.Creature.CombatState;
		if (combatState == null)
		{
			return;
		}
		List<Creature> list = combatState.Enemies.Where((Creature c) => c.IsAlive).ToList();
		if (list.Count == 0)
		{
			return;
		}
		Creature enemy = await WatcherIntentSelector.PickOneIntent(choiceContext, base.Owner, list, new LocString("card_selection", "TO_PICK_INTENT"));
		if (enemy?.Monster == null)
		{
			return;
		}
		List<MoveState> forecast = WatcherIntentTimeline.ForecastFutureMoves(enemy, 5);
		if (forecast.Count == 0)
		{
			await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
			{
				Source = this,
				AffectedEnemy = enemy,
				ChangedIntent = false
			});
			return;
		}
		List<(MoveState, Creature, string)> list2 = new List<(MoveState, Creature, string)>(forecast.Count);
		for (int num = 0; num < forecast.Count; num++)
		{
			string item = ((num == 0) ? "T+0" : $"T+{num}");
			list2.Add((forecast[num], enemy, item));
		}
		List<WatcherIntentProxy> proxies = WatcherIntentSelector.CreateProxiesFromMoves(base.Owner, list2);
		List<WatcherIntentProxy> list3;
		try
		{
			int maxSelect = ((!base.IsUpgraded) ? 1 : proxies.Count);
			int minSelect = ((proxies.Count != 0) ? 1 : 0);
			list3 = await WatcherIntentSelector.PickFromProxies(choiceContext, base.Owner, proxies, minSelect, maxSelect, new LocString("card_selection", "TO_DISCARD"));
		}
		finally
		{
			WatcherIntentSelector.DisposeProxiesPublic(base.Owner, proxies);
		}
		HashSet<int> hashSet = new HashSet<int>();
		for (int num2 = 0; num2 < proxies.Count; num2++)
		{
			if (list3.Contains(proxies[num2]))
			{
				hashSet.Add(num2);
			}
		}
		List<MoveState> plan = new List<MoveState>(forecast.Count);
		for (int num3 = 0; num3 < forecast.Count; num3++)
		{
			if (!hashSet.Contains(num3))
			{
				plan.Add(forecast[num3]);
			}
		}
		bool changedIntent = false;
		if (plan.Count == 0)
		{
			WatcherProphecy.ForceStunEnemy(enemy);
			changedIntent = true;
		}
		else
		{
			MoveState moveState = plan[0];
			if (moveState != enemy.Monster.NextMove)
			{
				try
				{
					enemy.Monster.SetMoveImmediate(moveState, forceTransition: true);
					WatcherProphecy.RefreshIntents(enemy);
					changedIntent = true;
				}
				catch (Exception ex)
				{
					Log.Error("[Watcher] ControlFuture set-head failed: " + ex.Message);
				}
			}
			if (plan.Count > 1)
			{
				ForecastedMovesPower forecastedMovesPower = await PowerCmd.Apply<ForecastedMovesPower>(enemy, 1m, base.Owner.Creature, this);
				if (forecastedMovesPower != null)
				{
					forecastedMovesPower.Queue.Clear();
					for (int num4 = 1; num4 < plan.Count; num4++)
					{
						forecastedMovesPower.Queue.Enqueue(plan[num4]);
					}
					changedIntent = true;
				}
			}
		}
		if (changedIntent)
		{
			await WatcherProphecy.ApplyConfusion(enemy, base.Owner.Creature, this);
		}
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this,
			AffectedEnemy = enemy,
			ChangedIntent = changedIntent
		});
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}
}
