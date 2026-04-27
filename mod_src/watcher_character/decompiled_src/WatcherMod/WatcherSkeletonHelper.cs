using System;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Logging;

namespace WatcherMod;

internal static class WatcherSkeletonHelper
{
	private static bool _logged;

	public static void ApplySkeletonVariant(MegaSprite sprite)
	{
		string activeSkeletonDataPath = WatcherModSettings.ActiveSkeletonDataPath;
		if (activeSkeletonDataPath == "res://animations/characters/watcher/watcher_skel_data.tres")
		{
			return;
		}
		try
		{
			Resource resource = ResourceLoader.Load<Resource>(activeSkeletonDataPath, null, ResourceLoader.CacheMode.Reuse);
			if (resource == null)
			{
				Log.Error("[Watcher] Failed to load skeleton data: " + activeSkeletonDataPath);
				return;
			}
			MegaSkeletonDataResource skeletonDataRes = new MegaSkeletonDataResource(resource);
			sprite.SetSkeletonDataRes(skeletonDataRes);
			if (!_logged)
			{
				_logged = true;
				GD.Print("[Watcher] Using V2 skeleton: " + activeSkeletonDataPath);
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Watcher] Skeleton swap error: " + ex.Message);
		}
	}

	public static void ApplySkeletonVariant(Node spineNode)
	{
		if (!(spineNode.GetClass() != "SpineSprite"))
		{
			ApplySkeletonVariant(new MegaSprite(spineNode));
		}
	}
}
