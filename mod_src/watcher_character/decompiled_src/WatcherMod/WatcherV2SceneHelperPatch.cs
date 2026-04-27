using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;

namespace WatcherMod;

[HarmonyPatch(typeof(SceneHelper), "GetScenePath")]
internal static class WatcherV2SceneHelperPatch
{
	private static void Postfix(ref string __result)
	{
		if (!string.IsNullOrEmpty(__result) && __result.Contains("watcher_v2"))
		{
			__result = __result.Replace("watcher_v2", "watcher");
		}
	}
}
