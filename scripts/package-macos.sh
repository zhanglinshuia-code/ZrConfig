#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

if [[ "$(uname -s)" != Darwin ]]; then
    echo '请在 macOS 上运行此脚本，以完成本机应用签名。' >&2
    exit 1
fi
case "${1:-$(uname -m)}" in
    arm64|osx-arm64) zr_rid=osx-arm64 ;;
    x86_64|osx-x64) zr_rid=osx-x64 ;;
    *) echo '支持的架构：osx-arm64、osx-x64。' >&2; exit 1 ;;
esac

zr_output="$PWD/artifacts/macos/$zr_rid"
mkdir -p "$zr_output"
zr_stage=$(mktemp -d "$zr_output/.package-XXXXXX")
zr_previous="$zr_stage/previous-Contents"
cleanup() {
    # 更新中断时恢复旧内容；成功后临时备份随暂存目录一起清理。
    if [[ -d "$zr_previous" && ! -e "$zr_output/ZrConfig.app/Contents" ]]; then
        mv "$zr_previous" "$zr_output/ZrConfig.app/Contents"
    fi
    rm -rf "$zr_stage"
}
trap cleanup EXIT
zr_bundle="$zr_stage/ZrConfig.app"
mkdir -p "$zr_bundle/Contents/MacOS" "$zr_bundle/Contents/Resources" "$zr_bundle/Contents/Frameworks"

# 自带 .NET 运行时；保留 DLL，供运行中的 Roslyn 编译规则时读取模型元数据。
bash scripts/dotnet.sh publish src/ZrConfig.App/ZrConfig.App.csproj \
    -c Release -r "$zr_rid" --self-contained true \
    -p:UseAppHost=true -p:PublishSingleFile=false -p:PublishTrimmed=false \
    --disable-build-servers -m:1 -p:UseSharedCompilation=false -p:NuGetAudit=false -p:UsedAvaloniaProducts= \
    -o "$zr_bundle/Contents/MacOS"
bash scripts/maintain.sh check-package-licenses "$zr_bundle/Contents/MacOS"
cp src/ZrConfig.App/Info.plist "$zr_bundle/Contents/Info.plist"
printf 'APPL????' > "$zr_bundle/Contents/PkgInfo"
chmod +x "$zr_bundle/Contents/MacOS/ZrConfig.App"
plutil -lint "$zr_bundle/Contents/Info.plist"

# macOS 要求代码与资源分目录。相对链接保留 .NET 按可执行文件目录查找依赖的路径。
for zr_item in "$zr_bundle/Contents/MacOS/"*; do
    zr_name=$(basename "$zr_item")
    case "$zr_name" in
        *.dylib) zr_target=Frameworks ;;
        *)
            case "$(file -b "$zr_item")" in
                Mach-O*) continue ;;
                *) zr_target=Resources ;;
            esac
            ;;
    esac
    mv "$zr_item" "$zr_bundle/Contents/$zr_target/$zr_name"
    ln -s "../$zr_target/$zr_name" "$zr_item"
    # .NET 会将入口 DLL 的路径解析到 Resources，因此运行库也需要从该目录可见。
    if [[ "$zr_target" == Frameworks ]]; then
        ln -s "../Frameworks/$zr_name" "$zr_bundle/Contents/Resources/$zr_name"
    fi
done

# 为本机使用进行临时签名；先签原生库，再签外层应用包。
while IFS= read -r -d '' zr_library; do
    codesign --force --sign - "$zr_library"
done < <(find "$zr_bundle/Contents/Frameworks" "$zr_bundle/Contents/MacOS" -type f -print0)
codesign --force --sign - "$zr_bundle"
codesign --verify --deep --strict "$zr_bundle"

# 保留外层 .app 目录，避免 Dock/Finder 跟随被重命名的旧包；仅在新包验证成功后更新内容。
if [[ -e "$zr_output/ZrConfig.app" ]]; then
    # 每次重打包都保留可恢复的旧包，避免发布整理覆盖有效版本。
    ditto -c -k --sequesterRsrc --keepParent "$zr_output/ZrConfig.app" "$zr_output/ZrConfig-before-$(date -u +%Y%m%d-%H%M%S)-$$.zip"
    mv "$zr_output/ZrConfig.app/Contents" "$zr_previous"
    mv "$zr_bundle/Contents" "$zr_output/ZrConfig.app/Contents"
else
    mv "$zr_bundle" "$zr_output/ZrConfig.app"
fi
touch "$zr_output/ZrConfig.app"
printf '\n打包完成，可在 Finder 中双击：\n%s\n' "$zr_output/ZrConfig.app"
