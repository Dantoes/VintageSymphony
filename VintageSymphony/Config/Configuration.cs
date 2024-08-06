using Newtonsoft.Json;

namespace VintageSymphony.Config;

[JsonObject(MemberSerialization.Fields)]
public class Configuration
{
    public float GlobalVolume = 1f;
}