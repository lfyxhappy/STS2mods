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

public sealed class Compulsion : WatcherCard, IProphecyCard
{
	private const int ForecastSize = 4;

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

	public Compulsion()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
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
		List<MoveState> forecast = WatcherIntentTimeline.ForecastFutureMoves(enemy, 4);
		if (forecast.Count <= 1)
		{
			return;
		}
		List<(MoveState, Creature, string)> list2 = new List<(MoveState, Creature, string)>();
		for (int num = 1; num < forecast.Count; num++)
		{
			list2.Add((forecast[num], enemy, $"T+{num}"));
		}
		List<WatcherIntentProxy> proxies = WatcherIntentSelector.CreateProxiesFromMoves(base.Owner, list2);
		List<WatcherIntentProxy> list3;
		try
		{
			list3 = await WatcherIntentSelector.PickFromProxies(choiceContext, base.Owner, proxies, 1, 1, new LocString("card_selection", "TO_PLAY"));
		}
		finally
		{
			WatcherIntentSelector.DisposeProxiesPublic(base.Owner, proxies);
		}
		if (list3.Count == 0)
		{
			return;
		}
		int num2 = proxies.IndexOf(list3[0]);
		int pickedForecastIdx = num2 + 1;
		MoveState state = forecast[pickedForecastIdx];
		try
		{
			enemy.Monster.SetMoveImmediate(state, forceTransition: true);
			WatcherProphecy.RefreshIntents(enemy);
		}
		catch (Exception ex)
		{
			Log.Error("[Watcher] Compulsion SetMoveImmediate failed: " + ex.Message);
		}
		ForecastedMovesPower forecastedMovesPower = await PowerCmd.Apply<ForecastedMovesPower>(enemy, 1m, base.Owner.Creature, this);
		if (forecastedMovesPower != null)
		{
			forecastedMovesPower.Queue.Clear();
			for (int num3 = 0; num3 < pickedForecastIdx; num3++)
			{
				forecastedMovesPower.Queue.Enqueue(forecast[num3]);
			}
			for (int num4 = pickedForecastIdx + 1; num4 < forecast.Count; num4++)
			{
				forecastedMovesPower.Queue.Enqueue(forecast[num4]);
			}
		}
		await WatcherProphecy.ApplyConfusion(enemy, base.Owner.Creature, this);
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this,
			AffectedEnemy = enemy,
			ChangedIntent = true
		});
	}

	protected override void OnUpgrade()
	{
	}
}
