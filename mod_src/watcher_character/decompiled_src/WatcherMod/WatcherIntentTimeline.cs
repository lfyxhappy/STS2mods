using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;

namespace WatcherMod;

internal static class WatcherIntentTimeline
{
	public sealed class Snapshot
	{
		public MonsterState CurrentState;

		public bool PerformedFirstMove;

		public MoveState NextMove;

		public int RngCounter;

		public int StateLogCount;
	}

	private static readonly FieldInfo _currentStateField = typeof(MonsterMoveStateMachine).GetField("_currentState", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo _performedFirstMoveField = typeof(MonsterMoveStateMachine).GetField("_performedFirstMove", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo _rngRandomField = typeof(Rng).GetField("_random", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo? _rngCounterBacking = typeof(Rng).GetField("<Counter>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo? _monsterNextMoveBacking = typeof(MonsterModel).GetField("<NextMove>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

	public static List<MoveState> ForecastFutureMoves(Creature enemy, int count)
	{
		List<MoveState> list = new List<MoveState>(count);
		if (enemy?.Monster == null || enemy.Monster.MoveStateMachine == null)
		{
			return list;
		}
		CombatState combatState = enemy.CombatState;
		if (combatState == null)
		{
			return list;
		}
		MonsterModel monster = enemy.Monster;
		Rng rng = monster.RunRng?.MonsterAi;
		if (rng == null)
		{
			return list;
		}
		Snapshot snap = new Snapshot
		{
			CurrentState = (MonsterState)_currentStateField.GetValue(monster.MoveStateMachine),
			PerformedFirstMove = (bool)_performedFirstMoveField.GetValue(monster.MoveStateMachine),
			NextMove = monster.NextMove,
			RngCounter = rng.Counter,
			StateLogCount = monster.MoveStateMachine.StateLog.Count
		};
		try
		{
			list.Add(monster.NextMove);
			_performedFirstMoveField.SetValue(monster.MoveStateMachine, true);
			IReadOnlyList<Creature> playerCreatures = combatState.PlayerCreatures;
			for (int i = 1; i < count; i++)
			{
				try
				{
					MoveState moveState = monster.MoveStateMachine.RollMove(playerCreatures, enemy, rng);
					list.Add(moveState);
					monster.MoveStateMachine.OnMovePerformed(moveState);
				}
				catch (Exception ex)
				{
					Log.Warn($"[Watcher] Forecast stopped at slot {i}: {ex.Message}");
					break;
				}
			}
		}
		finally
		{
			RestoreSnapshot(monster, rng, snap);
		}
		return list;
	}

	private static void RestoreSnapshot(MonsterModel monster, Rng rng, Snapshot snap)
	{
		try
		{
			if (monster.MoveStateMachine != null)
			{
				_currentStateField.SetValue(monster.MoveStateMachine, snap.CurrentState);
				_performedFirstMoveField.SetValue(monster.MoveStateMachine, snap.PerformedFirstMove);
				List<MonsterState> stateLog = monster.MoveStateMachine.StateLog;
				while (stateLog.Count > snap.StateLogCount)
				{
					stateLog.RemoveAt(stateLog.Count - 1);
				}
			}
			_monsterNextMoveBacking?.SetValue(monster, snap.NextMove);
			_rngRandomField.SetValue(rng, new Random((int)rng.Seed));
			_rngCounterBacking?.SetValue(rng, 0);
			rng.FastForwardCounter(snap.RngCounter);
		}
		catch (Exception ex)
		{
			Log.Error("[Watcher] Timeline snapshot restore failed: " + ex.Message);
		}
	}

	public static MonsterPerformedMoveEntry? GetLastPerformedMove(Creature enemy)
	{
		try
		{
			return CombatManager.Instance?.History?.Entries.OfType<MonsterPerformedMoveEntry>().LastOrDefault((MonsterPerformedMoveEntry e) => e.Monster == enemy.Monster);
		}
		catch
		{
			return null;
		}
	}

	public static int SumDamageDealtTo(MonsterPerformedMoveEntry entry, Creature receiver)
	{
		try
		{
			return (from e in CombatManager.Instance.History.Entries.OfType<DamageReceivedEntry>()
				where e.Dealer == entry.Monster.Creature && e.Receiver == receiver && e.RoundNumber == entry.RoundNumber && e.CurrentSide == entry.CurrentSide
				select e).Sum((DamageReceivedEntry e) => e.Result.UnblockedDamage);
		}
		catch
		{
			return 0;
		}
	}

	public static List<(PowerModel Power, decimal Amount)> GetPowersAppliedTo(MonsterPerformedMoveEntry entry, Creature receiver)
	{
		List<(PowerModel, decimal)> list = new List<(PowerModel, decimal)>();
		try
		{
			foreach (PowerReceivedEntry item in from e in CombatManager.Instance.History.Entries.OfType<PowerReceivedEntry>()
				where e.Applier == entry.Monster.Creature && e.Actor == receiver && e.RoundNumber == entry.RoundNumber && e.CurrentSide == entry.CurrentSide
				select e)
			{
				list.Add((item.Power, item.Amount));
			}
		}
		catch
		{
		}
		return list;
	}
}
