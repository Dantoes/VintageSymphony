using Newtonsoft.Json.Linq;

namespace VintageSymphony.UpdateNotifier;

public class GitHubReleaseFetcher
{
	private static readonly HttpClient httpClient = new();

	public async Task<Version?>? GetLatestVersionAsync(string apiUrl)
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
				.Select(release => release["tag_name"]!.ToString())
				.Select(tag => tag.StartsWith("v") ? tag.Substring(1) : tag) // Remove 'v' if present
				.Select(tag =>
				{
					try
					{
						return new Version(tag);
					}
					catch
					{
						return null; // Return null if the version is invalid
					}
				})
				.Where(version => version != null)
				.MaxBy(version => version);
		}
		catch (Exception ex)
		{
			// Handle exceptions (e.g., network errors, JSON parsing errors)
			Console.WriteLine($"An error occurred: {ex.Message}");
			return null;
		}
	}
}