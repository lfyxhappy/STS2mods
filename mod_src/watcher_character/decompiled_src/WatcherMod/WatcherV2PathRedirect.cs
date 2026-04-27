using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

internal static class WatcherV2PathRedirect
{
	internal static void Apply(CharacterModel __instance, ref string __result)
	{
		if (__instance is WatcherV2 && !string.IsNullOrEmpty(__result) && __result.Contains("watcher_v2"))
		{
			__result = __result.Replace("watcher_v2", "watcher");
		}
	}
}
