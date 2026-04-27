using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace WatcherMod;

public sealed class DrawTalisman : WatcherCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlyArray<CardKeyword>(new CardKeyword[2]
	{
		CardKeyword.Exhaust,
		CardKeyword.Innate
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("MagicNumber", 2m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		WatcherHoverTips.Enchantment,
		WatcherHoverTips.Directed
	});

	public DrawTalisman()
		: base(2, CardType.Skill, CardRarity.Ancient, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		PlayerCombatState combat = base.Owner.PlayerCombatState;
		if (combat == null)
		{
			return;
		}
		IReadOnlyList<EnchantmentModel> pool = WatcherEnchantStack.RandomPool;
		if (pool.Count == 0)
		{
			return;
		}
		int times = base.DynamicVars["MagicNumber"].IntValue;
		CardModel optRandom = base.CombatState.CreateCard<DrawTalismanRandomDeck>(base.Owner);
		CardModel cardModel = base.CombatState.CreateCard<DrawTalismanDirectedHand>(base.Owner);
		if (base.IsUpgraded)
		{
			optRandom.UpgradeInternal();
			cardModel.UpgradeInternal();
		}
		CardModel cardModel2 = await WatcherCombatHelper.ChooseOne(base.Owner, new global::_003C_003Ez__ReadOnlyArray<CardModel>(new CardModel[2] { optRandom, cardModel }), new LocString("cards", "DRAW_TALISMAN.selectionScreenPrompt"));
		if (cardModel2 == null)
		{
			return;
		}
		uint seed = base.Owner.RunState.Rng.Seed;
		int valueOrDefault = (base.Owner.Creature?.CombatState?.RoundNumber).GetValueOrDefault();
		Rng rng = new Rng(seed ^ (uint)(int)(valueOrDefault * 2654435761u));
		List<CardModel> list;
		bool flag;
		if (cardModel2 == optRandom)
		{
			list = (from c in combat.AllPiles.SelectMany((CardPile p) => p.Cards)
				where c != this
				select c).ToList();
			flag = false;
		}
		else
		{
			list = combat.Hand.Cards.Where((CardModel c) => c != this).ToList();
			flag = true;
		}
		foreach (CardModel card in list)
		{
			for (int num = 0; num < times; num++)
			{
				IReadOnlyList<EnchantmentModel> readOnlyList;
				if (!flag)
				{
					readOnlyList = pool;
				}
				else
				{
					IReadOnlyList<EnchantmentModel> readOnlyList2 = pool.Where((EnchantmentModel e) => e.CanEnchantCardType(card.Type)).ToList();
					readOnlyList = readOnlyList2;
				}
				IReadOnlyList<EnchantmentModel> readOnlyList3 = readOnlyList;
				if (readOnlyList3.Count != 0)
				{
					EnchantmentModel canonical = readOnlyList3[rng.NextInt(readOnlyList3.Count)];
					WatcherEnchantStack.ApplyTempEnchantment(card, canonical);
				}
			}
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(1m);
	}
}
