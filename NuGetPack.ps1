Switch ("$env:Build_SourceBranchName")
{
	"master" { dotnet pack "HiP-EventStoreLib\HiP-EventStoreLib.csproj" -o . }
	"develop" { dotnet pack "HiP-EventStoreLib\HiP-EventStoreLib.csproj" -o . --version-suffix "develop" }
	default { exit }
}

$nupkg = (ls HiP-EventStoreLib\*.nupkg).FullName
dotnet nuget push "$nupkg" -k "$env:MyGetKey" -s "$env:MyGetFeed"