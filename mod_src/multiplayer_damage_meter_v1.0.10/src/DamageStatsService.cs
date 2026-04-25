using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace MultiplayerDamageMeter;

public static class DamageStatsService
{
	private static readonly Dictionary<ulong, int> runTotals = new Dictionary<ulong, int>();

	private static readonly Dictionary<ulong, int> combatTotals = new Dictionary<ulong, int>();

	private static readonly Dictionary<string, EffectContributionLedger> effectLedgers = new Dictionary<string, EffectContributionLedger>(StringComparer.Ordinal);

	private static readonly Dictionary<string, PendingPoisonTickSnapshot> pendingPoisonTicks = new Dictionary<string, PendingPoisonTickSnapshot>(StringComparer.Ordinal);

	private static readonly Dictionary<string, PendingDoomKillSnapshot> pendingDoomKills = new Dictionary<string, PendingDoomKillSnapshot>(StringComparer.Ordinal);

	private static List<PlayerDamageSnapshot>? _lastCombatSummary;

	private static CombatState? _currentCombatState;

	private static uint _summaryVersion;

	private static readonly object persistenceLock = new object();

	private static DamageStatsRunStorageContext? _currentRunStorageContext;

	private static PendingRunStorageContext? _pendingRunStorageContext;

	private static PersistedActiveCombatState? _pendingCombatRestore;

	private static CombatSide? _pendingPoisonSide;

	private static bool _resetCombatTotalsOnResumeForCurrentCombat;

	private static bool _currentCombatTotalsCommitted;

	private const ValueProp PoisonDamageProps = ValueProp.Unblockable | ValueProp.Unpowered;

	public static event Action<ulong>? PlayerStatsChanged;

	public static event Action? RefreshRequested;

	public static void Initialize()
	{
		runTotals.Clear();
		combatTotals.Clear();
		effectLedgers.Clear();
		pendingPoisonTicks.Clear();
		pendingDoomKills.Clear();
		_lastCombatSummary = null;
		_currentCombatState = null;
		_summaryVersion = 0u;
		_currentRunStorageContext = null;
		_pendingRunStorageContext = null;
		_pendingCombatRestore = null;
		_pendingPoisonSide = null;
		_resetCombatTotalsOnResumeForCurrentCombat = false;
		_currentCombatTotalsCommitted = false;
	}

	public static void PrepareForUpcomingRun(RunState runState, long startTime, bool restoreRunTotals)
	{
		_pendingRunStorageContext = new PendingRunStorageContext(DamageStatsFileStore.CreateRunContext(runState, startTime), restoreRunTotals);
	}

	public static void OnRunStarted(RunState runState)
	{
		runTotals.Clear();
		combatTotals.Clear();
		effectLedgers.Clear();
		pendingPoisonTicks.Clear();
		pendingDoomKills.Clear();
		_lastCombatSummary = null;
		_currentCombatState = null;
		_currentRunStorageContext = null;
		_pendingCombatRestore = null;
		_pendingPoisonSide = null;
		_resetCombatTotalsOnResumeForCurrentCombat = false;
		_currentCombatTotalsCommitted = false;
		if (_pendingRunStorageContext is PendingRunStorageContext pendingContext)
		{
			_currentRunStorageContext = pendingContext.RunContext;
			if (pendingContext.ShouldRestoreRunTotals)
			{
				RestorePersistedRunState(runState, pendingContext.RunContext);
			}
		}

		_pendingRunStorageContext = null;
		_summaryVersion++;
		RefreshRequested?.Invoke();
		PersistCurrentRunState();
	}

	public static void FlushCurrentRunState()
	{
		PersistCurrentRunState();
	}

	public static void MarkCurrentCombatSavedForResumeReset()
	{
		if (_currentCombatState == null)
		{
			return;
		}

		_resetCombatTotalsOnResumeForCurrentCombat = true;
	}

	public static void MarkCurrentCombatSavedAsFinished()
	{
		if (_currentCombatState == null)
		{
			return;
		}

		_currentCombatState = null;
		CommitCurrentCombatTotalsToRunTotals();
		effectLedgers.Clear();
		pendingPoisonTicks.Clear();
		pendingDoomKills.Clear();
		_pendingPoisonSide = null;
		_pendingCombatRestore = null;
		_resetCombatTotalsOnResumeForCurrentCombat = false;
	}

	public static void MarkCurrentRunCompleted()
	{
		_resetCombatTotalsOnResumeForCurrentCombat = false;
		if (_currentRunStorageContext == null)
		{
			return;
		}

		lock (persistenceLock)
		{
			RunState? runState = GetRunStateForSummary();
			if (runState == null)
			{
				return;
			}

			DamageStatsFileStore.MarkRunCompleted(_currentRunStorageContext, runState);
		}
	}

	public static void OnCombatSetUp(CombatState state)
	{
		_currentCombatState = state;
		_lastCombatSummary = null;
		combatTotals.Clear();
		effectLedgers.Clear();
		pendingPoisonTicks.Clear();
		pendingDoomKills.Clear();
		_pendingPoisonSide = null;
		_resetCombatTotalsOnResumeForCurrentCombat = false;
		_currentCombatTotalsCommitted = false;
		if (_pendingCombatRestore != null)
		{
			RestorePersistedCombatState(state, _pendingCombatRestore);
			_pendingCombatRestore = null;
		}

		_summaryVersion++;
		RefreshRequested?.Invoke();
		PersistCurrentRunState();
	}

