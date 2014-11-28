[CmdletBinding(DefaultParameterSetName='Foo')]
param
(
    [Parameter(Mandatory=$true, ParameterSetName='Foo')]
	$Arg1,

    [Parameter(Mandatory=$true, ParameterSetName='Bar')]
	$Arg2
)

Write-Host "Arg1 = $Arg1"
Write-Host "Arg2 = $Arg2"