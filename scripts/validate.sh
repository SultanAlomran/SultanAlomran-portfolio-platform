#!/usr/bin/env bash
set -euo pipefail
dotnet restore Portfolio.sln
dotnet build Portfolio.sln --configuration Release --no-restore
dotnet test Portfolio.sln --configuration Release --no-build
for application in Portfolio.Web Portfolio.Admin; do
  if [[ ! -f "src/$application/package-lock.json" ]]; then
    printf 'Missing src/%s/package-lock.json; frontend validation cannot run yet.\n' "$application" >&2
    exit 1
  fi
  npm --prefix "src/$application" ci
  npm --prefix "src/$application" run lint
  npm --prefix "src/$application" run build
done
git diff --check
