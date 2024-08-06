using VintageSymphony.Situations.Facts;

namespace VintageSymphony.Situations.Evaluator;

public interface IEvaluator
{
	bool IsEvaluatingSituation(Situation situation);
	float Evaluate(Situation situation, SituationalFacts facts);
}