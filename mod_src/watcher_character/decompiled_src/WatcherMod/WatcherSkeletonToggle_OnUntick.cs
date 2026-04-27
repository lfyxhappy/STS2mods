using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;

namespace WatcherMod;

[HarmonyPatch(typeof(NFastModeTickbox), "OnUntick")]
internal static class WatcherSkeletonToggle_OnUntick
{
	private static bool Prefix(NFastModeTickbox __instance)
	{
		if (__instance == WatcherSettingsToggle.Instance)
		{
			WatcherModSettings.UseV2Skeleton = false;
			GD.Print("[Watcher] Skeleton variant → Original (V1)");
			return false;
		}
		if (__instance == WatcherPortraitToggle.Instance)
		{
			WatcherModSettings.UseV2Portrait = false;
			GD.Print("[Watcher] Portrait → V1 (original)");
			return false;
		}
		if (__instance == WatcherV2ToggleHolder.Instance)
		{
			WatcherModSettings.EnableV2Watcher = false;
			GD.Print("[Watcher] Gen2 Watcher → disabled");
			return false;
		}
		return true;
	}
}
