using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace WatcherMod;

[HarmonyPatch(typeof(NCardLibrary), "_Ready")]
internal static class WatcherCardLibraryPatch
{
	private static void Postfix(NCardLibrary __instance)
	{
		WatcherCardLibraryInjector.Inject(__instance);
	}
}
