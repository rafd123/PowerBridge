# [CmdletBinding()]
param
(
    [Parameter(Mandatory=$true)]
    [string]$MSBuildProjectFile
)

Write-Host 'Hello world'
Write-Host "Check it out: $MSBuildProjectFile"

#Write-Warning 'Warning world'
#Write-Error 'Goodbye world'