var target = Argument("target", "PushNuGet");
var packageFeedUrl = "https://skynetcode.pkgs.visualstudio.com/_packaging/skynetpackagefeed/nuget/v3/index.json";

void SetUpNuget()
{
	Information("Setting up Nuget feed...");

	var feed = new
	{
		Name = "SkynetNuget",
	    Source = packageFeedUrl
	};

	if (!NuGetHasSource(source:feed.Source))
	{
		Warning($"Nuget feed {feed.Source} not found, adding...");
	    var nugetSourceSettings = new NuGetSourcesSettings
                             {
                                 UserName = "skynetcode",
                                 Password = EnvironmentVariable("NUGET_PAT"),
                                 Verbosity = NuGetVerbosity.Detailed
                             };	

		Information($"NUGET_PAT was {EnvironmentVariable("NUGET_PAT")}");

		NuGetAddSource(
		    name:feed.Name,
		    source:feed.Source,
		    settings:nugetSourceSettings);
	}	
	else
	{
		Information($"Nuget feed {feed.Source} already exists!");
	}
}

Task("Restore")
    .Does(() => {
		Information("Restoring nuget packages...");
		DotNetCoreRestore("./src/JwtAuthenticationHelper/JwtAuthenticationHelper.csproj");	
});

Task("Build")
	.IsDependentOn("Restore")
    .Does(()=>{
		var config = new DotNetCoreBuildSettings
		{
			Configuration = "Release"
		};
		Information("Building solution...");
        DotNetCoreBuild("./src/JwtAuthenticationHelper/JwtAuthenticationHelper.csproj", config);
});

Task("Pack")
	.IsDependentOn("Build")
	.Does(()=>{
		var settings = new DotNetCorePackSettings
		{
		    Configuration = "Release",
		    OutputDirectory = "./artifacts/",
			NoBuild = true,
			NoRestore = true
		};

		Information("Packing binaries...");
		DotNetCorePack("./src/JwtAuthenticationHelper/JwtAuthenticationHelper.csproj", settings);
});

Task("PushToNuGet")
	.IsDependentOn("Pack")
	.Does(()=>{
		Information($"Publishing to {packageFeedUrl}...");
		var files = GetFiles("./**/JwtAuthenticationHelper.*.nupkg");
		var pat = EnvironmentVariable("NUGET_PAT");

		foreach(var file in files)
		{
			Information("File: {0}", file);

			using(var process = StartAndReturnProcess("dotnet", 
				new ProcessSettings
				{ 
					Arguments = $"nuget push {file} --skip-duplicate -n true -s {packageFeedUrl} -k {pat}" 
				}))
			{
				process.WaitForExit();
				// This should output 0 as valid arguments supplied
				var exitCode = process.GetExitCode();

				if (exitCode > 0)
					throw new InvalidOperationException($"Failed to publish to Nuget with exit code {exitCode}.");
			}
		}
		// var settings = new DotNetCoreNuGetPushSettings
		// {
		//     Source = packageFeedUrl,
		//     ApiKey = ""
		// };
		// Information($"Pushing the package up to the nuget feed {packageFeedUrl}...");
		// DotNetCoreNuGetPush("./artifacts/JwtAuthenticationHelper*.nupkg", settings);
});

RunTarget(target);