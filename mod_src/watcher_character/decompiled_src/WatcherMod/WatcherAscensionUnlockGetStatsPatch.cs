using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;

namespace WatcherMod;

[HarmonyPatch(typeof(ProgressState), "GetStatsForCharacter")]
internal static class WatcherAscensionUnlockGetStatsPatch
{
	private static void Postfix(ModelId characterId, ref CharacterStats? __result)
	{
		if (!(characterId != ModelDb.GetId(typeof(Watcher))) || !(characterId != ModelDb.GetId(typeof(WatcherV2))))
		{
			if (__result == null)
			{
				__result = SaveManager.Instance?.Progress?.GetOrCreateCharacterStats(characterId);
			}
			if (__result != null && __result.MaxAscension < 10)
			{
				__result.MaxAscension = 10;
			}
		}
	}
}
