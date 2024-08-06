using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace VintageSymphony.Engine;

public sealed class MusicTrackWrapper : MusicTrack
{
	private IMusicTrack wrappedTrack;

	public MusicTrackWrapper(IMusicTrack wrappedTrack)
	{
		this.wrappedTrack = wrappedTrack;

		if (wrappedTrack is SurfaceMusicTrack wrappedSurfaceTrack)
		{
			Location = wrappedSurfaceTrack.Location ?? AssetLocation.Create("undefined");
			Priority = wrappedSurfaceTrack.Priority;
			StartPriority = wrappedSurfaceTrack.StartPriority;
			Situation = Situations.Situation.Calm.ToString();
		}

		if (wrappedTrack is CaveMusicTrack)
		{
			Location = AssetLocation.Create("undefined");
			Situation = Situations.Situation.Cave.ToString();
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