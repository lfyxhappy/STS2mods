using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

[HarmonyPatch(typeof(AncientDialogueSet), "GetValidDialogues")]
internal static class WatcherDialogueDiagnosticPatch
{
	private static readonly string WatcherEntry = ModelDb.GetId(typeof(Watcher)).Entry;

	private static void Postfix(IEnumerable<AncientDialogue> __result, ModelId characterId, int charVisits, int totalVisits, bool allowAnyCharacterDialogues)
	{
		if (characterId.Entry != WatcherEntry)
		{
			return;
		}
		int num = 0;
		foreach (AncientDialogue item in __result)
		{
			_ = item;
			num++;
		}
		GD.Print($"[Watcher] GetValidDialogues: char={characterId.Entry} charVisits={charVisits} totalVisits={totalVisits} allowAgnostic={allowAnyCharacterDialogues} → {num} dialogue(s)");
		if (num == 0)
		{
			GD.PrintErr("[Watcher] WARNING: GetValidDialogues returned EMPTY for Watcher — event will get stuck!");
		}
	}
}
