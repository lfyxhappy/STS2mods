using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class OmniCognition : WatcherCard, IProphecyCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords
	{
		get
		{
			if (!base.IsUpgraded)
			{
				return Array.Empty<CardKeyword>();
			}
			return new CardKeyword[1] { CardKeyword.Retain };
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar("MagicNumber", 10));

	public OmniCognition()
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
			await WatcherCombatHelper.Scry(choiceContext, base.Owner, intValue, this);
			List<CardModel> list = ((!base.IsUpgraded) ? topPeek : topPeek.Where((CardModel c) => c.Type == CardType.Attack).ToList());
			if (list.Count != 0 && list.All((CardModel c) => combat.DiscardPile.Cards.Contains(c)))
			{
				await WatcherCombatHelper.TakeExtraTurn(base.Owner);
			}
		}
	}

	protected override void OnUpgrade()
	{
	}
}