	public static void OnCombatEnded(CombatRoom _)
	{
		CommitCurrentCombatTotalsToRunTotals();
		RunState? runState = GetRunStateForSummary();
		_lastCombatSummary = CreateSnapshots(runState);
		_currentCombatState = null;
		combatTotals.Clear();
		effectLedgers.Clear();
		pendingPoisonTicks.Clear();
		pendingDoomKills.Clear();
		_pendingPoisonSide = null;
		_pendingCombatRestore = null;
		_resetCombatTotalsOnResumeForCurrentCombat = false;
		uint version = ++_summaryVersion;
		RefreshRequested?.Invoke();
		PersistCurrentRunState();
		TryShowSummaryPopupNextFrame(version);
	}

	public static void RegisterDamage(Creature? dealer, CardModel? cardSource, DamageResult result, ValueProp props, Creature target)
	{
		int countedDamage = result.BlockedDamage + result.UnblockedDamage;
		if (countedDamage <= 0)
		{
			return;
		}

		if (TryRegisterPoisonDamage(dealer, cardSource, result, props, target, countedDamage))
		{
			return;
		}

		Player? player = ResolvePlayerOwner(dealer, cardSource);
		if (player == null)
		{
			return;
		}

		AddDamageAssignments(new Dictionary<ulong, int>(1) { [player.NetId] = countedDamage });
	}

	public static void RegisterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (amount == 0m || _currentCombatState == null || !TryGetEffectKind(power, out DamageEffectKind effectKind))
		{
			return;
		}

		if (!TryCreateTargetKey(power.Owner, _currentCombatState, out string targetKey))
		{
			return;
		}

		EffectContributionLedger ledger = GetOrCreateLedger(effectKind, targetKey);
		int delta = (int)amount;
		if (delta > 0)
		{
			Player? contributor = ResolvePlayerOwner(applier, cardSource);
			ledger.AddContribution(contributor?.NetId, delta);
		}
		else
		{
			ApplyLedgerReduction(ledger, -delta, power.Owner.CombatState);
		}

		ReconcileLedgerToCurrentPowerAmount(ledger, power.Amount, power.Owner.CombatState);
		if (power.Amount <= 0 || ledger.IsEmpty)
		{
			effectLedgers.Remove(ledger.CompositeKey);
		}

