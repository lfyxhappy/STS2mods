using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace WatcherMod;

public sealed class HandPicked : WatcherCard, IProphecyCard
{
	private const int _cardsToAdd = 3;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromPower<ProphetPower>(),
		HoverTipFactory.FromCard<Insight>()
	});

	public HandPicked()
		: base(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (base.Owner.PlayerCombatState == null)
		{
			return;
		}
		List<SerializableCard> list = PickCardsFromVictoryRuns(base.Owner, 3);
		int insightFallback = 3 - list.Count;
		foreach (SerializableCard serial in list)
		{
			if (serial.Id == null)
			{
				insightFallback++;
				continue;
			}
			CardModel byIdOrNull = ModelDb.GetByIdOrNull<CardModel>(serial.Id);
			if (byIdOrNull == null)
			{
				insightFallback++;
				continue;
			}
			try
			{
				CardModel cardModel = base.Owner.Creature.CombatState.CreateCard(byIdOrNull, base.Owner);
				int num = Math.Min(serial.CurrentUpgradeLevel, 9);
				for (int i = 0; i < num; i++)
				{
					if (!cardModel.IsUpgradable)
					{
						break;
					}
					CardCmd.Upgrade(cardModel);
				}
				await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, addedByPlayer: true);
			}
			catch (Exception ex)
			{
				Log.Warn($"[Watcher] HandPicked spawn failed for {serial.Id}: {ex.Message}");
				insightFallback++;
			}
		}
		for (int j = 0; j < insightFallback; j++)
		{
			await CardPileCmd.AddGeneratedCardToCombat(await WatcherCombatHelper.CreateWatcherCard<Insight>(base.Owner), PileType.Hand, addedByPlayer: true);
		}
		await PowerCmd.Apply<ProphetPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this
		});
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}

	private static List<SerializableCard> PickCardsFromVictoryRuns(Player owner, int count)
	{
		List<SerializableCard> list = new List<SerializableCard>();
		try
		{
			SaveManager instance = SaveManager.Instance;
			if (instance == null)
			{
				return list;
			}
			List<string> list2 = instance.GetAllRunHistoryNames() ?? new List<string>();
			if (list2.Count == 0)
			{
				return list;
			}
			Rng combatCardSelection = owner.RunState.Rng.CombatCardSelection;
			List<string> list3 = list2.ToList();
			combatCardSelection.Shuffle(list3);
			foreach (string item in list3)
			{
				if (list.Count >= count)
				{
					break;
				}
				ReadSaveResult<RunHistory> readSaveResult;
				try
				{
					readSaveResult = instance.LoadRunHistory(item);
				}
				catch (Exception ex)
				{
					Log.Warn("[Watcher] HandPicked load '" + item + "' threw: " + ex.Message);
					continue;
				}
				if (!readSaveResult.Success)
				{
					continue;
				}
				RunHistory saveData = readSaveResult.SaveData;
				if (saveData == null || !saveData.Win || saveData.WasAbandoned)
				{
					continue;
				}
				List<SerializableCard> list4 = (from c in saveData.Players?.SelectMany((RunHistoryPlayer p) => p?.Deck ?? Enumerable.Empty<SerializableCard>())
					where c != null && c.Id != null
					select c).ToList() ?? new List<SerializableCard>();
				if (list4.Count != 0)
				{
					SerializableCard serializableCard = combatCardSelection.NextItem(list4);
					if (serializableCard != null)
					{
						list.Add(serializableCard);
					}
				}
			}
		}
		catch (Exception ex2)
		{
			Log.Warn("[Watcher] HandPicked run-history scan failed: " + ex2.Message);
		}
		return list;
	}
}
