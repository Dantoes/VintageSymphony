using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Clean;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Core;
using Cake.Frosting;
using Cake.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Common;
using Path = System.IO.Path;

namespace CakeBuild;

public static class Program
{
	public static string SolutionDirectory = null!;

	public static int Main(string[] args)
	{
		SolutionDirectory = FindSolutionDirectory();
		return new CakeHost()
			.UseContext<BuildContext>()
			.Run(args);
	}

	static string FindSolutionDirectory()
	{
		string? directory = Directory.GetCurrentDirectory();

		while (directory != null)
		{
			string[] solutionFiles = Directory.GetFiles(directory, "*.sln");
			if (solutionFiles.Length > 0)
			{
				return directory;
			}

			directory = Directory.GetParent(directory)?.FullName;
		}

		throw new Exception("Failed to find solution file");
	}
}

public class BuildContext : FrostingContext
{
	public const string ProjectName = "VintageSymphony";
	public string BuildConfiguration { get; set; }
	public string Version { get; }
	public string Name { get; }
	public bool SkipJsonValidation { get; set; }

	public BuildContext(ICakeContext context)
		: base(context)
	{
		BuildConfiguration = context.Argument("configuration", "Release");
		SkipJsonValidation = context.Argument("skipJsonValidation", false);
		var modInfo =
			context.DeserializeJsonFromFile<ModInfo>(Path.Combine(Program.SolutionDirectory, ProjectName,
				"modinfo.json"));
		Version = modInfo.Version;
		Name = modInfo.ModID;
	}
}

[TaskName("ValidateJson")]
public sealed class ValidateJsonTask : FrostingTask<BuildContext>
{
	public override void Run(BuildContext context)
	{
		if (context.SkipJsonValidation)
		{
			return;
		}

		var jsonFiles = context.GetFiles(Path.Combine(Program.SolutionDirectory, BuildContext.ProjectName, "assets",
			"**", "*.json"));
		foreach (var file in jsonFiles)
		{
			try
			{
				var json = File.ReadAllText(file.FullPath);
				JToken.Parse(json);
			}
			catch (JsonException ex)
			{
				throw new Exception(
					$"Validation failed for JSON file: {file.FullPath}{Environment.NewLine}{ex.Message}", ex);
			}
		}
	}
}

[TaskName("Build")]
[IsDependentOn(typeof(ValidateJsonTask))]
public sealed class BuildTask : FrostingTask<BuildContext>
{
	public override void Run(BuildContext context)
	{
		var projectFile = Path.Combine(Program.SolutionDirectory, BuildContext.ProjectName,
			$"{BuildContext.ProjectName}.csproj");
		context.DotNetClean(projectFile,
			new DotNetCleanSettings
			{
				Configuration = context.BuildConfiguration
			});


		context.DotNetPublish(projectFile,
			new DotNetPublishSettings
			{
				Configuration = context.BuildConfiguration
			});
	}
}

[TaskName("Package")]
[IsDependentOn(typeof(BuildTask))]
public sealed class PackageTask : FrostingTask<BuildContext>
{
	public override void Run(BuildContext context)
	{
		var projectDir = Path.Combine(Program.SolutionDirectory, BuildContext.ProjectName);
		var projectFile = Path.Combine(projectDir, $"{BuildContext.ProjectName}.csproj");

		context.EnsureDirectoryExists("../Releases");
		context.CleanDirectory("../Releases");
		var buildDir = $"../Releases/{context.Name}";

		context.EnsureDirectoryExists(buildDir);
		context.CopyFiles($"{projectDir}/bin/{context.BuildConfiguration}/{BuildContext.ProjectName}.dll", buildDir);

		if (context.BuildConfiguration == "Debug")
		{
			context.CopyFiles($"{projectDir}/bin/{context.BuildConfiguration}/{BuildContext.ProjectName}.pdb",
				buildDir);
		}

		var musicAssetsPath = $"{buildDir}/assets/{context.Name}/music/";
		context.EnsureDirectoryExists(musicAssetsPath);
		var releasePath = $"{Program.SolutionDirectory}/Releases";
		context.EnsureDirectoryExists(releasePath);
		
		context.CopyFiles($"{Program.SolutionDirectory}/Assets/music/*.ogg", musicAssetsPath);
		context.CopyFiles($"{Program.SolutionDirectory}/Assets/music/musicconfig.json", musicAssetsPath);
		context.CopyFile($"{projectDir}/modinfo.json", $"{buildDir}/modinfo.json");

		context.Zip(buildDir, $"{releasePath}/{context.Name}_{context.Version}.zip");
		context.DeleteDirectory(buildDir, new DeleteDirectorySettings { Recursive = true });
	}
}

[TaskName("Default")]
[IsDependentOn(typeof(PackageTask))]
public class DefaultTask : FrostingTask
{
}