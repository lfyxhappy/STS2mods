using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;

namespace WatcherMod;

[HarmonyPatch(typeof(NFastModeTickbox), "OnTick")]
internal static class WatcherSkeletonToggle_OnTick
{
	private static bool Prefix(NFastModeTickbox __instance)
	{
		if (__instance == WatcherSettingsToggle.Instance)
		{
			WatcherModSettings.UseV2Skeleton = true;
			GD.Print("[Watcher] Skeleton variant → Community (V2)");
			return false;
		}
		if (__instance == WatcherPortraitToggle.Instance)
		{
			WatcherModSettings.UseV2Portrait = true;
			GD.Print("[Watcher] Portrait → V2 (new)");
			return false;
		}
		if (__instance == WatcherV2ToggleHolder.Instance)
		{
			WatcherModSettings.EnableV2Watcher = true;
			GD.Print("[Watcher] Gen2 Watcher → enabled");
			return false;
		}
		return true;
	}
}
