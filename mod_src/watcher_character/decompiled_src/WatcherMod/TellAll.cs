using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;

namespace WatcherMod;

public sealed class TellAll : WatcherCard, IProphecyCard
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DynamicVar("MagicNumber", 2m),
		new DynamicVar("ConfusionAmount", 3m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromPower<StrengthPower>(),
		HoverTipFactory.FromPower<ConfusionPower>()
	});

	public TellAll()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		Creature target = cardPlay.Target;
		List<MoveState> list = WatcherIntentTimeline.ForecastFutureMoves(target, 2);
		List<AbstractIntent> source = ((list.Count >= 2) ? list[1] : null)?.Intents.ToList() ?? new List<AbstractIntent>();
		HashSet<Type> correctCategories = new HashSet<Type>();
		if (source.OfType<AttackIntent>().Any())
		{
			correctCategories.Add(typeof(GuessAttack));
		}
		if (source.OfType<DefendIntent>().Any())
		{
			correctCategories.Add(typeof(GuessDefend));
		}
		if (source.OfType<BuffIntent>().Any())
		{
			correctCategories.Add(typeof(GuessBuff));
		}
		if (source.OfType<DebuffIntent>().Any())
		{
			correctCategories.Add(typeof(GuessDebuff));
		}
		if (source.OfType<CardDebuffIntent>().Any())
		{
			correctCategories.Add(typeof(GuessCardDebuff));
		}
		if (source.OfType<StatusIntent>().Any())
		{
			correctCategories.Add(typeof(GuessStatus));
		}
		if (source.OfType<SummonIntent>().Any())
		{
			correctCategories.Add(typeof(GuessSummon));
		}
		if (source.OfType<HealIntent>().Any())
		{
			correctCategories.Add(typeof(GuessHeal));
		}
		if (source.OfType<EscapeIntent>().Any())
		{
			correctCategories.Add(typeof(GuessEscape));
		}
		if (source.OfType<SleepIntent>().Any())
		{
			correctCategories.Add(typeof(GuessSleep));
		}
		if (source.OfType<StunIntent>().Any())
		{
			correctCategories.Add(typeof(GuessStun));
		}
		List<Type> source2 = new List<Type>
		{
			typeof(GuessAttack),
			typeof(GuessDefend),
			typeof(GuessBuff),
			typeof(GuessDebuff),
			typeof(GuessCardDebuff),
			typeof(GuessStatus),
			typeof(GuessSummon),
			typeof(GuessHeal),
			typeof(GuessEscape),
			typeof(GuessSleep),
			typeof(GuessStun)
		};
		Rng niche = base.Owner.RunState.Rng.Niche;
		List<Type> list2 = source2.Where((Type t) => !correctCategories.Contains(t)).ToList();
		niche.Shuffle(list2);
		List<Type> list3 = correctCategories.ToList();
		niche.Shuffle(list3);
		List<Type> list4 = new List<Type>();
		if (list3.Count > 0)
		{
			list4.Add(list3[0]);
			list4.AddRange(list2.Take(2));
			int num = 1;
			while (list4.Count < 3 && num < list3.Count)
			{
				list4.Add(list3[num++]);
			}
		}
		else
		{
			list4.AddRange(list2.Take(3));
		}
		niche.Shuffle(list4);
		List<CardModel> cards = list4.Select(MakeGuess).ToList();
		CardModel cardModel = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards, base.Owner);
		if (cardModel != null && correctCategories.Contains(cardModel.GetType()))
		{
			int intValue = base.DynamicVars["MagicNumber"].IntValue;
			int confusionAmount = base.DynamicVars["ConfusionAmount"].IntValue;
			await PowerCmd.Apply<StrengthPower>(base.Owner.Creature, intValue, base.Owner.Creature, this);
			await WatcherProphecy.ApplyConfusion(target, base.Owner.Creature, this, confusionAmount);
		}
		await WatcherProphecy.Trigger(base.Owner, new ProphecyContext
		{
			Source = this,
			AffectedEnemy = target
		});
		CardModel MakeGuess(Type t)
		{
			if (!(t == typeof(GuessAttack)))
			{
				if (!(t == typeof(GuessDefend)))
				{
					if (!(t == typeof(GuessBuff)))
					{
						if (!(t == typeof(GuessDebuff)))
						{
							if (!(t == typeof(GuessCardDebuff)))
							{
								if (!(t == typeof(GuessStatus)))
								{
									if (!(t == typeof(GuessSummon)))
									{
										if (!(t == typeof(GuessHeal)))
										{
											if (!(t == typeof(GuessEscape)))
											{
												if (!(t == typeof(GuessSleep)))
												{
													return base.CombatState.CreateCard<GuessStun>(base.Owner);
												}
												return base.CombatState.CreateCard<GuessSleep>(base.Owner);
											}
											return base.CombatState.CreateCard<GuessEscape>(base.Owner);
										}
										return base.CombatState.CreateCard<GuessHeal>(base.Owner);
									}
									return base.CombatState.CreateCard<GuessSummon>(base.Owner);
								}
								return base.CombatState.CreateCard<GuessStatus>(base.Owner);
							}
							return base.CombatState.CreateCard<GuessCardDebuff>(base.Owner);
						}
						return base.CombatState.CreateCard<GuessDebuff>(base.Owner);
					}
					return base.CombatState.CreateCard<GuessBuff>(base.Owner);
				}
				return base.CombatState.CreateCard<GuessDefend>(base.Owner);
			}
			return base.CombatState.CreateCard<GuessAttack>(base.Owner);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(1m);
	}
}
