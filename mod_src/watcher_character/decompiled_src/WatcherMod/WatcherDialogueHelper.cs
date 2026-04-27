using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

internal static class WatcherDialogueHelper
{
	private static readonly string WatcherEntry = ModelDb.GetId(typeof(Watcher)).Entry;

	public static void InjectWatcherDialogue(AncientDialogueSet dialogueSet, string ancientEntry, IReadOnlyList<AncientDialogue> dialogues)
	{
		if (dialogueSet.CharacterDialogues.ContainsKey(WatcherEntry))
		{
			return;
		}
		dialogueSet.CharacterDialogues[WatcherEntry] = dialogues;
		for (int i = 0; i < dialogues.Count; i++)
		{
			dialogues[i].PopulateLines(ancientEntry, WatcherEntry, i);
			IReadOnlyList<AncientDialogueLine> lines = dialogues[i].Lines;
			for (int j = 0; j < lines.Count - 1; j++)
			{
				AncientDialogueLine ancientDialogueLine = lines[j];
				if (ancientDialogueLine.LineText != null)
				{
					string locEntryKey = ancientDialogueLine.LineText.LocEntryKey;
					string text = locEntryKey.Substring(0, locEntryKey.LastIndexOf('.'));
					ancientDialogueLine.NextButtonText = new LocString("ancients", text + ".next");
				}
			}
		}
	}

	public static AncientDialogue Lines(int lineCount, int? visitIndex = null)
	{
		string[] array = new string[lineCount];
		Array.Fill(array, "");
		return new AncientDialogue(array)
		{
			VisitIndex = visitIndex
		};
	}

	public static AncientDialogue ArchitectLines(int lineCount, int? visitIndex = null, ArchitectAttackers endAttackers = ArchitectAttackers.Both, ArchitectAttackers startAttackers = ArchitectAttackers.None)
	{
		string[] array = new string[lineCount];
		Array.Fill(array, "");
		return new AncientDialogue(array)
		{
			VisitIndex = visitIndex,
			StartAttackers = startAttackers,
			EndAttackers = endAttackers
		};
	}
}
