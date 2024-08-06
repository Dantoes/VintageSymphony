using Vintagestory.GameContent;
using VintageSymphony.Situations.Facts;

namespace VintageSymphony.Situations.Evaluator;

public class TemporalStormEvaluator : IEvaluator
{
	private readonly SystemTemporalStability? temporalStabilitySystem;

	public TemporalStormEvaluator()
	{
		temporalStabilitySystem = MusicManager.ClientApi.ModLoader.GetModSystem<SystemTemporalStability>();
	}

	public bool IsEvaluatingSituation(Situation situation)
	{
		return situation == Situation.TemporalStorm;
	}

	public float Evaluate(Situation situation, SituationalFacts facts)
	{
		return temporalStabilitySystem?.StormData.nowStormActive ?? false ? 1 : 0;
	}
}