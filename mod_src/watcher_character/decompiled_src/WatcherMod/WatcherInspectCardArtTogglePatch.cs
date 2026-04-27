using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens;

namespace WatcherMod;

[HarmonyPatch(typeof(NInspectCardScreen), "SetCard")]
internal static class WatcherInspectCardArtTogglePatch
{
	private static void Postfix(NInspectCardScreen __instance)
	{
		WatcherInspectCardArtToggleInjector.UpdateToggle(__instance);
	}
}
