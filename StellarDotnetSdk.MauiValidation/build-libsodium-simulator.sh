#!/usr/bin/env bash
# Builds libsodium for the iOS Simulator, so the validation app can run there.
#
# The libsodium NuGet package (1.0.22) ships no simulator build: an iossimulator-arm64 build links the device library
# and fails ("building for 'iOS-simulator', but linking in object file ... built for 'iOS'"), and iossimulator-x64
# finds no library at all (docs/maui-compatibility.md §1). This script builds the same libsodium version from the
# signed release tarball, for arm64 and x86_64 simulators, into native/iossimulator/libsodium.a. When that file
# exists, the project links it instead of the package's library for iossimulator-* builds. Device builds are
# unchanged.
#
# Usage: ./build-libsodium-simulator.sh
#
# Needs macOS with Xcode and minisign (brew install minisign). Takes a few minutes.
#
# Environment (defaults in brackets):
#   LIBSODIUM_VERSION   must match the libsodium version NSec resolves [1.0.22]
#   IOS_SIMULATOR_MIN   minimum simulator iOS version [15.0, the project's iOS floor]
set -euo pipefail

version="${LIBSODIUM_VERSION:-1.0.22}"
ios_min="${IOS_SIMULATOR_MIN:-15.0}"
# libsodium's release signing key: https://doc.libsodium.org/installation#integrity-checking
public_key="RWQf6LRCGA9i53mlYecO4IzT51TGPpvWucNSCh1CBM0QTaLn73Y7GFO3"

here="$(cd "$(dirname "$0")" && pwd)"
out_dir="$here/native/iossimulator"
work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT

command -v minisign >/dev/null || { echo "minisign not found (brew install minisign)" >&2; exit 1; }
sdk="$(xcrun --sdk iphonesimulator --show-sdk-path)"

cd "$work"
tarball="libsodium-$version.tar.gz"
curl -fsSLO "https://download.libsodium.org/libsodium/releases/$tarball"
curl -fsSLO "https://download.libsodium.org/libsodium/releases/$tarball.minisig"
minisign -Vm "$tarball" -P "$public_key"
tar xzf "$tarball"
cd "libsodium-$version"

# Same configure flags as libsodium's dist-build/apple-xcframework.sh, simulator part only.
build() {
    local arch="$1" host="$2"
    shift 2
    export CFLAGS="-O3 -arch $arch -isysroot $sdk -mios-simulator-version-min=$ios_min"
    export LDFLAGS="-arch $arch -isysroot $sdk -mios-simulator-version-min=$ios_min"
    make distclean >/dev/null 2>&1 || true
    ./configure "$@" --host="$host" --prefix="$work/$arch" --disable-shared --enable-static >/dev/null
    make -j"$(sysctl -n hw.ncpu)" install >/dev/null
}
build arm64 aarch64-apple-darwin23
build x86_64 x86_64-apple-darwin23 cross_compiling=yes

mkdir -p "$out_dir"
lipo -create "$work/arm64/lib/libsodium.a" "$work/x86_64/lib/libsodium.a" -output "$out_dir/libsodium.a"
lipo -info "$out_dir/libsodium.a"
echo "Wrote $out_dir/libsodium.a"