		PersistCurrentRunState();
	}

	public static void CapturePendingPoisonTicks(CombatSide side, CombatState combatState)
	{
		pendingPoisonTicks.Clear();
		pendingDoomKills.Clear();
		_pendingPoisonSide = side;
		IReadOnlyList<Creature> creatures = combatState.GetCreaturesOnSide(side);
		for (int i = 0; i < creatures.Count; i++)
		{
			Creature target = creatures[i];
			if (!target.IsAlive)
			{
				continue;
			}

			PoisonPower? poison = target.GetPower<PoisonPower>();
			if (poison == null || poison.Amount <= 0)
			{
				continue;
			}

			int triggerCount = GetPoisonTriggerCount(poison);
			if (triggerCount <= 0)
			{
				continue;
			}

			if (!TryCreateTargetKey(target, combatState, out string targetKey))
			{
				continue;
			}

			pendingPoisonTicks[targetKey] = new PendingPoisonTickSnapshot(targetKey, triggerCount);
		}

		PersistCurrentRunState();
	}

	public static void FinalizePendingPoisonTicks(CombatSide side)
	{
		if (_pendingPoisonSide != side)
		{
			return;
		}

		pendingPoisonTicks.Clear();
		_pendingPoisonSide = null;
		PersistCurrentRunState();
	}

	public static void CapturePendingDoomKills(IReadOnlyList<Creature> creatures)
	{
		pendingDoomKills.Clear();
		CombatState? combatState = _currentCombatState ?? creatures.FirstOrDefault(static creature => creature.CombatState != null)?.CombatState;
		if (combatState == null)
		{
			return;
		}

		for (int i = 0; i < creatures.Count; i++)
		{
			Creature target = creatures[i];
			if (target.CurrentHp <= 0 || !TryCreateTargetKey(target, combatState, out string targetKey))
			{
				continue;
			}

			DoomPower? doom = target.GetPower<DoomPower>();
			if (doom == null)
			{
				continue;
			}

			EffectContributionLedger ledger = GetOrCreateLedger(DamageEffectKind.Doom, targetKey);
			ReconcileLedgerToCurrentPowerAmount(ledger, doom.Amount, combatState);
			pendingDoomKills[targetKey] = new PendingDoomKillSnapshot(targetKey, target.CurrentHp, ledger.ClonePlayerContributions(), ledger.UnattributedAmount);
		}

		PersistCurrentRunState();
	}

	public static void RegisterDoomKill(IReadOnlyList<Creature> creatures)
	{
		if (_currentCombatState == null)
		{
			pendingDoomKills.Clear();
			PersistCurrentRunState();
			return;
		}

		for (int i = 0; i < creatures.Count; i++)
		{
			Creature target = creatures[i];
			if (!TryCreateTargetKey(target, _currentCombatState, out string targetKey))
			{
				continue;
			}

			if (pendingDoomKills.TryGetValue(targetKey, out PendingDoomKillSnapshot? snapshot))
			{
				Dictionary<ulong, int> assignments = AllocateProportionalAmounts(snapshot.PlayerContributions, snapshot.UnattributedAmount, snapshot.CountedDamage, _currentCombatState);
				AddDamageAssignments(assignments);
			}

			effectLedgers.Remove(CreateCompositeLedgerKey(DamageEffectKind.Doom, targetKey));
		}

		pendingDoomKills.Clear();
		PersistCurrentRunState();
	}

	public static void ClearTargetLedgers(Creature creature)
	{
		CombatState? combatState = _currentCombatState ?? creature.CombatState;
		if (combatState == null || !TryCreateTargetKey(creature, combatState, out string targetKey))
		{
			return;
		}

		effectLedgers.Remove(CreateCompositeLedgerKey(DamageEffectKind.Poison, targetKey));
		effectLedgers.Remove(CreateCompositeLedgerKey(DamageEffectKind.Doom, targetKey));
		pendingPoisonTicks.Remove(targetKey);
		pendingDoomKills.Remove(targetKey);
		PersistCurrentRunState();
	}

	public static int GetRunTotal(ulong playerId)
	{
		if (runTotals.TryGetValue(playerId, out int value))
		{
			return value;
		}

		return 0;
	}

	public static int GetCombatTotal(ulong playerId)
	{
		if (combatTotals.TryGetValue(playerId, out int value))
		{
			return value;
		}

		return 0;
	}

	public static bool IsMultiplayerRun()
	{
		RunState? runState = RunManager.Instance.DebugOnlyGetState();
		return runState != null && runState.Players.Count > 1;
	}

	private static RunState? GetRunStateForSummary()
	{
		if (_currentCombatState?.RunState is RunState runState)
		{
			return runState;
		}

		return RunManager.Instance.DebugOnlyGetState();
	}

	private static List<PlayerDamageSnapshot> CreateSnapshots(RunState? runState)
	{
		List<PlayerDamageSnapshot> snapshots = new List<PlayerDamageSnapshot>();
		if (runState == null)
		{
			return snapshots;
		}

		for (int i = 0; i < runState.Players.Count; i++)
		{
			Player player = runState.Players[i];
			snapshots.Add(new PlayerDamageSnapshot(player.NetId, PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, player.NetId), GetCombatTotal(player.NetId), GetRunTotal(player.NetId), i));
		}

		return snapshots;
	}

	private static async void TryShowSummaryPopupNextFrame(uint summaryVersion)
	{
		NGame? game = NGame.Instance;
		if (game == null)
		{
			return;
		}

		SceneTree? tree = game.GetTree();
		if (tree == null)
		{
			return;
		}

		await game.ToSignal(tree, SceneTree.SignalName.ProcessFrame);

		if (summaryVersion != _summaryVersion)
		{
			return;
		}

		if (!IsMultiplayerRun())
		{
			return;
		}

		List<PlayerDamageSnapshot>? summary = _lastCombatSummary;
		if (summary == null || !summary.Any(static snapshot => snapshot.CombatDamage > 0))
		{
			return;
		}

		NModalContainer? modalContainer = NModalContainer.Instance;
		if (modalContainer == null || modalContainer.OpenModal != null)
		{
			return;
		}

		modalContainer.Add(new DamageSummaryPopup(summary), showBackstop: true);
	}

	private static bool TryRegisterPoisonDamage(Creature? dealer, CardModel? cardSource, DamageResult result, ValueProp props, Creature target, int countedDamage)
	{
		if (_pendingPoisonSide == null || dealer != null || cardSource != null || !props.HasFlag(ValueProp.Unblockable) || !props.HasFlag(ValueProp.Unpowered))
		{
			return false;
		}

		CombatState? combatState = _currentCombatState ?? target.CombatState;
		if (combatState == null || !TryCreateTargetKey(target, combatState, out string targetKey))
		{
			return false;
		}

		if (!pendingPoisonTicks.TryGetValue(targetKey, out PendingPoisonTickSnapshot? snapshot))
		{
			return false;
		}

		PoisonPower? poison = target.GetPower<PoisonPower>();
		if (poison == null || poison.Amount <= 0)
		{
			return false;
		}

		if (snapshot.RemainingHitCount <= 1)
		{
			pendingPoisonTicks.Remove(targetKey);
		}
		else
		{
			pendingPoisonTicks[targetKey] = snapshot with { RemainingHitCount = snapshot.RemainingHitCount - 1 };
		}

		EffectContributionLedger ledger = GetOrCreateLedger(DamageEffectKind.Poison, targetKey);
		ReconcileLedgerToCurrentPowerAmount(ledger, poison.Amount, combatState);
		Dictionary<ulong, int> assignments = AllocateDamageFromLedger(ledger, countedDamage, combatState);
		AddDamageAssignments(assignments);

		return true;
	}

	private static Dictionary<ulong, int> AllocateDamageFromLedger(EffectContributionLedger ledger, int countedDamage, CombatState? combatState)
	{
		if (countedDamage <= 0)
		{
			return EmptyAssignments();
		}

		return AllocateProportionalAmounts(ledger.PlayerContributions, ledger.UnattributedAmount, countedDamage, combatState);
	}

	private static Dictionary<ulong, int> AllocateProportionalAmounts(Dictionary<ulong, int> playerWeights, int unattributedWeight, int totalAmount, CombatState? combatState)
	{
		if (totalAmount <= 0)
		{
			return EmptyAssignments();
		}

		List<AllocationBucket> buckets = new List<AllocationBucket>(playerWeights.Count + (unattributedWeight > 0 ? 1 : 0));
		Dictionary<ulong, int> playerOrder = GetPlayerOrderLookup(combatState);
		foreach (KeyValuePair<ulong, int> pair in playerWeights)
		{
			if (pair.Value <= 0)
			{
				continue;
			}

			int order = playerOrder.TryGetValue(pair.Key, out int playerIndex) ? playerIndex : int.MaxValue;
			buckets.Add(new AllocationBucket(pair.Key, pair.Value, order));
		}

		if (unattributedWeight > 0)
		{
			buckets.Add(new AllocationBucket(null, unattributedWeight, -1));
		}

		if (buckets.Count == 0)
		{
			return EmptyAssignments();
		}

		int totalWeight = buckets.Sum(static bucket => bucket.Weight);
		if (totalWeight <= 0)
		{
			return EmptyAssignments();
		}

		int clampedAmount = Math.Min(totalAmount, totalWeight > 0 ? int.MaxValue : totalAmount);
		List<AllocationResult> allocations = new List<AllocationResult>(buckets.Count);
		int assignedAmount = 0;
		for (int i = 0; i < buckets.Count; i++)
		{
			AllocationBucket bucket = buckets[i];
			long weightedAmount = (long)clampedAmount * bucket.Weight;
			int baseAmount = (int)(weightedAmount / totalWeight);
			long remainder = weightedAmount % totalWeight;
			allocations.Add(new AllocationResult(bucket.PlayerId, bucket.Order, baseAmount, remainder));
			assignedAmount += baseAmount;
		}

		int remaining = clampedAmount - assignedAmount;
		if (remaining > 0)
		{
			allocations.Sort(static (left, right) =>
			{
				int remainderComparison = right.Remainder.CompareTo(left.Remainder);
				if (remainderComparison != 0)
				{
					return remainderComparison;
				}

				return left.Order.CompareTo(right.Order);
			});

			for (int i = 0; i < remaining && i < allocations.Count; i++)
			{
				allocations[i] = allocations[i] with { Amount = allocations[i].Amount + 1 };
			}
		}

		Dictionary<ulong, int> assignments = new Dictionary<ulong, int>();
		for (int i = 0; i < allocations.Count; i++)
		{
			AllocationResult allocation = allocations[i];
			if (allocation.PlayerId is ulong playerId && allocation.Amount > 0)
			{
				assignments[playerId] = allocation.Amount;
			}
		}

		return assignments;
	}

	private static void ApplyLedgerReduction(EffectContributionLedger ledger, int reductionAmount, CombatState? combatState)
	{
		if (reductionAmount <= 0 || ledger.IsEmpty)
		{
			return;
		}

		List<AllocationBucket> buckets = BuildReductionBuckets(ledger, combatState);
		if (buckets.Count == 0)
		{
			return;
		}

		int totalWeight = buckets.Sum(static bucket => bucket.Weight);
		if (totalWeight <= 0)
		{
			return;
		}

		int clampedReduction = Math.Min(reductionAmount, totalWeight);
		List<AllocationResult> reductions = new List<AllocationResult>(buckets.Count);
		int assignedAmount = 0;
		for (int i = 0; i < buckets.Count; i++)
		{
			AllocationBucket bucket = buckets[i];
			long weightedReduction = (long)clampedReduction * bucket.Weight;
			int baseAmount = (int)(weightedReduction / totalWeight);
			long remainder = weightedReduction % totalWeight;
			reductions.Add(new AllocationResult(bucket.PlayerId, bucket.Order, baseAmount, remainder));
			assignedAmount += baseAmount;
		}

		int remaining = clampedReduction - assignedAmount;
		if (remaining > 0)
		{
			reductions.Sort(static (left, right) =>
			{
				int remainderComparison = right.Remainder.CompareTo(left.Remainder);
				if (remainderComparison != 0)
				{
					return remainderComparison;
				}

				return left.Order.CompareTo(right.Order);
			});

			for (int i = 0; i < remaining && i < reductions.Count; i++)
			{
				reductions[i] = reductions[i] with { Amount = reductions[i].Amount + 1 };
			}
		}

		for (int i = 0; i < reductions.Count; i++)
		{
			AllocationResult reduction = reductions[i];
			if (reduction.Amount <= 0)
			{
				continue;
			}

			if (reduction.PlayerId is ulong playerId)
			{
				ledger.SubtractPlayerContribution(playerId, reduction.Amount);
			}
			else
			{
				ledger.SubtractUnattributed(reduction.Amount);
			}
		}
	}

	private static List<AllocationBucket> BuildReductionBuckets(EffectContributionLedger ledger, CombatState? combatState)
	{
		List<AllocationBucket> buckets = new List<AllocationBucket>(ledger.PlayerContributions.Count + (ledger.UnattributedAmount > 0 ? 1 : 0));
		Dictionary<ulong, int> playerOrder = GetPlayerOrderLookup(combatState);
		foreach (KeyValuePair<ulong, int> pair in ledger.PlayerContributions)
		{
			if (pair.Value <= 0)
			{
				continue;
			}

			int order = playerOrder.TryGetValue(pair.Key, out int playerIndex) ? playerIndex : int.MaxValue;
			buckets.Add(new AllocationBucket(pair.Key, pair.Value, order));
		}

		if (ledger.UnattributedAmount > 0)
		{
			buckets.Add(new AllocationBucket(null, ledger.UnattributedAmount, -1));
		}

		return buckets;
	}

	private static Dictionary<ulong, int> GetPlayerOrderLookup(CombatState? combatState)
	{
		Dictionary<ulong, int> order = new Dictionary<ulong, int>();
		IReadOnlyList<Player>? players = combatState?.Players;
		if (players == null || players.Count == 0)
		{
			players = GetRunStateForSummary()?.Players;
		}

		if (players == null)
		{
			return order;
		}

		for (int i = 0; i < players.Count; i++)
		{
			order[players[i].NetId] = i;
		}

		return order;
	}

	private static void AddDamageAssignments(Dictionary<ulong, int> assignments)
	{
		if (assignments.Count == 0)
		{
			return;
		}

		List<ulong> changedPlayers = new List<ulong>(assignments.Count);
		foreach (KeyValuePair<ulong, int> pair in assignments)
		{
			if (pair.Value <= 0)
			{
				continue;
			}

			ulong playerId = pair.Key;
			if (_currentCombatState != null)
			{
				combatTotals[playerId] = GetCombatTotal(playerId) + pair.Value;
			}
			else
			{
				runTotals[playerId] = GetRunTotal(playerId) + pair.Value;
			}

			changedPlayers.Add(playerId);
		}

		if (changedPlayers.Count == 0)
		{
			return;
		}

		PersistCurrentRunState();
		Dictionary<ulong, int> order = GetPlayerOrderLookup(_currentCombatState);
		changedPlayers.Sort((left, right) =>
		{
			int leftOrder = order.TryGetValue(left, out int leftIndex) ? leftIndex : int.MaxValue;
			int rightOrder = order.TryGetValue(right, out int rightIndex) ? rightIndex : int.MaxValue;
			return leftOrder.CompareTo(rightOrder);
		});
		for (int i = 0; i < changedPlayers.Count; i++)
		{
			PlayerStatsChanged?.Invoke(changedPlayers[i]);
		}
	}

	private static Player? ResolvePlayerOwner(Creature? dealer, CardModel? cardSource)
	{
		if (dealer?.Player is Player playerDealer)
		{
			return playerDealer;
		}

		if (dealer?.PetOwner is Player petOwner)
		{
			return petOwner;
		}

		return cardSource?.Owner;
	}

	private static bool TryGetEffectKind(PowerModel power, out DamageEffectKind effectKind)
	{
		if (power is PoisonPower)
		{
			effectKind = DamageEffectKind.Poison;
			return true;
		}

		if (power is DoomPower)
		{
			effectKind = DamageEffectKind.Doom;
			return true;
		}

		effectKind = default;
		return false;
	}

	private static int GetPoisonTriggerCount(PoisonPower poison)
	{
		CombatState? combatState = poison.Owner.CombatState;
		if (combatState == null)
		{
			return 0;
		}

		int accelerantAmount = combatState.GetOpponentsOf(poison.Owner).Where(static creature => creature.IsAlive).Sum(static creature => creature.GetPowerAmount<AccelerantPower>());
		return Math.Min(poison.Amount, 1 + accelerantAmount);
	}

	private static EffectContributionLedger GetOrCreateLedger(DamageEffectKind effectKind, string targetKey)
	{
		string compositeKey = CreateCompositeLedgerKey(effectKind, targetKey);
		if (!effectLedgers.TryGetValue(compositeKey, out EffectContributionLedger? ledger))
		{
			ledger = new EffectContributionLedger(effectKind, targetKey);
			effectLedgers[compositeKey] = ledger;
		}

		return ledger;
	}

	private static void ReconcileLedgerToCurrentPowerAmount(EffectContributionLedger ledger, int currentPowerAmount, CombatState? combatState)
	{
		if (currentPowerAmount <= 0)
		{
			effectLedgers.Remove(ledger.CompositeKey);
			return;
		}

		int totalTrackedAmount = ledger.TotalContributionAmount;
		if (totalTrackedAmount < currentPowerAmount)
		{
			ledger.AddContribution(null, currentPowerAmount - totalTrackedAmount);
			return;
		}

		if (totalTrackedAmount > currentPowerAmount)
		{
			ApplyLedgerReduction(ledger, totalTrackedAmount - currentPowerAmount, combatState);
		}
	}

	private static string CreateCompositeLedgerKey(DamageEffectKind effectKind, string targetKey)
	{
		return effectKind + "|" + targetKey;
	}

	private static bool TryCreateTargetKey(Creature target, CombatState? combatState, out string targetKey)
	{
		if (target.IsPlayer)
		{
			targetKey = "player:" + target.Player!.NetId.ToString(CultureInfo.InvariantCulture);
			return true;
		}

		if (combatState == null)
		{
			targetKey = string.Empty;
			return false;
		}

		if (target.PetOwner is Player petOwner)
		{
			string modelId = target.ModelId.ToString();
			int ordinal = 0;
			IReadOnlyList<Creature> creatures = combatState.Creatures;
			for (int i = 0; i < creatures.Count; i++)
			{
				Creature candidate = creatures[i];
				if (candidate == target)
				{
					break;
				}

				if (candidate.PetOwner?.NetId == petOwner.NetId && candidate.ModelId == target.ModelId)
				{
					ordinal++;
				}
			}

			targetKey = "pet:" + petOwner.NetId.ToString(CultureInfo.InvariantCulture) + ":" + modelId + ":" + ordinal.ToString(CultureInfo.InvariantCulture);
			return true;
		}

		string normalizedSlotName = target.SlotName ?? string.Empty;
		int enemyOrdinal = 0;
		IReadOnlyList<Creature> enemies = combatState.Enemies;
		for (int i = 0; i < enemies.Count; i++)
		{
			Creature candidate = enemies[i];
			if (candidate == target)
			{
				break;
			}

			if (candidate.ModelId == target.ModelId && string.Equals(candidate.SlotName ?? string.Empty, normalizedSlotName, StringComparison.Ordinal))
			{
				enemyOrdinal++;
			}
		}

		targetKey = "enemy:" + target.ModelId + ":" + normalizedSlotName + ":" + enemyOrdinal.ToString(CultureInfo.InvariantCulture);
		return true;
	}

	private static void PersistCurrentRunState()
	{
		if (_currentRunStorageContext == null)
		{
			return;
		}

		lock (persistenceLock)
		{
			try
			{
				PersistedRunSnapshot? snapshot = CreatePersistedRunSnapshot();
				if (snapshot == null)
				{
					return;
				}

				DamageStatsFileStore.SaveSnapshot(_currentRunStorageContext, snapshot);
			}
			catch (Exception exception)
			{
				Log.Error($"Failed to persist damage stats for run {_currentRunStorageContext.RunId}. {exception}");
			}
		}
	}

	private static void CommitCurrentCombatTotalsToRunTotals()
	{
		if (_currentCombatTotalsCommitted)
		{
			return;
		}

		foreach (KeyValuePair<ulong, int> pair in combatTotals)
		{
			if (pair.Value <= 0)
			{
				continue;
			}

			runTotals[pair.Key] = GetRunTotal(pair.Key) + pair.Value;
		}

		_currentCombatTotalsCommitted = true;
	}

	private static PersistedRunSnapshot? CreatePersistedRunSnapshot()
	{
		if (_currentRunStorageContext == null)
		{
			return null;
		}

		RunState? runState = GetRunStateForSummary();
		if (runState == null)
		{
			return null;
		}

		return new PersistedRunSnapshot
		{
			Metadata = DamageStatsFileStore.CreateMetadata(runState, _currentRunStorageContext),
			StatsSchemaVersion = DamageStatsFileStore.CurrentStatsSchemaVersion,
			LastUpdatedUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
			RunTotals = runTotals.ToDictionary(static pair => pair.Key.ToString(CultureInfo.InvariantCulture), static pair => pair.Value),
			IsCombatActive = _currentCombatState != null,
			ResetCombatTotalsOnResume = _resetCombatTotalsOnResumeForCurrentCombat && _currentCombatState != null,
			ActiveCombat = CreatePersistedCombatState()
		};
	}

	private static PersistedActiveCombatState? CreatePersistedCombatState()
	{
		if (_currentCombatState == null)
		{
			return null;
		}

		PruneInvalidLedgers(_currentCombatState);
		PersistedActiveCombatState persistedCombat = new PersistedActiveCombatState
		{
			CombatTotals = combatTotals.ToDictionary(static pair => pair.Key.ToString(CultureInfo.InvariantCulture), static pair => pair.Value)
		};
		foreach (EffectContributionLedger ledger in effectLedgers.Values)
		{
			if (!ledger.IsEmpty)
			{
				persistedCombat.CombatEffectLedgers.Add(ledger.ToPersisted());
			}
		}

		foreach (KeyValuePair<string, PendingPoisonTickSnapshot> pair in pendingPoisonTicks)
		{
			persistedCombat.PendingPoisonTicks[pair.Key] = pair.Value;
		}

		foreach (KeyValuePair<string, PendingDoomKillSnapshot> pair in pendingDoomKills)
		{
			persistedCombat.PendingDoomKills[pair.Key] = new PendingDoomKillSnapshot(pair.Value.TargetKey, pair.Value.CountedDamage, new Dictionary<ulong, int>(pair.Value.PlayerContributions), pair.Value.UnattributedAmount);
		}

		persistedCombat.PendingPoisonSide = _pendingPoisonSide;
		return persistedCombat;
	}

	private static void RestorePersistedRunState(RunState runState, DamageStatsRunStorageContext context)
	{
		lock (persistenceLock)
		{
			PersistedRunSnapshot? persisted;
			try
			{
				persisted = DamageStatsFileStore.LoadSnapshot(context, runState);
			}
			catch (Exception exception)
			{
				Log.Error($"Failed to restore damage stats for run {context.RunId}. {exception}");
				return;
			}

			if (persisted == null)
			{
				return;
			}

			runTotals.Clear();
			combatTotals.Clear();
			effectLedgers.Clear();
			pendingPoisonTicks.Clear();
			pendingDoomKills.Clear();
			_pendingPoisonSide = null;
			_pendingCombatRestore = null;
			_resetCombatTotalsOnResumeForCurrentCombat = false;
			_currentCombatTotalsCommitted = false;
			for (int i = 0; i < runState.Players.Count; i++)
			{
				Player player = runState.Players[i];
				if (persisted.RunTotals.TryGetValue(player.NetId.ToString(CultureInfo.InvariantCulture), out int value) && value > 0)
				{
					runTotals[player.NetId] = value;
				}
			}

			if (persisted.IsCombatActive)
			{
				if (persisted.StatsSchemaVersion < DamageStatsFileStore.CurrentStatsSchemaVersion && persisted.ActiveCombat != null)
				{
					SubtractCombatTotalsFromRunTotals(runState, persisted.ActiveCombat.CombatTotals);
				}

				return;
			}

			_pendingCombatRestore = null;
		}
	}

	private static void RestorePersistedCombatState(CombatState combatState, PersistedActiveCombatState persistedCombat)
	{
		combatTotals.Clear();
		effectLedgers.Clear();
		pendingPoisonTicks.Clear();
		pendingDoomKills.Clear();
		_pendingPoisonSide = persistedCombat.PendingPoisonSide;
		IReadOnlyList<Player> players = combatState.Players;
		for (int i = 0; i < players.Count; i++)
		{
			Player player = players[i];
			if (persistedCombat.CombatTotals.TryGetValue(player.NetId.ToString(CultureInfo.InvariantCulture), out int value) && value > 0)
			{
				combatTotals[player.NetId] = value;
			}
		}

		HashSet<string> validTargetKeys = BuildValidTargetKeySet(combatState);
		for (int i = 0; i < persistedCombat.CombatEffectLedgers.Count; i++)
		{
			PersistedCombatLedger persistedLedger = persistedCombat.CombatEffectLedgers[i];
			if (string.IsNullOrEmpty(persistedLedger.TargetKey) || !validTargetKeys.Contains(persistedLedger.TargetKey))
			{
				continue;
			}

			EffectContributionLedger ledger = new EffectContributionLedger(persistedLedger.EffectKind, persistedLedger.TargetKey);
			foreach (KeyValuePair<string, int> pair in persistedLedger.PlayerContributions)
			{
				if (!ulong.TryParse(pair.Key, NumberStyles.None, CultureInfo.InvariantCulture, out ulong playerId) || pair.Value <= 0)
				{
					continue;
				}

				if (players.Any(player => player.NetId == playerId))
				{
					ledger.AddContribution(playerId, pair.Value);
				}
			}

			if (persistedLedger.UnattributedAmount > 0)
			{
				ledger.AddContribution(null, persistedLedger.UnattributedAmount);
			}

			if (!ledger.IsEmpty)
			{
				effectLedgers[ledger.CompositeKey] = ledger;
			}
		}

		foreach (KeyValuePair<string, PendingPoisonTickSnapshot> pair in persistedCombat.PendingPoisonTicks)
		{
			if (validTargetKeys.Contains(pair.Key))
			{
				pendingPoisonTicks[pair.Key] = pair.Value with { TargetKey = pair.Key };
			}
		}

		foreach (KeyValuePair<string, PendingDoomKillSnapshot> pair in persistedCombat.PendingDoomKills)
		{
			if (validTargetKeys.Contains(pair.Key))
			{
				pendingDoomKills[pair.Key] = new PendingDoomKillSnapshot(pair.Key, pair.Value.CountedDamage, pair.Value.PlayerContributions.Where(static entry => entry.Value > 0).ToDictionary(static entry => entry.Key, static entry => entry.Value), pair.Value.UnattributedAmount);
			}
		}

		PruneInvalidLedgers(combatState);
	}

	private static void SubtractCombatTotalsFromRunTotals(RunState runState, IReadOnlyDictionary<string, int> persistedCombatTotals)
	{
		for (int i = 0; i < runState.Players.Count; i++)
		{
			Player player = runState.Players[i];
			if (!persistedCombatTotals.TryGetValue(player.NetId.ToString(CultureInfo.InvariantCulture), out int combatValue) || combatValue <= 0)
			{
				continue;
			}

			int adjustedRunTotal = GetRunTotal(player.NetId) - combatValue;
			if (adjustedRunTotal > 0)
			{
				runTotals[player.NetId] = adjustedRunTotal;
			}
			else
			{
				runTotals.Remove(player.NetId);
			}
		}
	}

	private static void PruneInvalidLedgers(CombatState combatState)
	{
		HashSet<string> validTargetKeys = BuildValidTargetKeySet(combatState);
		foreach (string compositeKey in effectLedgers.Where(pair => pair.Value.IsEmpty || !validTargetKeys.Contains(pair.Value.TargetKey)).Select(static pair => pair.Key).ToArray())
		{
			effectLedgers.Remove(compositeKey);
		}
	}

	private static HashSet<string> BuildValidTargetKeySet(CombatState combatState)
	{
		HashSet<string> targetKeys = new HashSet<string>(StringComparer.Ordinal);
		IReadOnlyList<Creature> creatures = combatState.Creatures;
		for (int i = 0; i < creatures.Count; i++)
		{
			if (TryCreateTargetKey(creatures[i], combatState, out string targetKey))
			{
				targetKeys.Add(targetKey);
			}
		}

		return targetKeys;
	}

	private static Dictionary<ulong, int> EmptyAssignments()
	{
		return new Dictionary<ulong, int>();
	}
}

