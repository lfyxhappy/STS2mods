using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;

namespace WatcherMod;

[HarmonyPatch(typeof(HandPosHelper), "GetAngle")]
internal static class HandPosOverflowAnglePatch
{
	private static readonly float[] Max = new float[10] { -15f, -12f, -9f, -6f, -3f, 3f, 6f, 9f, 12f, 15f };

	private static bool Prefix(int handSize, int cardIndex, ref float __result)
	{
		if (handSize <= 10)
		{
			return true;
		}
		float num = (float)cardIndex / (float)(handSize - 1) * 9f;
		int num2 = Math.Clamp((int)num, 0, 8);
		float num3 = num - (float)num2;
		__result = Max[num2] + (Max[num2 + 1] - Max[num2]) * num3;
		return false;
	}
}
