var target = Argument("target", "PushNuGet");
var packageFeedUrl = "https://skynetcode.pkgs.visualstudio.com/_packaging/skynetpackagefeed/nuget/v3/index.json";
var solutionFilePath = "./JwtAuthenticationHelper.sln";
var coreLibPath = "./src/JwtHelper.Core/JwtHelper.Core.csproj";
var cookieExtensionLibPath = "./src/JwtHelper.ServiceCollection.Extensions.Cookies/JwtHelper.ServiceCollection.Extensions.Cookies.csproj";
var jwtExtensionLibPath = "./src/JwtHelper.ServiceCollection.Extensions.JwtBearer/JwtHelper.ServiceCollection.Extensions.JwtBearer.csproj";

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

Task("Verify-PR")
	.IsDependentOn("Build")
	.Does(()=> {
		var config = new DotNetCoreTestSettings
		{
			NoBuild = true,
			Configuration = "Release"
		};
		DotNetCoreTest(solutionFilePath, config);
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

		var settings = new DotNetCoreNuGetPushSettings
        {
            Source = packageFeedUrl,
            ApiKey = EnvironmentVariable("SYSTEM_ACCESSTOKEN"),
            SkipDuplicate = true
        };

		foreach(var file in files)
		{
			Information("File: {0}", file);
        	DotNetCoreNuGetPush(file.FullPath, settings);			
		}
});

RunTarget(target);