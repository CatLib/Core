param(
[string]$version="master-dev"
)

$dist = "dist"
$root = (Get-Item -Path ".\").FullName
$publishDir = $root + "\src\CatLib.Core\bin\Release\netstandard2.0\publish"
$version = $version.Trim()
$versionParttern = "^v?(?<master>(?<major>\d{1,5})(?<minor>\.\d+)?(?<patch>\.\d+)?(?<revision>\.\d+)?|master)(?<stability>\-(?:stable|beta|b|RC|alpha|a|patch|pl|p|dev)(?:(?:[.-]?\d+)+)?)?(?<build>\+[0-9A-Za-z\-\.]+)?$"
if(!($version -match $versionParttern))
{
    throw ("Invalid version, must conform to the semver version: " + $version)
}

if($matches["stability"] -eq $null)
{
	$matches["stability"] = "-stable"
}

if($matches["master"] -eq "master")
{
	$matches["major"] = "0"
}

if($matches["minor"] -eq $null)
{
	$matches["minor"] = ".0"
}

if($matches["patch"] -eq $null)
{
	$matches["patch"] = ".0"
}

if($matches["build"] -match "^[0-9a-f]{40}$")
{
	$env:CATLIB_COMMIT_SHA = $matches["build"].substring(1).trim()
    $matches["build"] = $matches["build"].substring(0, 8)
}
elseif($env:CI_COMMIT_SHA -ne $null)
{
	$env:CATLIB_COMMIT_SHA = ($env:CI_COMMIT_SHA).trim()
	$matches["build"] = "+" + $env:CATLIB_COMMIT_SHA.substring(0, 7)
}
else
{
	$env:CATLIB_COMMIT_SHA = (git rev-parse HEAD).trim()
	$matches["build"] = "+" + $env:CATLIB_COMMIT_SHA.substring(0, 7)
}

$major = $matches["major"]
$minor = $matches["minor"]
$assemblyVersion = $major + ".0.0.0"
$versionNormalized = $major + $minor + $matches["patch"] + $matches["revision"] + $matches["stability"] + $matches["build"]

$beginDateTime = ([DateTime] "01/01/2000");
$midnightDateTime = (Get-Date -Hour 0 -Minute 0 -Second 0);

$elapseDay = -1 * (New-TimeSpan -end $beginDateTime).Days
$elapseMidnight = [math]::floor((New-TimeSpan $midnightDateTime -End (Get-Date)).TotalSeconds * 0.5)
$fileVersion = $major + $minor + "." + $elapseDay + "." + $elapseMidnight

dotnet publish src\CatLib.Core\CatLib.Core.csproj -c Release /p:Version=$versionNormalized /p:AssemblyVersion=$assemblyVersion /p:FileVersion=$fileVersion --self-contained false

if($LastExitCode){
	echo "Abnormal. build failded."
	exit(1)
}

if(Test-Path -Path $dist)
{
	Remove-Item $dist -Recurse
}

mkdir $dist
cp $publishDir\* $dist -Recurse

$env:CATLIB_PROJECT_VERSION=$versionNormalized.trim()
$env:CATLIB_FILE_VERSION=$fileVersion.trim()
$env:CATLIB_STABILITY=$matches["stability"].substring(1).trim()
$env:CATLIB_MIN_DOTNET_CORE="3.0.0".trim()
$env:CATLIB_PUBLISH_DIR=$publishDir.trim()

echo "CATLIB_COMMIT_SHA $env:CATLIB_COMMIT_SHA"
echo "CATLIB_FILE_VERSION $env:CATLIB_FILE_VERSION"
echo "CATLIB_PROJECT_VERSION $env:CATLIB_PROJECT_VERSION"
echo "CATLIB_STABILITY $env:CATLIB_STABILITY"
echo "CATLIB_MIN_DOTNET_CORE $env:CATLIB_MIN_DOTNET_CORE"
echo "CATLIB_PUBLISH_DIR $env:CATLIB_PUBLISH_DIR"