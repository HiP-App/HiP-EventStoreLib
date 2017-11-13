Param(
	[string]$MyGetFeed
	[string]$MyGetKey
)

echo "Feed: $env:MyGetFeed"
echo "Key: $MyGetKey"

Switch ("$env:Build_SourceBranchName")
{
	"master" { dotnet pack "HiP-EventStoreLib\HiP-EventStoreLib.csproj" -o . }
	"develop" { dotnet pack "HiP-EventStoreLib\HiP-EventStoreLib.csproj" -o . --version-suffix "develop" }
	"nuget" { dotnet pack "HiP-EventStoreLib\HiP-EventStoreLib.csproj" -o . --version-suffix "nuget" }
	default { exit }
}

$nupkg = (ls HiP-EventStoreLib\*.nupkg).FullName
dotnet nuget push "$nupkg" -k "$MyGetKey" -s "$MyGetFeed"