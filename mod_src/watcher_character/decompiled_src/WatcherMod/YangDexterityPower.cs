using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace WatcherMod;

public sealed class YangDexterityPower : TemporaryDexterityPower
{
	public override AbstractModel OriginModel => ModelDb.Relic<Yang>();
}
