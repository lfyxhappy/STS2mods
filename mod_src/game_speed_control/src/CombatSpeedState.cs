namespace GameSpeedControl;

internal sealed class CombatSpeedState
{
	public double TargetSpeed { get; private set; } = 1.0;

	public double EffectiveSpeed { get; private set; } = 1.0;

	public bool IsInCombat { get; private set; }

	public void LoadTargetSpeed(double speed)
	{
		TargetSpeed = SpeedPresetCycle.Normalize(speed);
		RefreshEffectiveSpeed();
	}

	public double CycleTargetSpeed()
	{
		TargetSpeed = SpeedPresetCycle.Next(TargetSpeed);
		RefreshEffectiveSpeed();
		return TargetSpeed;
	}

	public void EnterCombat()
	{
		IsInCombat = true;
		RefreshEffectiveSpeed();
	}

	public void ExitCombat()
	{
		IsInCombat = false;
		RefreshEffectiveSpeed();
	}

	private void RefreshEffectiveSpeed()
	{
		EffectiveSpeed = IsInCombat ? TargetSpeed : 1.0;
	}
}
