using System.Globalization;

namespace GameSpeedControl;

internal static class SpeedPresetCycle
{
	public static readonly double[] Presets = [1.0, 1.5, 2.0, 2.5, 3.0, 3.5, 4.0];

	public static double Normalize(double value)
	{
		foreach (double preset in Presets)
		{
			if (Math.Abs(value - preset) < 0.001)
			{
				return preset;
			}
		}

		return 1.0;
	}

	public static double Next(double current)
	{
		double normalized = Normalize(current);
		int index = Array.IndexOf(Presets, normalized);
		if (index < 0 || index == Presets.Length - 1)
		{
			return Presets[0];
		}

		return Presets[index + 1];
	}

	public static string FormatLabel(double speed)
	{
		double normalized = Normalize(speed);
		return "速度 " + normalized.ToString("0.#", CultureInfo.InvariantCulture) + "x";
	}
}
