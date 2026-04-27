using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;

namespace WatcherMod;

[HarmonyPatch(typeof(NFastModeTickbox), "SetFromSettings")]
internal static class WatcherSkeletonToggle_SetFromSettings
{
	private static bool Prefix(NFastModeTickbox __instance)
	{
		if (__instance == WatcherSettingsToggle.Instance)
		{
			__instance.IsTicked = WatcherModSettings.UseV2Skeleton;
			return false;
		}
		if (__instance == WatcherPortraitToggle.Instance)
		{
			__instance.IsTicked = WatcherModSettings.UseV2Portrait;
			return false;
		}
		if (__instance == WatcherV2ToggleHolder.Instance)
		{
			__instance.IsTicked = WatcherModSettings.EnableV2Watcher;
			return false;
		}
		return true;
	}
}
