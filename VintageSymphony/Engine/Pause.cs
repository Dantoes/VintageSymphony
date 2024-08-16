namespace VintageSymphony.Engine;

public class Pause
{
	private const long Inactive = -1L;
	private readonly Func<long> timeProvider;
	private long startTime = Inactive;
	private long duration;

	public bool Active => startTime != Inactive && timeProvider() < startTime + duration;

	public Pause(Func<long> timeProvider)
	{
		this.timeProvider = timeProvider;
	}

	public void Start(long durationMs)
	{
		startTime = timeProvider();
		duration = durationMs;
	}

	public void Stop()
	{
		startTime = Inactive;
	}

	public void UpdateDuration(long durationMs)
	{
		duration = durationMs;
	}

	public int GetRemainingTimeS()
	{
		return (int)((startTime + duration - timeProvider()) / 1000L);
	}
}