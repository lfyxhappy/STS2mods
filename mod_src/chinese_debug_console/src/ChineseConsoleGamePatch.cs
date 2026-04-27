using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;

namespace ChineseDebugConsole;

[HarmonyPatch(typeof(NGame), nameof(NGame._Ready))]
internal static class ChineseConsoleReadyPatch
{
	private static void Postfix(NGame __instance)
	{
		ChineseConsoleOverlay.EnsureMounted(__instance);
	}
}

[HarmonyPatch(typeof(NGame), nameof(NGame._Input))]
internal static class ChineseConsoleInputPatch
{
	private static bool Prefix(InputEvent inputEvent)
	{
		ChineseConsoleOverlay? overlay = ChineseConsoleOverlay.Instance;
		if (overlay == null)
		{
			return true;
		}

		return !overlay.HandleGlobalInput(inputEvent);
	}
}
