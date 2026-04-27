using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace WatcherMod;

public sealed class ColdObservation : WatcherCard
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Retain);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("MagicNumber", 1m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<ColdObservationPower>());

	public ColdObservation()
		: base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllAllies)
	{
	}

	protected override void AddExtraArgsToDescription(LocString description)
	{
		bool flag = (base.RunState?.Players.Count ?? 1) > 1;
		description.Add("IsMultiplayer", flag);
		string variable;
		switch (LocManager.Instance?.Language ?? "eng")
		{
		case "zhs":
		case "zht":
		case "jpn":
		case "kor":
			variable = (flag ? "所有玩家" : "你");
			break;
		default:
			variable = (flag ? "All players'" : "Your");
			break;
		}
		description.Add("Subject", variable);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		IEnumerable<Creature> enumerable = from c in base.CombatState.GetTeammatesOf(base.Owner.Creature)
			where c != null && c.IsAlive && c.IsPlayer
			select c;
		foreach (Creature item in enumerable)
		{
			await PowerCmd.Apply<ColdObservationPower>(item, base.DynamicVars["MagicNumber"].BaseValue, base.Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["MagicNumber"].UpgradeValueBy(1m);
	}
}
