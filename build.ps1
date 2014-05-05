Framework '4.0'

Properties {
    $framework = $psake.context.Peek().config.framework
    $build_configuration = "Release"
	$build_dir = Split-Path $psake.build_script_file	
    $build_artifacts_dir_name = 'bin'
	$build_artifacts_dir = Join-Path $build_dir $build_artifacts_dir_name
	$code_dir = Join-Path $build_dir 'src'
    $sln_file_info = Get-ChildItem $code_dir *.sln | Select-Single
    $sln_path = $sln_file_info.FullName
    $sln_name = $sln_file_info.Name
    $nuget_dir = Join-Path $sln_file_info.DirectoryName '.nuget'
    $nuget_targets_path = Join-Path $nuget_dir 'NuGet.targets'
    $nuget_exe_path = Join-Path $nuget_dir 'NuGet.exe'    
    $artifacts = @(
        '*.dll'
        '*.exe'
        '*.pdb'
        '*.nupkg'
    )
}

FormatTaskName {
   param($taskName)

   Write-Host @"
   
----------------------------------------------------------------------
$taskName
----------------------------------------------------------------------

"@
}

Task Default -Depends Clean, RestoreNuGetPackages, Build, Test, CopyArtifacts, PackNuGetPackages

Task Clean {
	Write-Host "Cleaning artifacts from $build_artifacts_dir_name directory" -ForegroundColor Green
    
	if (Test-Path $build_artifacts_dir) 
	{	
		Remove-Item $build_artifacts_dir -Recurse -Force | Out-Null
	}
	
	mkdir $build_artifacts_dir | Out-Null
    
    Write-Host "Cleaning $sln_name" -ForegroundColor Green
    Exec { msbuild "$sln_path" /t:Clean /p:Configuration=$build_configuration /p:Platform="Any CPU" /v:quiet } 
}

Task RestoreNuGetPackages {
    Write-Host "Restoring NuGet packages for $sln_name" -ForegroundColor Green

    Exec { msbuild "$nuget_targets_path" /t:RestorePackages /p:DownloadNuGetExe=true /p:Configuration=$build_configuration /p:Platform="Any CPU" /v:quiet } 
    
    Assert (Test-Path $nuget_exe_path) ('The NuGet executable was not found at {0}' -f $nuget_exe_path)    
    Exec { & $nuget_exe_path restore $sln_path }
}

Task Build -Depends Clean {
    Write-Host "Building $sln_name" -ForegroundColor Green
    Exec { msbuild "$sln_path" /t:Build /p:Configuration=$build_configuration /p:Platform="Any CPU" /v:quiet }
}

Task Test -Depends Build {
    $nunit_exe_path = Get-ChildItem $build_dir nunit-console.exe -Recurse -File |
        Sort-Object VersionInfo.FileVersion -Descending |
        Select-Object -First 1 |
        Select-Object -ExpandProperty FullName

    Assert $nunit_exe_path 'NUnit runner was not found'
        
    Get-ChildItem $code_dir *Test*.csproj -Recurse |
        ForEach-Object {
            Write-Host "Running $($_.BaseName)" -ForegroundColor Green
            Exec { & $nunit_exe_path $_.FullName /config:$build_configuration /framework:net-$framework /nologo }
        }
}

Task PackNuGetPackages -Depends Build -Precondition { $build_configuration -eq 'Release' } {
    Get-ChildItem $code_dir *.nuspec -Recurse |
        ForEach-Object {
            Write-Host "Packing NuGet package for $($_.Name)" -ForegroundColor Green
            $project_path = Join-Path $_.DirectoryName "$($_.BaseName).csproj"
            Exec { & $nuget_exe_path pack $project_path -Tool -Symbols -OutputDirectory $build_artifacts_dir -Properties Configuration=$build_configuration -NoPackageAnalysis }
        }
}

Task CopyArtifacts -Depends Build {
    Write-Host "Copying artifacts to $build_artifacts_dir_name directory" -ForegroundColor Green
    Get-ChildItem $code_dir $build_configuration -Directory -Recurse |
        Where-Object { $_.Parent.Name -eq 'bin' } |
        Where-Object { $_.FullName -notlike '*test*' } |
        ForEach-Object { Get-ChildItem "$($_.FullName)\*" -Include $artifacts  -File } |
        ForEach-Object { Write-Host $_.Name; $_ } |
        Copy-Item -Destination $build_artifacts_dir
}

function Select-Single
{
    begin
    {
        $results = @()
    }
    
    process
    {
        $results += $_
    }
    
    end
    {
        if($results.Count -eq 0)
        {
            throw 'No results were found'
        }
        
        if($results.Count -gt 1)
        {
            throw 'More than one result was found'
        }
        
        return $results[0]
    }
}
