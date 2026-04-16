@echo off
set DOTNET_TieredPGO=1
set DOTNET_TC_QuickJitForLoops=1
set DOTNET_ReadyToRun=0
dotnet run -c Release --project Content.Server -- %*
pause
