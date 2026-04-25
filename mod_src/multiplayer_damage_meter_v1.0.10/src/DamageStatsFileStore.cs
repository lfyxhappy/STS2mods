using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace MultiplayerDamageMeter;

internal static class DamageStatsFileStore
{
	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions();

	private const string StorageRootDirectoryName = "multiplayer_damage_meter";

	private const string RunsDirectoryName = "runs";

	private const string StateFileName = "state.json";

	private const string BackupFileName = "state.bak";

	private const string TempFileName = "state.tmp";

	private const string ActiveStatus = "active";

	private const string CompletedStatus = "completed";

	public const int CurrentStatsSchemaVersion = 2;

	public static DamageStatsRunStorageContext CreateRunContext(RunState runState, long startTime)
	{
		string cacheKey = CreateRunCacheKey(runState, startTime);
		string modeDirectoryName = runState.Players.Count > 1 ? "multi" : "single";
		string runId = startTime.ToString(CultureInfo.InvariantCulture) + "_" + ComputeShortHash(cacheKey);
		string relativePath = Path.Combine(UserDataPathProvider.SavesDir, StorageRootDirectoryName, RunsDirectoryName, modeDirectoryName, runId).Replace('\\', '/');
		string godotDirectoryPath = SaveManager.Instance.GetProfileScopedPath(relativePath);
		string directoryPath = ProjectSettings.GlobalizePath(godotDirectoryPath);
		return new DamageStatsRunStorageContext(modeDirectoryName, startTime, cacheKey, runId, directoryPath, Path.Combine(directoryPath, StateFileName), Path.Combine(directoryPath, BackupFileName), Path.Combine(directoryPath, TempFileName));
	}

	public static PersistedRunMetadata CreateMetadata(RunState runState, DamageStatsRunStorageContext context)
	{
		return new PersistedRunMetadata
		{
			Mode = context.ModeDirectoryName,
			StartTime = context.StartTime,
			CacheKey = context.CacheKey,
			Seed = runState.Rng.StringSeed,
			GameMode = runState.GameMode.ToString(),
			AscensionLevel = runState.AscensionLevel,
			Players = runState.Players.Select(static player => new PersistedRunPlayerMetadata
			{
				NetId = player.NetId,
				CharacterId = player.Character.Id.ToString()
			}).ToList()
		};
	}

	public static PersistedRunSnapshot CreateEmptySnapshot(RunState runState, DamageStatsRunStorageContext context)
	{
		return new PersistedRunSnapshot
		{
			Metadata = CreateMetadata(runState, context),
			StatsSchemaVersion = CurrentStatsSchemaVersion,
			RunStatus = ActiveStatus
		};
	}

	public static PersistedRunSnapshot? LoadSnapshot(DamageStatsRunStorageContext context, RunState runState)
	{
		if (TryReadValidatedSnapshot(context.StatePath, context, runState, out PersistedRunSnapshot? snapshot))
		{
			return snapshot;
		}

		if (TryReadValidatedSnapshot(context.BackupPath, context, runState, out snapshot))
		{
			Log.Warn($"Recovered damage stats from backup for run {context.RunId}.");
			SaveSnapshot(context, snapshot!);
			return snapshot;
		}

		if (TryMigrateLegacySnapshot(context, runState, out snapshot))
		{
			Log.Info($"Migrated legacy damage stats cache for run {context.RunId}.");
			SaveSnapshot(context, snapshot!);
			return snapshot;
		}

		return null;
	}

	public static PersistedRunSnapshot? LoadExistingSnapshot(DamageStatsRunStorageContext context)
	{
		if (TryReadSnapshot(context.StatePath, out PersistedRunSnapshot? snapshot))
		{
			return snapshot;
		}

		if (TryReadSnapshot(context.BackupPath, out snapshot))
		{
			return snapshot;
		}

		return null;
	}

	public static void SaveSnapshot(DamageStatsRunStorageContext context, PersistedRunSnapshot snapshot)
	{
		Directory.CreateDirectory(context.DirectoryPath);
		snapshot.LastUpdatedUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		string json = JsonSerializer.Serialize(snapshot, JsonOptions);
		_ = JsonSerializer.Deserialize<PersistedRunSnapshot>(json, JsonOptions) ?? throw new InvalidOperationException("Serialized run snapshot could not be deserialized for verification.");
		byte[] payload = Encoding.UTF8.GetBytes(json);
		using (FileStream tempStream = new FileStream(context.TempPath, FileMode.Create, System.IO.FileAccess.Write, FileShare.None, 4096, System.IO.FileOptions.WriteThrough))
		{
			tempStream.Write(payload, 0, payload.Length);
			tempStream.Flush(flushToDisk: true);
		}

		if (File.Exists(context.StatePath))
		{
			File.Replace(context.TempPath, context.StatePath, context.BackupPath, ignoreMetadataErrors: true);
			return;
		}

		File.Move(context.TempPath, context.StatePath, overwrite: true);
		if (!File.Exists(context.BackupPath))
		{
			File.Copy(context.StatePath, context.BackupPath);
		}
	}

