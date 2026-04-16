@echo off
pushd "%~dp0"
dotnet run --project Content.Server --configuration Release -- --config-file Resources\ConfigPresets\_Gehenna\production.toml
set EXITCODE=%ERRORLEVEL%
popd
pause
exit /b %EXITCODE%
