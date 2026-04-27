using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;

namespace WatcherMod;

[HarmonyPatch(typeof(AttackCommand), "WithHitFx")]
internal static class AttackCommandWithHitFxPatch
{
	private static void Prefix(ref string? vfx)
	{
		if (CataclysmVfxRandomizer.Active && vfx != null)
		{
			vfx = CataclysmVfxRandomizer.NextVfx();
		}
	}
}
