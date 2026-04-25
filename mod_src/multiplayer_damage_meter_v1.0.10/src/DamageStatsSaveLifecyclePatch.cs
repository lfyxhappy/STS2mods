using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace MultiplayerDamageMeter;

[HarmonyPatch]
public static class DamageStatsSaveLifecyclePatch
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SaveRun))]
	public static void BeforeSaveRun(AbstractRoom? preFinishedRoom)
	{
		if (preFinishedRoom != null)
		{
			DamageStatsService.MarkCurrentCombatSavedAsFinished();
		}
		else
		{
			DamageStatsService.MarkCurrentCombatSavedForResumeReset();
		}

		DamageStatsService.FlushCurrentRunState();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SaveManager), nameof(SaveManager.DeleteCurrentRun))]
	public static void BeforeDeleteCurrentRun(SaveManager __instance)
	{
		MarkRunCompleted(__instance, isMultiplayer: false);
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SaveManager), nameof(SaveManager.DeleteCurrentMultiplayerRun))]
	public static void BeforeDeleteCurrentMultiplayerRun(SaveManager __instance)
	{
		MarkRunCompleted(__instance, isMultiplayer: true);
	}

	private static void MarkRunCompleted(SaveManager saveManager, bool isMultiplayer)
	{
		if (RunManager.Instance.IsInProgress)
		{
			DamageStatsService.MarkCurrentRunCompleted();
			return;
		}

		try
		{
			if (isMultiplayer)
			{
				ReadSaveResult<SerializableRun> result = saveManager.LoadAndCanonicalizeMultiplayerRunSave(PlatformUtil.GetLocalPlayerId(PlatformUtil.PrimaryPlatform));
				if (result.Success && result.SaveData != null)
				{
					DamageStatsFileStore.MarkRunCompletedFromSave(result.SaveData);
				}

				return;
			}

			ReadSaveResult<SerializableRun> singleResult = saveManager.LoadRunSave();
			if (singleResult.Success && singleResult.SaveData != null)
			{
				DamageStatsFileStore.MarkRunCompletedFromSave(singleResult.SaveData);
			}
		}
		catch (Exception exception)
		{
			Log.Warn($"Failed to mark damage stats run as completed before deleting the base save. {exception.Message}");
		}
	}
}
