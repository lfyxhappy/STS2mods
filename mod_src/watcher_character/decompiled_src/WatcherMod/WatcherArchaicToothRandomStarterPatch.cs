using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Random;

namespace WatcherMod;

[HarmonyPatch(typeof(ArchaicTooth), "GetTranscendenceStarterCard")]
internal static class WatcherArchaicToothRandomStarterPatch
{
	private static bool Prefix(Player player, ref CardModel? __result)
	{
		PropertyInfo property = typeof(ArchaicTooth).GetProperty("TranscendenceUpgrades", BindingFlags.Static | BindingFlags.NonPublic);
		if (property == null)
		{
			return true;
		}
		Dictionary<ModelId, CardModel> upgrades = property.GetValue(null) as Dictionary<ModelId, CardModel>;
		if (upgrades == null)
		{
			return true;
		}
		List<CardModel> list = player.Deck.Cards.Where((CardModel c) => upgrades.ContainsKey(c.Id)).ToList();
		if (list.Count <= 1)
		{
			return true;
		}
		list.Sort((CardModel a, CardModel b) => string.CompareOrdinal(a.Id.Entry, b.Id.Entry));
		Rng rng = new Rng(player.RunState.Rng.Seed ^ 0x4ADC1A50);
		__result = rng.NextItem(list);
		return false;
	}
}
