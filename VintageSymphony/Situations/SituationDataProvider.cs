using System.Reflection;
using VintageSymphony.Situations.Evaluator;

namespace VintageSymphony.Situations;

public static class SituationDataProvider
{
    private static readonly Dictionary<Situation, SituationDataAttribute> SituationData;

    static SituationDataProvider()
    {
        SituationData = Enum.GetValues(typeof(Situation))
            .Cast<Situation>()
            .ToDictionary(situation => situation, LoadSituationData);
    }

    private static SituationDataAttribute LoadSituationData(Situation situation)
    {
        FieldInfo? fieldInfo = situation.GetType().GetField(situation.ToString());
        return fieldInfo?.GetCustomAttribute<SituationDataAttribute>() ?? new SituationDataAttribute();
    }

    public static float GetPriorityValue(Situation situation)
    {
        return SituationData[situation].Priority;
    }
    
    public static bool GetBreaksPause(Situation situation)
    {
        return SituationData[situation].BreaksPause;
    }
    
    public static bool GetBreaksJustStartedTracks(Situation situation)
    {
        return SituationData[situation].BreaksJustStartedTracks;
    }
    
    public static bool GetSmoothIncreasingCertainty(Situation situation)
    {
        return SituationData[situation].SmoothIncreasingCertainty;
    }
        
    public static bool GetSmoothDecreasingCertainty(Situation situation)
    {
        return SituationData[situation].SmoothDecreasingCertainty;
    }
}