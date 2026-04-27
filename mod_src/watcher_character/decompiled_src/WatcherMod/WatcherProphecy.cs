using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace WatcherMod;

internal static class WatcherProphecy
{
	public static async Task Trigger(Player owner, ProphecyContext ctx)
	{
		if (owner?.Creature == null)
		{
			return;
		}
		foreach (PowerModel power in owner.Creature.Powers.ToList())
		{
			if (power is IWatcherProphecyListener watcherProphecyListener)
			{
				try
				{
					await watcherProphecyListener.OnProphecy(owner, ctx);
				}
				catch (Exception ex)
				{
					Log.Error("[Watcher] Prophecy power " + power.GetType().Name + " failed: " + ex.Message);
				}
			}
		}
		foreach (RelicModel relic in owner.Relics.ToList())
		{
			if (relic is IWatcherProphecyListener watcherProphecyListener2)
			{
				try
				{
					await watcherProphecyListener2.OnProphecy(owner, ctx);
				}
				catch (Exception ex2)
				{
					Log.Error("[Watcher] Prophecy relic " + relic.GetType().Name + " failed: " + ex2.Message);
				}
			}
		}
	}

	public static MoveState? GetCurrentMove(Creature enemy)
	{
		try
		{
			return enemy?.Monster?.NextMove;
		}
		catch
		{
			return null;
		}
	}

	public static void RerollIntent(Creature enemy, IEnumerable<Creature> targets)
	{
		try
		{
			enemy.Monster?.RollMove(targets);
			NCombatRoom.Instance?.GetCreatureNode(enemy)?.RefreshIntents();
		}
		catch (Exception ex)
		{
			Log.Error("[Watcher] RerollIntent failed: " + ex.Message);
		}
	}

	public static void RefreshIntents(Creature enemy)
	{
		try
		{
			NCombatRoom.Instance?.GetCreatureNode(enemy)?.RefreshIntents();
		}
		catch
		{
		}
	}

	public static void StunEnemy(Creature enemy)
	{
		try
		{
			if (enemy?.Monster != null && !enemy.IsDead)
			{
				enemy.StunInternal((IReadOnlyList<Creature> _) => Task.CompletedTask, null);
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Watcher] StunEnemy failed: " + ex.Message);
		}
	}

	public static async Task ApplyConfusion(Creature? target, Creature applier, CardModel source, int amount = 2)
	{
		try
		{
			if (target != null && !target.IsDead && amount > 0)
			{
				await PowerCmd.Apply<ConfusionPower>(target, amount, applier, source);
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Watcher] ApplyConfusion failed: " + ex.Message);
		}
	}

	public static void ForceStunEnemy(Creature enemy)
	{
		try
		{
			if (enemy?.Monster != null && !enemy.IsDead)
			{
				string text = enemy.Monster.NextMove?.Id;
				if (string.IsNullOrEmpty(text))
				{
					List<MonsterState> stateLog = enemy.Monster.MoveStateMachine.StateLog;
					text = ((stateLog.Count > 0) ? stateLog.Last().Id : null);
				}
				MoveState state = new MoveState("STUNNED", (IReadOnlyList<Creature> _) => Task.CompletedTask, new StunIntent())
				{
					FollowUpStateId = text,
					MustPerformOnceBeforeTransitioning = true
				};
				enemy.Monster.SetMoveImmediate(state, forceTransition: true);
				RefreshIntents(enemy);
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Watcher] ForceStunEnemy failed: " + ex.Message);
		}
	}

	public static bool IsIntentFixed(Creature enemy)
	{
		try
		{
			return enemy != null && enemy.Monster?.NextMove?.CanTransitionAway == false;
		}
		catch
		{
			return false;
		}
	}

	public static bool RerollOrStun(Creature enemy, IEnumerable<Creature> targets)
	{
		if (enemy?.Monster == null)
		{
			return false;
		}
		if (IsIntentFixed(enemy))
		{
			StunEnemy(enemy);
			return true;
		}
		string obj = enemy.Monster.NextMove?.Id;
		RerollIntent(enemy, targets);
		string text = enemy.Monster.NextMove?.Id;
		if (obj == text)
		{
			StunEnemy(enemy);
			return true;
		}
		return true;
	}

	public static bool SwapIntents(Creature a, Creature b)
	{
		try
		{
			if (a?.Monster == null || b?.Monster == null)
			{
				return false;
			}
			if (a == b)
			{
				return false;
			}
			MoveState nextMove = a.Monster.NextMove;
			MoveState nextMove2 = b.Monster.NextMove;
			bool flag = !nextMove.CanTransitionAway;
			bool flag2 = !nextMove2.CanTransitionAway;
			if (flag && flag2)
			{
				StunEnemy(a);
				StunEnemy(b);
				return true;
			}
			if (flag)
			{
				StunEnemy(a);
				return true;
			}
			if (flag2)
			{
				StunEnemy(b);
				return true;
			}
			a.Monster.SetMoveImmediate(nextMove2, forceTransition: true);
			b.Monster.SetMoveImmediate(nextMove, forceTransition: true);
			RefreshIntents(a);
			RefreshIntents(b);
			return nextMove.Id != nextMove2.Id || true;
		}
		catch (Exception ex)
		{
			Log.Error("[Watcher] SwapIntents failed: " + ex.Message);
			return false;
		}
	}
}
