$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$zrDotNet = if ($env:DOTNET_BIN) { $env:DOTNET_BIN } else { (Get-Command dotnet -ErrorAction Stop).Source }
& $zrDotNet run --project (Join-Path $PSScriptRoot 'ZrConfig.Maintenance.csproj') --no-launch-profile -p:NuGetAudit=false -p:UseSharedCompilation=false -- @args
exit $LASTEXITCODE
