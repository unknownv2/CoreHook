
Add-Type -AssemblyName System.IO.Compression.FileSystem
function Unzip
{
    param([string]$zipfile, [string]$outpath)

    [System.IO.Compression.ZipFile]::ExtractToDirectory($zipfile, $outpath)
}

Write-Host Downloading native binaries from GitHub Releases...

$detourModulePath  = "deps/corehook"
$hostingModulePath = "deps/coreload"

$architecturewinx86   = "win-x86"
$architecturewinx64   = "win-x64"
$architecturewinarm   = "win-arm"
$architecturewinarm64 = "win-arm64"

if (Test-Path "$PSScriptRoot/$detourModulePath/")
{
    Remove-Item "$PSScriptRoot/$detourModulePath/" -Force -Recurse | Out-Null
}

if (Test-Path "$PSScriptRoot/$hostingModulePath/")
{
    Remove-Item "$PSScriptRoot/$hostingModulePath/" -Force -Recurse | Out-Null
}

$detourModulePathWinx86 = "$PSScriptRoot/$detourModulePath/$architecturewinx86"
$detourModulePathWinx64 = "$PSScriptRoot/$detourModulePath/$architecturewinx64"
$detourModulePathWinARM = "$PSScriptRoot/$detourModulePath/$architecturewinarm"
$detourModulePathWinARM64 = "$PSScriptRoot/$detourModulePath/$architecturewinarm64"

$hostingModulePathWinx86 = "$PSScriptRoot/$hostingModulePath/$architecturewinx86"
$hostingModulePathWinx64 = "$PSScriptRoot/$hostingModulePath/$architecturewinx64"
$hostingModulePathWinARM = "$PSScriptRoot/$hostingModulePath/$architecturewinarm"
$hostingModulePathWinARM64 = "$PSScriptRoot/$hostingModulePath/$architecturewinarm64"

New-Item -ItemType Directory -Force -Path "$detourModulePathWinx86" | Out-Null
New-Item -ItemType Directory -Force -Path "$detourModulePathWinx64" | Out-Null
New-Item -ItemType Directory -Force -Path "$detourModulePathWinARM" | Out-Null
New-Item -ItemType Directory -Force -Path "$detourModulePathWinARM64" | Out-Null

New-Item -ItemType Directory -Force -Path "$hostingModulePathWinx86" | Out-Null
New-Item -ItemType Directory -Force -Path "$hostingModulePathWinx64" | Out-Null
New-Item -ItemType Directory -Force -Path "$hostingModulePathWinARM" | Out-Null
New-Item -ItemType Directory -Force -Path "$hostingModulePathWinARM64" | Out-Null

function DownloadRelease
{
    param([string]$repository, [string]$releasefile, [string]$outDir)

    $downloadUrl = "$repository/releases/download/$tag/$releasefile"

    Write-Host "Download url: $downloadUrl"

    $client = New-Object System.Net.WebClient
    $client.DownloadFile(
        $downloadUrl,
        "$outDir/$releasefile")
    if( -not $? )
    {
        $msg = $Error[0].Exception.Message
        Write-Error "Couldn't download $releasefile. This most likely indicates the Windows native build failed."
        exit
    }

    Unzip "$outDir/$releasefile" "$outDir"        
}

# Download the application function detour module
$repo = "unknownv2/CoreHook.Hooking"
$releases = "https://api.github.com/repos/$repo/releases"

Write-Host "Determining latest release for $repo"
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$tag = (Invoke-WebRequest -Uri $releases -UseBasicParsing | ConvertFrom-Json)[0].tag_name

Write-Host "Dowloading latest release from $repo"

$repository = "https://github.com/unknownv2/CoreHook.Hooking"

DownloadRelease $repository "corehook-Release-x86.zip" $detourModulePathWinx86
DownloadRelease $repository "corehook-Release-x64.zip" $detourModulePathWinx64
DownloadRelease $repository "corehook-Release-ARM.zip" $detourModulePathWinARM
DownloadRelease $repository "corehook-Release-ARM64.zip" $detourModulePathWinARM64


# Download the CoreCLR hosting module
$repo = "unknownv2/CoreHook.Host"
$releases = "https://api.github.com/repos/$repo/releases"

Write-Host "Determining latest release for $repo"
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$tag = (Invoke-WebRequest -Uri $releases -UseBasicParsing | ConvertFrom-Json)[0].tag_name

Write-Host "Dowloading latest release from $repo"

$repository = "https://github.com/unknownv2/CoreHook.Host"

DownloadRelease $repository "coreload-Release-x86.zip" $hostingModulePathWinx86
DownloadRelease $repository "coreload-Release-x64.zip" $hostingModulePathWinx64
DownloadRelease $repository "coreload-Release-ARM.zip" $hostingModulePathWinARM
DownloadRelease $repository "coreload-Release-ARM64.zip" $hostingModulePathWinARM64