	public static void MarkRunCompleted(DamageStatsRunStorageContext context, RunState runState)
	{
		PersistedRunSnapshot snapshot = LoadExistingSnapshot(context) ?? CreateEmptySnapshot(runState, context);
		snapshot.IsCombatActive = false;
		snapshot.ResetCombatTotalsOnResume = false;
		snapshot.ActiveCombat = null;
		snapshot.RunStatus = CompletedStatus;
		SaveSnapshot(context, snapshot);
	}

	public static void MarkRunCompletedFromSave(SerializableRun saveData)
	{
		RunState runState = RunState.FromSerializable(saveData);
		DamageStatsRunStorageContext context = CreateRunContext(runState, saveData.StartTime);
		PersistedRunSnapshot snapshot = LoadExistingSnapshot(context) ?? CreateEmptySnapshot(runState, context);
		snapshot.IsCombatActive = false;
		snapshot.ResetCombatTotalsOnResume = false;
		snapshot.ActiveCombat = null;
		snapshot.RunStatus = CompletedStatus;
		SaveSnapshot(context, snapshot);
	}

	private static bool TryReadValidatedSnapshot(string path, DamageStatsRunStorageContext context, RunState runState, out PersistedRunSnapshot? snapshot)
	{
		if (!TryReadSnapshot(path, out snapshot))
		{
			return false;
		}

		if (snapshot == null || !IsMetadataMatch(snapshot.Metadata, context, runState))
		{
			Log.Warn($"Ignoring mismatched damage stats snapshot at {path}.");
			snapshot = null;
			return false;
		}

		return true;
	}

	private static bool TryReadSnapshot(string path, out PersistedRunSnapshot? snapshot)
	{
		snapshot = null;
		if (!File.Exists(path))
		{
			return false;
		}

		try
		{
			string json = File.ReadAllText(path);
			snapshot = JsonSerializer.Deserialize<PersistedRunSnapshot>(json, JsonOptions);
			if (snapshot == null)
			{
				Log.Warn($"Damage stats snapshot at {path} was empty after deserialization.");
				return false;
			}

			return true;
		}
		catch (Exception exception)
		{
			Log.Warn($"Failed to read damage stats snapshot at {path}. {exception.Message}");
			return false;
		}
	}

	private static bool TryMigrateLegacySnapshot(DamageStatsRunStorageContext context, RunState runState, out PersistedRunSnapshot? snapshot)
	{
		snapshot = null;
		string legacyCachePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppContext.BaseDirectory, "damage_stats_cache.json");
		if (!File.Exists(legacyCachePath))
		{
			return false;
		}

