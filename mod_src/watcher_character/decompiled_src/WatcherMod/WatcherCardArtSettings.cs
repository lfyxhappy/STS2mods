using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

internal static class WatcherCardArtSettings
{
	private class SettingsData
	{
		public bool GlobalHandDrawn { get; set; }

		public Dictionary<string, bool> PerCard { get; set; } = new Dictionary<string, bool>();
	}

	private static readonly string SettingsPath = Path.Combine(OS.GetUserDataDir(), "watcher_card_art.json");

	private static Dictionary<string, bool>? _perCardSettings;

	private static bool _globalHandDrawn;

	private static bool _loaded;

	public const string PlaceholderPortraitPath = "res://images/packed/card_portraits/watcher/_placeholder.png";

	public static bool GlobalHandDrawn
	{
		get
		{
			EnsureLoaded();
			return _globalHandDrawn;
		}
		set
		{
			EnsureLoaded();
			_globalHandDrawn = value;
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
		_perCardSettings = new Dictionary<string, bool>();
		try
		{
			if (!File.Exists(SettingsPath))
			{
				return;
			}
			SettingsData settingsData = JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(SettingsPath));
			if (settingsData != null)
			{
				_globalHandDrawn = settingsData.GlobalHandDrawn;
				if (settingsData.PerCard != null)
				{
					_perCardSettings = new Dictionary<string, bool>(settingsData.PerCard);
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Watcher] Failed to load card art settings: " + ex.Message);
		}
	}

	private static void Save()
	{
		try
		{
			string contents = JsonSerializer.Serialize(new SettingsData
			{
				GlobalHandDrawn = _globalHandDrawn,
				PerCard = (_perCardSettings ?? new Dictionary<string, bool>())
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
			Log.Error("[Watcher] Failed to save card art settings: " + ex.Message);
		}
	}

	public static bool IsHandDrawn(CardModel card)
	{
		EnsureLoaded();
		string key = card.Id.Entry.ToLower();
		if (_perCardSettings != null && _perCardSettings.TryGetValue(key, out var value))
		{
			return value;
		}
		return _globalHandDrawn;
	}

	public static bool HasPerCardOverride(CardModel card)
	{
		EnsureLoaded();
		string key = card.Id.Entry.ToLower();
		return _perCardSettings?.ContainsKey(key) ?? false;
	}

	public static bool ToggleCard(CardModel card)
	{
		EnsureLoaded();
		string key = card.Id.Entry.ToLower();
		bool flag = !IsHandDrawn(card);
		if (_perCardSettings == null)
		{
			_perCardSettings = new Dictionary<string, bool>();
		}
		if (flag == _globalHandDrawn)
		{
			_perCardSettings.Remove(key);
		}
		else
		{
			_perCardSettings[key] = flag;
		}
		Save();
		return flag;
	}

	public static string GetEffectivePortraitPath(CardModel card)
	{
		string portraitPath = card.PortraitPath;
		if (IsHandDrawn(card))
		{
			string betaPortraitPath = GetBetaPortraitPath(card);
			if (WatcherTextureHelper.LoadTexture(betaPortraitPath) != null)
			{
				return betaPortraitPath;
			}
		}
		if (WatcherTextureHelper.LoadTexture(portraitPath) != null)
		{
			return portraitPath;
		}
		return "res://images/packed/card_portraits/watcher/_placeholder.png";
	}

	public static string GetBetaPortraitPath(CardModel card)
	{
		return "res://images/packed/card_portraits/watcher/beta/" + card.Id.Entry.ToLower() + ".png";
	}

	public static bool HasBetaArt(CardModel card)
	{
		return WatcherTextureHelper.LoadTexture(GetBetaPortraitPath(card)) != null;
	}
}
