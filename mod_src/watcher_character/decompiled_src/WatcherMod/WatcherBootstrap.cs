using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace WatcherMod;

[ModInitializer("Init")]
public static class WatcherBootstrap
{
	private static bool _initialized;

	[DllImport("libdl.so.2", EntryPoint = "dlopen")]
	private static extern nint LinuxDlopen(string? path, int mode);

	private static void PreloadMonoModNativeDeps()
	{
		if (!OperatingSystem.IsLinux())
		{
			return;
		}
		try
		{
			Log.Info((LinuxDlopen("libgcc_s.so.1", 258) != IntPtr.Zero) ? "[Watcher] Pre-loaded libgcc_s.so.1 for MonoMod native detour compatibility." : "[Watcher] libgcc_s.so.1 not found; Harmony patches may fail on this platform.");
		}
		catch (Exception ex)
		{
			Log.Info("[Watcher] Could not pre-load libgcc_s (" + ex.Message + "); Harmony patches may fail.");
		}
	}

	public static void Init()
	{
		if (_initialized)
		{
			return;
		}
		_initialized = true;
		PreloadMonoModNativeDeps();
		Harmony instance = new Harmony("boninall.watcher");
		int num = 0;
		int num2 = 0;
		bool flag = OS.GetName() == "Android";
		HashSet<string> hashSet = (flag ? new HashSet<string>
		{
			"WatcherCharSelectIconPatch", "WatcherCharSelectLockedIconPatch", "WatcherIconTexturePatch", "WatcherIconOutlineTexturePatch", "WatcherIconScenePatch", "WatcherRelicIconPatch", "WatcherRelicIconOutlinePatch", "WatcherPowerIconPatch", "WatcherPowerBigIconPatch", "WatcherCardModelPortraitPatch",
			"WatcherCardHasPortraitPatch", "WatcherCardHasBetaPortraitPatch", "WatcherArmPointingPathPatch", "WatcherArmRockPathPatch", "WatcherArmPaperPathPatch", "WatcherArmScissorsPathPatch", "WatcherMerchantCharacterPatch", "WatcherCardLibraryPatch", "MasterRealityPatch", "WatcherInspectCardArtTogglePatch",
			"WatcherDarvRelicPatch", "WatcherOrobasGenerateOptionsPatch", "WatcherTouchOfOrobasAfterObtainedPatch", "WatcherArchaicToothAfterObtainedPatch", "WatcherArchaicToothTranscendencePatch", "WatcherArchaicToothRandomStarterPatch", "WatcherAscensionUnlockPatch", "WatcherAscensionUnlockBeginRunPatch", "WatcherAscensionUnlockGetStatsPatch", "MegaSpriteGetAnimStatePatch",
			"DieAnimFallbackPatch", "WatcherBlockVarEnchantExtrasPatch", "WatcherNCardExtraEnchantTabsPatch"
		} : null);
		int num3 = 0;
		Type[] types = Assembly.GetExecutingAssembly().GetTypes();
		Log.Info($"[Watcher] Total types in assembly: {types.Length}");
		Type[] array = types;
		foreach (Type type in array)
		{
			try
			{
				if (type.GetCustomAttributes(typeof(HarmonyPatch), inherit: true).Length != 0)
				{
					Log.Info("[Watcher] Considering: " + type.Name);
					if (hashSet != null && hashSet.Contains(type.Name))
					{
						num3++;
						Log.Info("[Watcher] Skipped (Android): " + type.Name);
						continue;
					}
					Log.Info("[Watcher] Patching: " + type.Name + "...");
					new PatchClassProcessor(instance, type).Patch();
					num++;
					Log.Info("[Watcher] Patched OK: " + type.Name);
				}
			}
			catch (Exception ex)
			{
				num2++;
				Log.Error("[Watcher] Harmony patch failed for " + type.Name + ": " + (ex.InnerException?.Message ?? ex.Message));
			}
		}
		Log.Info($"[Watcher] Harmony patches: {num} applied, {num2} failed, {num3} skipped (Android).");
		WatcherEnchantStack.RegisterSubscriptions();
		if (flag)
		{
			WatcherCardLibraryInjector.InstallSceneTreeListener();
			WatcherInspectCardArtToggleInjector.InstallSceneTreeListener();
			Log.Info("[Watcher] Installed SceneTree listeners for Android fallbacks.");
		}
		ModHelper.AddModelToPool<EventRelicPool, HolyWater>();
		if (flag)
		{
			Callable.From(InjectDarvVioletLotus).CallDeferred();
		}
		Log.Info("[Watcher] 已初始化观者模组。");
	}

	private static void InjectDarvVioletLotus()
	{
		try
		{
			if (!(AccessTools.Field(typeof(Darv), "_validRelicSets")?.GetValue(null) is IList list))
			{
				return;
			}
			Type nestedType = typeof(Darv).GetNestedType("ValidRelicSet", BindingFlags.NonPublic);
			if (nestedType == null)
			{
				return;
			}
			Type type = typeof(Func<, >).MakeGenericType(typeof(Player), typeof(bool));
			ConstructorInfo constructor = nestedType.GetConstructor(new Type[2]
			{
				type,
				typeof(RelicModel[])
			});
			if (!(constructor == null))
			{
				Func<Player, bool> func = (Player owner) => owner?.Character is Watcher;
				object value = constructor.Invoke(new object[2]
				{
					func,
					new RelicModel[1] { ModelDb.Relic<VioletLotus>() }
				});
				list.Add(value);
				Log.Info("[Watcher] Injected VioletLotus into Darv relic sets.");
			}
		}
		catch (Exception ex)
		{
			Log.Error("[Watcher] Failed to inject Darv VioletLotus: " + ex.Message);
		}
	}

	private static void ForceJitTargets(Type patchClass)
	{
		object[] customAttributes = patchClass.GetCustomAttributes(typeof(HarmonyPatch), inherit: true);
		for (int i = 0; i < customAttributes.Length; i++)
		{
			if (!(customAttributes[i] is HarmonyPatch harmonyPatch))
			{
				continue;
			}
			try
			{
				Type declaringType = harmonyPatch.info.declaringType;
				string methodName = harmonyPatch.info.methodName;
				if (!(declaringType == null) && methodName != null)
				{
					MethodInfo methodInfo = ((harmonyPatch.info.methodType != MethodType.Getter) ? AccessTools.Method(declaringType, methodName) : declaringType.GetProperty(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetGetMethod(nonPublic: true));
					if (methodInfo != null)
					{
						RuntimeHelpers.PrepareMethod(methodInfo.MethodHandle);
					}
				}
			}
			catch
			{
			}
		}
	}
}
