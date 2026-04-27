using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

[HarmonyPatch(typeof(CardPileCmd), "AddGeneratedCardToCombat", new Type[]
{
	typeof(CardModel),
	typeof(PileType),
	typeof(bool),
	typeof(CardPilePosition)
})]
internal static class MasterRealityPatch
{
	private static void Prefix(CardModel card)
	{
		Player owner = card.Owner;
		if (owner != null && owner.Creature?.HasPower<MasterRealityPower>() == true && card.IsUpgradable)
		{
			CardCmd.Upgrade(card);
		}
	}
}
