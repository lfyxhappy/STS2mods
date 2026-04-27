using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace WatcherMod;

public class Wish_P : WatcherCard
{
	public override string PortraitPath => "res://images/packed/card_portraits/watcher/wish.png";

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("MagicNumber", 25m));

	public Wish_P()
		: base(3, CardType.Skill, CardRarity.Rare, TargetType.None)
	{
	}

	protected virtual Task<bool> TryConsumeKnowFateBoost()
	{
		return Task.FromResult(result: false);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		bool flag = await TryConsumeKnowFateBoost();
		CardModel optStrength = base.CombatState.CreateCard<WishAlmighty>(base.Owner);
		CardModel optArmor = base.CombatState.CreateCard<WishLiveForever>(base.Owner);
		CardModel optGold = base.CombatState.CreateCard<WishFameAndFortune>(base.Owner);
		bool boosted = base.IsUpgraded || flag;
		if (boosted)
		{
			optStrength.UpgradeInternal();
			optArmor.UpgradeInternal();
			optGold.UpgradeInternal();
		}
		List<CardModel> options = new List<CardModel>(3) { optStrength, optArmor, optGold };
		CardModel cardModel = await WatcherCombatHelper.ChooseOne(base.Owner, options, new LocString("cards", "WISH_P.selectionScreenPrompt"));
		if (cardModel == optStrength)
		{
			int num = (boosted ? 4 : 3);
			await PowerCmd.Apply<StrengthPower>(base.Owner.Creature, num, base.Owner.Creature, this);
		}
		else if (cardModel == optArmor)
		{
			int num2 = (boosted ? 8 : 6);
			await PowerCmd.Apply<WishPlatedArmorPower>(base.Owner.Creature, num2, base.Owner.Creature, this);
		}
		else if (cardModel == optGold)
		{
			await PlayerCmd.GainGold(boosted ? 30 : 25, base.Owner);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(5m);
	}
}
