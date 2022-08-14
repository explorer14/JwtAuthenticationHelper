#!/usr/bin/pwsh
Param (    
    [string]$Target = "Build",
    [string]$Configuration = "Release",
    [string]$Verbosity= "Normal",
    [Switch]$NoBuild
)

Invoke-Expression -Command "dotnet new tool-manifest --force" -ErrorAction Stop;
Invoke-Expression -Command "dotnet tool install Cake.Tool --version 0.38.5" -ErrorAction Stop;
Invoke-Expression -Command "dotnet cake --target='$Target' --configuration='$Configuration' --verbosity='$Verbosity' --doNotBuild='$NoBuild'" -ErrorAction Stop;