public sealed record PlayerDamageSnapshot(ulong PlayerId, string PlayerName, int CombatDamage, int RunDamage, int PlayerOrder);

internal enum DamageEffectKind
{
	Poison,
	Doom
}

internal sealed record PendingRunStorageContext(DamageStatsRunStorageContext RunContext, bool ShouldRestoreRunTotals);

internal sealed record PendingPoisonTickSnapshot(string TargetKey, int RemainingHitCount);

internal sealed record PendingDoomKillSnapshot(string TargetKey, int CountedDamage, Dictionary<ulong, int> PlayerContributions, int UnattributedAmount);

internal readonly record struct AllocationBucket(ulong? PlayerId, int Weight, int Order);

internal readonly record struct AllocationResult(ulong? PlayerId, int Order, int Amount, long Remainder);

internal sealed class EffectContributionLedger
{
	public EffectContributionLedger(DamageEffectKind effectKind, string targetKey)
	{
		EffectKind = effectKind;
		TargetKey = targetKey;
	}

	public DamageEffectKind EffectKind { get; }

	public string TargetKey { get; }

	public Dictionary<ulong, int> PlayerContributions { get; } = new Dictionary<ulong, int>();

	public int UnattributedAmount { get; private set; }

	public int TotalContributionAmount => UnattributedAmount + PlayerContributions.Values.Sum();

