using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace WatcherMod;

internal static class StanceVfxHelper
{
	private const string VignetteShader = "\nshader_type canvas_item;\n\nuniform vec4 border_color : source_color = vec4(1.0, 0.2, 0.1, 0.5);\nuniform vec4 border_color_2 : source_color = vec4(1.0, 0.5, 0.1, 0.3);\nuniform float border_width : hint_range(0.01, 0.5) = 0.15;\nuniform float noise_scale : hint_range(1.0, 30.0) = 8.0;\nuniform float noise_speed : hint_range(0.0, 5.0) = 1.5;\nuniform float noise_intensity : hint_range(0.0, 1.0) = 0.3;\nuniform vec2 noise_direction = vec2(0.0, -1.0);\nuniform float pulse_alpha : hint_range(0.0, 1.0) = 1.0;\n\nfloat hash(vec2 p) {\n    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);\n}\n\nfloat value_noise(vec2 p) {\n    vec2 i = floor(p);\n    vec2 f = fract(p);\n    f = f * f * (3.0 - 2.0 * f);\n    float a = hash(i);\n    float b = hash(i + vec2(1.0, 0.0));\n    float c = hash(i + vec2(0.0, 1.0));\n    float d = hash(i + vec2(1.0, 1.0));\n    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);\n}\n\nfloat fbm(vec2 p) {\n    float v = 0.0;\n    float a = 0.5;\n    for (int i = 0; i < 3; i++) {\n        v += a * value_noise(p);\n        p *= 2.0;\n        a *= 0.5;\n    }\n    return v;\n}\n\nvoid fragment() {\n    vec2 uv = UV;\n    float dx = min(uv.x, 1.0 - uv.x);\n    float dy = min(uv.y, 1.0 - uv.y);\n    float d = min(dx, dy);\n\n    // Animated fractal noise for organic border\n    vec2 noise_uv = uv * noise_scale + noise_direction * TIME * noise_speed;\n    float n1 = fbm(noise_uv);\n    float n2 = fbm(noise_uv * 0.7 - vec2(TIME * noise_speed * 0.4, 0.0));\n    float n = (n1 + n2) * 0.5;\n\n    // Distort edge distance with noise for fiery/watery look\n    float distorted_d = d + (n - 0.5) * noise_intensity * border_width;\n\n    float vignette = 1.0 - smoothstep(0.0, border_width, distorted_d);\n    // Softer inner glow layer\n    float inner_glow = 1.0 - smoothstep(0.0, border_width * 1.8, distorted_d);\n\n    // Two-tone color blending driven by noise\n    vec3 col = mix(border_color.rgb, border_color_2.rgb, n);\n    float base_alpha = mix(border_color.a, border_color_2.a, n * 0.5);\n\n    // Combine sharp border + soft inner glow\n    float alpha = max(vignette * base_alpha, inner_glow * base_alpha * 0.3);\n    COLOR = vec4(col, alpha * pulse_alpha);\n}\n";

	private static Texture2D? _exhaustTexture;

	private static Texture2D? _glowSparkTexture;

	private static Texture2D? _calmOrbTexture;

	public static Node2D? SpawnOnCreature(Creature owner, Node2D vfx)
	{
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(owner);
		if (nCreature == null)
		{
			vfx.QueueFree();
			return null;
		}
		nCreature.Visuals.AddChild(vfx, forceReadableName: false, Node.InternalMode.Disabled);
		return vfx;
	}

	public static void Remove(ref Node2D? vfx)
	{
		if (vfx != null && GodotObject.IsInstanceValid(vfx))
		{
			vfx.QueueFree();
		}
		vfx = null;
	}

	public static Node? SpawnWrathBorder()
	{
		return SpawnBorder(new Color(0.8f, 0f, 0.1f, 0.55f), new Color(1f, 0.15f, 0.05f, 0.4f), 0.18f, 12f, 2.5f, 0.55f, new Vector2(0.3f, -1f));
	}

	public static Node? SpawnCalmBorder()
	{
		return SpawnBorder(new Color(0.2f, 0.6f, 1f, 0.35f), new Color(0.5f, 0.7f, 1f, 0.2f), 0.13f, 6f, 0.5f, 0.35f, new Vector2(0.8f, 0.3f));
	}

	public static Node? SpawnDivinityBorder()
	{
		return SpawnBorder(new Color(0.85f, 0.15f, 0.85f, 0.7f), new Color(1f, 0.6f, 1f, 0.5f), 0.25f, 8f, 1.2f, 0.4f, new Vector2(0f, -0.5f));
	}

