using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace WatcherMod;

public sealed class ColdObservationPower : PowerModel
{
	private CardModel? _lastProcessedCard;

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		if (dealer == base.Owner && props.HasFlag(ValueProp.Move) && !props.HasFlag(ValueProp.Unpowered) && result.TotalDamage > 0)
		{
			await CreatureCmd.GainBlock(base.Owner, result.TotalDamage, ValueProp.Unpowered, null);
			if (cardSource != _lastProcessedCard)
			{
				_lastProcessedCard = cardSource;
				await PowerCmd.ModifyAmount(this, -1m, null, null);
			}
		}
	}
}
