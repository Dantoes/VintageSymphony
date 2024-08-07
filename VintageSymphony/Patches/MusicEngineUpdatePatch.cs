using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming

namespace VintageSymphony.Patches;

[HarmonyPatch(typeof(SystemMusicEngine), "OnEverySecond")]
public class MusicEngineUpdatePatch
{
    static bool Prefix(float dt, 
        IMusicTrack[] ___shuffledTracks)
    {
        VintageSymphony.MusicEngine?.LoadTracks(___shuffledTracks);
        return false; 
    }
}
