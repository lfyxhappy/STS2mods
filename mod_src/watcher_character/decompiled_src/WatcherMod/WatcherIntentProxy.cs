using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace WatcherMod;

public sealed class WatcherIntentProxy : WatcherCard
{
	private static readonly FieldInfo _titleLocStringField = typeof(CardModel).GetField("_titleLocString", BindingFlags.Instance | BindingFlags.NonPublic);

	public override bool CanBeGeneratedInCombat => false;

	public override bool CanBeGeneratedByModifiers => false;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new StringVar("EnemyName"),
		new StringVar("IntentTitle"),
		new StringVar("IntentSummary")
	});

	public Creature? AttachedEnemy { get; private set; }

	public MoveState? AttachedMove { get; private set; }

	public WatcherIntentProxy()
		: base(0, CardType.Skill, CardRarity.Token, TargetType.Self, shouldShowInCardLibrary: false)
	{
	}

	public void ConfigureFromEnemy(Creature enemy)
	{
		AttachedEnemy = enemy;
		string enemyName = SafeEnemyName(enemy);
		string intentTitle = SafeIntentTitle(enemy);
		string intentSummary = SafeIntentSummary(enemy);
		PopulateVars(enemyName, intentTitle, intentSummary);
	}

	public void ConfigureFromMove(MoveState move, Creature ownerEnemy, string? slotLabel = null)
	{
		AttachedMove = move;
		AttachedEnemy = ownerEnemy;
		string text = SafeEnemyName(ownerEnemy);
		if (!string.IsNullOrEmpty(slotLabel))
		{
			text = "[" + slotLabel + "] " + text;
		}
		string intentTitle = SafeIntentTitleFromMove(move, ownerEnemy);
		string intentSummary = SafeIntentSummaryFromMove(move, ownerEnemy);
		PopulateVars(text, intentTitle, intentSummary);
	}

	private void PopulateVars(string enemyName, string intentTitle, string intentSummary)
	{
		if (base.DynamicVars["EnemyName"] is StringVar stringVar)
		{
			stringVar.StringValue = enemyName;
		}
		if (base.DynamicVars["IntentTitle"] is StringVar stringVar2)
		{
			stringVar2.StringValue = intentTitle;
		}
		if (base.DynamicVars["IntentSummary"] is StringVar stringVar3)
		{
			stringVar3.StringValue = intentSummary;
		}
		LocString locString = new LocString("cards", base.Id.Entry + ".title");
		locString.Add("EnemyName", enemyName);
		locString.Add("IntentTitle", intentTitle);
		locString.Add("IntentSummary", intentSummary);
		_titleLocStringField.SetValue(this, locString);
	}

	private static string SafeEnemyName(Creature enemy)
	{
		try
		{
			return enemy?.Name ?? "";
		}
		catch
		{
			return "";
		}
	}

	private static string SafeIntentTitle(Creature enemy)
	{
		try
		{
			AbstractIntent abstractIntent = enemy?.Monster?.NextMove?.Intents?.FirstOrDefault();
			if (abstractIntent == null)
			{
				return "";
			}
			return abstractIntent.GetHoverTip(enemy.CombatState?.Creatures.Where((Creature c) => c.IsAlive) ?? Array.Empty<Creature>(), enemy).Title ?? "";
		}
		catch (Exception ex)
		{
			Log.Warn("[Watcher] IntentProxy title extraction failed: " + ex.Message);
			return "";
		}
	}

	private static string SafeIntentSummary(Creature enemy)
	{
		try
		{
			IReadOnlyList<AbstractIntent> readOnlyList = enemy?.Monster?.NextMove?.Intents ?? Array.Empty<AbstractIntent>();
			if (readOnlyList.Count == 0)
			{
				return "";
			}
			IEnumerable<Creature> targets = enemy.CombatState?.Creatures.Where((Creature c) => c.IsAlive) ?? Array.Empty<Creature>();
			List<string> list = new List<string>();
			foreach (AbstractIntent item in readOnlyList)
			{
				string text = item.GetIntentLabel(targets, enemy).GetFormattedText() ?? "";
				if (!string.IsNullOrWhiteSpace(text))
				{
					list.Add(text);
				}
			}
			string text2 = string.Join(" / ", list);
			MonsterModel? monster = enemy.Monster;
			if (monster != null && monster.NextMove?.CanTransitionAway == false && !string.IsNullOrWhiteSpace(text2))
			{
				text2 = "[" + text2 + "]";
			}
			return text2;
		}
		catch (Exception ex)
		{
			Log.Warn("[Watcher] IntentProxy summary extraction failed: " + ex.Message);
			return "";
		}
	}

	private static string SafeIntentTitleFromMove(MoveState move, Creature owner)
	{
		try
		{
			AbstractIntent abstractIntent = move?.Intents?.FirstOrDefault();
			if (abstractIntent == null || owner == null)
			{
				return "";
			}
			IEnumerable<Creature> targets = owner.CombatState?.Creatures.Where((Creature c) => c.IsAlive) ?? Array.Empty<Creature>();
			return abstractIntent.GetHoverTip(targets, owner).Title ?? "";
		}
		catch (Exception ex)
		{
			Log.Warn("[Watcher] IntentProxy title-from-move failed: " + ex.Message);
			return "";
		}
	}

	private static string SafeIntentSummaryFromMove(MoveState move, Creature owner)
	{
		try
		{
			IReadOnlyList<AbstractIntent> readOnlyList = move?.Intents ?? Array.Empty<AbstractIntent>();
			if (readOnlyList.Count == 0 || owner == null)
			{
				return "";
			}
			IEnumerable<Creature> targets = owner.CombatState?.Creatures.Where((Creature c) => c.IsAlive) ?? Array.Empty<Creature>();
			List<string> list = new List<string>();
			foreach (AbstractIntent item in readOnlyList)
			{
				string text = item.GetIntentLabel(targets, owner).GetFormattedText() ?? "";
				if (!string.IsNullOrWhiteSpace(text))
				{
					list.Add(text);
				}
			}
			string text2 = string.Join(" / ", list);
			if (!move.CanTransitionAway && !string.IsNullOrWhiteSpace(text2))
			{
				text2 = "[" + text2 + "]";
			}
			return text2;
		}
		catch (Exception ex)
		{
			Log.Warn("[Watcher] IntentProxy summary-from-move failed: " + ex.Message);
			return "";
		}
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return Task.CompletedTask;
	}
}
