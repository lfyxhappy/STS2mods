using System;

namespace WatcherMod;

internal static class CataclysmVfxRandomizer
{
	private static readonly string[] VfxPool = new string[18]
	{
		"vfx/vfx_attack_slash", "vfx/vfx_attack_blunt", "vfx/vfx_attack_lightning", "vfx/vfx_bite", "vfx/vfx_bloody_impact", "vfx/vfx_chain", "vfx/vfx_flying_slash", "vfx/vfx_giant_horizontal_slash", "vfx/vfx_dagger_throw", "vfx/vfx_dagger_spray",
		"vfx/vfx_dramatic_stab", "vfx/vfx_rock_shatter", "vfx/vfx_scratch", "vfx/vfx_sandy_impact", "vfx/vfx_slime_impact", "vfx/vfx_thrash", "vfx/vfx_heavy_blunt", "vfx/vfx_starry_impact"
	};

	private static readonly Random Rng = new Random();

	[ThreadStatic]
	private static int _depth;

	public static bool Active => _depth > 0;

	public static void Enter()
	{
		_depth++;
	}

	public static void Exit()
	{
		if (_depth > 0)
		{
			_depth--;
		}
	}

	public static string NextVfx()
	{
		return VfxPool[Rng.Next(VfxPool.Length)];
	}
}
