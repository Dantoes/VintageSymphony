using System.Runtime.CompilerServices;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace VintageSymphony.Engine;

public class TrackRestrictionMatcher
{
    private readonly IClientGameCalendar calendar;

    public TrackRestrictionMatcher(IClientGameCalendar calendar)
    {
        this.calendar = calendar;
    }

    public bool IsWithinConfiguredRestrictions(MusicTrack musicTrack, TrackedPlayerProperties props, ClimateCondition conds, BlockPos pos)
    {
        return IsHourInRange(calendar.HourOfDay, musicTrack.MinHour, musicTrack.MaxHour)
               && IsBetween((float)Math.Abs(calendar.OnGetLatitude(pos.Z)), musicTrack.MinLatitude,
                   musicTrack.MaxLatitude)
               && IsBetween(calendar.GetSeasonRel(pos), musicTrack.MinSeason, musicTrack.MaxSeason)
               && IsBetween(conds.Temperature, musicTrack.MinTemperature, musicTrack.MaxTemperature)
               && IsBetween(conds.WorldGenTemperature, musicTrack.MinWorldGenTemperature, musicTrack.MaxWorldGenTemperature)
               && conds.Rainfall >= musicTrack.MinRainFall
               && IsBetween(conds.WorldgenRainfall, musicTrack.MinWorldGenRainfall, musicTrack.MaxWorldGenRainfall)
               && props.sunSlight >= musicTrack.MinSunlight
               && props.DistanceToSpawnPoint >= musicTrack.DistanceToSpawnPoint;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsBetween(float value, float min, float max)
    {
        return value >= min && value <= max;
    }
	
    private bool IsHourInRange(float hourOfDay, float minHour, float maxHour)
    {
        if (minHour <= maxHour)
        {
            // Simple case: range does not cross midnight
            return hourOfDay >= minHour && hourOfDay < maxHour;
        }

        // Range crosses midnight
        return hourOfDay >= minHour || hourOfDay < maxHour;
		
    }
}