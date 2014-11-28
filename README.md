PowerBridge
============

What is PowerBridge?
--------------------------------
PowerBridge is a build task library that bridges the worlds of MSBuild and PowerShell. PowerBridge's build tasks host the PowerShell runtime and forwards Write-Host, Write-Warning, Write-Verbose and Write-Error messages to MSBuild appropriately, allowing you to call your PowerShell scripts from within your build with deep MSBuild integration.

Where can I get it?
--------------------------------
PowerBridge is available as a [NuGet package](https://www.nuget.org/packages/PowerBridge/). You can install it from Visual Studio's NuGet Package Manager UI, or from the package manager console:

    PM> Install-Package PowerBridge

How do I get started?
--------------------------------
- Add the PowerBridge NuGet package to your project
- Edit the .BuildScripts\BeforeBuild.ps1 and .BuildScripts\AfterBuild.ps1 that were automatically added to your project.

Check out the [getting started guide](https://github.com/PowerBridge/PowerBridge/wiki/Getting-Started) for more info. 


