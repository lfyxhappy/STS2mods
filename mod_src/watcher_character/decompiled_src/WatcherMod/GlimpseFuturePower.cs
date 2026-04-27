using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class GlimpseFuturePower : PowerModel
{
	private int _nextExpected = 1;

	private readonly Dictionary<CardModel, int> _labels = new Dictionary<CardModel, int>();

	private bool _invalidated;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public int MantraBonus { get; set; }

	public void AssignLabels(IEnumerable<CardModel> cards)
	{
		_labels.Clear();
		_invalidated = false;
		_nextExpected = 1;
		int num = 1;
		foreach (CardModel card in cards)
		{
			_labels[card] = num++;
		}
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (_invalidated || cardPlay.Card.Owner != base.Owner.Player || !_labels.TryGetValue(cardPlay.Card, out var value))
		{
			return;
		}
		if (value == _nextExpected)
		{
			Flash();
			int amount = value + MantraBonus;
			await WatcherCombatHelper.GainMantra(base.Owner.Player, amount, cardPlay.Card);
			_labels.Remove(cardPlay.Card);
			_nextExpected++;
			if (_labels.Count == 0)
			{
				await PowerCmd.Remove(this);
			}
		}
		else
		{
			await Invalidate();
		}
	}

	public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
	{
		if (_invalidated || card.Owner != base.Owner.Player)
		{
			return;
		}
		if (oldPileType == PileType.Hand)
		{
			CardPile? pile = card.Pile;
			if ((pile == null || pile.Type != PileType.Play) && _labels.ContainsKey(card))
			{
				await Invalidate();
				return;
			}
		}
		CardPile? pile2 = card.Pile;
		if (pile2 != null && pile2.Type == PileType.Hand && oldPileType != PileType.Hand && card is IProphecyCard && !_labels.ContainsKey(card))
		{
			await Invalidate();
		}
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == base.Owner.Side)
		{
			await PowerCmd.Remove(this);
		}
	}

	private async Task Invalidate()
	{
		_invalidated = true;
		_labels.Clear();
		await PowerCmd.Remove(this);
	}
}
