using System.Text;
using Vintagestory.API.Client;
using VintageSymphony.Engine;

namespace VintageSymphony.Debug;

#nullable disable
public class DebugOverlay : HudElement
{
	private const string DebugOverlayTextKey = "vsdbg_text";
	private readonly MusicEngine musicEngine;
	private GuiComposer debugTextComposer;
	private GuiElementDynamicText textElement;
	private readonly StringBuilder sb = new(1024);

	public DebugOverlay(ICoreClientAPI api, MusicEngine musicEngine)
		: base(api)
	{
		this.musicEngine = musicEngine;
		SetupDialog();
	}

	private void SetupDialog()
	{
		debugTextComposer = capi.Gui
			.CreateCompo("debugScreenText",
				ElementBounds.Percentual(EnumDialogArea.RightTop, 0.5, 0.7).WithFixedAlignmentOffset(-5.0, 5.0))
			.AddDynamicText("", CairoFont.WhiteSmallishText().WithOrientation(EnumTextOrientation.Right),
				ElementBounds.Fill, DebugOverlayTextKey).OnlyDynamic()
			.Compose();
		textElement = debugTextComposer.GetDynamicText(DebugOverlayTextKey);
	}

	public override void OnFinalizeFrame(float dt)
	{
		if (musicEngine.SituationBlackboard != null)
		{
			UpdateText(dt);
		}

		debugTextComposer.PostRender(dt);
	}

	public override void OnRenderGUI(float deltaTime)
	{
		debugTextComposer.Render(deltaTime);
	}

	private void UpdateText(float deltaTime)
	{
		var playerPosition = VintageSymphony.ClientApi.World.Player.Entity.Pos.AsBlockPos;
		var climateCondition = VintageSymphony.ClientApi.World.BlockAccessor.GetClimateAt(playerPosition);

		var facts = musicEngine.SituationBlackboard.SituationalFacts;

		sb.Clear();
		sb.Append(nameof(facts.DistanceTravelledTotal)).Append(": ")
			.AppendLine(facts.DistanceTravelledTotal.ToString("0.##"));
		sb.Append(nameof(facts.DistanceTravelledDiagonal)).Append(": ")
			.AppendLine(facts.DistanceTravelledDiagonal.ToString("0.##"));
		sb.Append(nameof(facts.DistanceFromHome)).Append(": ").AppendLine(facts.DistanceFromHome.ToString("0.##"));
		sb.Append(nameof(facts.MovementRadius)).Append(": ").AppendLine(facts.MovementRadius.ToString("0.##"));
		sb.Append(nameof(facts.Time)).Append(": ").AppendLine(facts.Time.ToString("0.##"));
		sb.Append(nameof(facts.RelativeHeight)).Append(": ").AppendLine(facts.RelativeHeight.ToString("0.##"));
		sb.Append(nameof(facts.DistanceToSurface)).Append(": ").AppendLine(facts.DistanceToSurface.ToString("0.##"));
		sb.Append(nameof(facts.EnemyDistance)).Append(": ").AppendLine(facts.EnemyDistance.ToString("0.##"));
		sb.Append(nameof(facts.SecondsSinceLastDamage)).Append(": ")
			.AppendLine(facts.SecondsSinceLastDamage.ToString("0.##"));
		sb.Append(nameof(facts.IsHoldingWeapon)).Append(": ").AppendLine(facts.IsHoldingWeapon.ToString());
		sb.Append(nameof(facts.SunLevel)).Append(": ").AppendLine(facts.SunLevel.ToString("0"));
		sb.Append("Temperature: ").AppendLine(climateCondition.Temperature.ToString("0.##"));
		sb.AppendLine("---");
		foreach (var assessment in musicEngine.SituationBlackboard.Blackboard.OrderByDescending(s => s.Certainty))
		{
			sb.Append(assessment.Situation).Append(": ")
				.AppendLine(assessment.Certainty.ToString("0.##"));
		}

		sb.AppendLine("---");
		var track = VintageSymphony.MusicEngine.CurrentMusicTrack;
		if (track != null)
		{
			sb.Append(nameof(track)).Append(": ")
				.Append(track.Title)
				.Append(" (")
				.Append(track.Situation)
				.AppendLine(")");
		}
		else
		{
			sb.Append("Pause (").Append(VintageSymphony.MusicEngine.GetRemainingPauseDurationS()).AppendLine("s)");
		}


		textElement.SetNewTextAsync(sb.ToString());
	}
}