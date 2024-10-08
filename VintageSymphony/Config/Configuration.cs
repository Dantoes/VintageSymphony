using Newtonsoft.Json;

namespace VintageSymphony.Config;

[JsonObject(MemberSerialization.Fields)]
public class Configuration
{
    public bool InitialConfigurationShown = false;
    public float GlobalVolume = 1f;
    public bool LoadGameMusic = false;
    public bool LoadVintageSymphonyMusic = true;
    public bool LoadCaveTrack = true;
}