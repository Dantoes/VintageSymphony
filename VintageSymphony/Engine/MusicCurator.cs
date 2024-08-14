using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using VintageSymphony.Situations;

namespace VintageSymphony.Engine;

public class MusicCurator
{
	private readonly ICoreClientAPI clientApi;
	private readonly SituationBlackboard situationBlackboard;
	private readonly Dictionary<Situation, List<ScoredMusicTrack>> tracksBySituation = new();
	private readonly List<ScoredMusicTrack> allScoredTracks = new();
	private readonly TrackRestrictionMatcher trackRestrictionMatcher;
	private readonly TrackCooldownManager trackCooldownManager;

	private List<MusicTrack> tracks = new();

	public List<MusicTrack> Tracks
	{
		get => tracks;
		set
		{
			tracks = value;
			InitializeTracks();
		}
	}

	public MusicCurator(ICoreClientAPI clientApi, SituationBlackboard situationBlackboard,
		TrackCooldownManager trackCooldownManager)
	{
		this.clientApi = clientApi;
		this.situationBlackboard = situationBlackboard;
		this.trackCooldownManager = trackCooldownManager;
		trackRestrictionMatcher = new TrackRestrictionMatcher(clientApi.World.Calendar);

		foreach (var situation in Enum.GetValues<Situation>())
		{
			tracksBySituation[situation] = new List<ScoredMusicTrack>();
		}
	}

	private void InitializeTracks()
	{
		allScoredTracks.Clear();
		allScoredTracks.AddRange(tracks.Select(t => new ScoredMusicTrack(t)));

		foreach (var situationTracks in tracksBySituation)
		{
			situationTracks.Value.Clear();
			situationTracks.Value.AddRange(allScoredTracks.Where(t =>
				t.Track.TrackSituations.Contains(situationTracks.Key)));
		}
	}

	private bool ShouldReplaceCurrentTrack(MusicTrack track, IList<SituationAssessment> highestAssessments)
	{
		var trackMatchesCurrentSituation = highestAssessments
			.Select(a => a.Situation)
			.Intersect(track.TrackSituations)
			.Any();
		if (!trackMatchesCurrentSituation)
		{
			return true;
		}

		var highestPrioritizedTrackSituation = track.TrackSituations
			.Max(s => SituationDataProvider.GetAttributes(s).Priority);
		var highestSituationAssessment = highestAssessments
			.Max(a => SituationDataProvider.GetAttributes(a.Situation).Priority);

		return highestPrioritizedTrackSituation < highestSituationAssessment;
	}

	public MusicTrack? GetReplacementTrack(MusicTrack currentTrack)
	{
		var highestAssessments = GetHighestAssessments();
		if (!ShouldReplaceCurrentTrack(currentTrack, highestAssessments))
		{
			return null;
		}

		return FindBestMatchingTracks(highestAssessments)
			.Select(st => st.Track)
			.FirstOrDefault();
	}

	public MusicTrack? GetReplacementTrackForPause()
	{
		var highestAssessments = GetHighestAssessments();
		if (highestAssessments.Any(a => SituationDataProvider.GetAttributes(a.Situation).BreaksPause))
		{
			return FindBestMatchingTracks(highestAssessments)
				.Select(st => st.Track)
				.FirstOrDefault();
		}

		return null;
	}

	public MusicTrack? FindBestMatchingTrack()
	{
		return FindBestMatchingTracks().FirstOrDefault();
	}

	public IEnumerable<MusicTrack> FindBestMatchingTracks()
	{
		return FindBestMatchingTracks(GetHighestAssessments())
			.Select(t => t.Track);
	}

	private IEnumerable<ScoredMusicTrack> FindBestMatchingTracks(IList<SituationAssessment> highestAssessments)
	{
		var playerProperties = VintageSymphony.ClientMain.playerProperties;
		var playerPosition = clientApi.World.Player.Entity.Pos.AsBlockPos;
		var climateCondition = clientApi.World.BlockAccessor.GetClimateAt(playerPosition);

		UpdateScoredTracks(highestAssessments);
		return allScoredTracks
			.Where(t => t.Score > 0f)
			.Where(t => !trackCooldownManager.IsOnCooldown(t.Track))
			.Where(t => trackRestrictionMatcher.IsWithinConfiguredRestrictions(t.Track, playerProperties,
				climateCondition, playerPosition))
			.OrderByDescending(GetTrackOrder);
	}

	private void UpdateScoredTracks(IList<SituationAssessment> assessments)
	{
		for (int i = 0; i < allScoredTracks.Count; i++)
		{
			CalculateTrackScore(allScoredTracks[i], assessments);
			allScoredTracks[i].Track.BeginSort();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private double GetTrackOrder(ScoredMusicTrack track)
	{
		const double m = 1000000.0;
		const double m2 = m * m;
		return track.Track.SituationPriority * m2
		       + track.Track.Priority * m
		       + track.Track.StartPriority * track.Score;
	}

	private void CalculateTrackScore(ScoredMusicTrack track, IList<SituationAssessment> assessments)
	{
		track.Score = CalculateTrackScore(track.Track, assessments);
	}

	private float CalculateTrackScore(MusicTrack track, IList<SituationAssessment> assessments)
	{
		float score = 0f;
		for (int i = 0; i < assessments.Count; i++)
		{
			for (int j = 0; j < track.TrackSituations.Length; j++)
			{
				if (assessments[i].Situation == track.TrackSituations[j])
				{
					score += assessments[i].WeightedCertainty;
					break;
				}
			}
		}

		return score;
	}

	private IList<SituationAssessment> GetHighestAssessments()
	{
		const float certaintyFuzziness = 0.2f;
		float highestCertainty = situationBlackboard.Blackboard[0].WeightedCertainty;
		IList<SituationAssessment> highestAssessments = situationBlackboard.Blackboard
			.TakeWhile(s => s.WeightedCertainty >= highestCertainty - certaintyFuzziness)
			.OrderByDescending(s => s.WeightedCertainty)
			.ToList();
		return highestAssessments;
	}
}