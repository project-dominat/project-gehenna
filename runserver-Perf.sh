#!/usr/bin/env bash
export DOTNET_TieredPGO=1
export DOTNET_TC_QuickJitForLoops=1
export DOTNET_ReadyToRun=0

case "$(uname -s)" in
  Linux|Darwin)
    export ROBUST_NUMERICS_AVX=1
    ;;
esac

dotnet run -c Release --project Content.Server -- "$@"
read -r -p "Press enter to continue"