	private static Node? SpawnBorder(Color borderColor, Color borderColor2, float borderWidth, float noiseScale, float noiseSpeed, float noiseIntensity, Vector2 noiseDirection)
	{
		if (NCombatRoom.Instance == null)
		{
			return null;
		}
		CanvasLayer canvasLayer = new CanvasLayer();
		canvasLayer.Name = "StanceBorderVfx";
		canvasLayer.Layer = 2;
		ColorRect colorRect = new ColorRect();
		colorRect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect, Control.LayoutPresetMode.Minsize);
		colorRect.MouseFilter = Control.MouseFilterEnum.Ignore;
		Shader shader = new Shader();
		shader.Code = "\nshader_type canvas_item;\n\nuniform vec4 border_color : source_color = vec4(1.0, 0.2, 0.1, 0.5);\nuniform vec4 border_color_2 : source_color = vec4(1.0, 0.5, 0.1, 0.3);\nuniform float border_width : hint_range(0.01, 0.5) = 0.15;\nuniform float noise_scale : hint_range(1.0, 30.0) = 8.0;\nuniform float noise_speed : hint_range(0.0, 5.0) = 1.5;\nuniform float noise_intensity : hint_range(0.0, 1.0) = 0.3;\nuniform vec2 noise_direction = vec2(0.0, -1.0);\nuniform float pulse_alpha : hint_range(0.0, 1.0) = 1.0;\n\nfloat hash(vec2 p) {\n    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);\n}\n\nfloat value_noise(vec2 p) {\n    vec2 i = floor(p);\n    vec2 f = fract(p);\n    f = f * f * (3.0 - 2.0 * f);\n    float a = hash(i);\n    float b = hash(i + vec2(1.0, 0.0));\n    float c = hash(i + vec2(0.0, 1.0));\n    float d = hash(i + vec2(1.0, 1.0));\n    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);\n}\n\nfloat fbm(vec2 p) {\n    float v = 0.0;\n    float a = 0.5;\n    for (int i = 0; i < 3; i++) {\n        v += a * value_noise(p);\n        p *= 2.0;\n        a *= 0.5;\n    }\n    return v;\n}\n\nvoid fragment() {\n    vec2 uv = UV;\n    float dx = min(uv.x, 1.0 - uv.x);\n    float dy = min(uv.y, 1.0 - uv.y);\n    float d = min(dx, dy);\n\n    // Animated fractal noise for organic border\n    vec2 noise_uv = uv * noise_scale + noise_direction * TIME * noise_speed;\n    float n1 = fbm(noise_uv);\n    float n2 = fbm(noise_uv * 0.7 - vec2(TIME * noise_speed * 0.4, 0.0));\n    float n = (n1 + n2) * 0.5;\n\n    // Distort edge distance with noise for fiery/watery look\n    float distorted_d = d + (n - 0.5) * noise_intensity * border_width;\n\n    float vignette = 1.0 - smoothstep(0.0, border_width, distorted_d);\n    // Softer inner glow layer\n    float inner_glow = 1.0 - smoothstep(0.0, border_width * 1.8, distorted_d);\n\n    // Two-tone color blending driven by noise\n    vec3 col = mix(border_color.rgb, border_color_2.rgb, n);\n    float base_alpha = mix(border_color.a, border_color_2.a, n * 0.5);\n\n    // Combine sharp border + soft inner glow\n    float alpha = max(vignette * base_alpha, inner_glow * base_alpha * 0.3);\n    COLOR = vec4(col, alpha * pulse_alpha);\n}\n";
		ShaderMaterial shaderMaterial = new ShaderMaterial();
		shaderMaterial.Shader = shader;
		shaderMaterial.SetShaderParameter("border_color", borderColor);
		shaderMaterial.SetShaderParameter("border_color_2", borderColor2);
		shaderMaterial.SetShaderParameter("border_width", borderWidth);
		shaderMaterial.SetShaderParameter("noise_scale", noiseScale);
		shaderMaterial.SetShaderParameter("noise_speed", noiseSpeed);
		shaderMaterial.SetShaderParameter("noise_intensity", noiseIntensity);
		shaderMaterial.SetShaderParameter("noise_direction", noiseDirection);
		shaderMaterial.SetShaderParameter("pulse_alpha", 1f);
		colorRect.Material = shaderMaterial;
		canvasLayer.AddChild(colorRect, forceReadableName: false, Node.InternalMode.Disabled);
		NCombatRoom.Instance.AddChild(canvasLayer, forceReadableName: false, Node.InternalMode.Disabled);
		colorRect.Modulate = new Color(1f, 1f, 1f, 0f);
		Tween tween = colorRect.CreateTween();
		tween.TweenProperty(colorRect, "modulate:a", 1f, 0.30000001192092896).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		tween.TweenInterval(0.5);
		tween.TweenProperty(colorRect, "modulate:a", 0f, 1.7000000476837158).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		tween.TweenCallback(Callable.From(delegate
		{
			if (GodotObject.IsInstanceValid(canvasLayer))
			{
				canvasLayer.QueueFree();
			}
		}));
		return canvasLayer;
	}

	public static void RemoveBorder(ref Node? borderVfx)
	{
		Node node = borderVfx;
		borderVfx = null;
		if (node == null || !GodotObject.IsInstanceValid(node))
		{
			return;
		}
		if (node is CanvasLayer canvasLayer && canvasLayer.GetChildCount() > 0 && canvasLayer.GetChild(0) is Control control)
		{
			Tween tween = control.CreateTween();
			tween.TweenProperty(control, "modulate:a", 0f, 0.25);
			tween.TweenCallback(Callable.From(delegate
			{
				if (GodotObject.IsInstanceValid(node))
				{
					node.QueueFree();
				}
			}));
		}
		else
		{
			node.QueueFree();
		}
	}

	public static Node2D CreateCalmVfx()
	{
		Node2D obj = new Node2D
		{
			Name = "CalmVfx"
		};
		CpuParticles2D cpuParticles2D = CreateFogParticles(8, 2.0, new Vector2(30f, 40f), 2f, 2.7f, new Color(0.5f, 0.65f, 1f), 0.15f, 40f);
		cpuParticles2D.Position = new Vector2(0f, -120f);
		obj.AddChild(cpuParticles2D, forceReadableName: false, Node.InternalMode.Disabled);
		CpuParticles2D cpuParticles2D2 = CreateParticles(10, 0.8, new Vector2(20f, 40f), new Vector2(-1f, -0.3f), 30f, 50f, 160f, 0.3f, 0.6f, new Color(0.25f, 0.7f, 1f, 0.35f), new Color(0.2f, 0.65f, 1f, 0f), "res://images/vfx/stance/calm_orb.png");
		cpuParticles2D2.Position = new Vector2(60f, -100f);
		obj.AddChild(cpuParticles2D2, forceReadableName: false, Node.InternalMode.Disabled);
		return obj;
	}

	public static Node2D CreateWrathVfx()
	{
		Node2D obj = new Node2D
		{
			Name = "WrathVfx"
		};
		CpuParticles2D cpuParticles2D = CreateFogParticles(8, 2.0, new Vector2(30f, 30f), 2f, 2.7f, new Color(0.65f, 0f, 0.15f), 0.15f, 40f);
		cpuParticles2D.Position = new Vector2(0f, -110f);
		obj.AddChild(cpuParticles2D, forceReadableName: false, Node.InternalMode.Disabled);
		CpuParticles2D cpuParticles2D2 = CreateParticles(16, 1.5, new Vector2(35f, 15f), new Vector2(0f, -1f), 25f, 30f, 70f, 0.15f, 0.45f, new Color(0.8f, 0f, 0.1f, 0.4f), new Color(0.6f, 0f, 0.15f, 0f));
		cpuParticles2D2.Position = new Vector2(0f, -85f);
		obj.AddChild(cpuParticles2D2, forceReadableName: false, Node.InternalMode.Disabled);
		return obj;
	}

	public static Node2D CreateDivinityVfx()
	{
		Node2D obj = new Node2D
		{
			Name = "DivinityVfx"
		};
		CpuParticles2D cpuParticles2D = CreateFogParticles(10, 2.0, new Vector2(30f, 35f), 2f, 2.7f, new Color(0.65f, 0.05f, 0.65f), 0.15f, 40f);
		cpuParticles2D.Position = new Vector2(0f, -130f);
		obj.AddChild(cpuParticles2D, forceReadableName: false, Node.InternalMode.Disabled);
		CpuParticles2D cpuParticles2D2 = CreateParticles(14, 1.5, new Vector2(25f, 25f), new Vector2(0f, -1f), 180f, 15f, 45f, 0.15f, 0.4f, new Color(0.9f, 0.6f, 0.9f, 0.4f), new Color(0.8f, 0.5f, 0.8f, 0f));
		cpuParticles2D2.Position = new Vector2(0f, -130f);
		obj.AddChild(cpuParticles2D2, forceReadableName: false, Node.InternalMode.Disabled);
		return obj;
	}

	private static Texture2D? LoadCached(ref Texture2D? cache, string path)
	{
		if (cache != null)
		{
			return cache;
		}
		cache = WatcherTextureHelper.LoadTexture(path);
		return cache;
	}

	private static CpuParticles2D CreateFogParticles(int amount, double lifetime, Vector2 emissionExtents, float scaleMin, float scaleMax, Color peakColor, float peakAlpha, float angularVelocity)
	{
		CpuParticles2D cpuParticles2D = new CpuParticles2D();
		cpuParticles2D.Emitting = true;
		cpuParticles2D.Amount = amount;
		cpuParticles2D.Lifetime = lifetime;
		cpuParticles2D.OneShot = false;
		cpuParticles2D.Preprocess = lifetime;
		cpuParticles2D.Randomness = 1f;
		cpuParticles2D.LifetimeRandomness = 0.3;
		cpuParticles2D.LocalCoords = true;
		Texture2D texture2D = LoadCached(ref _exhaustTexture, "res://images/vfx/stance/exhaust_l.png");
		if (texture2D != null)
		{
			cpuParticles2D.Texture = texture2D;
		}
		CanvasItemMaterial canvasItemMaterial = new CanvasItemMaterial();
		canvasItemMaterial.BlendMode = CanvasItemMaterial.BlendModeEnum.Add;
		cpuParticles2D.Material = canvasItemMaterial;
		cpuParticles2D.EmissionShape = CpuParticles2D.EmissionShapeEnum.Rectangle;
		cpuParticles2D.EmissionRectExtents = emissionExtents;
		cpuParticles2D.Direction = new Vector2(0f, -1f);
		cpuParticles2D.Spread = 30f;
		cpuParticles2D.Gravity = Vector2.Zero;
		cpuParticles2D.InitialVelocityMin = 2f;
		cpuParticles2D.InitialVelocityMax = 8f;
		cpuParticles2D.ScaleAmountMin = scaleMin;
		cpuParticles2D.ScaleAmountMax = scaleMax;
		cpuParticles2D.AngularVelocityMin = 0f - angularVelocity;
		cpuParticles2D.AngularVelocityMax = angularVelocity;
		Gradient gradient = new Gradient();
		gradient.Offsets = new float[3] { 0f, 0.3f, 1f };
		gradient.Colors = new Color[3]
		{
			new Color(peakColor.R, peakColor.G, peakColor.B, 0f),
			new Color(peakColor.R, peakColor.G, peakColor.B, peakAlpha),
			new Color(peakColor.R, peakColor.G, peakColor.B, 0f)
		};
		cpuParticles2D.ColorRamp = gradient;
		return cpuParticles2D;
	}

	private static CpuParticles2D CreateParticles(int amount, double lifetime, Vector2 emissionExtents, Vector2 direction, float spread, float velocityMin, float velocityMax, float scaleMin, float scaleMax, Color colorStart, Color colorEnd, string? texturePath = null)
	{
		CpuParticles2D cpuParticles2D = new CpuParticles2D();
		cpuParticles2D.Emitting = true;
		cpuParticles2D.Amount = amount;
		cpuParticles2D.Lifetime = lifetime;
		cpuParticles2D.OneShot = false;
		cpuParticles2D.Preprocess = lifetime;
		cpuParticles2D.Randomness = 1f;
		cpuParticles2D.LifetimeRandomness = 0.4;
		cpuParticles2D.LocalCoords = true;
		Texture2D texture2D = ((texturePath == null) ? LoadCached(ref _glowSparkTexture, "res://images/vfx/stance/glow_spark.png") : LoadCached(ref _calmOrbTexture, texturePath));
		if (texture2D != null)
		{
			cpuParticles2D.Texture = texture2D;
		}
		CanvasItemMaterial canvasItemMaterial = new CanvasItemMaterial();
		canvasItemMaterial.BlendMode = CanvasItemMaterial.BlendModeEnum.Add;
		cpuParticles2D.Material = canvasItemMaterial;
		cpuParticles2D.EmissionShape = CpuParticles2D.EmissionShapeEnum.Rectangle;
		cpuParticles2D.EmissionRectExtents = emissionExtents;
		cpuParticles2D.Direction = direction;
		cpuParticles2D.Spread = spread;
		cpuParticles2D.Gravity = Vector2.Zero;
		cpuParticles2D.InitialVelocityMin = velocityMin;
		cpuParticles2D.InitialVelocityMax = velocityMax;
		cpuParticles2D.ScaleAmountMin = scaleMin;
		cpuParticles2D.ScaleAmountMax = scaleMax;
		Gradient gradient = new Gradient();
		gradient.SetColor(0, colorStart);
		gradient.SetColor(1, colorEnd);
		cpuParticles2D.ColorRamp = gradient;
		return cpuParticles2D;
	}
}
