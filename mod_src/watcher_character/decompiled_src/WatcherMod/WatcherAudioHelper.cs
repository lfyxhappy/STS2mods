using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Helpers;

namespace WatcherMod;

internal static class WatcherAudioHelper
{
	private struct ActiveSound
	{
		public AudioStreamPlayer Player;

		public Callable FinishedCb;
	}

	private static readonly Dictionary<string, AudioStreamOggVorbis?> StreamCache = new Dictionary<string, AudioStreamOggVorbis>();

	private static readonly HashSet<string> NegativeCache = new HashSet<string>();

	private static readonly StringName SfxBus = new StringName("SFX");

	private static readonly List<AudioStreamPlayer> FreePool = new List<AudioStreamPlayer>();

	private static readonly List<ActiveSound> Playing = new List<ActiveSound>();

	public static void PlayOneShot(string resPath, float volumeLinear = 1f)
	{
		if (NonInteractiveMode.IsActive)
		{
			return;
		}
		AudioStreamOggVorbis audioStreamOggVorbis = LoadStream(resPath);
		if (audioStreamOggVorbis != null)
		{
			AudioStreamPlayer player;
			if (FreePool.Count > 0)
			{
				player = FreePool[FreePool.Count - 1];
				FreePool.RemoveAt(FreePool.Count - 1);
			}
			else
			{
				player = new AudioStreamPlayer();
				player.Bus = SfxBus;
				((SceneTree)Engine.GetMainLoop()).Root.AddChild(player, forceReadableName: false, Node.InternalMode.Disabled);
			}
			player.Stream = audioStreamOggVorbis;
			player.VolumeDb = Mathf.LinearToDb(Mathf.Pow(volumeLinear, 2f));
			Callable callable = Callable.From(delegate
			{
				OnFinished(player);
			});
			player.Connect(AudioStreamPlayer.SignalName.Finished, callable);
			Playing.Add(new ActiveSound
			{
				Player = player,
				FinishedCb = callable
			});
			player.Play();
		}
	}

	private static void OnFinished(AudioStreamPlayer player)
	{
		for (int i = 0; i < Playing.Count; i++)
		{
			if (Playing[i].Player == player)
			{
				player.Disconnect(AudioStreamPlayer.SignalName.Finished, Playing[i].FinishedCb);
				Playing.RemoveAt(i);
				player.Stop();
				FreePool.Add(player);
				break;
			}
		}
	}

	private static AudioStreamOggVorbis? LoadStream(string resPath)
	{
		if (NegativeCache.Contains(resPath))
		{
			return null;
		}
		if (StreamCache.TryGetValue(resPath, out AudioStreamOggVorbis value))
		{
			return value;
		}
		try
		{
			using FileAccess fileAccess = FileAccess.Open(resPath, FileAccess.ModeFlags.Read);
			if (fileAccess == null)
			{
				NegativeCache.Add(resPath);
				return null;
			}
			AudioStreamOggVorbis audioStreamOggVorbis = AudioStreamOggVorbis.LoadFromBuffer(fileAccess.GetBuffer((long)fileAccess.GetLength()));
			StreamCache[resPath] = audioStreamOggVorbis;
			return audioStreamOggVorbis;
		}
		catch
		{
			NegativeCache.Add(resPath);
			return null;
		}
	}
}
