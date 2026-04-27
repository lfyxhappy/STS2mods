using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;

namespace WatcherMod;

public sealed class Preach : WatcherCard
{
	private bool IsMultiplayer => (base.RunState?.Players.Count ?? 1) > 1;

	protected override IEnumerable<IHoverTip> ExtraHoverTips
	{
		get
		{
			if (!IsMultiplayer)
			{
				return new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[4]
				{
					HoverTipFactory.FromPower<DevotionPower>(),
					HoverTipFactory.FromPower<GospelPower>(),
					HoverTipFactory.FromPower<Mantra>(),
					WatcherHoverTips.Stance
				});
			}
			return new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[6]
			{
				HoverTipFactory.FromPower<DevotionPower>(),
				HoverTipFactory.FromPower<GospelPower>(),
				HoverTipFactory.FromPower<DivineDoomPower>(),
				HoverTipFactory.FromPower<Mantra>(),
				WatcherHoverTips.Stance,
				HoverTipFactory.FromPower<DoomPower>()
			});
		}
	}

	public Preach()
		: base(3, CardType.Power, CardRarity.Ancient, TargetType.Self)
	{
	}

	protected override void AddExtraArgsToDescription(LocString description)
	{
		description.Add("IsMultiplayer", IsMultiplayer);
		string variable;
		switch (LocManager.Instance?.Language ?? "eng")
		{
		case "zhs":
		case "zht":
		case "jpn":
		case "kor":
			variable = (IsMultiplayer ? "和[gold]天罚[/gold]" : "");
			break;
		default:
			variable = (IsMultiplayer ? " and [gold]Divine Judgment[/gold]" : "");
			break;
		}
		description.Add("DoomClause", variable);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<DevotionPower>(base.Owner.Creature, 2m, base.Owner.Creature, this);
		await PowerCmd.Apply<GospelPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
		if (IsMultiplayer)
		{
			await PowerCmd.Apply<DivineDoomPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
		AddKeyword(CardKeyword.Innate);
	}
}
