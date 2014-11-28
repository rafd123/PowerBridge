param
(
    # You can use any of the well-known Visual Studio and MSBuild properties as script parameters.
    # To use one, simply declare it as a parameter.
    # To get a complete list, see:
    # - Macros for Build Commands and Properties: http://msdn.microsoft.com/en-us/library/c02as0cs.aspx
    # - Common MSBuild Project Properties: http://msdn.microsoft.com/en-us/library/bb629394.aspx
    # - MSBuild Reserved and Well-Known Properties: http://msdn.microsoft.com/en-us/library/ms164309.aspx
    $SolutionDir,
    $ProjectDir
)

$scriptFileName = Split-Path $MyInvocation.MyCommand.Path -Leaf 
Write-Warning "$scriptFileName was added as part of the PowerBridge NuGet package. Double-click here to edit/customize the script. If it's not needed, simply delete the file."