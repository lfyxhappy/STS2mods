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
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace WatcherMod;

public sealed class RewriteFate : WatcherCard, IProphecyCard
{
	private const int ForecastSize = 4;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	public RewriteFate()
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
		CardModel deleteTok = await WatcherCombatHelper.CreateWatcherCard<RewriteChoiceDelete>(base.Owner);
		CardModel item = await WatcherCombatHelper.CreateWatcherCard<RewriteChoiceAdvance>(base.Owner);
		List<CardModel> options = new List<CardModel> { deleteTok, item };
		CardModel cardModel = await WatcherCombatHelper.ChooseOne(base.Owner, options, new LocString("card_selection", "TO_PLAY"));
		List<MoveState> list2 = WatcherIntentTimeline.ForecastFutureMoves(enemy, 4);
		if (list2.Count == 0)
		{
			await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
			{
				Source = this,
				AffectedEnemy = enemy,
				ChangedIntent = false
			});
			return;
		}
		bool changed = ((!(cardModel is RewriteChoiceAdvance) || list2.Count < 2) ? (await DeleteFutureIntent(choiceContext, enemy, list2)) : (await AdvanceFutureIntent(choiceContext, enemy, list2)));
		if (changed)
		{
			await WatcherProphecy.ApplyConfusion(enemy, base.Owner.Creature, this);
		}
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this,
			AffectedEnemy = enemy,
			ChangedIntent = changed
		});
	}

	private async Task<bool> DeleteFutureIntent(PlayerChoiceContext choiceContext, Creature enemy, List<MoveState> forecast)
	{
		List<(MoveState, Creature, string)> list = new List<(MoveState, Creature, string)>();
		for (int i = 0; i < forecast.Count; i++)
		{
			list.Add((forecast[i], enemy, (i == 0) ? "T+0" : $"T+{i}"));
		}
		List<WatcherIntentProxy> proxies = WatcherIntentSelector.CreateProxiesFromMoves(base.Owner, list);
		List<WatcherIntentProxy> list2;
		try
		{
			list2 = await WatcherIntentSelector.PickFromProxies(choiceContext, base.Owner, proxies, 1, 1, new LocString("card_selection", "TO_DISCARD"));
		}
		finally
		{
			WatcherIntentSelector.DisposeProxiesPublic(base.Owner, proxies);
		}
		if (list2.Count == 0)
		{
			return false;
		}
		int num = proxies.IndexOf(list2[0]);
		List<MoveState> plan = new List<MoveState>();
		for (int j = 0; j < forecast.Count; j++)
		{
			if (j != num)
			{
				plan.Add(forecast[j]);
			}
		}
		bool changed = false;
		if (plan.Count == 0)
		{
			WatcherProphecy.ForceStunEnemy(enemy);
			changed = true;
		}
		else
		{
			if (plan[0] != enemy.Monster.NextMove)
			{
				try
				{
					enemy.Monster.SetMoveImmediate(plan[0], forceTransition: true);
					WatcherProphecy.RefreshIntents(enemy);
					changed = true;
				}
				catch (Exception ex)
				{
					Log.Error("[Watcher] RewriteFate delete set-head failed: " + ex.Message);
				}
			}
			if (plan.Count > 1)
			{
				ForecastedMovesPower forecastedMovesPower = await PowerCmd.Apply<ForecastedMovesPower>(enemy, 1m, base.Owner.Creature, this);
				if (forecastedMovesPower != null)
				{
					forecastedMovesPower.Queue.Clear();
					for (int k = 1; k < plan.Count; k++)
					{
						forecastedMovesPower.Queue.Enqueue(plan[k]);
					}
					changed = true;
				}
			}
		}
		return changed;
	}

	private async Task<bool> AdvanceFutureIntent(PlayerChoiceContext choiceContext, Creature enemy, List<MoveState> forecast)
	{
		List<(MoveState, Creature, string)> list = new List<(MoveState, Creature, string)>();
		for (int i = 1; i < forecast.Count; i++)
		{
			list.Add((forecast[i], enemy, $"T+{i}"));
		}
		List<WatcherIntentProxy> proxies = WatcherIntentSelector.CreateProxiesFromMoves(base.Owner, list);
		List<WatcherIntentProxy> list2;
		try
		{
			list2 = await WatcherIntentSelector.PickFromProxies(choiceContext, base.Owner, proxies, 1, 1, new LocString("card_selection", "TO_PLAY"));
		}
		finally
		{
			WatcherIntentSelector.DisposeProxiesPublic(base.Owner, proxies);
		}
		if (list2.Count == 0)
		{
			return false;
		}
		int num = proxies.IndexOf(list2[0]);
		int pickedForecastIdx = num + 1;
		MoveState state = forecast[pickedForecastIdx];
		try
		{
			enemy.Monster.SetMoveImmediate(state, forceTransition: true);
			WatcherProphecy.RefreshIntents(enemy);
		}
		catch (Exception ex)
		{
			Log.Error("[Watcher] RewriteFate advance failed: " + ex.Message);
			return false;
		}
		ForecastedMovesPower forecastedMovesPower = await PowerCmd.Apply<ForecastedMovesPower>(enemy, 1m, base.Owner.Creature, this);
		if (forecastedMovesPower != null)
		{
			forecastedMovesPower.Queue.Clear();
			for (int j = 0; j < pickedForecastIdx; j++)
			{
				forecastedMovesPower.Queue.Enqueue(forecast[j]);
			}
			for (int k = pickedForecastIdx + 1; k < forecast.Count; k++)
			{
				forecastedMovesPower.Queue.Enqueue(forecast[k]);
			}
		}
		return true;
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}
}
