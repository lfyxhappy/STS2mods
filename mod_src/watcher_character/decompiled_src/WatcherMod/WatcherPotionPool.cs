using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Models;

namespace WatcherMod;

public sealed class WatcherPotionPool : PotionPoolModel
{
	public override string EnergyColorName => "watcher";

	public override Color LabOutlineColor => new Color("9E68FF");

	protected override IEnumerable<PotionModel> GenerateAllPotions()
	{
		return Array.Empty<PotionModel>();
	}
}
