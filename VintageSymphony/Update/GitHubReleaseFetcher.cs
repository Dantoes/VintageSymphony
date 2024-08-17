using Newtonsoft.Json.Linq;

namespace VintageSymphony.Update;

public class GitHubReleaseFetcher
{
	private static readonly HttpClient httpClient = new();

	public async Task<Release?>? GetLatestReleaseAsync(string apiUrl)
	{
		try
		{
			httpClient.DefaultRequestHeaders.Add("User-Agent", "C# App");

			HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
			response.EnsureSuccessStatusCode();
			string content = await response.Content.ReadAsStringAsync();
			JArray releases = JArray.Parse(content);

			// Find the latest version by comparing tag_name
			return releases
				.Select(GetRelease)
				.Where(release => release != null)
				.MaxBy(release => release?.Version ?? null);
		}
		catch (Exception ex)
		{
			// Handle exceptions (e.g., network errors, JSON parsing errors)
			Console.WriteLine($"An error occurred: {ex.Message}");
			return null;
		}
	}

	private Release? GetRelease(JToken obj)
	{
		var tagName = obj["tag_name"]!.ToString();
		if (tagName.StartsWith("v"))
		{
			tagName = tagName.Substring(1);
		}

		if (obj["assets"] is not JArray assets || assets.Count == 0)
		{
			return null;
		}


		try
		{
			return new Release
			{
				Version = new Version(tagName),
				DownloadUrl = assets[0]["browser_download_url"]!.ToString(),
				FileName = assets[0]["name"]!.ToString(),
			};
		}
		catch
		{
			return null;
		}

	}
}