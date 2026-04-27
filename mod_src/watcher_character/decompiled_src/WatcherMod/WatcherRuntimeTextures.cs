using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

internal static class WatcherRuntimeTextures
{
	private static readonly Dictionary<string, Texture2D?> TextureCache = new Dictionary<string, Texture2D>();

	public static string GetCardPortraitPath(CardModel model)
	{
		return "res://images/packed/card_portraits/watcher/" + model.Id.Entry.ToLower() + ".png";
	}

	public static Texture2D? LoadTexture(string path)
	{
		if (TextureCache.TryGetValue(path, out Texture2D value))
		{
			if (value != null && GodotObject.IsInstanceValid(value))
			{
				return value;
			}
			TextureCache.Remove(path);
		}
		Texture2D texture2D = null;
		try
		{
			Image image = Image.LoadFromFile(path);
			if (image.GetWidth() > 0 && image.GetHeight() > 0)
			{
				texture2D = ImageTexture.CreateFromImage(image);
			}
		}
		catch
		{
			texture2D = null;
		}
		TextureCache[path] = texture2D;
		return texture2D;
	}
}