		try
		{
			string json = File.ReadAllText(legacyCachePath);
			LegacyPersistedDamageStatsCache? legacyCache = JsonSerializer.Deserialize<LegacyPersistedDamageStatsCache>(json, JsonOptions);
			if (legacyCache == null || !legacyCache.Runs.TryGetValue(context.CacheKey, out LegacyPersistedRunTotals? legacyRun))
			{
				return false;
			}

			snapshot = new PersistedRunSnapshot
			{
				Metadata = CreateMetadata(runState, context),
				StatsSchemaVersion = 1,
				LastUpdatedUnixTime = legacyRun.LastUpdatedUnixTime,
				RunTotals = new Dictionary<string, int>(legacyRun.RunTotals, StringComparer.Ordinal),
				IsCombatActive = legacyRun.ActiveCombat != null,
				ActiveCombat = legacyRun.ActiveCombat == null ? null : new PersistedActiveCombatState
				{
					CombatTotals = new Dictionary<string, int>(legacyRun.ActiveCombat.CombatTotals, StringComparer.Ordinal),
					CombatEffectLedgers = legacyRun.ActiveCombat.CombatEffectLedgers.Select(static ledger => new PersistedCombatLedger
					{
						EffectKind = ledger.EffectKind,
						TargetKey = ledger.TargetKey,
						PlayerContributions = new Dictionary<string, int>(ledger.PlayerContributions, StringComparer.Ordinal),
						UnattributedAmount = ledger.UnattributedAmount
					}).ToList()
				},
				RunStatus = ActiveStatus
			};
			return true;
		}
		catch (Exception exception)
		{
			Log.Warn($"Failed to migrate legacy damage stats cache for run {context.RunId}. {exception.Message}");
			return false;
		}
	}

	private static bool IsMetadataMatch(PersistedRunMetadata? metadata, DamageStatsRunStorageContext context, RunState runState)
	{
		if (metadata == null)
		{
			return false;
		}

		if (!string.Equals(metadata.Mode, context.ModeDirectoryName, StringComparison.Ordinal) || metadata.StartTime != context.StartTime || !string.Equals(metadata.CacheKey, context.CacheKey, StringComparison.Ordinal) || !string.Equals(metadata.Seed, runState.Rng.StringSeed, StringComparison.Ordinal) || !string.Equals(metadata.GameMode, runState.GameMode.ToString(), StringComparison.Ordinal) || metadata.AscensionLevel != runState.AscensionLevel)
		{
			return false;
		}

		if (metadata.Players.Count != runState.Players.Count)
		{
			return false;
		}

		for (int i = 0; i < runState.Players.Count; i++)
		{
			if (metadata.Players[i].NetId != runState.Players[i].NetId || !string.Equals(metadata.Players[i].CharacterId, runState.Players[i].Character.Id.ToString(), StringComparison.Ordinal))
			{
				return false;
			}
		}

		return true;
	}

	private static string CreateRunCacheKey(RunState runState, long startTime)
	{
		StringBuilder builder = new StringBuilder();
		builder.Append(startTime);
		builder.Append('|');
		builder.Append(runState.GameMode);
		builder.Append('|');
		builder.Append(runState.AscensionLevel);
		builder.Append('|');
		builder.Append(runState.Rng.StringSeed);
		for (int i = 0; i < runState.Players.Count; i++)
		{
			builder.Append('|');
			builder.Append(runState.Players[i].NetId);
			builder.Append(':');
			builder.Append(runState.Players[i].Character.Id);
		}

		return builder.ToString();
	}

	private static string ComputeShortHash(string value)
	{
		string hex = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
		return hex.Substring(0, 12).ToLowerInvariant();
	}
}

internal sealed record DamageStatsRunStorageContext(string ModeDirectoryName, long StartTime, string CacheKey, string RunId, string DirectoryPath, string StatePath, string BackupPath, string TempPath);

internal sealed class PersistedRunSnapshot
{
	public PersistedRunMetadata Metadata { get; set; } = new PersistedRunMetadata();

	public int StatsSchemaVersion { get; set; }

	public long LastUpdatedUnixTime { get; set; }

	public Dictionary<string, int> RunTotals { get; set; } = new Dictionary<string, int>();

	public bool IsCombatActive { get; set; }

	public bool ResetCombatTotalsOnResume { get; set; }

	public PersistedActiveCombatState? ActiveCombat { get; set; }

	public string RunStatus { get; set; } = "active";
}

internal sealed class PersistedRunMetadata
{
	public string Mode { get; set; } = string.Empty;

	public long StartTime { get; set; }

	public string CacheKey { get; set; } = string.Empty;

	public string Seed { get; set; } = string.Empty;

	public string GameMode { get; set; } = string.Empty;

	public int AscensionLevel { get; set; }

	public List<PersistedRunPlayerMetadata> Players { get; set; } = new List<PersistedRunPlayerMetadata>();
}

internal sealed class PersistedRunPlayerMetadata
{
	public ulong NetId { get; set; }

	public string CharacterId { get; set; } = string.Empty;
}

internal sealed class LegacyPersistedDamageStatsCache
{
	public Dictionary<string, LegacyPersistedRunTotals> Runs { get; set; } = new Dictionary<string, LegacyPersistedRunTotals>();
}

internal sealed class LegacyPersistedRunTotals
{
	public long LastUpdatedUnixTime { get; set; }

	public Dictionary<string, int> RunTotals { get; set; } = new Dictionary<string, int>();

	public LegacyPersistedActiveCombatState? ActiveCombat { get; set; }
}

internal sealed class LegacyPersistedActiveCombatState
{
	public Dictionary<string, int> CombatTotals { get; set; } = new Dictionary<string, int>();

	public List<LegacyPersistedCombatLedger> CombatEffectLedgers { get; set; } = new List<LegacyPersistedCombatLedger>();
}

internal sealed class LegacyPersistedCombatLedger
{
	public DamageEffectKind EffectKind { get; set; }

	public string TargetKey { get; set; } = string.Empty;

	public Dictionary<string, int> PlayerContributions { get; set; } = new Dictionary<string, int>();

	public int UnattributedAmount { get; set; }
}
