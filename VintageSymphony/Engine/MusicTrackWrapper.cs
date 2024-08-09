using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace VintageSymphony.Engine;

public sealed class MusicTrackWrapper : MusicTrack
{
	private static readonly GameTrackSituationLibrary SituationLibrary = new ();
	private IMusicTrack wrappedTrack;
	private AssetLocation DefaultAssetLocation => AssetLocation.Create("undefined");

	public MusicTrackWrapper(IMusicTrack wrappedTrack)
	{
		this.wrappedTrack = wrappedTrack;

		if (wrappedTrack is SurfaceMusicTrack wrappedSurfaceTrack)
		{
			Location = wrappedSurfaceTrack.Location ?? DefaultAssetLocation;
			Situation = SituationLibrary.GetSituationString(wrappedTrack);
			Priority = wrappedSurfaceTrack.Priority;
			StartPriority = wrappedSurfaceTrack.StartPriority;
		}

		if (wrappedTrack is CaveMusicTrack)
		{
			Location = DefaultAssetLocation;
			Situation = SituationLibrary.GetSituationString(wrappedTrack);
			DisableCooldown = true;
			MinSunlight = 0;
		}

		InternalInitialize();
	}

	public override bool IsPlaying => wrappedTrack.IsActive;

	public override void BeginSort()
	{
		wrappedTrack.BeginSort();
	}

	public override void Initialize(IAssetManager assetManager, ICoreClientAPI capi, IMusicEngine musicEngine)
	{
		wrappedTrack.Initialize(assetManager, capi, musicEngine);
	}

	public override bool ShouldPlay(TrackedPlayerProperties props, ClimateCondition conds, BlockPos pos)
	{
		return wrappedTrack.ShouldPlay(props, conds, pos);
	}

	public override void BeginPlay(TrackedPlayerProperties props)
	{
		wrappedTrack.BeginPlay(props);
	}

	public override bool ContinuePlay(float dt, TrackedPlayerProperties props)
	{
		return wrappedTrack.ContinuePlay(dt, props);
	}

	public override void UpdateVolume()
	{
		wrappedTrack.UpdateVolume();
	}

	public override void FadeOut(float seconds, Action? onFadedOut = null)
	{
		wrappedTrack.FadeOut(seconds, onFadedOut);
	}

	public override void FastForward(float seconds)
	{
		wrappedTrack.FastForward(seconds);
	}
}