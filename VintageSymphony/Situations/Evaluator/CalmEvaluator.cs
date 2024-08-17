using VintageSymphony.Situations.Facts;
using VintageSymphony.Util;

namespace VintageSymphony.Situations.Evaluator;

public class CalmEvaluator : IEvaluator
{
	public bool IsEvaluatingSituation(Situation situation)
	{
		return situation == Situation.Calm;
	}

	public float Evaluate(Situation situation, SituationalFacts facts)
	{
		float enemies = MoreMath.ClampMap(facts.EnemyDistance, 10, SituationalFacts.EnemyDistanceMax, 1, 0);
		float rifts = MoreMath.ClampMap(facts.RiftDistance, 0, 50, 1, 0);
		float holdingWeapon = facts.IsHoldingWeapon ? 1f : 0f;
		float damage = MoreMath.ClampMap(facts.SecondsSinceLastDamage, 0, 60, 1, 0);

		return 1f
		       - enemies
		       - rifts * .5f
		       - holdingWeapon * .15f
		       - damage * .3f;
	}
}