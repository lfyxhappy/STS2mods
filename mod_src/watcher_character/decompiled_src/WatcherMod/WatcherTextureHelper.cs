using System.Collections.Generic;
using Godot;

namespace WatcherMod;

internal static class WatcherTextureHelper
{
	private static readonly Dictionary<string, Texture2D?> TextureCache = new Dictionary<string, Texture2D>();

	private static readonly HashSet<string> NegativeCache = new HashSet<string>();

	public static Texture2D? LoadTexture(string path)
	{
		if (NegativeCache.Contains(path))
		{
			return null;
		}
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
			if (path.StartsWith("res://"))
			{
				if (ResourceLoader.Exists(path))
				{
					texture2D = ResourceLoader.Load<Texture2D>(path, null, ResourceLoader.CacheMode.Reuse);
				}
				if (texture2D == null && FileAccess.FileExists(path))
				{
					Image image = Image.LoadFromFile(path);
					if (image != null && image.GetWidth() > 0 && image.GetHeight() > 0)
					{
						texture2D = ImageTexture.CreateFromImage(image);
					}
				}
			}
			else
			{
				Image image2 = Image.LoadFromFile(path);
				if (image2.GetWidth() > 0 && image2.GetHeight() > 0)
				{
					texture2D = ImageTexture.CreateFromImage(image2);
				}
			}
		}
		catch
		{
			texture2D = null;
		}
		if (texture2D != null)
		{
			TextureCache[path] = texture2D;
		}
		else
		{
			NegativeCache.Add(path);
		}
		return texture2D;
	}
}
