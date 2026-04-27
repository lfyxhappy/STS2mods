using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

internal static class WatcherCombatHelper
{
	private static readonly List<CardModel> _deferredRetainCards = new List<CardModel>();

	internal static void DeferRetainCard(CardModel card)
	{
		_deferredRetainCards.Add(card);
	}

	internal static async Task ProcessDeferredRetainCards()
	{
		if (_deferredRetainCards.Count == 0)
		{
			return;
		}
		foreach (CardModel deferredRetainCard in _deferredRetainCards)
		{
			await CardPileCmd.Add(deferredRetainCard, PileType.Hand);
		}
		_deferredRetainCards.Clear();
	}

	internal static void ClearDeferredRetainCards()
	{
		_deferredRetainCards.Clear();
	}

	public static async Task EnterWrath(Player owner, CardModel? source)
	{
		await ChangeStance<Wrath>(owner, source);
	}

	public static async Task EnterCalm(Player owner, CardModel? source)
	{
		await ChangeStance<Calm>(owner, source);
	}

	public static async Task EnterDivinity(Player owner, CardModel? source)
	{
		await ChangeStance<Divinity>(owner, source);
	}

	public static async Task ExitStance(Player owner)
	{
		Type currentStance = GetCurrentStance(owner.Creature);
		if (!(currentStance == null))
		{
			await RemoveAllStances(owner.Creature);
			await OnStanceChanged(owner, currentStance, null);
		}
	}

	public static bool IsInStance<T>(Creature creature) where T : PowerModel
	{
		return creature.HasPower<T>();
	}

	public static async Task GainMantra(Player owner, int amount, CardModel? source)
	{
		if (amount > 0)
		{
			WatcherStatePower obj = await EnsureState(owner);
			obj.TotalMantraGainedThisCombat += amount;
			obj.MantraGainedThisTurn += amount;
			await PowerCmd.Apply<Mantra>(owner.Creature, amount, owner.Creature, source);
		}
	}

	public static int GetTotalMantraGained(Player owner)
	{
		return owner.Creature.GetPower<WatcherStatePower>()?.TotalMantraGainedThisCombat ?? 0;
	}

	public static int GetMantraGainedThisTurn(Player owner)
	{
		return owner.Creature.GetPower<WatcherStatePower>()?.MantraGainedThisTurn ?? 0;
	}

	public static int GetAttacksPlayedThisTurn(Player owner)
	{
		return owner.Creature.GetPower<WatcherStatePower>()?.AttacksPlayedThisTurn ?? 0;
	}

	public static int GetProphecyPlaysThisTurn(Player owner)
	{
		return owner.Creature.GetPower<WatcherStatePower>()?.ProphecyPlaysThisTurn ?? 0;
	}

	public static int GetScryPlaysThisTurn(Player owner)
	{
		return owner.Creature.GetPower<WatcherStatePower>()?.ScryPlaysThisTurn ?? 0;
	}

	public static async Task<CardModel?> ChooseOne(Player owner, IReadOnlyList<CardModel> options, LocString prompt, bool cancelable = false)
	{
		return await CardSelectCmd.FromChooseACardScreen(new BlockingPlayerChoiceContext(), options, owner, cancelable);
	}

	public static int GetEffectiveScryAmount(Player owner, int amount)
	{
		if (amount <= 0)
		{
			return 0;
		}
		if (owner.Relics.Any((RelicModel relic) => relic.Id.Entry == "GOLDEN_EYE"))
		{
			amount += 2;
		}
		if (owner.Creature.HasPower<GuardNextScryPower>())
		{
			amount = Math.Max(0, amount - 2);
		}
		return amount;
	}

