namespace VintageSymphony.Situations;

public class SituationAssessment
{
    private readonly float priority;
    public readonly bool SmoothIncreasingCertainty;
    public readonly bool SmoothDecreasingCertainty;
    
    public Situation Situation { get; }

    public float Certainty { get; set; }

    public float WeightedCertainty => Certainty * priority;

    public SituationAssessment(Situation situation, float certainty)
    {
        Situation = situation;
        Certainty = certainty;
        priority = SituationDataProvider.GetPriorityValue(Situation);
        SmoothIncreasingCertainty = SituationDataProvider.GetSmoothIncreasingCertainty(situation);
        SmoothDecreasingCertainty = SituationDataProvider.GetSmoothDecreasingCertainty(situation);
    }
}