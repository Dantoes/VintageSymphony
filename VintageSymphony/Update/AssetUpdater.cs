using Vintagestory.API.Common;

namespace VintageSymphony.Update;

// ReSharper disable once UnusedType.Global
public class AssetUpdater : BaseModSystem
{
	private const string ApiUrl = "https://api.github.com/repos/Dantoes/VintageSymphony-Assets-Release/releases";
	private const string AssetModId = "vintage-symphony-assets";
	
	private readonly GitHubReleaseFetcher releaseFetcher = new();
	private Task<Release?>? releaseFetcherTask;
	private Task? upgradeTask;
	private UpdateInstalledOverlay? updateOverlay;

	public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;
	private long releaseFetcherListener;
	private long upgradeListener;
	private long showOverlayListener;

	protected override void OnGameStarted()
	{
		clientApi!.Logger.Notification($"Checking {AssetModId} for available updates…");

		releaseFetcherListener = clientApi!.World.RegisterGameTickListener(FetchLatestRelease, 1000, 2000);
		releaseFetcherTask = releaseFetcher.GetLatestReleaseAsync(ApiUrl);
		updateOverlay = new UpdateInstalledOverlay(clientApi);
	}

	private void FetchLatestRelease(float dt)
	{
		if (!releaseFetcherTask?.IsCompleted ?? true)
		{
			return;
		}
		
		clientApi!.World.UnregisterGameTickListener(releaseFetcherListener);
		var release = releaseFetcherTask.Result;
		if (release == null)
		{
			clientApi.Logger.Error($"Failed to get {AssetModId} release information from GitHub");
			return;
		}
		InterpretRelease(release);
	}

	private void InterpretRelease(Release release)
	{
		var installedVersion = GetInstalledVersion();
		if (release.Version == installedVersion)
		{
			clientApi!.Logger.Notification($"{AssetModId} is up to date");
			return;
		}
		
		clientApi!.Logger.Notification($"Updating {AssetModId} to version {release.Version}…");
		upgradeTask = UpgradeToRelease(release, installedVersion != null);
		upgradeListener = clientApi!.World.RegisterGameTickListener(CheckUpgradeProgress, 1000, 1000);
	}

	private void CheckUpgradeProgress(float obj)
	{
		if (!upgradeTask?.IsCompleted ?? true)
		{
			return;
		}
		clientApi!.World.UnregisterGameTickListener(upgradeListener);

		updateOverlay!.TryOpen();
		showOverlayListener = clientApi!.World.RegisterGameTickListener(CloseUpdateOverlay, 1000, 60000);
	}


	private async Task UpgradeToRelease(Release release, bool deleteObsoleteFiles)
	{
		string modsPath = Path.Combine(clientApi!.DataBasePath, "Mods");
		string[] obsoleteModFiles = Directory.GetFiles(modsPath, $"{AssetModId}*");
		
		await DownloadReleaseAsync(release);

		if (!deleteObsoleteFiles)
		{
			return;
		}

		foreach (var obsoleteModFile in obsoleteModFiles)
		{
			try
			{
				File.Delete(obsoleteModFile);
			}
			catch (Exception e)
			{
				clientApi.Logger.Error($"Failed to delete obsolete mod file: {e.Message}, Path: {obsoleteModFile}");
			}
		}
		
	}
	
	private async Task DownloadReleaseAsync(Release release)
	{
		try
		{
			using HttpClient client = new HttpClient();
			string filePath = Path.Combine(clientApi!.DataBasePath, "Mods", release.FileName);
			byte[] fileData = await client.GetByteArrayAsync(release.DownloadUrl);
			await File.WriteAllBytesAsync(filePath, fileData);
		}
		catch (Exception ex)
		{
			clientApi!.Logger.Error($"$Failed to download {AssetModId} release: {ex.Message}");
		}
	}
	

	private Version? GetInstalledVersion()
	{
		var versionString = GetModInfo()?.Version;
		return (versionString == null) ? null : new Version(versionString);
	}

	private ModInfo? GetModInfo()
	{
		return clientApi!.ModLoader.GetMod(AssetModId)?.Info;
	}

	private void CloseUpdateOverlay(float obj)
	{
		clientApi!.World.UnregisterGameTickListener(showOverlayListener);
		updateOverlay!.TryClose();
	}
}