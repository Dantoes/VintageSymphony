namespace VintageSymphony.Situations.Facts;

public struct SituationalFacts
{
	public SituationalFacts()
	{
	}
	
	public float DistanceTravelledTotal;
	public float DistanceTravelledDiagonal;
	public float MovementRadius;
	public int SecondsSinceLastDamage = Int32.MaxValue;
	public float DistanceFromHome;
	public float Time;
	public long Now;
	public float RelativeHeight;
	public float DistanceToSurface;
	public bool IsHoldingWeapon;
	public float EnemyDistance = EnemyDistanceMax + 1;
	public const float EnemyDistanceMax = 50f;
	public float SunLevel;
	public const float SunLevelMax = 32f;
}