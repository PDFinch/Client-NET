#
# Builds and packages the libraries.
#
Param
(
	[Parameter()]
	[string]$Configuration,

	[Parameter()]
	[string]$Version
)

if ($Configuration -eq "")
{
    $Configuration = "Debug"
}

if ($Version -eq "")
{
    $Version = "0.9.0-dev01"
}

Write-Host "Building and packaging libraries, configuration: $Configuration, version: $Version"

dotnet build -c $Configuration -p:`"Version=$Version`"

$packParameters = @(
	"-c:$Configuration",
	"-o:NuGetPackageStagingArea",
	"-p:Version=$Version"
)

dotnet pack "src/PDFinch.Client.Common/PDFinch.Client.Common.csproj" $packParameters
dotnet pack "src/PDFinch.Client/PDFinch.Client.csproj" $packParameters
dotnet pack "src/PDFinch.Client.Extensions/PDFinch.Client.Extensions.csproj" $packParameters

explorer NuGetPackageStagingArea
