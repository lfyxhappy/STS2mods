using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace WatcherMod;

internal static class WatcherIntentSelector
{
	private static List<WatcherIntentProxy> CreateProxies(Player owner, IEnumerable<Creature> enemies)
	{
		CombatState combatState = owner.Creature.CombatState ?? throw new InvalidOperationException("WatcherIntentSelector requires an active CombatState.");
		List<WatcherIntentProxy> list = new List<WatcherIntentProxy>();
		foreach (Creature enemy in enemies)
		{
			if (enemy != null && enemy.IsAlive)
			{
				WatcherIntentProxy watcherIntentProxy = combatState.CreateCard<WatcherIntentProxy>(owner);
				watcherIntentProxy.ConfigureFromEnemy(enemy);
				list.Add(watcherIntentProxy);
			}
		}
		return list;
	}

	private static void DisposeProxies(Player owner, IEnumerable<WatcherIntentProxy> proxies)
	{
		CombatState combatState = owner.Creature.CombatState;
		if (combatState == null)
		{
			return;
		}
		foreach (WatcherIntentProxy proxy in proxies)
		{
			try
			{
				combatState.RemoveCard(proxy);
			}
			catch
			{
			}
		}
	}

	public static async Task<Creature?> PickOneIntent(PlayerChoiceContext choiceContext, Player owner, IEnumerable<Creature> enemies, LocString prompt, bool cancelable = false)
	{
		List<WatcherIntentProxy> proxies = CreateProxies(owner, enemies);
		if (proxies.Count == 0)
		{
			return null;
		}
		if (proxies.Count == 1)
		{
			Creature? attachedEnemy = proxies[0].AttachedEnemy;
			DisposeProxies(owner, proxies);
			return attachedEnemy;
		}
		try
		{
			CardSelectorPrefs prefs = WatcherCombatHelper.SetCancelable(new CardSelectorPrefs(prompt, 1, 1), cancelable);
			return ((await CardSelectCmd.FromSimpleGrid(choiceContext, proxies.Cast<CardModel>().ToList(), owner, prefs)).FirstOrDefault() as WatcherIntentProxy)?.AttachedEnemy;
		}
		finally
		{
			DisposeProxies(owner, proxies);
		}
	}

	internal static List<WatcherIntentProxy> CreateProxiesFromMoves(Player owner, IReadOnlyList<(MoveState Move, Creature OwnerEnemy, string? Label)> moves)
	{
		CombatState combatState = owner.Creature.CombatState ?? throw new InvalidOperationException("WatcherIntentSelector requires an active CombatState.");
		List<WatcherIntentProxy> list = new List<WatcherIntentProxy>(moves.Count);
		foreach (var move in moves)
		{
			if (move.Move != null && move.OwnerEnemy != null)
			{
				WatcherIntentProxy watcherIntentProxy = combatState.CreateCard<WatcherIntentProxy>(owner);
				watcherIntentProxy.ConfigureFromMove(move.Move, move.OwnerEnemy, move.Label);
				list.Add(watcherIntentProxy);
			}
		}
		return list;
	}

	internal static void DisposeProxiesPublic(Player owner, IEnumerable<WatcherIntentProxy> proxies)
	{
		DisposeProxies(owner, proxies);
	}

	public static async Task<List<WatcherIntentProxy>> PickFromProxies(PlayerChoiceContext choiceContext, Player owner, List<WatcherIntentProxy> proxies, int minSelect, int maxSelect, LocString prompt, bool cancelable = false)
	{
		List<WatcherIntentProxy> result = new List<WatcherIntentProxy>();
		if (proxies.Count == 0)
		{
			return result;
		}
		if (maxSelect <= 0)
		{
			return result;
		}
		int num = Math.Max(0, Math.Min(minSelect, proxies.Count));
		int maxCount = Math.Max(num, Math.Min(maxSelect, proxies.Count));
		CardSelectorPrefs prefs = WatcherCombatHelper.SetCancelable(new CardSelectorPrefs(prompt, num, maxCount), cancelable);
		foreach (CardModel item2 in await CardSelectCmd.FromSimpleGrid(choiceContext, proxies.Cast<CardModel>().ToList(), owner, prefs))
		{
			if (item2 is WatcherIntentProxy item)
			{
				result.Add(item);
			}
		}
		return result;
	}

	public static async Task<List<Creature>> PickOrder(PlayerChoiceContext choiceContext, Player owner, IEnumerable<Creature> enemies, LocString prompt)
	{
		List<WatcherIntentProxy> proxies = CreateProxies(owner, enemies);
		if (proxies.Count == 0)
		{
			return new List<Creature>();
		}
		if (proxies.Count == 1)
		{
			Creature attachedEnemy = proxies[0].AttachedEnemy;
			DisposeProxies(owner, proxies);
			return (attachedEnemy != null) ? new List<Creature> { attachedEnemy } : new List<Creature>();
		}
		try
		{
			int count = proxies.Count;
			CardSelectorPrefs prefs = WatcherCombatHelper.SetCancelable(new CardSelectorPrefs(prompt, count, count), value: false);
			IEnumerable<CardModel> obj = await CardSelectCmd.FromSimpleGrid(choiceContext, proxies.Cast<CardModel>().ToList(), owner, prefs);
			List<Creature> list = new List<Creature>();
			foreach (CardModel item in obj)
			{
				if (item is WatcherIntentProxy { AttachedEnemy: not null } watcherIntentProxy)
				{
					list.Add(watcherIntentProxy.AttachedEnemy);
				}
			}
			return list;
		}
		finally
		{
			DisposeProxies(owner, proxies);
		}
	}
}
