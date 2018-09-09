#!/usr/bin/env pwsh
[CmdletBinding(PositionalBinding = $false)]
param(
    [ValidateSet('Debug', 'Release')]
    $Configuration = $null,
    $OutputDir = $null,
    $ExampleDir = $null,
    [string]
    $example,    
    [string]
    $runtime
)

Set-StrictMode -Version 1
$ErrorActionPreference = 'Stop'

# 
# Main
#

if (!$Configuration) {
    $Configuration = 'Release'
}

if (!$OutputDir) {
    $OutputDir = Split-Path $MyInvocation.MyCommand.Path
    $OutputDir = [io.path]::combine($OutputDir, 'Publish')
}


if ($example -eq 'win32') {
    $ExampleName = 'CoreHook.FileMonitor' 
}
if ($example -eq 'uwp') {
    $ExampleName = 'CoreHook.UWP.FileMonitor' 
}
if ($example -eq 'unix') {
    $ExampleName = 'CoreHook.Unix.FileMonitor'
}

$ExamplesDir = 'examples'
$ExampleHookName = 'Hook'

$ExampleDir = [io.path]::combine($ExamplesDir, $example, $ExampleName)
$ExampleOutputDir = [io.path]::combine($OutputDir, $example, $runtime)

$ExampleHookDir = $ExampleDir + '.' + $ExampleHookName 
$ExampleHookOutputDir = [io.path]::combine($OutputDir, $example, $runtime, $ExampleHookName)

write-host -ForegroundColor DarkGray ">>> $ExampleDir $example $ExampleOutputDir $runtime $Configuration $ExampleHookOutputDir"

function exec([string]$_cmd) {
    write-host -ForegroundColor DarkGray ">>> $_cmd $args"
    $ErrorActionPreference = 'Continue'
    & $_cmd @args
    $ErrorActionPreference = 'Stop'
    if ($LASTEXITCODE -ne 0) {
        write-error "Failed with exit code $LASTEXITCODE"
        exit 1
    }
}

exec dotnet publish $ExampleDir --configuration $Configuration -r $runtime -o $ExampleOutputDir '-warnaserror:CS1591'
exec dotnet publish $ExampleHookDir --configuration $Configuration -r $runtime -o $ExampleHookOutputDir '-warnaserror:CS1591'


