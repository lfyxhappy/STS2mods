using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace MultiplayerDamageMeter;

internal static class DamageStatsGlassTheme
{
	public static readonly Color GlassText = new Color(0.95f, 0.98f, 1f, 0.97f);

	public static readonly Color GlassTextMuted = new Color(0.84f, 0.90f, 0.96f, 0.86f);

	public static readonly Color GlassAccentGold = new Color(0.96f, 0.84f, 0.56f, 1f);

	public static readonly Color GlassAccentSilver = new Color(0.85f, 0.90f, 0.97f, 1f);

	public static readonly Color GlassAccentBronze = new Color(0.93f, 0.73f, 0.48f, 1f);

	public static readonly Color GlassAccentNeutral = new Color(0.88f, 0.93f, 0.98f, 1f);

	public static readonly Color GlassOutline = new Color(0.02f, 0.05f, 0.12f, 0.96f);

	public static readonly Color GlassShadow = new Color(0.02f, 0.06f, 0.11f, 0.22f);

	public static readonly Color GlassDivider = new Color(0.96f, 0.98f, 1f, 0.12f);

	private static readonly Shader BackdropShader = new Shader
	{
		Code = """
shader_type canvas_item;
render_mode unshaded, blend_mix;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear_mipmap;
uniform vec4 tint_color : source_color = vec4(0.74, 0.82, 0.92, 0.32);
uniform vec4 edge_color : source_color = vec4(1.0, 1.0, 1.0, 0.20);
uniform float blur_radius = 1.15;
uniform float glass_alpha = 0.78;
uniform float corner_radius = 0.10;
uniform float brightness_boost = 0.04;

float rounded_box_sdf(vec2 p, vec2 b, float r) {
	vec2 q = abs(p) - b + vec2(r);
	return length(max(q, vec2(0.0))) + min(max(q.x, q.y), 0.0) - r;
}

float mask_from_uv(vec2 uv, float radius) {
	float sdf = rounded_box_sdf(uv - vec2(0.5), vec2(0.5 - radius), radius);
	float feather = max(SCREEN_PIXEL_SIZE.x, SCREEN_PIXEL_SIZE.y) * 2.5;
	return 1.0 - smoothstep(0.0, feather, sdf);
}

vec3 sample_glass(vec2 uv) {
	vec2 px = SCREEN_PIXEL_SIZE * blur_radius;
	vec3 color = textureLod(screen_texture, uv, 1.0).rgb * 0.36;
	color += textureLod(screen_texture, uv + vec2(px.x * 1.5, 0.0), 1.0).rgb * 0.16;
	color += textureLod(screen_texture, uv - vec2(px.x * 1.5, 0.0), 1.0).rgb * 0.16;
	color += textureLod(screen_texture, uv + vec2(0.0, px.y * 1.5), 1.0).rgb * 0.16;
	color += textureLod(screen_texture, uv - vec2(0.0, px.y * 1.5), 1.0).rgb * 0.16;
	return color;
}

void fragment() {
	float mask = mask_from_uv(UV, clamp(corner_radius, 0.01, 0.49));
	if (mask <= 0.0) {
		COLOR = vec4(0.0);
		return;
	}

	vec3 sampled = sample_glass(SCREEN_UV);
	vec3 base_color = mix(sampled, tint_color.rgb, tint_color.a);
	float top_glow = pow(max(0.0, 1.0 - UV.y), 2.2) * 0.55;
	float edge_distance = min(min(UV.x, 1.0 - UV.x), min(UV.y, 1.0 - UV.y));
	float rim = 1.0 - smoothstep(0.0, 0.14, edge_distance);
	base_color += edge_color.rgb * (top_glow * edge_color.a + rim * edge_color.a * 0.45);
	base_color += vec3(brightness_boost);
	COLOR = vec4(base_color, glass_alpha * mask);
}
"""
	};

	private static readonly Shader HighlightShader = new Shader
	{
		Code = """
shader_type canvas_item;
render_mode unshaded, blend_mix;

uniform vec4 top_glow : source_color = vec4(1.0, 1.0, 1.0, 0.24);
uniform vec4 rim_glow : source_color = vec4(0.96, 0.86, 0.62, 0.12);
uniform vec4 caustic_glow : source_color = vec4(0.74, 0.84, 1.0, 0.12);
uniform float corner_radius = 0.10;
uniform float opacity = 0.80;

float rounded_box_sdf(vec2 p, vec2 b, float r) {
	vec2 q = abs(p) - b + vec2(r);
	return length(max(q, vec2(0.0))) + min(max(q.x, q.y), 0.0) - r;
}

float mask_from_uv(vec2 uv, float radius) {
	float sdf = rounded_box_sdf(uv - vec2(0.5), vec2(0.5 - radius), radius);
	float feather = max(SCREEN_PIXEL_SIZE.x, SCREEN_PIXEL_SIZE.y) * 2.5;
	return 1.0 - smoothstep(0.0, feather, sdf);
}

void fragment() {
	float mask = mask_from_uv(UV, clamp(corner_radius, 0.01, 0.49));
	if (mask <= 0.0) {
		COLOR = vec4(0.0);
		return;
	}

	float top = pow(max(0.0, 1.0 - UV.y), 2.6);
	float edge_distance = min(min(UV.x, 1.0 - UV.x), min(UV.y, 1.0 - UV.y));
	float rim = 1.0 - smoothstep(0.0, 0.16, edge_distance);
	float caustic = smoothstep(0.14, 0.32, UV.y) * (1.0 - smoothstep(0.32, 0.62, UV.y)) * (1.0 - UV.x * 0.34);
	vec3 color = top_glow.rgb * top * top_glow.a;
	color += rim_glow.rgb * rim * rim_glow.a;
	color += caustic_glow.rgb * caustic * caustic_glow.a;
	float alpha = max(max(top * top_glow.a, rim * rim_glow.a), caustic * caustic_glow.a) * opacity;
	COLOR = vec4(color, alpha * mask);
}
"""
	};

