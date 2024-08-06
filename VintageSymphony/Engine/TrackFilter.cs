using Vintagestory.API.Client;

namespace VintageSymphony.Engine;

public class TrackFilter
{
    public bool KeepTrack(IMusicTrack track)
    {
        if (track is SurfaceMusicTrack surfaceTrack)
        {
            return surfaceTrack.Location.Domain != "game";
        }

        return true;
    }
}