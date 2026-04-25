#if ENABLE_BELIEVE_IN_YOU_TEMPLATE
using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;

namespace CardEffectTweaks.Cards;

// Disabled template for replacing BelieveInYou.OnPlay.
// Before enabling this symbol, replace the body with the intended effect and
// update the card text/localization plus manual multiplayer/singleplayer tests.
[HarmonyPatch(typeof(BelieveInYou), "OnPlay")]
internal static class BelieveInYouTemplatePatch
{
	[HarmonyPrefix]
	private static bool ReplaceOnPlay(BelieveInYou __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		__result = PlayerCmd.GainEnergy(__instance.DynamicVars.Energy.IntValue, cardPlay.Target.Player);
		return false;
	}
}
#endif
