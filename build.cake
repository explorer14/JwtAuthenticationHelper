var target = Argument("target", "PushNuGet");

void SetUpNuget()
{
	var feed = new
	{
		Name = "SkynetNuget",
	    Source = "https://skynetcode.pkgs.visualstudio.com/_packaging/skynetpackagefeed/nuget/v3/index.json"
	};

	if (!NuGetHasSource(source:feed.Source))
	{
	    var nugetSourceSettings = new NuGetSourcesSettings
                             {
                                 UserName = "skynetcode",
                                 Password = EnvironmentVariable("NUGET_PAT"),
                                 Verbosity = NuGetVerbosity.Detailed
                             };		

		NuGetAddSource(
		    name:feed.Name,
		    source:feed.Source,
		    settings:nugetSourceSettings);
	}	
}

Task("Restore")
    .Does(() => {		
		SetUpNuget();
		DotNetCoreRestore("./JwtAuthenticationHelper.sln");	
});

Task("Build")
	.IsDependentOn("Restore")
    .Does(()=>{
		var config = new DotNetCoreBuildSettings
		{
			Configuration = "Release"
		};
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

		DotNetCorePack("./src/JwtAuthenticationHelper/JwtAuthenticationHelper.csproj", settings);
});

Task("PushNuGet")
	.IsDependentOn("Pack")
	.Does(()=>{
		var settings = new DotNetCoreNuGetPushSettings
		{
		    Source = "https://skynetcode.pkgs.visualstudio.com/_packaging/skynetpackagefeed/nuget/v3/index.json",
		    ApiKey = ""
		};

		DotNetCoreNuGetPush("./artifacts/JwtAuthenticationHelper*.nupkg", settings);
});

RunTarget(target);