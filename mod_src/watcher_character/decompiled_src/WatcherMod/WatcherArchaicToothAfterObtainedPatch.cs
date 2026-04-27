using System;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace WatcherMod;

[HarmonyPatch(typeof(ArchaicTooth), "AfterObtained")]
internal static class WatcherArchaicToothAfterObtainedPatch
{
	private static Exception? Finalizer(Exception? __exception, ref Task __result)
	{
		if (__exception != null)
		{
			GD.PrintErr("[Watcher] ArchaicTooth.AfterObtained crashed: " + __exception.Message);
			__result = Task.CompletedTask;
			return null;
		}
		return __exception;
	}
}
