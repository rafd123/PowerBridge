param
(
	$TextFilePath
)

Write-Warning 'Warnings and errors can be triggered in PS1 files, too. Double-click me!'
Write-Warning "$TextFilePath(2,47) : Standard custom-build formatted messages are honored in Write-Warning messages. Double-click me!"
Write-Error "$TextFilePath(3,47) : They're honored for Write-Error, too. Double-click me!"
Write-Error '...and of course Write-Error fails the build. Double-click me!'