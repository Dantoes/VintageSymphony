using Vintagestory.API.Client;
using VintageSymphony.Situations;

namespace VintageSymphony;

public class GameTrackSituationLibrary
{
	public string GetSituationString(IMusicTrack track)
	{
		return string.Join('|', GetSituations(track));
	}

	public Situation[] GetSituations(IMusicTrack track)
	{
		var name = GetTrackName(track);
		if (name == null)
		{
			return Array.Empty<Situation>();
		}

		switch (name)
		{
			case "adventuring":
			case "peaceful-village":
				return new[] { Situation.Adventure };

			case "arcticwinds":
			case "creating":
			case "hallowcroft":
			case "heartbeat-survive":
			case "midnight":
			case "mirror":
			case "sunny-village-create":
			case "heartbeat-create":
			case "setting-sun":
			case "spring":
			case "winter":
			case "night-to-day":
				return new[] { Situation.Calm, Situation.Idle };

			case "through-the-grass-survive":
			case "to-dawn":
			case "nostalgic":
				return new[] { Situation.Calm, Situation.Idle, Situation.Adventure };

			case "building":
				return new[] { Situation.Idle };

			case "fall-o'-croft":
			case "daylight":
			case "groove":
			case "radianceandrust":
			case "vintagestory":
			case "summer-day":
				return new[] { Situation.Calm, Situation.Adventure };

			case "theresonancearchives-barren":
				return new[] { Situation.Danger };

			case "cave":
				return new[] { Situation.Cave };
			
			// TODO: Eidolon battle as fight music. Is in assets on disc but not in AssetManager?
			default:
				return new[] { Situation.Calm };
		}
	}

	private string? GetTrackName(IMusicTrack track)
	{
		if (track is SurfaceMusicTrack)
		{
			return (track as SurfaceMusicTrack)?.Location.PathOmittingPrefixAndSuffix("music/", ".ogg");
		}

		if (track is CaveMusicTrack)
		{
			return "cave";
		}

		return null;
	}
}