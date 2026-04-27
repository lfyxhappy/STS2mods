using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class Manifest : WatcherCard, IProphecyCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar("MagicNumber", 4));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromCard<Insight>());

	public Manifest()
		: base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		PlayerCombatState playerCombatState = base.Owner.PlayerCombatState;
		if (playerCombatState != null)
		{
			int intValue = base.DynamicVars["MagicNumber"].IntValue;
			int effectiveScryAmount = WatcherCombatHelper.GetEffectiveScryAmount(base.Owner, intValue);
			List<CardModel> source = playerCombatState.DrawPile.Cards.Take(effectiveScryAmount).ToList();
			int statusCount = source.Count((CardModel c) => c.Type == CardType.Status);
			await WatcherCombatHelper.Scry(choiceContext, base.Owner, intValue, this);
			for (int i = 0; i < statusCount; i++)
			{
				await CardPileCmd.AddGeneratedCardToCombat(await WatcherCombatHelper.CreateWatcherCard<Insight>(base.Owner), PileType.Hand, addedByPlayer: true);
			}
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(2m);
	}
}
