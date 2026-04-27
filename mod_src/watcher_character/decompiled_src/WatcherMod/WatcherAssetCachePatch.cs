using System.Collections.Concurrent;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;

namespace WatcherMod;

[HarmonyPatch(typeof(AssetCache), "LoadAsset")]
internal static class WatcherAssetCachePatch
{
	private static readonly AccessTools.FieldRef<AssetCache, ConcurrentDictionary<string, Resource>> CacheRef = AccessTools.FieldRefAccess<AssetCache, ConcurrentDictionary<string, Resource>>("_cache");

	private static bool Prefix(AssetCache __instance, string path, ref Resource __result)
	{
		if (!IsWatcherAssetPath(path))
		{
			return true;
		}
		Texture2D texture2D = WatcherTextureHelper.LoadTexture(path);
		if (texture2D != null)
		{
			CacheRef(__instance)[path] = texture2D;
			__result = texture2D;
			return false;
		}
		return true;
	}

	private static bool IsWatcherAssetPath(string path)
	{
		if (!path.Contains("/watcher") && !path.Contains("/Watcher"))
		{
			return path.Contains("char_select_watcher");
		}
		return true;
	}
}
