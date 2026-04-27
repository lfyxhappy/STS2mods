using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;

namespace WatcherMod;

[HarmonyPatch(typeof(HandPosHelper), "GetPosition")]
internal static class HandPosOverflowPositionPatch
{
	private static readonly Vector2[] Max = new Vector2[10]
	{
		new Vector2(-610f, 38f),
		new Vector2(-472f, 5f),
		new Vector2(-340f, -21f),
		new Vector2(-200f, -41f),
		new Vector2(-64f, -50f),
		new Vector2(64f, -50f),
		new Vector2(200f, -41f),
		new Vector2(340f, -21f),
		new Vector2(472f, 5f),
		new Vector2(610f, 38f)
	};

	private static bool Prefix(int handSize, int cardIndex, ref Vector2 __result)
	{
		if (handSize <= 10)
		{
			return true;
		}
		float num = (float)cardIndex / (float)(handSize - 1) * 9f;
		int num2 = Math.Clamp((int)num, 0, 8);
		float weight = num - (float)num2;
		__result = Max[num2].Lerp(Max[num2 + 1], weight);
		return false;
	}
}
