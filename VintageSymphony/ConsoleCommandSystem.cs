using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace VintageSymphony;

// ReSharper disable once UnusedType.Global
public class ConsoleCommandSystem : ModSystem
{
	public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

	public override void StartClientSide(ICoreClientAPI api)
	{
		base.StartClientSide(api);
		api.ChatCommands.Get("music")
			.BeginSubCommand("next")
			.WithDescription("Play the next track")
			.HandleWith(NextTrack)
			.EndSubCommand()
			// --
			.BeginSubCommand("info")
			.WithDescription("Displays the currently playing track")
			.HandleWith(OutputCurrentTrack)
			.EndSubCommand()
			// --
			.BeginSubCommand("stop")
			.HandleWith(StopTrack)
			.EndSubCommand()
			// --
			.BeginSubCommand("debug")
			.WithDescription("Toggle debug overlay")
			.HandleWith(ToggleDebugOverlay)
			.EndSubCommand()
			// --
			.BeginSubCommand("config")
			.WithDescription("Toggle Vintage Symphony configuration")
			.HandleWith(ToggleConfigurationDialog)
			.EndSubCommand();
	}

	private TextCommandResult ToggleConfigurationDialog(TextCommandCallingArgs args)
	{
		var configurationDialog = VintageSymphony.ConfigurationDialog;
		if (configurationDialog.IsOpened())
		{
			configurationDialog.TryClose();
		}
		else
		{
			configurationDialog.TryOpen();
		}

		return TextCommandResult.Success();
	}

	private TextCommandResult ToggleDebugOverlay(TextCommandCallingArgs args)
	{
		var debugOverlay = VintageSymphony.DebugOverlay;
		if (debugOverlay.IsOpened())
		{
			debugOverlay.TryClose();
		}
		else
		{
			debugOverlay.TryOpen();
		}

		return TextCommandResult.Success();
	}

	private TextCommandResult StopTrack(TextCommandCallingArgs args)
	{
		VintageSymphony.MusicEngine?.StopTrackAndPause();
		return TextCommandResult.Success();
	}

	private TextCommandResult OutputCurrentTrack(TextCommandCallingArgs args)
	{
		var track = VintageSymphony.MusicEngine?.CurrentMusicTrack;
		if (track == null)
		{
			return TextCommandResult.Success("&gt; no track playing");
		}

		return TextCommandResult.Success($"&gt; {track.Title} [{track.PositionString}]");
	}

	private TextCommandResult NextTrack(TextCommandCallingArgs args)
	{
		VintageSymphony.MusicEngine?.NextTrack();
		return TextCommandResult.Success();
	}
}