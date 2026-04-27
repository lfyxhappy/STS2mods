using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace WatcherMod;

public sealed class JudgmentV2 : WatcherCard
{
	private const int _baseThreshold = 30;

	private int _currentThreshold = 30;

	private int _increasedThreshold;

	[SavedProperty]
	public int CurrentThreshold
	{
		get
		{
			return _currentThreshold;
		}
		set
		{
			AssertMutable();
			_currentThreshold = value;
			base.DynamicVars["MagicNumber"].BaseValue = _currentThreshold;
		}
	}

	[SavedProperty]
	public int IncreasedThreshold
	{
		get
		{
			return _increasedThreshold;
		}
		set
		{
			AssertMutable();
			_increasedThreshold = value;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DynamicVar("MagicNumber", CurrentThreshold),
		new DynamicVar("GainAmount", 2m)
	});

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	public JudgmentV2()
		: base(1, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		if (cardPlay.Target.CurrentHp <= base.DynamicVars["MagicNumber"].IntValue)
		{
			await CreatureCmd.Kill(cardPlay.Target);
			int intValue = base.DynamicVars["GainAmount"].IntValue;
			BumpFromKill(intValue);
			(base.DeckVersion as JudgmentV2)?.BumpFromKill(intValue);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["GainAmount"].UpgradeValueBy(1m);
	}

	protected override void AfterDowngraded()
	{
		UpdateThreshold();
	}

	private void BumpFromKill(int extra)
	{
		IncreasedThreshold += extra;
		UpdateThreshold();
	}

	private void UpdateThreshold()
	{
		CurrentThreshold = 30 + IncreasedThreshold;
	}
}