	public bool IsEmpty => TotalContributionAmount <= 0;

	public string CompositeKey => EffectKind + "|" + TargetKey;

	public void AddContribution(ulong? playerId, int amount)
	{
		if (amount <= 0)
		{
			return;
		}

		if (playerId is ulong value)
		{
			PlayerContributions[value] = GetPlayerContribution(value) + amount;
			return;
		}

		UnattributedAmount += amount;
	}

	public void SubtractPlayerContribution(ulong playerId, int amount)
	{
		if (amount <= 0 || !PlayerContributions.TryGetValue(playerId, out int currentAmount))
		{
			return;
		}

		int newAmount = Math.Max(0, currentAmount - amount);
		if (newAmount == 0)
		{
			PlayerContributions.Remove(playerId);
		}
		else
		{
			PlayerContributions[playerId] = newAmount;
		}
	}

	public void SubtractUnattributed(int amount)
	{
		if (amount <= 0)
		{
			return;
		}

		UnattributedAmount = Math.Max(0, UnattributedAmount - amount);
	}

	public int GetPlayerContribution(ulong playerId)
	{
		if (PlayerContributions.TryGetValue(playerId, out int value))
		{
			return value;
		}

		return 0;
	}

	public Dictionary<ulong, int> ClonePlayerContributions()
	{
		return PlayerContributions.Where(static pair => pair.Value > 0).ToDictionary(static pair => pair.Key, static pair => pair.Value);
	}