	public static ColorRect CreateGlassBackdrop(float glassAlpha = 0.8f, float blurRadius = 1.15f, float tintMix = 0.32f, float cornerRadius = 0.1f, float brightnessBoost = 0.04f)
	{
		ColorRect rect = CreateFullRectRect();
		rect.Material = CreateGlassBackdropMaterial(glassAlpha, blurRadius, tintMix, cornerRadius, brightnessBoost);
		return rect;
	}

	public static ColorRect CreateGlassHighlight(float cornerRadius = 0.1f, float opacity = 0.82f)
	{
		ColorRect rect = CreateFullRectRect();
		rect.Material = CreateGlassHighlightMaterial(cornerRadius, opacity);
		return rect;
	}

	public static ShaderMaterial CreateGlassBackdropMaterial(float glassAlpha, float blurRadius, float tintMix, float cornerRadius, float brightnessBoost)
	{
		ShaderMaterial material = new ShaderMaterial
		{
			Shader = BackdropShader
		};
		material.SetShaderParameter("tint_color", new Color(0.74f, 0.82f, 0.92f, tintMix));
		material.SetShaderParameter("edge_color", new Color(1f, 1f, 1f, 0.2f));
		material.SetShaderParameter("blur_radius", blurRadius);
		material.SetShaderParameter("glass_alpha", glassAlpha);
		material.SetShaderParameter("corner_radius", cornerRadius);
		material.SetShaderParameter("brightness_boost", brightnessBoost);
		return material;
	}

	public static ShaderMaterial CreateGlassHighlightMaterial(float cornerRadius, float opacity)
	{
		ShaderMaterial material = new ShaderMaterial
		{
			Shader = HighlightShader
		};
		material.SetShaderParameter("top_glow", new Color(1f, 1f, 1f, 0.26f));
		material.SetShaderParameter("rim_glow", new Color(0.96f, 0.86f, 0.62f, 0.12f));
		material.SetShaderParameter("caustic_glow", new Color(0.74f, 0.84f, 1f, 0.12f));
		material.SetShaderParameter("corner_radius", cornerRadius);
		material.SetShaderParameter("opacity", opacity);
		return material;
	}

	public static StyleBoxFlat CreateShellStyle(float backgroundAlpha, float borderAlpha, int radius, int shadowSize, float shadowAlpha)
	{
		return CreateStyle(new Color(0.54f, 0.64f, 0.76f, backgroundAlpha), new Color(0.98f, 0.98f, 1f, borderAlpha), radius, 1, shadowSize, new Color(0.02f, 0.06f, 0.11f, shadowAlpha));
	}

	public static StyleBoxFlat CreateGlassPanelStyle(Color accentColor, float backgroundAlpha, float borderAlpha, int radius, int shadowSize)
	{
		Color background = Blend(new Color(0.66f, 0.76f, 0.88f, 1f), accentColor, 0.12f);
		Color border = Blend(new Color(0.98f, 0.99f, 1f, 1f), accentColor, 0.3f);
		return CreateStyle(Alpha(background, backgroundAlpha), Alpha(border, borderAlpha), radius, 1, shadowSize, GlassShadow);
	}

	public static StyleBoxFlat CreateGlassBarTrackStyle(Color accentColor)
	{
		Color background = Blend(new Color(0.76f, 0.84f, 0.92f, 1f), accentColor, 0.08f);
		return CreateStyle(Alpha(background, 0.12f), Alpha(new Color(0.98f, 0.99f, 1f, 1f), 0.08f), 7, 1, 0, Colors.Transparent);
	}

	public static StyleBoxFlat CreateGlassBarFillStyle(Color accentColor)
	{
		Color fill = Blend(accentColor, Colors.White, 0.14f);
		return CreateStyle(Alpha(fill, 0.9f), Alpha(Colors.White, 0.12f), 7, 1, 0, Colors.Transparent);
	}

	public static void ApplyLabelStyle(MegaLabel label, int fontSize, Color color, float outlineAlpha = 0.9f)
	{
		Font font = label.GetThemeFont("font", "Label");
		label.AddThemeFontOverride("font", font);
		label.AddThemeFontSizeOverride("font_size", fontSize);
		label.AddThemeColorOverride("font_color", color);
		label.AddThemeColorOverride("font_outline_color", Alpha(GlassOutline, outlineAlpha));
	}

