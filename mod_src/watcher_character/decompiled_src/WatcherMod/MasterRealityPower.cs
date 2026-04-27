using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class MasterRealityPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override Task AfterCardGeneratedForCombat(CardModel card, bool addedByPlayer)
	{
		if (card.Owner == base.Owner.Player && card.IsUpgradable)
		{
			CardCmd.Upgrade(card);
		}
		return Task.CompletedTask;
	}
}