	public PersistedCombatLedger ToPersisted()
	{
		return new PersistedCombatLedger
		{
			EffectKind = EffectKind,
			TargetKey = TargetKey,
			PlayerContributions = PlayerContributions.ToDictionary(static pair => pair.Key.ToString(CultureInfo.InvariantCulture), static pair => pair.Value),
			UnattributedAmount = UnattributedAmount
		};
	}
}

internal sealed class PersistedActiveCombatState
{
	public Dictionary<string, int> CombatTotals { get; set; } = new Dictionary<string, int>();

	public List<PersistedCombatLedger> CombatEffectLedgers { get; set; } = new List<PersistedCombatLedger>();

	public Dictionary<string, PendingPoisonTickSnapshot> PendingPoisonTicks { get; set; } = new Dictionary<string, PendingPoisonTickSnapshot>(StringComparer.Ordinal);

	public Dictionary<string, PendingDoomKillSnapshot> PendingDoomKills { get; set; } = new Dictionary<string, PendingDoomKillSnapshot>(StringComparer.Ordinal);

	public CombatSide? PendingPoisonSide { get; set; }
}

internal sealed class PersistedCombatLedger
{
	public DamageEffectKind EffectKind { get; set; }

	public string TargetKey { get; set; } = string.Empty;

	public Dictionary<string, int> PlayerContributions { get; set; } = new Dictionary<string, int>();

	public int UnattributedAmount { get; set; }
}
