using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.Client.NoObf;
using VintageSymphony.Situations;

namespace VintageSymphony.Engine;

// ReSharper disable once ClassNeverInstantiated.Global
public class MusicEngine : BaseModSystem
{
	public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;
	public override double ExecuteOrder() => 1.6;

	private ICoreClientAPI clientApi = null!;
	private Func<long> currentTimeMs => () => clientApi.ElapsedMilliseconds;
	private int musicFrequency;
	public readonly Pause Pause;
	public readonly Pause ForcedPause;

	private long playbackUpdateEventId;
	private const int PlaybackUpdateIntervalMs = 1 * 1000;
	private const int PlaybackUpdateDelayMs = 10 * 1000 + 50;

	private long songReplacementUpdateEventId;
	private const int SongReplacementUpdateIntervalMs = 1 * 1000;
	private const int MinPlaybackTimeForSongReplacementMs = 8 * 1000;

	private const int TrackCooldownCleanupIntervalMs = 2 * 60 * 1000;
	private TrackCooldownManager trackCooldownManager = null!;
	private long trackCooldownCleanupEventId;

	private SituationBlackboard situationBlackboard = null!;
	private long situationUpdateEventId;
	private const int SituationUpdateIntervalMs = 1 * 1000;

	private const int DisableTrackCooldownThresholdMs = 30 * 1000;

	public SituationBlackboard SituationBlackboard => situationBlackboard;
	private TrackedPlayerProperties PlayerProperties => VintageSymphony.ClientMain.playerProperties;
	private ILogger Logger => clientApi.Logger;
	public MusicTrack? CurrentMusicTrack { get; private set; }
	private MusicCurator musicCurator = null!;
	private long TrackPlayTimeMs => (long)((CurrentMusicTrack?.Sound?.PlaybackPosition ?? 9999f) * 1000L);

	private static readonly float[][] PauseDurations =
	{
		new[] { 960f, 480f },
		new[] { 420f, 240f },
		new[] { 180f, 120f },
		new float[2]
	};


	public MusicEngine()
	{
		Pause = new Pause(currentTimeMs);
		ForcedPause = new Pause(currentTimeMs);
	}


	public override void StartClientSide(ICoreClientAPI api)
	{
		base.StartClientSide(api);

		clientApi = api;
		musicFrequency = clientApi.Settings.Int["musicFrequency"];
		clientApi.Settings.Int.AddWatcher("musicFrequency", newValue =>
		{
			musicFrequency = newValue;
			if (Pause.Active)
			{
				Pause.UpdateDuration(GetPauseDuration());
			}
		});
	}

	protected override void OnGameStarted()
	{
		situationBlackboard = new SituationBlackboard(VintageSymphony.Instance.AttributeStorage);
		trackCooldownManager = new TrackCooldownManager(() => clientApi.ElapsedMilliseconds);
		musicCurator = new MusicCurator(clientApi, situationBlackboard, trackCooldownManager);

		ClientSettings.Inst.AddWatcher<int>("musicLevel", OnMusicLevelChanged);

		playbackUpdateEventId =
			clientApi.World.RegisterGameTickListener(UpdatePlayback, PlaybackUpdateIntervalMs, PlaybackUpdateDelayMs);
		songReplacementUpdateEventId = clientApi.World.RegisterGameTickListener(ConsiderTrackReplacement,
			SongReplacementUpdateIntervalMs, SongReplacementUpdateIntervalMs + 80);
		trackCooldownCleanupEventId = clientApi.World.RegisterGameTickListener(
			_ => trackCooldownManager.CleanupRoutine(), TrackCooldownCleanupIntervalMs, TrackCooldownCleanupIntervalMs);
		situationUpdateEventId =
			clientApi.World.RegisterGameTickListener(UpdateSituation, SituationUpdateIntervalMs + 20);

		Pause.Start(GetPauseDuration());
		ForcedPause.Start(15 * 1000L);
	}

	private void OnMusicLevelChanged(int volume)
	{
		CurrentMusicTrack?.UpdateVolume();
	}

	public override void Dispose()
	{
		void UnregisterTickListeners(long eventId)
		{
			if (eventId != 0L)
			{
				clientApi.World.UnregisterGameTickListener(eventId);
			}
		}

		UnregisterTickListeners(playbackUpdateEventId);
		UnregisterTickListeners(songReplacementUpdateEventId);
		UnregisterTickListeners(trackCooldownCleanupEventId);
		UnregisterTickListeners(situationUpdateEventId);
		base.Dispose();
	}


	private void UpdatePlayback(float dt)
	{
		if (!IsGameStarted || !TracksLoaded() || ClientSettings.MusicLevel == 0)
		{
			return;
		}

		MonitorCurrentTrack();

		if (!ShouldPlaySong())
		{
			return;
		}

		var track = musicCurator.FindBestMatchingTrack();
		if (track != null)
		{
			StopTrack();
			PlayTrack(track);
		}
	}

