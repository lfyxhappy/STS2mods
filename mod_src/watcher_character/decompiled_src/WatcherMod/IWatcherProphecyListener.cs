using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;

namespace WatcherMod;

public interface IWatcherProphecyListener
{
	Task OnProphecy(Player owner, ProphecyContext ctx);
}
