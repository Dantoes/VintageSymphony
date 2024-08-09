using Vintagestory.API.Common;

namespace VintageSymphony.UpdateNotifier;

// ReSharper disable once UnusedType.Global
public class UpdateNotifier : BaseModSystem
{
	private const string ApiUrl = "https://api.github.com/repos/Dantoes/VintageSymphony/releases";
	private readonly GitHubReleaseFetcher releaseFetcher = new();
	private Task<Version?>? releaseFetcherTask;
	private AvailableUpdateOverlay updateOverlay;

	public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;
	private long checkVersionResultListener;
	private long showOverlayListener;

	protected override void OnGameStarted()
	{
		checkVersionResultListener = clientApi!.World.RegisterGameTickListener(CheckVersionResult, 1000, 10000);
		releaseFetcherTask = releaseFetcher.GetLatestVersionAsync(ApiUrl);
		updateOverlay = new AvailableUpdateOverlay(clientApi);
	}

	private void CheckVersionResult(float dt)
	{
		if (releaseFetcherTask?.IsCompleted ?? false)
		{
			clientApi!.World.UnregisterGameTickListener(checkVersionResultListener);
			var version = releaseFetcherTask.Result;
			if (version != null)
			{
				InterpretRecentVersion(version);
			}
		}
	}

	private void InterpretRecentVersion(Version version)
	{
		var modVersion = new Version(Mod.Info.Version);
		if (version <= modVersion)
		{
			return;
		}
		
		updateOverlay.SetVersion(modVersion, version);
		updateOverlay.TryOpen();
		showOverlayListener = clientApi!.World.RegisterGameTickListener(CloseUpdateOverlay, 1000, 15000);
	}

	private void CloseUpdateOverlay(float obj)
	{
		clientApi!.World.UnregisterGameTickListener(showOverlayListener);
		updateOverlay.TryClose();
	}
}