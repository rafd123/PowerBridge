param
(
    [Parameter(Position=0)]
    [System.Collections.Hashtable]$Properties = @{}    
)

$scriptDirectoryPath = Split-Path $MyInvocation.MyCommand.Definition
$psakeFilePath = Join-Path $scriptDirectoryPath 'tools\psake\psake.ps1'
    
Set-StrictMode -Off
& $psakeFilePath -NoLogo -Properties $Properties