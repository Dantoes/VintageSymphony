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
	public string ModVersion { get; }
	public string ModName { get; }
	public string ModAssetsVersion { get; }
	public string ModAssetsName { get; }
	public bool SkipJsonValidation { get; set; }

	public BuildContext(ICakeContext context)
		: base(context)
	{
		BuildConfiguration = context.Argument("configuration", "Release");
		SkipJsonValidation = context.Argument("skipJsonValidation", false);
		
		var modInfoPath = Path.Combine(Program.SolutionDirectory, ProjectName, "modinfo.json");
		var modInfo = context.DeserializeJsonFromFile<ModInfo>(modInfoPath);
		ModVersion = modInfo.Version;
		ModName = modInfo.ModID;
		
		var modAssetsInfoPath = Path.Combine(Program.SolutionDirectory, "Assets", "modinfo.json");
		var modAssetsInfo = context.DeserializeJsonFromFile<ModInfo>(modAssetsInfoPath);
		ModAssetsVersion = modAssetsInfo.Version;
		ModAssetsName = modAssetsInfo.ModID;
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

[TaskName("PackageMod")]
[IsDependentOn(typeof(BuildTask))]
public sealed class PackageModTask : FrostingTask<BuildContext>
{
	public override void Run(BuildContext context)
	{
		var projectDir = Path.Combine(Program.SolutionDirectory, BuildContext.ProjectName);
		
		var releasePath = $"{Program.SolutionDirectory}/Releases";
		context.EnsureDirectoryExists(releasePath);		

		context.EnsureDirectoryExists("../Releases");
		context.CleanDirectory("../Releases");
		
		var modBuildDir = $"../Releases/{context.ModName}";
		PackageModArchive(context, modBuildDir, projectDir, releasePath);

		var assetsBuildDir = $"../Releases/{context.ModAssetsName}";
		PackageModAssetsArchive(context, assetsBuildDir, releasePath);


		context.DeleteDirectory(modBuildDir, new DeleteDirectorySettings { Recursive = true });
	}

	private static void PackageModAssetsArchive(BuildContext context, string assetsBuildDir, string releasePath)
	{
		var musicAssetsPath = $"{assetsBuildDir}/assets/{context.ModName}/music/";
		context.EnsureDirectoryExists(musicAssetsPath);
		
		context.CopyFiles($"{Program.SolutionDirectory}/Assets/music/*.ogg", musicAssetsPath);
		context.CopyFiles($"{Program.SolutionDirectory}/Assets/music/musicconfig.json", musicAssetsPath);
		context.CopyFile($"{Program.SolutionDirectory}/Assets/modinfo.json", $"{assetsBuildDir}/modinfo.json");
		context.Zip(assetsBuildDir, $"{releasePath}/{context.ModAssetsName}_{context.ModAssetsVersion}.zip");
	}

	private static void PackageModArchive(BuildContext context, string buildDir, string projectDir, string releasePath)
	{
		// Copy mod DLL
		context.EnsureDirectoryExists(buildDir);
		context.CopyFiles($"{projectDir}/bin/{context.BuildConfiguration}/{BuildContext.ProjectName}.dll", buildDir);
		
		// Copy mod debug symbols
		if (context.BuildConfiguration == "Debug")
		{
			context.CopyFiles($"{projectDir}/bin/{context.BuildConfiguration}/{BuildContext.ProjectName}.pdb",
				buildDir);
		}
		
		// copy modinfo.json
		context.CopyFile($"{projectDir}/modinfo.json", $"{buildDir}/modinfo.json");
		
		// package mod
		context.Zip(buildDir, $"{releasePath}/{context.ModName}_{context.ModVersion}.zip");
	}
}


[TaskName("Default")]
[IsDependentOn(typeof(PackageModTask))]
public class DefaultTask : FrostingTask
{
}