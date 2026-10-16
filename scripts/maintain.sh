#!/usr/bin/env bash
set -euo pipefail
zr_script_dir="$(cd "$(dirname "$0")" && pwd)"
exec bash "$zr_script_dir/dotnet.sh" run --project "$zr_script_dir/ZrConfig.Maintenance.csproj" --no-launch-profile -p:NuGetAudit=false -p:UseSharedCompilation=false -- "$@"
