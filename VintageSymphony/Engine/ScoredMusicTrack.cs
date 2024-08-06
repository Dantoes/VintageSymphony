namespace VintageSymphony.Engine;

public class ScoredMusicTrack
{
	public MusicTrack Track { get; private set; }
	public float Score { get; set; } = 0f;

	public ScoredMusicTrack(MusicTrack track, float score = 0f)
	{
		Track = track;
		Score = score;
	}

	public override string ToString()
	{
		return $"[{Score}] {Track}";
	}
}