	public static async Task<List<CardModel>> Scry(PlayerChoiceContext choiceContext, Player owner, int amount, CardModel? source = null)
	{
		List<CardModel> emptyResult = new List<CardModel>();
		if (amount <= 0)
		{
			return emptyResult;
		}
		if (source != null)
		{
			owner.Creature.GetPower<WatcherStatePower>()?.RegisterScryPlay(source);
		}
		CardPile pile = PileType.Draw.GetPile(owner);
		if (pile.Cards.Count == 0)
		{
			await CardPileCmd.ShuffleIfNecessary(choiceContext, owner);
			pile = PileType.Draw.GetPile(owner);
			if (pile.Cards.Count == 0)
			{
				return emptyResult;
			}
		}
		int effectiveScryAmount = GetEffectiveScryAmount(owner, amount);
		List<CardModel> topCards = pile.Cards.Take(effectiveScryAmount).ToList();
		if (topCards.Count == 0)
		{
			return emptyResult;
		}
		GuardNextScryPower power = owner.Creature.GetPower<GuardNextScryPower>();
		if (power != null)
		{
			await PowerCmd.Remove(power);
			foreach (CardModel card in topCards.ToList())
			{
				CardPile? pile2 = card.Pile;
				if (pile2 != null && pile2.Type == PileType.Draw)
				{
					if (owner.Creature.IsDead)
					{
						break;
					}
					await CardPileCmd.Add(card, PileType.Play);
					await CardCmd.AutoPlay(choiceContext, card, null);
				}
			}
			await OnScry(owner);
			await WatcherProphecy.Trigger(owner, new ProphecyContext
			{
				Source = source,
				CardsDiscarded = topCards.Count,
				FromScry = true,
				PeekedCards = topCards
			});
			return topCards;
		}
		CardSelectorPrefs prefs = SetCancelable(new CardSelectorPrefs(new LocString("card_selection", "TO_DISCARD"), 0, topCards.Count), value: true);
		List<CardModel> selectedList = (await CardSelectCmd.FromSimpleGrid(choiceContext, topCards, owner, prefs)).ToList();
		foreach (CardModel item in selectedList)
		{
			await CardPileCmd.Add(item, PileType.Discard);
		}
		foreach (CardModel item2 in selectedList)
		{
			if (item2 is IOnScryDiscarded onScryDiscarded)
			{
				await onScryDiscarded.OnScryDiscarded(choiceContext, owner);
			}
		}
		await OnScry(owner);
		await WatcherProphecy.Trigger(owner, new ProphecyContext
		{
			Source = source,
			CardsDiscarded = selectedList.Count,
			FromScry = true,
			PeekedCards = topCards
		});
		return selectedList;
	}

	public static async Task<List<CardModel>> ScryAutoDiscard(PlayerChoiceContext choiceContext, Player owner, int amount, Func<IReadOnlyList<CardModel>, IEnumerable<CardModel>> selector, CardModel? source = null)
	{
		List<CardModel> empty = new List<CardModel>();
		if (amount <= 0)
		{
			return empty;
		}
		if (source != null)
		{
			owner.Creature.GetPower<WatcherStatePower>()?.RegisterScryPlay(source);
		}
		CardPile pile = PileType.Draw.GetPile(owner);
		if (pile.Cards.Count == 0)
		{
			await CardPileCmd.ShuffleIfNecessary(choiceContext, owner);
			pile = PileType.Draw.GetPile(owner);
			if (pile.Cards.Count == 0)
			{
				return empty;
			}
		}
		int effectiveScryAmount = GetEffectiveScryAmount(owner, amount);
		List<CardModel> topCards = pile.Cards.Take(effectiveScryAmount).ToList();
		if (topCards.Count == 0)
		{
			return empty;
		}
		GuardNextScryPower power = owner.Creature.GetPower<GuardNextScryPower>();
		if (power != null)
		{
			await PowerCmd.Remove(power);
			foreach (CardModel card in topCards.ToList())
			{
				CardPile? pile2 = card.Pile;
				if (pile2 != null && pile2.Type == PileType.Draw)
				{
					if (owner.Creature.IsDead)
					{
						break;
					}
					await CardPileCmd.Add(card, PileType.Play);
					await CardCmd.AutoPlay(choiceContext, card, null);
				}
			}
			await OnScry(owner);
			await WatcherProphecy.Trigger(owner, new ProphecyContext
			{
				Source = source,
				CardsDiscarded = topCards.Count,
				FromScry = true,
				PeekedCards = topCards
			});
			return topCards;
		}
		List<CardModel> discarded = selector(topCards).Distinct().ToList();
		foreach (CardModel item in discarded)
		{
			await CardPileCmd.Add(item, PileType.Discard);
		}
		foreach (CardModel item2 in discarded)
		{
			if (item2 is IOnScryDiscarded onScryDiscarded)
			{
				await onScryDiscarded.OnScryDiscarded(choiceContext, owner);
			}
		}
		await OnScry(owner);
		await WatcherProphecy.Trigger(owner, new ProphecyContext
		{
			Source = source,
			CardsDiscarded = discarded.Count,
			FromScry = true,
			PeekedCards = topCards
		});
		return discarded;
	}

	public static Task<CardModel> CreateWatcherCard<T>(Player owner) where T : CardModel
	{
		CardModel cardModel = owner.Creature.CombatState.CreateCard<T>(owner);
		UpgradeIfMasterReality(owner, cardModel);
		return Task.FromResult(cardModel);
	}

	public static CardModel CreateWatcherCard(Player owner, CardModel canonicalCard)
	{
		CardModel cardModel = owner.Creature.CombatState.CreateCard(canonicalCard, owner);
		UpgradeIfMasterReality(owner, cardModel);
		return cardModel;
	}

	private static void UpgradeIfMasterReality(Player owner, CardModel card)
	{
		if (owner.Creature.HasPower<MasterRealityPower>() && card.IsUpgradable)
		{
			CardCmd.Upgrade(card);
		}
	}

	public static async Task TakeExtraTurn(Player owner)
	{
		await PowerCmd.Apply<WatcherExtraTurnPower>(owner.Creature, 1m, owner.Creature, null);
		PlayerCmd.EndTurn(owner, canBackOut: false);
	}

