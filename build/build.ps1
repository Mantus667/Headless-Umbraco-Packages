$PSScriptFilePath = Get-Item $MyInvocation.MyCommand.Path
$RepoRoot = $PSScriptFilePath.Directory.Parent.FullName
$BuildFolder = Join-Path -Path $RepoRoot -ChildPath "build";
$SolutionRoot = Join-Path -Path $RepoRoot "src";
$ProjFileForNuget = Join-Path -Path $SolutionRoot "HeadlessUmbracoPackages.Package\HeadlessUmbracoPackages.Package.csproj"

# Locate visual studio 2017
# using vswhere from https://github.com/Microsoft/vswhere
$vsloc = ./vswhere -latest -requires Microsoft.Component.MSBuild
$vspath = ""
$vsloc | ForEach {
	if ($_.StartsWith("installationPath: ")) {
		$vspath = $_.SubString("installationPath: ".Length)
	}
}
if ($vspath -eq "") {
	Write-Warning "Could not find VS 2017"
	Exit
}
$MSBuild = "$vspath\MSBuild\15.0\Bin\MSBuild.exe";

####### DO THE SLN BUILD PART #############

# Go get nuget.exe if we don't have it
$NuGet = "$BuildFolder\nuget.exe"
$FileExists = Test-Path $NuGet 
If ($FileExists -eq $False) {
	$SourceNugetExe = "http://nuget.org/nuget.exe"
	Invoke-WebRequest $SourceNugetExe -OutFile $NuGet
}
& $NuGet update -self

# Build the solution in release mode
$SolutionPath = Join-Path -Path $SolutionRoot -ChildPath "HeadlessUmbracoPackages.sln";

# clean sln for all deploys
& $MSBuild "$SolutionPath" /p:Configuration=Release /maxcpucount /t:Clean
if (-not $?)
{
	throw "The MSBuild process returned an error code."
}

#build
& $MSBuild "$SolutionPath" /p:Configuration=Release /maxcpucount
if (-not $?)
{
	throw "The MSBuild process returned an error code."
}

$nugetparams = "-IncludeReferencedProjects", "-Prop Configuration=Release"

Write-Host "Starting nuget packaging"

& $NuGet pack $ProjFileForNuget -IncludeReferencedProjects -Prop Configuration=Release

Write-Host "Nuget packaging finished"
