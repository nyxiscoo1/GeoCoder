#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./src/GeoCoder/bin") + Directory(configuration);
var distrDir = Directory("./distrib");

var assemblyInfo = ParseAssemblyInfo("./src/GeoCoder/Properties/AssemblyInfo.cs");

var fullVersion = assemblyInfo.AssemblyVersion.Substring(0, assemblyInfo.AssemblyVersion.LastIndexOf("."));
Information("Version: {0}", fullVersion);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./src/GeoCoder.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild("./src/GeoCoder.sln", settings =>
        settings.SetConfiguration(configuration)
          
          );
    }
    else
    {
      // Use XBuild
      XBuild("./src/GeoCoder.sln", settings =>
        settings.SetConfiguration(configuration));
    }
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit3("./src/**/bin/" + configuration + "/*.Tests.dll", new NUnit3Settings {
        NoResults = true
        });
});

Task("Publish")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    EnsureDirectoryExists(distrDir);
    Zip(buildDir, distrDir + File("GeoCoder v" + fullVersion + ".zip"));
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Publish");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
