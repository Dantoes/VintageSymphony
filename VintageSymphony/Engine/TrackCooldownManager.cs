namespace VintageSymphony.Engine;

public class TrackCooldownManager
{
	private class TrackCooldown
	{
		public readonly long CooldownUntil;
		public readonly MusicTrack Track;

		public TrackCooldown(long cooldownUntil, MusicTrack track)
		{
			CooldownUntil = cooldownUntil;
			Track = track;
		}
	}

	private const long TrackCooldownMs = 30L * 60L * 1000L;
	private const long TrackCooldownVarianceMs = 4L * 60L * 1000L;
	private readonly List<TrackCooldown> tracksOnCooldown = new();
	private readonly Func<long> currentTimeMs;

	public TrackCooldownManager(Func<long> currentTimeMs)
	{
		this.currentTimeMs = currentTimeMs;
	}

	public void PutOnCooldown(MusicTrack musicTrack)
	{
		tracksOnCooldown.Add(new TrackCooldown(GetCooldownEndTime(), musicTrack));
	}

	public bool IsOnCooldown(MusicTrack musicTrack)
	{
		var now = currentTimeMs();
		return tracksOnCooldown.Exists(t => t.Track == musicTrack && now < t.CooldownUntil);
	}

	public void CleanupRoutine()
	{
		var now = currentTimeMs();
		tracksOnCooldown.RemoveAll(t => now > t.CooldownUntil);
	}

	public void Remove(MusicTrack musicTrack)
	{
		tracksOnCooldown.RemoveAll(t => t.Track == musicTrack);
	}

	private long GetCooldownEndTime()
	{
		var cooldownDuration = (long)(TrackCooldownMs + TrackCooldownVarianceMs * Random.Shared.NextSingle());
		var cooldownUntil = currentTimeMs() + cooldownDuration;
		return cooldownUntil;
	}
}