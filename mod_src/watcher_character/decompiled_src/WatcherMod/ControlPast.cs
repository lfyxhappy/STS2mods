using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace WatcherMod;

public sealed class ControlPast : WatcherCard, IProphecyCard
{
	private static readonly HashSet<MonsterPerformedMoveEntry> _undoneMoves = new HashSet<MonsterPerformedMoveEntry>();

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

	public ControlPast()
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
		CombatHistory combatHistory = CombatManager.Instance?.History;
		if (combatHistory == null)
		{
			return;
		}
		List<MonsterPerformedMoveEntry> pastMoves = (from e in combatHistory.Entries.OfType<MonsterPerformedMoveEntry>()
			where e.Monster?.Creature != null && e.Monster.Creature.IsAlive && !_undoneMoves.Contains(e)
			select e).Reverse().Take(6).ToList();
		if (pastMoves.Count == 0)
		{
			await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
			{
				Source = this
			});
			return;
		}
		int currentRound = combatState.RoundNumber;
		List<(MoveState, Creature, string)> moves = pastMoves.Select((MonsterPerformedMoveEntry e) => (Move: e.Move, Creature: e.Monster.Creature, $"T-{Math.Max(1, currentRound - e.RoundNumber)}")).ToList();
		List<WatcherIntentProxy> proxies = WatcherIntentSelector.CreateProxiesFromMoves(base.Owner, moves);
		List<WatcherIntentProxy> source;
		try
		{
			source = await WatcherIntentSelector.PickFromProxies(choiceContext, base.Owner, proxies, 1, 1, new LocString("card_selection", "TO_DISCARD"));
		}
		finally
		{
			WatcherIntentSelector.DisposeProxiesPublic(base.Owner, proxies);
		}
		WatcherIntentProxy chosen = source.FirstOrDefault();
		if (chosen?.AttachedMove == null || chosen.AttachedEnemy == null)
		{
			await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
			{
				Source = this
			});
			return;
		}
		MonsterPerformedMoveEntry entry = pastMoves.FirstOrDefault((MonsterPerformedMoveEntry e) => e.Move == chosen.AttachedMove && e.Monster?.Creature == chosen.AttachedEnemy);
		if (entry == null)
		{
			await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
			{
				Source = this
			});
			return;
		}
		_undoneMoves.Add(entry);
		Creature player = base.Owner.Creature;
		int num = WatcherIntentTimeline.SumDamageDealtTo(entry, player);
		if (num > 0)
		{
			try
			{
				await CreatureCmd.Heal(player, num);
			}
			catch (Exception ex)
			{
				Log.Error("[Watcher] ControlPast heal failed: " + ex.Message);
			}
		}
		foreach (var (power, num2) in WatcherIntentTimeline.GetPowersAppliedTo(entry, player))
		{
			try
			{
				PowerModel power2 = player.GetPower(power.Id);
				if (power2 != null && num2 > 0m)
				{
					decimal num3 = power2.Amount;
					decimal num4 = -Math.Min(num2, num3);
					if (!(num3 + num4 <= 0m))
					{
						await PowerCmd.ModifyAmount(power2, num4, base.Owner.Creature, this);
					}
					else
					{
						await PowerCmd.Remove(power2);
					}
				}
			}
			catch (Exception ex2)
			{
				Log.Warn("[Watcher] ControlPast undo-power " + power.Id.Entry + " failed: " + ex2.Message);
			}
		}
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this,
			AffectedEnemy = chosen.AttachedEnemy,
			ChangedIntent = true
		});
	}

	protected override void OnUpgrade()
	{
		RemoveKeyword(CardKeyword.Exhaust);
	}
}
