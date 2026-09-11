#!/usr/bin/env sh
cd "$(dirname "$0")" || exit 1
dotnet tool restore
exec dotnet run --project build/EasyBuild.fsproj -- "$@"
