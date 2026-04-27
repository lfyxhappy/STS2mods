using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace WatcherMod;

public sealed class AskHeaven : WatcherCard, IProphecyCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new PowerVar<KnowFatePower>(5m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<KnowFatePower>());

	public AskHeaven()
		: base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		Creature target = cardPlay.Target;
		CardModel attackTok = await WatcherCombatHelper.CreateWatcherCard<AskHeavenAttack>(base.Owner);
		CardModel item = await WatcherCombatHelper.CreateWatcherCard<AskHeavenOther>(base.Owner);
		List<CardModel> options = new List<CardModel> { attackTok, item };
		CardModel cardModel = await WatcherCombatHelper.ChooseOne(base.Owner, options, new LocString("card_selection", "TO_PLAY"));
		if (cardModel != null)
		{
			bool num = cardModel is AskHeavenAttack;
			bool valueOrDefault = target.Monster?.NextMove?.Intents.OfType<AttackIntent>().Any() == true;
			if (num == valueOrDefault)
			{
				await PowerCmd.Apply<KnowFatePower>(base.Owner.Creature, base.DynamicVars[typeof(KnowFatePower).Name].BaseValue, base.Owner.Creature, this);
			}
			await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
			{
				Source = this,
				AffectedEnemy = target
			});
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars[typeof(KnowFatePower).Name].UpgradeValueBy(2m);
	}
}
