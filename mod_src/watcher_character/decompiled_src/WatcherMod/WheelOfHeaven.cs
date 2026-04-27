using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class WheelOfHeaven : WatcherCard, IProphecyCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new CardsVar("MagicNumber", 8),
		new DynamicVar("Threshold", 5m)
	});

	public WheelOfHeaven()
		: base(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		PlayerCombatState combat = base.Owner.PlayerCombatState;
		if (combat != null)
		{
			int intValue = base.DynamicVars["MagicNumber"].IntValue;
			int effectiveScryAmount = WatcherCombatHelper.GetEffectiveScryAmount(base.Owner, intValue);
			List<CardModel> topPeek = combat.DrawPile.Cards.Take(effectiveScryAmount).ToList();
			int prophecyInPeek = topPeek.Count((CardModel c) => c is IProphecyCard);
			await WatcherCombatHelper.Scry(choiceContext, base.Owner, intValue, this);
			int intValue2 = base.DynamicVars["Threshold"].IntValue;
			bool num = prophecyInPeek >= intValue2;
			bool flag = topPeek.Count > 0 && topPeek.All((CardModel c) => combat.DiscardPile.Cards.Contains(c));
			if (num || flag)
			{
				await WatcherCombatHelper.TakeExtraTurn(base.Owner);
			}
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["Threshold"].UpgradeValueBy(-1m);
	}
}
