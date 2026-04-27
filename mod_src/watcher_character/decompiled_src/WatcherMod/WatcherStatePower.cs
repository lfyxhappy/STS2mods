using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class WatcherStatePower : PowerModel
{
	private CardType? _lastPlayedCardType;

	private int _cardsPlayedThisTurn;

	private int _attacksPlayedThisTurn;

	private int _totalMantraGainedThisCombat;

	private int _mantraGainedThisTurn;

	private int _cataclysmPlaysThisCombat;

	private int _prophecyPlaysThisCombat;

	private int _prophecyPlaysThisTurn;

	private int _knowFateLastObserved;

	private bool _knowFateConsumedThisTurn;

	private readonly HashSet<CardModel> _scryingCardsThisTurn = new HashSet<CardModel>();

	protected override bool IsVisibleInternal => false;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.None;

	public CardType? LastPlayedCardType
	{
		get
		{
			return _lastPlayedCardType;
		}
		set
		{
			AssertMutable();
			_lastPlayedCardType = value;
		}
	}

	public int CardsPlayedThisTurn
	{
		get
		{
			return _cardsPlayedThisTurn;
		}
		set
		{
			AssertMutable();
			_cardsPlayedThisTurn = value;
		}
	}

	public int AttacksPlayedThisTurn
	{
		get
		{
			return _attacksPlayedThisTurn;
		}
		set
		{
			AssertMutable();
			_attacksPlayedThisTurn = value;
		}
	}

	public int TotalMantraGainedThisCombat
	{
		get
		{
			return _totalMantraGainedThisCombat;
		}
		set
		{
			AssertMutable();
			_totalMantraGainedThisCombat = value;
		}
	}

	public int MantraGainedThisTurn
	{
		get
		{
			return _mantraGainedThisTurn;
		}
		set
		{
			AssertMutable();
			_mantraGainedThisTurn = value;
		}
	}

	public int CataclysmPlaysThisCombat
	{
		get
		{
			return _cataclysmPlaysThisCombat;
		}
		set
		{
			AssertMutable();
			_cataclysmPlaysThisCombat = value;
		}
	}

	public int ProphecyPlaysThisCombat
	{
		get
		{
			return _prophecyPlaysThisCombat;
		}
		set
		{
			AssertMutable();
			_prophecyPlaysThisCombat = value;
		}
	}

	public int ProphecyPlaysThisTurn
	{
		get
		{
			return _prophecyPlaysThisTurn;
		}
		set
		{
			AssertMutable();
			_prophecyPlaysThisTurn = value;
		}
	}

	public bool KnowFateConsumedThisTurn
	{
		get
		{
			return _knowFateConsumedThisTurn;
		}
		set
		{
			AssertMutable();
			_knowFateConsumedThisTurn = value;
		}
	}

	public int KnowFateLastObserved
	{
		get
		{
			return _knowFateLastObserved;
		}
		set
		{
			AssertMutable();
			_knowFateLastObserved = value;
		}
	}

	public int ScryPlaysThisTurn => _scryingCardsThisTurn.Count;

	public void RegisterScryPlay(CardModel source)
	{
		if (source != null)
		{
			_scryingCardsThisTurn.Add(source);
		}
	}

	public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != base.Owner.Player)
		{
			return Task.CompletedTask;
		}
		CardsPlayedThisTurn++;
		LastPlayedCardType = cardPlay.Card.Type;
		if (cardPlay.Card.Type == CardType.Attack)
		{
			AttacksPlayedThisTurn++;
		}
		if (cardPlay.Card is IProphecyCard)
		{
			ProphecyPlaysThisCombat++;
			ProphecyPlaysThisTurn++;
		}
		int powerAmount = base.Owner.GetPowerAmount<KnowFatePower>();
		if (powerAmount < KnowFateLastObserved)
		{
			KnowFateConsumedThisTurn = true;
		}
		KnowFateLastObserved = powerAmount;
		return Task.CompletedTask;
	}

	public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != base.Owner.Player)
		{
			return Task.CompletedTask;
		}
		CardsPlayedThisTurn = 0;
		AttacksPlayedThisTurn = 0;
		MantraGainedThisTurn = 0;
		ProphecyPlaysThisTurn = 0;
		LastPlayedCardType = null;
		KnowFateConsumedThisTurn = false;
		KnowFateLastObserved = base.Owner.GetPowerAmount<KnowFatePower>();
		_scryingCardsThisTurn.Clear();
		return Task.CompletedTask;
	}

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
	{
		if (player == base.Owner.Player)
		{
			await WatcherCombatHelper.ProcessDeferredRetainCards();
		}
	}
}
