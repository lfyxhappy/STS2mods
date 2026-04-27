using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

internal static class WatcherSimpleBHelper
{
	public static async Task<IEnumerable<CardModel>> TakeFromDiscard(PlayerChoiceContext context, Player owner, int count)
	{
		List<CardModel> list = PileType.Discard.GetPile(owner).Cards.ToList();
		if (list.Count == 0 || count <= 0)
		{
			return Array.Empty<CardModel>();
		}
		int num = Math.Min(count, list.Count);
		CardSelectorPrefs prefs = WatcherCombatHelper.SetCancelable(new CardSelectorPrefs(new LocString("card_selection", "TO_UPGRADE"), num, num), value: false);
		return await CardSelectCmd.FromSimpleGrid(context, list, owner, prefs);
	}
}
