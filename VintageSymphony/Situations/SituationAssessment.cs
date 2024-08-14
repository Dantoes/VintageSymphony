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
        var situationData = SituationDataProvider.GetAttributes(situation);
        Situation = situation;
        Certainty = certainty;
        priority = situationData.Priority;
        SmoothIncreasingCertainty = situationData.SmoothIncreasingCertainty;
        SmoothDecreasingCertainty = situationData.SmoothDecreasingCertainty;
    }
}