	private static async Task ChangeStance<T>(Player owner, CardModel? source) where T : PowerModel
	{
		if (!owner.Creature.HasPower<CannotChangeStancePower>())
		{
			Type targetStance = typeof(T);
			Type currentStance = GetCurrentStance(owner.Creature);
			if (!(currentStance == targetStance))
			{
				await RemoveAllStances(owner.Creature);
				await PowerCmd.Apply<T>(owner.Creature, 1m, owner.Creature, source);
				await OnStanceChanged(owner, currentStance, targetStance);
			}
		}
	}

	private static Type? GetCurrentStance(Creature creature)
	{
		if (creature.HasPower<Divinity>())
		{
			return typeof(Divinity);
		}
		if (creature.HasPower<Wrath>())
		{
			return typeof(Wrath);
		}
		if (creature.HasPower<Calm>())
		{
			return typeof(Calm);
		}
		return null;
	}

	private static async Task RemoveAllStances(Creature creature)
	{
		await PowerCmd.Remove(creature.GetPower<Divinity>());
		await PowerCmd.Remove(creature.GetPower<Wrath>());
		await PowerCmd.Remove(creature.GetPower<Calm>());
	}

	private static async Task OnScry(Player owner)
	{
		if (owner.Creature.HasPower<NirvanaPower>())
		{
			await CreatureCmd.GainBlock(owner.Creature, owner.Creature.GetPowerAmount<NirvanaPower>(), ValueProp.Unpowered, null);
		}
		List<CardModel> list = PileType.Discard.GetPile(owner).Cards.Where((CardModel card) => card.Id.Entry == "WEAVE").ToList();
		foreach (CardModel item in list)
		{
			await CardPileCmd.Add(item, PileType.Hand);
		}
	}

	private static async Task OnStanceChanged(Player owner, Type? oldStance, Type? newStance)
	{
		UpdateEyeAnimation(owner.Creature, newStance);
		if (newStance == typeof(Wrath) && owner.Creature.HasPower<RushdownPower>())
		{
			WatcherAudioHelper.PlayOneShot("res://audio/watcher/mantra.ogg");
			int powerAmount = owner.Creature.GetPowerAmount<RushdownPower>();
			if (!PileType.Play.GetPile(owner).Cards.Any())
			{
				await CardPileCmd.Draw(new BlockingPlayerChoiceContext(), powerAmount, owner);
			}
			else
			{
				owner.Creature.GetPower<RushdownPower>().PendingDraws += powerAmount;
			}
		}
		List<CardModel> list = PileType.Discard.GetPile(owner).Cards.Where((CardModel card) => card is FlurryOfBlows).ToList();
		foreach (CardModel item in list)
		{
			await CardPileCmd.Add(item, PileType.Hand);
		}
		if (owner.Creature.HasPower<MentalFortressPower>() && oldStance != newStance)
		{
			await CreatureCmd.GainBlock(owner.Creature, owner.Creature.GetPowerAmount<MentalFortressPower>(), ValueProp.Unpowered, null);
		}
		if (oldStance == typeof(Calm) && owner.Relics.Any((RelicModel relic) => relic.Id.Entry == "VIOLET_LOTUS"))
		{
			await PlayerCmd.GainEnergy(1m, owner);
		}
	}

	internal static void UpdateEyeAnimation(Creature creature, Type? stance)
	{
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(creature);
		if (nCreature == null)
		{
			return;
		}
		Node nodeOrNull = nCreature.Visuals.GetNodeOrNull("EyeAnchor/EyeSprite");
		if (nodeOrNull == null)
		{
			return;
		}
		string animationName = ((stance == typeof(Wrath)) ? "Wrath" : ((stance == typeof(Divinity)) ? "Divinity" : ((!(stance == typeof(Calm))) ? "Calm" : "Calm")));
		try
		{
			new MegaSprite(nodeOrNull).GetAnimationState()?.SetAnimation(animationName);
		}
		catch
		{
		}
	}

	private static async Task<WatcherStatePower> EnsureState(Player owner)
	{
		WatcherStatePower power = owner.Creature.GetPower<WatcherStatePower>();
		if (power != null)
		{
			return power;
		}
		return (await PowerCmd.Apply<WatcherStatePower>(owner.Creature, 1m, owner.Creature, null)) ?? owner.Creature.GetPower<WatcherStatePower>() ?? throw new InvalidOperationException("Failed to create WatcherStatePower.");
	}

	internal static CardSelectorPrefs SetCancelable(CardSelectorPrefs prefs, bool value)
	{
		object obj = prefs;
		typeof(CardSelectorPrefs).GetProperty("Cancelable", BindingFlags.Instance | BindingFlags.Public).SetValue(obj, value);
		return (CardSelectorPrefs)obj;
	}
}
