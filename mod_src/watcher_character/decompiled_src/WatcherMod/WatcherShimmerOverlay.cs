using Godot;

namespace WatcherMod;

internal static class WatcherShimmerOverlay
{
	private const string OverlayName = "WatcherShimmerOverlay";

	private const string ShaderCode = "shader_type canvas_item;\r\nrender_mode blend_add;\r\n\r\nuniform float speed : hint_range(0.0, 5.0) = 0.6;\r\nuniform float band_width : hint_range(0.05, 1.0) = 0.35;\r\nuniform float intensity : hint_range(0.0, 2.0) = 0.7;\r\nuniform vec4 tint : source_color = vec4(0.78, 0.55, 1.0, 1.0);\r\n\r\nvoid fragment() {\r\n    vec4 tex = texture(TEXTURE, UV);\r\n    float phase = fract(TIME * speed * 0.25);\r\n    float pos = phase * (1.0 + band_width * 2.0) - band_width;\r\n    float d = (UV.x + UV.y) * 0.5 - pos;\r\n    float band = exp(-pow(d / band_width, 2.0) * 4.0);\r\n    float mask = step(0.01, tex.a);\r\n    COLOR = vec4(tint.rgb * band * intensity * mask, tex.a * band * mask);\r\n}";

	public static void AttachTo(TextureRect? target)
	{
		if (target != null && target.FindChild("WatcherShimmerOverlay", recursive: false, owned: false) == null && target.Texture != null)
		{
			Shader shader = new Shader
			{
				Code = "shader_type canvas_item;\r\nrender_mode blend_add;\r\n\r\nuniform float speed : hint_range(0.0, 5.0) = 0.6;\r\nuniform float band_width : hint_range(0.05, 1.0) = 0.35;\r\nuniform float intensity : hint_range(0.0, 2.0) = 0.7;\r\nuniform vec4 tint : source_color = vec4(0.78, 0.55, 1.0, 1.0);\r\n\r\nvoid fragment() {\r\n    vec4 tex = texture(TEXTURE, UV);\r\n    float phase = fract(TIME * speed * 0.25);\r\n    float pos = phase * (1.0 + band_width * 2.0) - band_width;\r\n    float d = (UV.x + UV.y) * 0.5 - pos;\r\n    float band = exp(-pow(d / band_width, 2.0) * 4.0);\r\n    float mask = step(0.01, tex.a);\r\n    COLOR = vec4(tint.rgb * band * intensity * mask, tex.a * band * mask);\r\n}"
			};
			ShaderMaterial material = new ShaderMaterial
			{
				Shader = shader
			};
			TextureRect textureRect = new TextureRect
			{
				Name = "WatcherShimmerOverlay",
				Texture = target.Texture,
				StretchMode = target.StretchMode,
				ExpandMode = target.ExpandMode,
				MouseFilter = Control.MouseFilterEnum.Ignore,
				Material = material
			};
			target.AddChild(textureRect, forceReadableName: false, Node.InternalMode.Disabled);
			textureRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		}
	}
}
