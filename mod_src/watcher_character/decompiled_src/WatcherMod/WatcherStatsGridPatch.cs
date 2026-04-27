using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.StatsScreen;
using MegaCrit.Sts2.Core.Saves;

namespace WatcherMod;

[HarmonyPatch(typeof(NGeneralStatsGrid), "LoadStats")]
internal static class WatcherStatsGridPatch
{
	private static readonly MethodInfo? CreateCharSectionMethod = AccessTools.Method(typeof(NGeneralStatsGrid), "CreateCharacterSection");

	private static void Postfix(NGeneralStatsGrid __instance)
	{
		if (!(CreateCharSectionMethod == null))
		{
			CharacterModel byIdOrNull = ModelDb.GetByIdOrNull<CharacterModel>(ModelDb.GetId(typeof(Watcher)));
			if (byIdOrNull != null)
			{
				ProgressState progress = SaveManager.Instance.Progress;
				CreateCharSectionMethod.Invoke(__instance, new object[2] { progress, byIdOrNull.Id });
			}
		}
	}
}
