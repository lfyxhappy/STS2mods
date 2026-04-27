using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace WatcherMod;

public sealed class Mockery : WatcherCard
{
	internal bool IsGeneratedCopy { get; set; }

	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[3]
	{
		WatcherHoverTips.Stance,
		HoverTipFactory.FromPower<Wrath>(),
		HoverTipFactory.FromCard<Persuasion>()
	});

	public Mockery()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllAllies)
	{
	}

	protected override void AddExtraArgsToDescription(LocString description)
	{
		description.Add("ShowGenerate", !IsGeneratedCopy);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		IEnumerable<Creature> enumerable = from c in base.CombatState.GetTeammatesOf(base.Owner.Creature)
			where c != null && c.IsAlive && c.IsPlayer && c != base.Owner.Creature
			select c;
		foreach (Creature item in enumerable)
		{
			if (item.Player != null)
			{
				await WatcherCombatHelper.EnterWrath(item.Player, this);
			}
		}
		if (!IsGeneratedCopy)
		{
			Persuasion persuasion = base.CombatState.CreateCard<Persuasion>(base.Owner);
			persuasion.IsGeneratedCopy = true;
			persuasion.EnergyCost.SetCustomBaseCost(0);
			await CardPileCmd.AddGeneratedCardToCombat(persuasion, PileType.Hand, addedByPlayer: true);
		}
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}
}
