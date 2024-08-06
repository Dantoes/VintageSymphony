using VintageSymphony.Situations.Facts;
using VintageSymphony.Util;

namespace VintageSymphony.Situations.Evaluator;

public class DangerEvaluator : IEvaluator
{
	public bool IsEvaluatingSituation(Situation situation)
	{
		return situation == Situation.Danger;
	}

	public float Evaluate(Situation situation, SituationalFacts facts)
	{
		// TODO: add temporal stablility

		float enemies = MoreMath.ClampMap(facts.EnemyDistance, 5, 30, 1, 0);
		float holdingWeapon = facts.IsHoldingWeapon ? 1f : 0f;
		float damage = MoreMath.ClampMap(facts.SecondsSinceLastDamage, 0, 60, 1, 0);
		float damageWeight = damage >= 0.5f && facts.EnemyDistance < 15 ? 3f : 0.2f;

		return enemies + MoreMath.WeightedAverage(
			new Tuple<float, float>(holdingWeapon, 0.15f),
			new Tuple<float, float>(damage, damageWeight)
		);
	}
}