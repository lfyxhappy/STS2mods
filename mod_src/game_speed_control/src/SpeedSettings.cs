namespace GameSpeedControl;

internal sealed class SpeedSettings
{
	public int SchemaVersion { get; set; } = 1;

	public double SpeedMultiplier { get; set; } = 1.0;
}
