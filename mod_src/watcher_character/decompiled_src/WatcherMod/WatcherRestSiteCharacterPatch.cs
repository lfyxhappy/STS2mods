using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.RestSite;

namespace WatcherMod;

[HarmonyPatch(typeof(NRestSiteCharacter), "_Ready")]
internal static class WatcherRestSiteCharacterPatch
{
	private const string WatcherRestSpriteName = "WatcherRestPose";

	private static void Postfix(NRestSiteCharacter __instance)
	{
		foreach (Node child in __instance.GetChildren())
		{
			if (child is Sprite2D { Texture: null } sprite2D)
			{
				Texture2D texture2D = WatcherTextureHelper.LoadTexture("res://images/characters/watcher/watcher_idle.png");
				if (texture2D != null)
				{
					sprite2D.Texture = texture2D;
				}
			}
			else
			{
				if (!(child.GetClass() == "SpineSprite") || !(child is Node2D node2D))
				{
					continue;
				}
				MegaSprite megaSprite = new MegaSprite(child);
				if (!megaSprite.HasAnimation("overgrowth_loop") && !megaSprite.HasAnimation("hive_loop") && !megaSprite.HasAnimation("glory_loop"))
				{
					Texture2D texture2D2 = WatcherTextureHelper.LoadTexture("res://images/characters/watcher/watcher_rest.png");
					if (texture2D2 != null)
					{
						node2D.Visible = false;
						if (__instance.GetNodeOrNull<Sprite2D>("WatcherRestPose") == null)
						{
							Shader shader = new Shader
							{
								Code = "shader_type canvas_item;\r\nuniform vec3 glow_color : source_color = vec3(0.455, 0.992, 0.992);\r\nuniform float brightness : hint_range(0.0, 1.0) = 0.55;\r\nuniform float diffuse_strength : hint_range(0.0, 2.0) = 0.9;\r\nuniform float specular_strength : hint_range(0.0, 1.5) = 0.35;\r\nuniform float spec_threshold : hint_range(0.3, 1.0) = 0.7;\r\nuniform float flicker_speed = 2.5;\r\nuniform vec2 light_origin = vec2(1.0, 1.0); // bottom-right in UV space\r\nuniform float falloff : hint_range(0.1, 2.0) = 1.0;\r\nuniform float lum_power : hint_range(0.5, 4.0) = 1.6;\r\nvoid fragment() {\r\n    vec4 tex = texture(TEXTURE, UV);\r\n    vec3 base = tex.rgb;\r\n    vec3 dimmed = base * brightness;\r\n\r\n    // Reflectance mask: bright surfaces receive more light.\r\n    float lum_src = dot(base, vec3(0.2126, 0.7152, 0.0722));\r\n    float reflect_mask = pow(clamp(lum_src, 0.0, 1.0), lum_power);\r\n\r\n    float flicker = 0.9 + 0.07 * sin(TIME * flicker_speed)\r\n                        + 0.03 * sin(TIME * flicker_speed * 3.7);\r\n    float dist = distance(UV, light_origin);\r\n    float radial = 1.0 - smoothstep(0.0, falloff, dist);\r\n    float intensity = flicker * radial;\r\n\r\n    // Diffuse: light modulated by surface color — a red shirt stays red-ish under\r\n    // cyan light rather than being recolored outright, which is what makes the\r\n    // highlight read as \"light hitting the body\" instead of a tint overlay.\r\n    vec3 diffuse = base * glow_color * diffuse_strength * intensity * reflect_mask;\r\n\r\n    // Specular: tight rim highlight on the brightest spots only.\r\n    float spec_mask = smoothstep(spec_threshold, 1.0, lum_src);\r\n    vec3 specular = glow_color * specular_strength * intensity * spec_mask;\r\n\r\n    tex.rgb = dimmed + diffuse + specular;\r\n    COLOR = tex;\r\n}"
							};
							ShaderMaterial shaderMaterial = new ShaderMaterial
							{
								Shader = shader
							};
							Color color = TryFindFireColor(__instance) ?? new Color(0.455f, 0.992f, 0.992f);
							shaderMaterial.SetShaderParameter("glow_color", color);
							Sprite2D node = new Sprite2D
							{
								Name = "WatcherRestPose",
								Texture = texture2D2,
								Position = node2D.Position + new Vector2(0f, -110f),
								Scale = new Vector2(0.8f, 0.8f),
								Material = shaderMaterial
							};
							__instance.AddChild(node, forceReadableName: false, Node.InternalMode.Disabled);
						}
						continue;
					}
					WatcherSkeletonHelper.ApplySkeletonVariant(megaSprite);
					megaSprite.GetAnimationState().SetAnimation("Idle");
				}
				node2D.Scale = new Vector2(1.5f, 1.5f);
				node2D.SetDeferred("scale", new Vector2(1.5f, 1.5f));
			}
		}
	}

	private static Color? TryFindFireColor(Node start)
	{
		Node node = start;
		while (node.GetParent() != null)
		{
			node = node.GetParent();
		}
		return SearchFireColor(node);
	}

	private static Color? SearchFireColor(Node node)
	{
		try
		{
			if (node is CanvasItem { Material: ShaderMaterial material })
			{
				Variant shaderParameter = material.GetShaderParameter("OuterColor");
				if (shaderParameter.VariantType == Variant.Type.Color)
				{
					return shaderParameter.AsColor();
				}
			}
		}
		catch
		{
		}
		foreach (Node child in node.GetChildren())
		{
			Color? result = SearchFireColor(child);
			if (result.HasValue)
			{
				return result;
			}
		}
		return null;
	}
}
