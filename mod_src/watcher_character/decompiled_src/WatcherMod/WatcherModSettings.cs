using System;
using System.IO;
using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace WatcherMod;

internal static class WatcherModSettings
{
	private class SettingsData
	{
		public bool UseV2Skeleton { get; set; }

		public bool UseV2Portrait { get; set; }

		public bool EnableV2Watcher { get; set; }
	}

	private static readonly string SettingsPath = Path.Combine(OS.GetUserDataDir(), "watcher_mod_settings.json");

	private static bool _loaded;

	private static bool _useV2Skeleton;

	private static bool _useV2Portrait;

	private static bool _enableV2Watcher;

	public const string V1SkeletonDataPath = "res://animations/characters/watcher/watcher_skel_data.tres";

	public const string V2SkeletonDataPath = "res://animations/characters/watcher/v2/watcher_skel_data.tres";

	public const string V1PortraitPath = "res://images/ui/charSelect/watcherPortrait.jpg";

	public const string V2PortraitPath = "res://images/ui/charSelect/watcherPortrait_v2.jpg";

	public static bool UseV2Skeleton
	{
		get
		{
			EnsureLoaded();
			return _useV2Skeleton;
		}
		set
		{
			EnsureLoaded();
			_useV2Skeleton = value;
			Save();
		}
	}

	public static bool UseV2Portrait
	{
		get
		{
			EnsureLoaded();
			return _useV2Portrait;
		}
		set
		{
			EnsureLoaded();
			_useV2Portrait = value;
			Save();
		}
	}

	public static bool V2PortraitAvailable => ResourceLoader.Exists("res://images/ui/charSelect/watcherPortrait_v2.jpg");

	public static string ActivePortraitPath
	{
		get
		{
			if (!UseV2Portrait || !V2PortraitAvailable)
			{
				return "res://images/ui/charSelect/watcherPortrait.jpg";
			}
			return "res://images/ui/charSelect/watcherPortrait_v2.jpg";
		}
	}

	public static bool V2SkeletonAvailable => ResourceLoader.Exists("res://animations/characters/watcher/v2/watcher_skel_data.tres");

	public static string ActiveSkeletonDataPath
	{
		get
		{
			if (!UseV2Skeleton || !V2SkeletonAvailable)
			{
				return "res://animations/characters/watcher/watcher_skel_data.tres";
			}
			return "res://animations/characters/watcher/v2/watcher_skel_data.tres";
		}
	}

	public static bool EnableV2Watcher
	{
		get
		{
			EnsureLoaded();
			return _enableV2Watcher;
		}
		set
		{
			EnsureLoaded();
			_enableV2Watcher = value;
			Save();
		}
	}

	private static void EnsureLoaded()
	{
		if (_loaded)
		{
			return;
		}
		_loaded = true;
		try
		{
			if (File.Exists(SettingsPath))
			{
				SettingsData settingsData = JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(SettingsPath));
				if (settingsData != null)
				{
					_useV2Skeleton = settingsData.UseV2Skeleton;
					_useV2Portrait = settingsData.UseV2Portrait;
					_enableV2Watcher = settingsData.EnableV2Watcher;
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Watcher] Failed to load mod settings: " + ex.Message);
		}
	}

	private static void Save()
	{
		try
		{
			string contents = JsonSerializer.Serialize(new SettingsData
			{
				UseV2Skeleton = _useV2Skeleton,
				UseV2Portrait = _useV2Portrait,
				EnableV2Watcher = _enableV2Watcher
			}, new JsonSerializerOptions
			{
				WriteIndented = true
			});
			string directoryName = Path.GetDirectoryName(SettingsPath);
			if (directoryName != null)
			{
				Directory.CreateDirectory(directoryName);
			}
			File.WriteAllText(SettingsPath, contents);
		}
		catch (Exception ex)
		{
			Log.Error("[Watcher] Failed to save mod settings: " + ex.Message);
		}
	}
}
