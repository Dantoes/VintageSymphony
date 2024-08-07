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
	private long pauseStartTime;
	private long pauseDuration;
	private long trackStartTime;

	private long playbackUpdateEventId;
	private const int PlaybackUpdateIntervalMs = 1 * 1000;
	private const int PlaybackUpdateDelayMs = 10 * 1000 + 50;

	private long songReplacementUpdateEventId;
	private const int SongReplacementUpdateIntervalMs = 1 * 1000;
	private const int MinPlaybackTimeForSongReplacementMs = 10 * 1000;

	private const int TrackCooldownCleanupIntervalMs = 2 * 60 * 1000;
	private TrackCooldownManager trackCooldownManager = null!;
	private long trackCooldownCleanupEventId;

	private SituationBlackboard situationBlackboard = null!;
	private long situationUpdateEventId;
	private const int SituationUpdateIntervalMs = 1 * 1000;

	private const int DisableTrackCooldownThresholdMs = 30 * 1000;

	public SituationBlackboard SituationBlackboard => situationBlackboard;
	private TrackedPlayerProperties PlayerProperties => MusicManager.ClientMain.playerProperties;
	private ILogger Logger => clientApi.Logger;
	public MusicTrack? CurrentMusicTrack { get; private set; }
	private MusicCurator musicCurator = null!;
	private long TrackPlayTimeMs => currentTimeMs() - trackStartTime;

	private static readonly float[][] PauseDurations =
	{
		new [] { 960f, 480f },
		new [] { 420f, 240f },
		new [] { 180f, 120f },
		new float[2]
	};

	public override void StartClientSide(ICoreClientAPI api)
	{
		base.StartClientSide(api);

		clientApi = api;
		musicFrequency = clientApi.Settings.Int["musicFrequency"];
		clientApi.Settings.Int.AddWatcher("musicFrequency", newValue =>
		{
			musicFrequency = newValue;
			if (IsPausing())
			{
				UpdatePauseDuration();
			}
		});
	}

	protected override void OnGameStarted()
	{
		situationBlackboard = new SituationBlackboard(MusicManager.Instance.AttributeStorage);
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
		
		SetupPause();
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
			if (replacementTrack.BreaksPause)
			{
				PlayTrack(replacementTrack);
				CurrentMusicTrack = replacementTrack;
			}
		}
		else
		{
			if (TrackPlayTimeMs < DisableTrackCooldownThresholdMs)
			{
				trackCooldownManager.Remove(CurrentMusicTrack!);
			}

			// Handle the case when there is a current track playing
			bool ignoreMinPlayTime = CurrentMusicTrack!.BreaksJustStartedTracks ||
			                         TrackPlayTimeMs > MinPlaybackTimeForSongReplacementMs;

			if (ignoreMinPlayTime)
			{
				if (CurrentMusicTrack.BreaksPause && !replacementTrack.BreaksPause)
				{
					StopTrack(5);
					SetupPause();
				}
				else
				{
					StopTrack(5);
					PlayTrack(replacementTrack);
				}
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
		trackStartTime = currentTimeMs();
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
		CurrentMusicTrack.FadeOut(fadeOutTimeS);
		CurrentMusicTrack = null;
	}

	public void StopTrackAndPause(float fadeOutTimeS = 2f)
	{
		StopTrack(fadeOutTimeS);
		SetupPause();
	}


	public void NextTrack()
	{
		StopTrack(0.5f);
		UpdatePauseDuration(0);
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
			CurrentMusicTrack = null;
			SetupPause();
		}
	}

	public void LoadTracks(IMusicTrack[] allTracks)
	{
		if (TracksLoaded())
		{
			return;
		}

		var filter = new TrackFilter();
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IsPausing()
	{
		return currentTimeMs() < pauseStartTime + pauseDuration;
	}

	private void SetupPause()
	{
		pauseStartTime = currentTimeMs();
		UpdatePauseDuration();
		Logger.Debug($"Pausing music for {pauseDuration / 1000L}s");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdatePauseDuration()
	{
		int frequencySetting = Math.Clamp(musicFrequency, 0, 3);
		float baseDuration = PauseDurations[frequencySetting][0];
		float variance = PauseDurations[frequencySetting][1];
		float duration = baseDuration - (Random.Shared.NextSingle() * 2 - 1) * variance;
		UpdatePauseDuration((long)duration * 1000L);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void UpdatePauseDuration(long duration)
	{
		pauseDuration = duration;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool ShouldPlaySong()
	{
		return !CurrentMusicTrack?.IsPlaying ?? !IsPausing();
	}

	public int GetRemainingPauseDurationS()
	{
		return (int)((pauseStartTime + pauseDuration - currentTimeMs()) / 1000L);
	}
}