	public static void ApplyRichTextStyle(MegaRichTextLabel label, int fontSize, Color color)
	{
		Font font = label.GetThemeFont("normal_font");
		label.AddThemeFontOverride("normal_font", font);
		label.AddThemeFontSizeOverride("normal_font_size", fontSize);
		label.AddThemeColorOverride("default_color", color);
		label.AddThemeColorOverride("font_outline_color", Alpha(GlassOutline, 0.82f));
	}

	public static void TryStylePopupShell(NVerticalPopup popup)
	{
		foreach (Node node in EnumerateDescendants(popup))
		{
			if (node == popup.YesButton || node == popup.NoButton)
			{
				continue;
			}

			if (popup.YesButton != null && IsDescendantOf(node, popup.YesButton))
			{
				continue;
			}

			if (popup.NoButton != null && IsDescendantOf(node, popup.NoButton))
			{
				continue;
			}

			if (node.Name == "Header" || node.Name == "Description")
			{
				continue;
			}

			switch (node)
			{
				case ColorRect colorRect:
				{
					Color color = colorRect.Color;
					float alpha = color.A > 0f ? Mathf.Min(color.A, 0.08f) : 0.05f;
					colorRect.Color = new Color(0.82f, 0.89f, 0.97f, alpha);
					break;
				}
				case TextureRect textureRect:
					textureRect.SelfModulate = new Color(1f, 1f, 1f, 0.2f);
					break;
				case NinePatchRect ninePatchRect:
					ninePatchRect.SelfModulate = new Color(1f, 1f, 1f, 0.18f);
					break;
				case PanelContainer panelContainer:
					panelContainer.Modulate = new Color(1f, 1f, 1f, 0.22f);
					break;
				case Panel panel:
					panel.Modulate = new Color(1f, 1f, 1f, 0.22f);
					break;
			}
		}
	}

	public static void TryStylePopupButton(NPopupYesNoButton button, bool primary)
	{
		Color accent = primary ? GlassAccentGold : GlassAccentNeutral;
		if (button.FindChild("Label", recursive: true, owned: false) is MegaLabel label)
		{
			ApplyLabelStyle(label, 20, GlassText);
		}

		if (button.FindChild("Outline", recursive: true, owned: false) is CanvasItem outline)
		{
			outline.Modulate = Alpha(Colors.White, 0.78f);
			outline.SelfModulate = Alpha(accent, 0.58f);
		}

		if (button.FindChild("Image", recursive: true, owned: false) is CanvasItem image)
		{
			image.Modulate = Alpha(Blend(new Color(0.78f, 0.86f, 0.96f, 1f), accent, 0.12f), 0.96f);
		}

		if (button.FindChild("Visuals", recursive: true, owned: false) is Control visuals)
		{
			visuals.SelfModulate = Alpha(Colors.White, 0.94f);
		}
	}

	public static Color Alpha(Color color, float alpha)
	{
		color.A = alpha;
		return color;
	}

	private static StyleBoxFlat CreateStyle(Color backgroundColor, Color borderColor, int radius, int borderWidth, int shadowSize, Color shadowColor)
	{
		return new StyleBoxFlat
		{
			BgColor = backgroundColor,
			BorderColor = borderColor,
			BorderWidthLeft = borderWidth,
			BorderWidthTop = borderWidth,
			BorderWidthRight = borderWidth,
			BorderWidthBottom = borderWidth,
			CornerRadiusTopLeft = radius,
			CornerRadiusTopRight = radius,
			CornerRadiusBottomRight = radius,
			CornerRadiusBottomLeft = radius,
			ShadowColor = shadowColor,
			ShadowSize = shadowSize,
			ShadowOffset = new Vector2(0f, 2f),
			ContentMarginLeft = 0f,
			ContentMarginTop = 0f,
			ContentMarginRight = 0f,
			ContentMarginBottom = 0f
		};
	}

	private static ColorRect CreateFullRectRect()
	{
		ColorRect rect = new ColorRect
		{
			Color = Colors.White,
			MouseFilter = Control.MouseFilterEnum.Ignore
		};
		rect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		return rect;
	}

	private static Color Blend(Color from, Color to, float weight)
	{
		return new Color(Mathf.Lerp(from.R, to.R, weight), Mathf.Lerp(from.G, to.G, weight), Mathf.Lerp(from.B, to.B, weight), Mathf.Lerp(from.A, to.A, weight));
	}

	private static IEnumerable<Node> EnumerateDescendants(Node root)
	{
		foreach (Node child in root.GetChildren())
		{
			yield return child;
			foreach (Node grandChild in EnumerateDescendants(child))
			{
				yield return grandChild;
			}
		}
	}

	private static bool IsDescendantOf(Node node, Node ancestor)
	{
		Node? current = node.GetParent();
		while (current != null)
		{
			if (current == ancestor)
			{
				return true;
			}

			current = current.GetParent();
		}

		return false;
	}
}
