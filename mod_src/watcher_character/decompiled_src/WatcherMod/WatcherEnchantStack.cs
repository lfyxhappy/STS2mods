using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Rooms;

namespace WatcherMod;

public static class WatcherEnchantStack
{
	private static readonly object _sentinel = new object();

	private static readonly ConditionalWeakTable<CardModel, List<EnchantmentModel>> _extras = new ConditionalWeakTable<CardModel, List<EnchantmentModel>>();

	private static readonly ConditionalWeakTable<EnchantmentModel, object> _temp = new ConditionalWeakTable<EnchantmentModel, object>();

	private static readonly HashSet<string> _excludedFromRandomPool = new HashSet<string>
	{
		"Goopy", "Steady", "RoyallyApproved", "TezcatarasEmber", "SoulsPower", "Adroit", "Corrupted", "Swift", "Sown", "Momentum",
		"Inky", "Clone", "Imbued"
	};

	private static List<EnchantmentModel>? _cachedRandomPool;

	private static bool _subscribedCombatEnded;

	public static IReadOnlyList<EnchantmentModel> RandomPool
	{
		get
		{
			if (_cachedRandomPool != null)
			{
				return _cachedRandomPool;
			}
			_cachedRandomPool = (from e in ModelDb.DebugEnchantments
				where !(e is DeprecatedEnchantment)
				where !_excludedFromRandomPool.Contains(e.GetType().Name)
				where !HasOnEnchantOverride(e.GetType())
				where !HasOnPlayOverride(e.GetType())
				select e).ToList();
			return _cachedRandomPool;
		}
	}

	public static List<EnchantmentModel>? GetExtras(CardModel card)
	{
		if (!_extras.TryGetValue(card, out List<EnchantmentModel> value))
		{
			return null;
		}
		return value;
	}

	public static IEnumerable<EnchantmentModel> AllEnchantments(CardModel card)
	{
		if (card.Enchantment != null)
		{
			yield return card.Enchantment;
		}
		if (!_extras.TryGetValue(card, out List<EnchantmentModel> value))
		{
			yield break;
		}
		foreach (EnchantmentModel item in value)
		{
			yield return item;
		}
	}

	public static bool IsTemp(EnchantmentModel e)
	{
		object value;
		return _temp.TryGetValue(e, out value);
	}

	private static bool HasOnEnchantOverride(Type t)
	{
		MethodInfo method = t.GetMethod("OnEnchant", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (method != null)
		{
			return method.DeclaringType != typeof(EnchantmentModel);
		}
		return false;
	}

	private static bool HasOnPlayOverride(Type t)
	{
		MethodInfo method = t.GetMethod("OnPlay", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
		{
			typeof(PlayerChoiceContext),
			typeof(CardPlay)
		}, null);
		if (method != null)
		{
			return method.DeclaringType != typeof(EnchantmentModel);
		}
		return false;
	}

	public static EnchantmentModel? ApplyTempEnchantment(CardModel card, EnchantmentModel canonical, decimal amount = 1m)
	{
		if (!CanApplyAsTempStack(canonical, card))
		{
			return null;
		}
		EnchantmentModel enchantmentModel;
		try
		{
			enchantmentModel = canonical.ToMutable();
		}
		catch (Exception ex)
		{
			Log.Error("[Watcher] Failed to clone enchantment " + canonical.GetType().Name + ": " + ex.Message);
			return null;
		}
		try
		{
			if (card.Enchantment == null)
			{
				card.EnchantInternal(enchantmentModel, amount);
				enchantmentModel.ModifyCard();
			}
			else
			{
				enchantmentModel.ApplyInternal(card, amount);
				enchantmentModel.ModifyCard();
				_extras.GetValue(card, (CardModel _) => new List<EnchantmentModel>()).Add(enchantmentModel);
			}
			_temp.Add(enchantmentModel, _sentinel);
			return enchantmentModel;
		}
		catch (Exception ex2)
		{
			Log.Error($"[Watcher] Failed to apply temp enchantment {canonical.GetType().Name} to {card.Id}: {ex2.Message}");
			return null;
		}
	}

	private static bool CanApplyAsTempStack(EnchantmentModel canonical, CardModel card)
	{
		CardType type = card.Type;
		if ((uint)(type - 4) <= 2u)
		{
			return false;
		}
		if (!canonical.CanEnchantCardType(type))
		{
			return false;
		}
		CardPile? pile = card.Pile;
		if (pile != null && pile.Type == PileType.Deck && card.Keywords.Contains(CardKeyword.Unplayable))
		{
			return false;
		}
		Type canonicalType = canonical.GetType();
		if (card.Enchantment?.GetType() == canonicalType)
		{
			return false;
		}
		List<EnchantmentModel> extras = GetExtras(card);
		if (extras != null && extras.Any((EnchantmentModel e) => e.GetType() == canonicalType))
		{
			return false;
		}
		return true;
	}

	public static void ClearAllTemp(CombatState? combatState)
	{
		if (combatState == null)
		{
			return;
		}
		foreach (Player player in combatState.Players)
		{
			if (player.PlayerCombatState == null)
			{
				continue;
			}
			foreach (CardPile allPile in player.PlayerCombatState.AllPiles)
			{
				foreach (CardModel item in allPile.Cards.ToList())
				{
					ClearTempForCard(item);
				}
			}
		}
	}

	public static void ClearTempForCard(CardModel card)
	{
		if (_extras.TryGetValue(card, out List<EnchantmentModel> value))
		{
			for (int num = value.Count - 1; num >= 0; num--)
			{
				if (IsTemp(value[num]))
				{
					try
					{
						value[num].ClearInternal();
					}
					catch
					{
					}
					_temp.Remove(value[num]);
					value.RemoveAt(num);
				}
			}
			if (value.Count == 0)
			{
				_extras.Remove(card);
			}
		}
		if (card.Enchantment == null || !IsTemp(card.Enchantment))
		{
			return;
		}
		EnchantmentModel enchantment = card.Enchantment;
		_temp.Remove(enchantment);
		try
		{
			card.ClearEnchantmentInternal();
		}
		catch
		{
		}
		if (_extras.TryGetValue(card, out List<EnchantmentModel> value2) && value2.Count > 0)
		{
			EnchantmentModel enchantmentModel = value2[0];
			value2.RemoveAt(0);
			if (value2.Count == 0)
			{
				_extras.Remove(card);
			}
			try
			{
				int amount = enchantmentModel.Amount;
				enchantmentModel.ClearInternal();
				card.EnchantInternal(enchantmentModel, amount);
				enchantmentModel.ModifyCard();
			}
			catch (Exception ex)
			{
				Log.Error("[Watcher] Failed to promote extra enchantment: " + ex.Message);
			}
		}
	}

	public static void RegisterSubscriptions()
	{
		ModHelper.SubscribeForCombatStateHooks("watcher_enchant_extras", delegate
		{
			WatcherEnchantStackHookProxy instance = WatcherEnchantStackHookProxy.Instance;
			return (instance != null) ? ((IEnumerable<AbstractModel>)new AbstractModel[1] { instance }) : ((IEnumerable<AbstractModel>)Array.Empty<AbstractModel>());
		});
		if (!_subscribedCombatEnded)
		{
			_subscribedCombatEnded = true;
			CombatManager.Instance.CombatEnded += OnCombatEnded;
		}
	}

	private static void OnCombatEnded(CombatRoom _)
	{
		try
		{
			ClearAllTemp(CombatManager.Instance.DebugOnlyGetState());
		}
		catch (Exception ex)
		{
			Log.Error("[Watcher] Temp enchantment cleanup failed: " + ex.Message);
		}
	}
}
