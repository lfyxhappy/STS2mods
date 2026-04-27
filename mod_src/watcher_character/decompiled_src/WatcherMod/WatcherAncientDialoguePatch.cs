using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;

namespace WatcherMod;

[HarmonyPatch(typeof(AncientDialogueSet), "PopulateLocKeys")]
internal static class WatcherAncientDialoguePatch
{
	private static void Postfix(AncientDialogueSet __instance, string ancientEntry)
	{
		IReadOnlyList<AncientDialogue> dialoguesForAncient = GetDialoguesForAncient(ancientEntry);
		if (dialoguesForAncient != null)
		{
			try
			{
				WatcherDialogueHelper.InjectWatcherDialogue(__instance, ancientEntry, dialoguesForAncient);
			}
			catch (Exception value)
			{
				GD.PrintErr($"[Watcher] Dialogue injection failed for {ancientEntry}: {value}");
			}
		}
	}

	private static IReadOnlyList<AncientDialogue>? GetDialoguesForAncient(string entry)
	{
		return entry switch
		{
			"NEOW" => new AncientDialogue[3]
			{
				WatcherDialogueHelper.Lines(1, 0),
				WatcherDialogueHelper.Lines(1, 1),
				WatcherDialogueHelper.Lines(1, 4)
			}, 
			"NONUPEIPE" => new AncientDialogue[3]
			{
				WatcherDialogueHelper.Lines(1, 0),
				WatcherDialogueHelper.Lines(1, 1),
				WatcherDialogueHelper.Lines(1, 4)
			}, 
			"DARV" => new AncientDialogue[3]
			{
				WatcherDialogueHelper.Lines(1, 0),
				WatcherDialogueHelper.Lines(1, 1),
				WatcherDialogueHelper.Lines(1, 4)
			}, 
			"VAKUU" => new AncientDialogue[3]
			{
				WatcherDialogueHelper.Lines(1, 0),
				WatcherDialogueHelper.Lines(1, 1),
				WatcherDialogueHelper.Lines(1, 4)
			}, 
			"PAEL" => new AncientDialogue[3]
			{
				WatcherDialogueHelper.Lines(1, 0),
				WatcherDialogueHelper.Lines(1, 1),
				WatcherDialogueHelper.Lines(1, 4)
			}, 
			"OROBAS" => new AncientDialogue[3]
			{
				WatcherDialogueHelper.Lines(1, 0),
				WatcherDialogueHelper.Lines(1, 1),
				WatcherDialogueHelper.Lines(1, 4)
			}, 
			"TANX" => new AncientDialogue[3]
			{
				WatcherDialogueHelper.Lines(1, 0),
				WatcherDialogueHelper.Lines(1, 1),
				WatcherDialogueHelper.Lines(1, 4)
			}, 
			"TEZCATARA" => new AncientDialogue[3]
			{
				WatcherDialogueHelper.Lines(1, 0),
				WatcherDialogueHelper.Lines(1, 1),
				WatcherDialogueHelper.Lines(1, 4)
			}, 
			"THE_ARCHITECT" => new AncientDialogue[3]
			{
				WatcherDialogueHelper.ArchitectLines(1, 0),
				WatcherDialogueHelper.ArchitectLines(1, 1),
				WatcherDialogueHelper.ArchitectLines(1, 2)
			}, 
			_ => null, 
		};
	}
}
