#!/usr/bin/env bash
set -euo pipefail
if [[ -n "${DOTNET_BIN:-}" ]]; then
    exec "$DOTNET_BIN" "$@"
elif command -v dotnet >/dev/null 2>&1; then
    exec dotnet "$@"
elif [[ -n "${DOTNET_ROOT:-}" && -x "$DOTNET_ROOT/dotnet" ]]; then
    exec "$DOTNET_ROOT/dotnet" "$@"
elif [[ -x "$HOME/.dotnet/dotnet" ]]; then
    exec "$HOME/.dotnet/dotnet" "$@"
else
    echo '请安装 .NET SDK 10，或将 DOTNET_BIN 设置为 dotnet 可执行文件路径。' >&2
    exit 1
fi
