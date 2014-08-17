@echo off
powershell -NoProfile -ExecutionPolicy Bypass -Command "& '%~dp0\tools\psake\psake.ps1' '%~dp0\build.ps1' -NoLogo; if ($psake.build_success -eq $false) { exit 1 } else { exit 0 }"
exit /B %errorlevel%