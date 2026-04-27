using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace WatcherMod;

internal static class WatcherHoverTips
{
	internal static IHoverTip Stance { get; } = new HoverTip(new LocString("powers", "STANCE.title"), new LocString("powers", "STANCE.description"));

	internal static IHoverTip Prophecy { get; } = new HoverTip(new LocString("powers", "PROPHECY.title"), new LocString("powers", "PROPHECY.description"));

	internal static IHoverTip Scry { get; } = new HoverTip(new LocString("powers", "SCRY.title"), new LocString("powers", "SCRY.description"));

	internal static IHoverTip Enchantment { get; } = new HoverTip(new LocString("powers", "ENCHANTMENT.title"), new LocString("powers", "ENCHANTMENT.description"));

	internal static IHoverTip Directed { get; } = new HoverTip(new LocString("powers", "DIRECTED.title"), new LocString("powers", "DIRECTED.description"));
}
