var target = Argument("target", "PushNuGet");
var packageFeedUrl = "https://skynetcode.pkgs.visualstudio.com/_packaging/skynetpackagefeed/nuget/v3/index.json";					 
var solutionFilePath = "./JwtAuthenticationHelper.sln";
var coreLibPath = "./src/JwtHelper.Core/JwtHelper.Core.csproj";
var cookieExtensionLibPath = "./src/JwtHelper.ServiceCollection.Extensions.Cookies/JwtHelper.ServiceCollection.Extensions.Cookies.csproj";
var jwtExtensionLibPath = "./src/JwtHelper.ServiceCollection.Extensions.JwtBearer/JwtHelper.ServiceCollection.Extensions.JwtBearer.csproj";

Setup(ctx=>
{
    var buildNumber = EnvironmentVariable("BUILD_BUILDNUMBER");

    if(!string.IsNullOrWhiteSpace(buildNumber))
    {
        Information($"The build number was {buildNumber}");
        semVer = buildNumber;
    }
    else
    {
        Information($"The build number was empty, using the default semantic version of {semVer.ToString()}");
    }

	SetUpNuget();
});

void SetUpNuget()
{
	Information("Setting up Nuget feed...");

	var feed = new
	{
		Name = "SkynetNuget",
	    Source = packageFeedUrl
	};

	if (!DotNetCoreNuGetHasSource(name:feed.Name))
	{
		Warning($"Nuget feed {feed.Source} not found, adding...");
	    var nugetSourceSettings = new DotNetCoreNuGetSourceSettings
                             {
                                Source = feed.Source,
                                UserName = "skynetcode",
                                Password = EnvironmentVariable("SYSTEM_ACCESSTOKEN"),
				                StorePasswordInClearText = true
                                Verbosity = DotNetVerbosity.Detailed
                             };			

		try
        {
            DotNetCoreNuGetAddSource(
                name:feed.Name,
                settings:nugetSourceSettings);
        }
        catch(Exception ex)
        {
            Warning(ex.Message);
        }
	}	
	else
	{
		Information($"Nuget feed {feed.Name} already exists!");
	}
}

Task("Restore")
    .Does(() => {		
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
		Information($"Publishing to {packageFeedUrl}");
		var files = GetFiles("./artifacts/**/*.*.nupkg");		

		var settings = new DotNetCoreNuGetPushSettings
        {
            Source = "https://skynetcode.pkgs.visualstudio.com/_packaging/skynetpackagefeed/nuget/v3/index.json",
            ApiKey = "gibberish",
            SkipDuplicate = true
        };

		foreach(var file in files)
		{
			Information("File: {0}", file);
        	DotNetCoreNuGetPush(file.FullPath, settings);			
		}
});

RunTarget(target);