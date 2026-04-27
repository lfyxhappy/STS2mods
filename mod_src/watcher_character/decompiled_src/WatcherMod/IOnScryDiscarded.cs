using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace WatcherMod;

public interface IOnScryDiscarded
{
	Task OnScryDiscarded(PlayerChoiceContext choiceContext, Player owner);
}
