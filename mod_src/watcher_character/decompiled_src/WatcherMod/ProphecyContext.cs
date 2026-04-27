using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class ProphecyContext
{
	public CardModel? Source { get; init; }

	public int CardsDiscarded { get; init; }

	public Creature? AffectedEnemy { get; init; }

	public bool ChangedIntent { get; init; }

	public bool FromScry { get; init; }

	public IReadOnlyList<CardModel> PeekedCards { get; init; } = Array.Empty<CardModel>();
}
