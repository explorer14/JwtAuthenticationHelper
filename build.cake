var target = Argument("target", "PushNuGet");
var packageFeedUrl = "https://skynetcode.pkgs.visualstudio.com/_packaging/skynetpackagefeed/nuget/v3/index.json";
var solutionFilePath = "./JwtAuthenticationHelper.sln";
var coreLibPath = "./src/JwtGenerator/JwtGenerator.csproj";
var cookieExtensionLibPath = "./src/JwtGenerator.ServiceCollection.Extensions.Cookies/JwtGenerator.ServiceCollection.Extensions.Cookies.csproj";
var jwtExtensionLibPath = "./src/JwtGenerator.ServiceCollection.Extensions.JwtBearer/JwtGenerator.ServiceCollection.Extensions.JwtBearer.csproj";

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
                                 Password = EnvironmentVariable("SYSTEM_ACCESSTOKEN"),
                                 Verbosity = NuGetVerbosity.Detailed
                             };			

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
		SetUpNuget();
		Information("Restoring nuget packages...");
		DotNetCoreRestore(solutionFilePath);	
});

Task("Build")
	.IsDependentOn("Restore")
    .Does(()=>{
		var config = new DotNetCoreBuildSettings
		{
			Configuration = "Release"
		};
		Information("Building solution...");
        DotNetCoreBuild(solutionFilePath, config);
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
		DotNetCorePack(coreLibPath, settings);
		DotNetCorePack(cookieExtensionLibPath, settings);
		DotNetCorePack(jwtExtensionLibPath, settings);
});

Task("PushToNuGet")
	.IsDependentOn("Pack")
	.Does(()=>{
		Information($"Publishing to {packageFeedUrl}...");
		var files = GetFiles("./artifacts/**/*.*.nupkg");		

		foreach(var file in files)
		{
			Information("File: {0}", file);

			using(var process = StartAndReturnProcess("dotnet", 
				new ProcessSettings
				{ 
					Arguments = $"nuget push {file} --skip-duplicate -n true -s {packageFeedUrl} -k bla" 
				}))
			{
				process.WaitForExit();
				// This should output 0 as valid arguments supplied
				var exitCode = process.GetExitCode();

				if (exitCode > 0)
					throw new InvalidOperationException($"Failed to publish to Nuget with exit code {exitCode}.");
			}
		}
});

RunTarget(target);