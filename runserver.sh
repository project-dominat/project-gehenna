#!/usr/bin/env bash
cd "$(dirname "$0")" || exit 1
dotnet run --project Content.Server --configuration Release -- --config-file Resources/ConfigPresets/_Gehenna/production.toml
status=$?
read -r -p "Press enter to continue"
exit "$status"