	[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
	private void ConsiderTrackReplacement(float dt)
	{
		MusicTrack? replacementTrack = IsPlayingTrack()
			? musicCurator.GetReplacementTrack(CurrentMusicTrack!)
			: musicCurator.GetReplacementTrackForPause();
		if (replacementTrack == null)
		{
			return;
		}

		if (!IsPlayingTrack())
		{
			if (ForcedPause.Active && !replacementTrack.BreaksForcedPause)
			{
				return;
			}

			if (Pause.Active && !replacementTrack.BreaksPause)
			{
				return;
			}

			PlayTrack(replacementTrack);
		}
		else
		{
			if (TrackPlayTimeMs < DisableTrackCooldownThresholdMs)
			{
				trackCooldownManager.Remove(CurrentMusicTrack!);
			}

			// Handle the case when there is a current track playing
			var minPlaybackTimeReached = TrackPlayTimeMs > MinPlaybackTimeForSongReplacementMs;
			bool ignoreMinPlayTime = CurrentMusicTrack!.BreaksJustStartedTracks || minPlaybackTimeReached;
			if (!ignoreMinPlayTime)
			{
				return;
			}


			if ((CurrentMusicTrack.PauseAfterPlayback && !replacementTrack.BreaksPause)
			    || (CurrentMusicTrack.ForcedPauseAfterPlayback && !replacementTrack.BreaksForcedPause))
			{
				if (minPlaybackTimeReached)
				{
					StopTrackAndPause(5);
				}
			}
			else
			{
				StopTrack(5);
				PlayTrack(replacementTrack);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IsPlayingTrack()
	{
		return CurrentMusicTrack != null;
	}

	public void PlayTrack(MusicTrack track)
	{
		CurrentMusicTrack = track;
		CurrentMusicTrack.BeginPlay(PlayerProperties);
		if (!track.DisableCooldown)
		{
			trackCooldownManager.PutOnCooldown(CurrentMusicTrack);
		}

		Logger.Notification($"Playing track: {track.Name}");
	}

	public void StopTrack(float fadeOutTimeS = 2f)
	{
		if (!IsPlayingTrack())
		{
			return;
		}

		Logger.Debug($"Stopping track: {CurrentMusicTrack!.Name}");
		if (CurrentMusicTrack.IsPlaying)
		{
			CurrentMusicTrack.FadeOut(fadeOutTimeS);
		}

		CurrentMusicTrack = null;
	}

	public void StopTrackAndPause(float fadeOutTimeS = 2f)
	{
		var playingTrack = CurrentMusicTrack;
		StopTrack(fadeOutTimeS);
		if (playingTrack != null)
		{
			SetupPauseAfterTrack(playingTrack);
		}
	}


	public void NextTrack()
	{
		StopTrack(0.5f);
		Pause.Stop();
		ForcedPause.Stop();
	}

	private void MonitorCurrentTrack()
	{
		if (!IsPlayingTrack())
		{
			return;
		}

		// required for dynamic tracks to update (CaveMusicTrack)
		CurrentMusicTrack!.ContinuePlay(0f, PlayerProperties);

		if (!CurrentMusicTrack.IsPlaying)
		{
			StopTrackAndPause();
		}
	}

	public void LoadTracks(IMusicTrack[] allTracks)
	{
		if (TracksLoaded())
		{
			return;
		}

		var filter = new TrackFilter(VintageSymphony.Configuration, Mod.Info.ModID);
		musicCurator.Tracks = allTracks
			.Where(filter.KeepTrack)
			.Select(t => t as MusicTrack ?? new MusicTrackWrapper(t))
			.ToList();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool TracksLoaded()
	{
		return musicCurator.Tracks.Count > 0;
	}


	private void UpdateSituation(float dt)
	{
		situationBlackboard.Update(dt);
	}

	private void SetupPauseAfterTrack(MusicTrack track)
	{
		if (track.PauseAfterPlayback)
		{
			Pause.Start(GetPauseDuration());
		}
		if (track.ForcedPauseAfterPlayback)
		{
			ForcedPause.Start(8000L);
		}
	}


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private long GetPauseDuration()
	{
		int frequencySetting = Math.Clamp(musicFrequency, 0, 3);
		float baseDuration = PauseDurations[frequencySetting][0];
		float variance = PauseDurations[frequencySetting][1];
		float duration = baseDuration - (Random.Shared.NextSingle() * 2 - 1) * variance;
		return (long)duration * 1000L;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool ShouldPlaySong()
	{
		return !CurrentMusicTrack?.IsPlaying ?? !Pause.Active && !ForcedPause.Active;
	}
}