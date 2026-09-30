# .NET MAUI compatibility

Validation of the SDK in .NET MAUI apps (SCF Q3 2026 deliverable 2, "MAUI Validation"). Validated against `main` @ `f3375ed8` on 2026-09-30, and against the published NuGet package 15.1.0.

The multi-target build of `main` is **not published yet**. The newest release on nuget.org, 15.1.0, targets only `net8.0`, and it does not work on Android without the workaround below (§1). §5 lists what the SCF deliverable asked for and what was not done.

**Summary**

| Platform | Result |
|----------|--------|
| Android, x86_64 emulator (API 28), SDK built from `main` | **Works**, with one limitation: with the default HTTP handler, SSE streaming events arrive about 50 s late and can be lost if the connection drops. A small consumer-side workaround fixes both on Android (§3.3). §3.3 also describes a loss case that applies to every platform. Crypto, Horizon queries, transaction submit and Soroban simulate pass in a trimmed Release build. |
| Android, x86_64 emulator (API 28), NuGet package 15.1.0 | **Fails** at the first crypto call with `PlatformNotSupportedException`. Works with an added `NSec.Cryptography` 26.4.0 reference (§1). |
| Android, arm64 physical device | **Not exercised.** The arm64 libsodium binary ships in the APK but was not run (§4). |
| iOS device (AOT-only) | **Not exercised.** No macOS host was available. The desk check says it should work (§1); it is unverified (§4). |
| iOS Simulator | **Not exercised; expected not to work**: libsodium has no simulator build (desk check, §1). |
| Mac Catalyst | Not exercised; expected to work (§1). |

Workarounds consumers need today:

- Android with the published package (15.1.0): add `<PackageReference Include="NSec.Cryptography" Version="26.4.0" />` to the app (§1). The same reference fixes MAUI 9 apps, which get the same `net8.0` assembly even from `main`.
- `TrimMode=full`: add the workaround in §3.2. On Android the MAUI default (`TrimMode=partial`) needs nothing, but a plain .NET for Android app created with `dotnet new android` uses `TrimMode=full` by default.
- SSE on Android: give the stream `SocketsHttpHandler` (§3.3). Whatever the platform, do not rely on a stream alone for payments: resume from a saved paging token and reconcile with a query (§3.3).

## 1. Desk check: does the Ed25519 backend load on iOS and Android?

All signing and verification goes through the internal `Ed25519` facade (`StellarDotnetSdk/Crypto/Ed25519.cs`, `Ed25519Signer.cs`). Both backends are thin managed wrappers over native libsodium, so the question is whether libsodium is present and bindable for each mobile RID. The findings below come from the package contents in the local NuGet cache (`~/.nuget/packages`), not from package descriptions.

### Which backend a MAUI app gets

NuGet picks the SDK assembly nearest to the app's TFM, then resolves that assembly's dependency group:

| SDK | App TFM | SDK assembly | Ed25519 package | libsodium package |
|-----|---------|--------------|-----------------|-------------------|
| `main` (multi-target, unreleased) | `net10.0-android` / `net10.0-ios` (MAUI 10) | `net10.0` | NSec.Cryptography 26.4.0 | libsodium 1.0.22 (NSec range `[1.0.22, 1.0.23)`) |
| `main` (multi-target, unreleased) | `net9.0-android` / `net9.0-ios` (MAUI 9) | `net8.0` | NSec.Cryptography 25.4.0 | libsodium 1.0.20.1 (NSec range `[1.0.20.1, 1.0.21)`) |
| NuGet 15.1.0 (latest release) | any MAUI version | `net8.0` (the only one it ships) | NSec.Cryptography 25.4.0 | libsodium 1.0.20.1 |

A MAUI app never resolves the `netstandard2.1` assembly, so Sodium.Core is not on the MAUI path. It is listed below for completeness only. The `16.0.0-beta` git tag is `net8.0`-only as well and is not on nuget.org.

For the platform-specific TFMs, NuGet also picks the package's platform-specific managed assembly. NSec 26.4.0 ships `lib/net9.0-ios18.0`, `net9.0-maccatalyst18.0` and `net9.0-tvos18.0`; NSec 25.4.0 ships the same set under `net8.0-*`. Sodium.Core 1.4.1 ships `net9.0-ios18.0` and siblings as well.

### Native binding per assembly

The native library name each assembly binds to (from its metadata string heap):

| Assembly | Binds to | Interop style |
|----------|----------|---------------|
| NSec 26.4.0 / 25.4.0, `lib/net9.0` / `lib/net8.0` (used on Android) | `libsodium` (dynamic, `libsodium.so`) | `[LibraryImport]` (source-generated marshalling) |
| NSec 26.4.0 / 25.4.0, `lib/*-ios18.0`, `*-maccatalyst18.0`, `*-tvos18.0` | `__Internal` (statically linked into the app binary) | `[LibraryImport]` |
| Sodium.Core 1.4.1, `lib/net9.0-ios18.0` and siblings | `__Internal` | `[DllImport]` |
| Sodium.Core 1.4.1, `lib/netstandard2.0`, `lib/netstandard2.1`, `lib/net9.0` | `libsodium` | `[DllImport]` |

`[LibraryImport]` generates its marshalling stubs at compile time, so it needs no runtime IL stub generation. On iOS, where there is no JIT, the AOT compiler also precompiles `[DllImport]` wrappers, so both styles are expected to work there; neither was run on iOS.

### Native assets per libsodium version

| RID | libsodium 1.0.22 (MAUI 10 path) | libsodium 1.0.20.1 (MAUI 9 path) |
|-----|:-------------------------------:|:--------------------------------:|
| `android-arm64` | yes, `libsodium.so` | **no** |
| `android-x64` (emulator) | yes | **no** |
| `android-arm`, `android-x86` | yes | **no** |
| `ios-arm64` (device) | yes, `libsodium.a` | yes, `libsodium.a` |
| `iossimulator-arm64`, `iossimulator-x64` | **no** | **no** |
| `maccatalyst-arm64`, `maccatalyst-x64` | yes | yes |

Details checked on the binaries themselves:

- **Android** (1.0.22): the `.so` files are built with NDK r27d for API 21. Every `LOAD` segment is aligned to `0x4000`, which meets Google Play's 16 KB page-size requirement.
- **iOS** (1.0.22): `ios-arm64/native/libsodium.a` is a universal archive with an `arm64` slice (`LC_VERSION_MIN_IPHONEOS` 9.0) and an `arm64e` slice (`LC_BUILD_VERSION` platform 2 = iOS, minimum 14.0). It has **no simulator slice**: nothing is tagged platform 7 (iOSSimulator), and the package has no `iossimulator-*` RID folder.

### Conclusion

The premise that neither backend ships native libsodium for the mobile RIDs does not hold for the dependency set on `main`. It still holds for the published 15.1.0 package and for the `net8.0` path (item 4):

1. **Android, MAUI 10, SDK built from `main`: expected to work unchanged.** NSec 26.4.0 with libsodium 1.0.22 ships `android-arm64` and `android-x64` binaries, and they are 16 KB page aligned. This is verified on an emulator in section 3.
2. **iOS device, MAUI 10: expected to work unchanged.** NSec ships iOS-specific assemblies that bind to `__Internal`, and libsodium ships a device static library. This relies on the .NET for iOS build statically linking `runtimes/ios-arm64/native/libsodium.a` from the package. It needs a macOS host and a physical device to confirm (see section 4).
3. **iOS Simulator: expected not to work (not run).** There is no simulator slice. NuGet resolves `iossimulator-arm64` to the device library through RID fallback (`runtimes/ios-arm64/native/libsodium.a`), which the linker should reject because it is built for iOS, not the simulator. `iossimulator-x64` resolves no native library at all, so the `__Internal` symbols would be missing at run time. This affects development and CI only, not shipped apps. Mac Catalyst is covered.
4. **Android with the `net8.0` assembly does not work: MAUI 9 with `main`, and every MAUI version with the published 15.1.0.** That path pins libsodium 1.0.20.1, which has no Android binaries. NuGet falls back from the `android-*` RIDs to `linux-*` and packs the glibc builds of `libsodium.so`, which Android cannot load. The first crypto call throws `PlatformNotSupportedException` ("Could not initialize platform-specific components") with an inner `DllNotFoundException: libsodium`. Verified on the emulator with 15.1.0 (§3.1). The `net9.0-android` workload is already out of support in the .NET 10 SDK (warning NETSDK1202).
5. **Sodium.Core (`netstandard2.1`) is irrelevant to MAUI.** It would also work on Android and iOS devices through the same libsodium 1.0.22 package.

### Decision

**Keep NSec as the only backend on the MAUI path. Do not bundle libsodium ourselves, and do not add a managed Ed25519 fallback for this deliverable.** Document the two gaps with their workarounds:

- **iOS Simulator:** run on a physical device or on Mac Catalyst. Alternatively, an app can supply its own simulator build of libsodium as a `NativeReference`; that is unsupported by the SDK.
- **Android with the `net8.0` assembly (15.1.0 on any MAUI version, or MAUI 9):** add `<PackageReference Include="NSec.Cryptography" Version="26.4.0" />` to the app. NuGet then resolves NSec 26.4.0 and libsodium 1.0.22, which ships the Android binaries; the SDK's `NSec.Cryptography >= 25.4.0` dependency allows it, so there is no NU1608 warning. Verified on the emulator with 15.1.0 (§3.1). NSec 26.4.0 needs `net9.0` or later, so MAUI 8 has no workaround. Referencing libsodium 1.0.22 alone does **not** work: NSec 25.4.0 checks the library version at startup and throws `InvalidOperationException` ("Expected libsodium 1.0.20 but found 1.0.22").

Options considered:

| Option | For | Against |
|--------|-----|---------|
| **Keep NSec, document gaps** (chosen) | No code change. Keeps libsodium's audited constant-time implementation and the secure-memory (mlock) signing key handling added for `KeyPair`. Android and iOS device are already covered by upstream packages. | Simulator unsupported. Android apps on the `net8.0` assembly need the NSec reference until a multi-target release is published. |
| Bundle libsodium for missing RIDs in the SDK package | Would close the simulator and MAUI 9 gaps. | Makes the SDK responsible for building, signing and updating native binaries for each libsodium release. The bundled binaries would collide with NSec's own `libsodium` dependency when both provide the same RID. The work is out of proportion to two gaps that only affect development or an expiring runtime. |
| Managed Ed25519 (BouncyCastle) as a third backend | No native dependency. Works on simulator, WASM, and any AOT target. | Large dependency (only partly trimmable). No secure-memory key handling. Verification edge cases (non-canonical encodings, small-order points) differ subtly from libsodium, so equivalence tests would have to cover them. A third backend to test on every TFM. |
| Small in-repo managed Ed25519 | No dependency. | Self-maintained cryptography: constant-time properties and edge cases become this project's audit burden. Not justified by the gaps above. |

Reopen this decision if a supported target appears that no upstream libsodium package covers, for example WASM or a new mobile RID.

## 2. Environment setup

### Pinned versions

| Component | Version used here (Android run) | Notes |
|-----------|---------------------------------|-------|
| .NET SDK | 10.0.401 (Microsoft build, installed user-local with `dotnet-install.sh --version 10.0.401`) | The repo's `global.json` (10.0.100, `latestFeature`) accepts it. Distribution-built SDKs, such as Fedora's in `/usr/lib64/dotnet`, cannot take the MAUI workloads without root, and the workload packs are built against Microsoft's SDK. |
| Workload set | 10.0.401.1 | Contains `Microsoft.NET.Sdk.Maui` 10.0.110, `Microsoft.NET.Sdk.Android` 36.1.69, `Microsoft.NET.Sdk.iOS` / `MacCatalyst` 27.0.10722 |
| Microsoft.Maui.Controls | 10.0.110 (`$(MauiVersion)` from the workload) | |
| Android runtime | Mono (the .NET 10 default for Android) | Release builds are AOT-compiled (the APK contains `libaot-*.so`), with the JIT still available: the app logs `dynamicCodeCompiled=True`. CoreCLR on Android is opt-in and was not tested. |
| JDK | Microsoft OpenJDK 21.0.12.1 | .NET for Android supports JDK 17 and 21; the system JDK 25 was not used. |
| Android SDK | platform `android-36`, build-tools 36.0.0, platform-tools 37.0.0, cmdline-tools 20.0 | Only `android-36` (the target API of Android pack 36.1.69) is required. |
| Android Emulator | 36.6.11, started with `-gpu swangle_indirect` | With the default SwiftShader renderer (`-gpu swiftshader_indirect`, `guest`) it segfaults in `lib64/gles_swiftshader/libGLESv2.so` about 20–60 s after start on this host. |
| Emulator image | `system-images;android-28;google_apis;x86_64` rev 11 | The minimum the app supports is API 21. A current image (API 35/36) was not downloaded for this run. |
| Xcode (iOS / Mac Catalyst) | **Xcode 27.0 or later** (the minimum in the iOS workload 27.0.10722's `WorkloadDependencies.json`); Xcode's own macOS requirement applies | Not available here; see §4. |
| Host | Fedora 44, kernel 7.2.5, x86_64, KVM | |

### Reproducing the Android run

```bash
# 1. .NET SDK and workload (user-local; nothing system-wide)
curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
bash /tmp/dotnet-install.sh --version 10.0.401 --install-dir ~/.dotnet
export DOTNET_ROOT=~/.dotnet PATH=~/.dotnet:$PATH
dotnet workload install maui-android --version 10.0.401.1

# 2. JDK 21 (Microsoft OpenJDK 21.0.12.1)
mkdir -p ~/.local/lib
curl -sSL https://aka.ms/download-jdk/microsoft-jdk-21.0.12.1-linux-x64.tar.gz | tar -xz -C ~/.local/lib
export JAVA_HOME=~/.local/lib/jdk-21.0.12.1+1

# 3. Android SDK. First install the Android command-line tools (20.0 used here) from
#    https://developer.android.com/studio#command-line-tools-only into $ANDROID_SDK_ROOT/cmdline-tools/latest.
export ANDROID_SDK_ROOT=~/Android/Sdk
export PATH=$ANDROID_SDK_ROOT/cmdline-tools/latest/bin:$ANDROID_SDK_ROOT/emulator:$PATH
yes | sdkmanager --licenses
sdkmanager "platforms;android-36" "build-tools;36.0.0" "platform-tools" "emulator" "system-images;android-28;google_apis;x86_64"
echo no | avdmanager create avd -n stellar_api28 -k "system-images;android-28;google_apis;x86_64"   # "no" = no custom hardware profile
emulator -avd stellar_api28 -no-window -no-audio -no-snapshot -gpu swangle_indirect &

# 4. Build (Release, trimmed), install, run and collect results (the script waits for the emulator to boot)
cd StellarDotnetSdk.MauiValidation
./run-android.sh partial                                          # MAUI default trimming
./run-android.sh full                                             # full trimming + SDK workaround (§3.2)
./run-android.sh full -p:StellarDotnetSdkTrimWorkaround=false     # full trimming without the workaround (fails, §3.2)

# 5. Desktop baseline: the same checks as a console app (§3)
dotnet run --project Desktop
```

`sdkmanager` installs the newest `platform-tools` and `emulator`; the versions used here are in the table above. On a physical device, enable USB debugging and pass its serial: `ADB_SERIAL=<serial> ./run-android.sh partial` (`adb devices` lists it). On macOS the script also works: it builds only the Android target, so the iOS workloads are not needed.

`run-android.sh` cleans the app's Release output (switching trim modes over an incremental build reuses stale linked assemblies, which then crash inside MAUI at startup). It then publishes the APK, waits for the device, installs the APK, enlarges the device's logcat buffer to 8 MB (so that a long run cannot push out its first results; on some devices the setting stays until reboot), launches it, and prints every logcat line that carries the `STELLAR-MAUI-VALIDATION` prefix (the logcat tag is `DOTNET`). It stops with an error if no booted device appears within `BOOT_TIMEOUT_SECONDS` (default 300), if the app crashes (Java exception or native crash), or if it does not finish within `TIMEOUT_SECONDS` (default 1200). `./run-android.sh --help` prints the usage.

It exits 0 when every check passed, except the checks in `KNOWN_FAILURES` that failed for their known reason, which it reports as `KNOWN FAIL`. Only a check whose known reason is defined in the script can be listed there; any other name stops the script with an error before the build, because excusing a check by name alone would also hide unrelated failures of it. The default is `horizon.submit-and-sse-stream` failing with "SSE event arrived … after submit": the SDK's SSE stream with the default Android handler is late because of the SDK limitation in §3.3. Any other failure of that check (a submit error, a timeout, a second inconclusive measurement) still fails the run. The same stream with the workaround is its own check and must pass. Set `KNOWN_FAILURES=""` to make that failure fatal, for example after the SDK is fixed; the script also notes when a known failure starts passing. The app's own `RESULT` line counts every check, so it reads `RESULT FAIL 5/6` on Android.

### Reproducing on macOS (iOS, Mac Catalyst): not done

On a Mac with Xcode 27.0: `dotnet workload install maui-ios maui-maccatalyst --version 10.0.401.1`. Then build `-f net10.0-ios -r ios-arm64 -c Release` for a device (needs a provisioning profile) or `-f net10.0-maccatalyst`. The project only adds the Apple TFMs when built on macOS, and they have never been built. The app writes the same `STELLAR-MAUI-VALIDATION` lines to standard output, which is expected to appear in the device console (Console.app, or `xcrun devicectl device process launch --console`); this is unverified.

## 3. Validation results

The validation app, `StellarDotnetSdk.MauiValidation/`, is a single-page MAUI app that runs these checks on startup and again on a button press:

| Check | What it proves |
|-------|----------------|
| `crypto.rfc8032` | The native Ed25519 backend loads, and derives the RFC 8032 §7.1 test 1 public key and signature from its seed. |
| `crypto.random-keypair` | `KeyPair.Random`, sign/verify, a tampered signature is rejected, StrKey seed round trip. |
| `horizon.friendbot-and-account` | Friendbot funding and a Horizon account query (JSON deserialization of SDK models): the account id, a positive sequence number and a native balance come back. |
| `horizon.submit-and-sse-stream` | Starts an SSE payments stream from the UI thread (asserted in the MAUI app), builds, signs and submits a `CreateAccount` transaction, and waits for the stream to deliver it. Passes only if the submit finished within 20 s of the stream opening and the event arrived within 20 s of the submit, so an event held back until Horizon closes the stream (about 55 s after it opens) cannot pass. A submit slower than that makes the measurement inconclusive, not failed: the check is then run once more, and the second run's result counts, so a second inconclusive measurement fails it. For comparison it also reads the same stream with a bare `HttpClient` three ways; these never affect a verdict. |
| `horizon.sse-stream-sockets-handler` | The same stream, opened at the same time with `SocketsHttpHandler` (the §3.3 workaround), judged by the same two rules. It gets 5 s to open before the submit. Once the transaction is submitted, each stream is judged only on its own delivery. |
| `soroban.simulate` | Stellar RPC `simulateTransaction` of the native XLM Stellar Asset Contract's `balance(address)` for the funded account. The decoded `i128` must equal the account's native balance from Horizon, in stroops. Because Horizon and the RPC node can be a ledger apart, Horizon's account is re-read up to 4 more times, 2 s apart, until it shows the payment the previous check submitted, and the simulation is repeated the same way until the RPC node reports that it has reached the ledger that last changed the account; the check fails if Horizon is still behind, or if the RPC node is still behind or does not report its ledger. |

It is referenced by project, not by package, and is deliberately **not** in `stellar-dotnet-sdk.sln`, so building the solution never needs the MAUI workloads. With a project reference, the app resolves the SDK's `net10.0` assembly and its dependency graph (NSec 26.4.0, libsodium 1.0.22), the same as a `net10.0-android` app would from a package built from `main`. That is **not** what the published package gives it (§1). To run the checks against a published package instead, pass `-p:StellarDotnetSdkPackageVersion=15.1.0`, and add `-p:NSecPackageVersion=26.4.0` for the consumer-side workaround:

```bash
./run-android.sh partial -p:StellarDotnetSdkPackageVersion=15.1.0                               # fails at crypto
./run-android.sh partial -p:StellarDotnetSdkPackageVersion=15.1.0 -p:NSecPackageVersion=26.4.0  # workaround
```

The same checks also run as a desktop console app, `StellarDotnetSdk.MauiValidation/Desktop` (`dotnet run --project Desktop` from `StellarDotnetSdk.MauiValidation/`; exit code 0 only if every check passed), as a baseline. On linux-x64: 6/6 pass, and both SSE streams deliver about 2 s after the submit.

### 3.1 Android: results (x86_64 emulator, API 28, Mono runtime, Release)

From the final runs of `run-android.sh`. SSE times are after the submit returned.

| Check | `TrimMode=partial` (MAUI default) | `TrimMode=full` + workaround | `TrimMode=full`, no workaround |
|-------|:---:|:---:|:---:|
| `crypto.rfc8032` | pass | pass | pass |
| `crypto.random-keypair` | pass | pass | pass |
| `horizon.friendbot-and-account` | pass | pass | **fail**: `TypeInitializationException` in `JsonOptions` |
| `horizon.submit-and-sse-stream` | submit pass; **SSE late** (51 s), known failure | submit pass; **SSE late** (47 s), known failure | **fail**: no funded account |
| `horizon.sse-stream-sockets-handler` | pass (2.7 s) | pass (3.5 s) | **fail**: no measurement |
| `soroban.simulate` | pass | pass | **fail**: no funded account |
| `run-android.sh` exit code | 0 | 0 | 1 |
| Unique IL2xxx warnings (see §3.2 for how they are counted) | 0 | 124 | 99 |
| APK size (arm64-v8a + x86_64) | 32.9 MB | 29.2 MB | 28.0 MB |

The APK contains `lib/arm64-v8a/libsodium.so` and `lib/x86_64/libsodium.so` from the libsodium 1.0.22 package. The x86_64 one is the binary the emulator runs.

With the published package instead of `main` (`TrimMode=partial`, `-p:StellarDotnetSdkPackageVersion=15.1.0`, §3):

| Check | 15.1.0 | 15.1.0 + `NSec.Cryptography` 26.4.0 |
|-------|:---:|:---:|
| `crypto.rfc8032` | **fail**: `PlatformNotSupportedException` | pass |
| `crypto.random-keypair` | **fail**: same | pass |
| `horizon.friendbot-and-account` | **fail**: same (from `KeyPair.Random`) | pass |
| `horizon.submit-and-sse-stream` | **fail**: no funded account | submit pass; **SSE late** (47 s), known failure |
| `horizon.sse-stream-sockets-handler` | **fail**: no measurement | pass (3.3 s) |
| `soroban.simulate` | **fail**: no funded account | pass |
| `run-android.sh` exit code | 1 | 0 |

With 15.1.0 the APK contains the `linux-arm64` and `linux-x64` glibc builds of libsodium 1.0.20.1 under `lib/arm64-v8a` and `lib/x86_64` (NuGet's RID fallback), which Android cannot load. The app's `INFO assemblies` line shows which SDK and NSec builds ran (NSec 25.4.0 vs 26.4.0 here).

### 3.2 Trimming

**On Android, the MAUI default works as is.** Release builds of MAUI apps trim with `TrimMode=partial`, which only trims assemblies marked `IsTrimmable`. The SDK is not marked, so it is kept whole and no trim warnings are raised for it. The Android SDK also sets `JsonSerializerIsReflectionEnabledByDefault=true` in partial mode. A plain .NET for Android app is different: the `dotnet new android` template sets `TrimMode=full`, so it needs the workaround below from the start.

**`TrimMode=full` breaks the SDK at the first Horizon or RPC call** unless the app opts out for the SDK. Two separate problems, both confirmed on the emulator:

1. With `TrimMode=full`, the Android SDK does not set `JsonSerializerIsReflectionEnabledByDefault`, and the .NET trimming targets default it to false. `JsonOptions.CreateDefaultOptions` calls `MakeReadOnly(populateMissingResolver: true)`, which then throws `InvalidOperationException: JsonSerializerIsReflectionDisabled`, surfacing as a `TypeInitializationException` for `StellarDotnetSdk.Converters.JsonOptions`.
2. With reflection re-enabled but nothing else, the trimmer has removed the constructors of the SDK's response models: `NotSupportedException: DeserializeNoConstructor ... StellarDotnetSdk.Responses.FriendBotResponse`.

The workaround is a property, an item and a descriptor file in the app project. Add them unconditionally rather than under a condition on `$(TrimMode)`: a platform SDK can compute `TrimMode` after the project file is evaluated (`AndroidLinkMode`, `PublishAot`, `MtouchLink`), and a property condition then sees a different value than an item condition. (`StellarDotnetSdk.MauiValidation.csproj` can use conditions because it sets `TrimMode` itself.)

```xml
<PropertyGroup>
    <JsonSerializerIsReflectionEnabledByDefault>true</JsonSerializerIsReflectionEnabledByDefault>
</PropertyGroup>
<ItemGroup>
    <TrimmerRootDescriptor Include="ILLink.Descriptors.xml" />
</ItemGroup>
```

where `ILLink.Descriptors.xml` roots the SDK assembly:

```xml
<linker>
    <assembly fullname="StellarDotnetSdk" preserve="all" />
</linker>
```

With it, every check passes under full trimming except the known SSE failure (§3.3).

**iOS and Mac Catalyst (read from the iOS SDK 27.0.10722 targets, not run):** the iOS SDK sets `JsonSerializerIsReflectionEnabledByDefault=true` in every trim mode, and its default link mode for devices (`SdkOnly`) maps to `TrimMode=partial`. Full trimming comes from `MtouchLink=Full` or from `PublishAot=true` (NativeAOT); an app that uses either still needs the descriptor. Reflection-based System.Text.Json under iOS NativeAOT is untested.

Trim warnings with the SDK built from `main` at `83303a27`. MSBuild prints each warning several times, so they are counted once per source location and message; counting by message alone gives a few fewer (118 here), because one message can occur at several places in a file. The counts come from the `dotnet publish` output of `run-android.sh full`:

| Source | Code | Count (full, workaround) | Cause |
|--------|------|:---:|-------|
| SDK JSON converters (`EffectResponseJsonConverter`, `OperationResponseJsonConverter`, `PredicateJsonConverter`, `NonNullElementArrayJsonConverter`) | IL2026 | 92 | `JsonSerializer.(De)Serialize` with reflection-based metadata |
| SDK request/response plumbing (`ResponseHandler`, `Server`, `StellarRpcServer`, `RequestBuilderStreamable`, `JsonOptions`) | IL2026 | 5 | same |
| SDK SEP services (SEP-6, 9, 10, 24, 45) | IL2026 | 17 | same |
| SDK `UriTemplate` | IL2075 | 1 | `GetType().GetProperties()` on parameter objects |
| Nett (TOML, used by SEP-1 / federation) | IL2067, IL2070 | 7 | reflection-based TOML mapping |
| Common.Logging (dependency of LaunchDarkly.EventSource 3.3.2) | IL2072 | 2 | `Activator.CreateInstance` on a configured type |

Without the descriptor there are 99, because unreachable SDK code (SEP services, TOML) is trimmed away before it is analyzed. The SDK and Nett warnings are covered by the descriptor. The Common.Logging warning is on the path that instantiates a logging adapter named in configuration settings; neither the SDK nor this app configures one.

The durable fix is SDK-side: a source-generated `JsonSerializerContext` for the SDK models, and marking the SDK `IsTrimmable` once it is warning-free. That is a larger change and is not part of this deliverable.

### 3.3 SSE streaming on Android: the default HTTP handler holds events back

With the SDK's default setup, `Stream(...)` on Android hands an event to the listener only when more data arrives on the connection after it: the next event, or Horizon closing the connection. Horizon closes an SSE connection after 10 events or about 55 s, whichever comes first (`event: close`, `data: "byebye"`). On a quiet stream, such as the payments of one account, the event is therefore about 50 s late. Measured on the emulator in every configuration and run: the payment appeared 46–52 s after the submit.

The check reads the same payments URL five ways in one run. The check prints stopwatch times; the table subtracts its `submit done` time, so these are times after the submit returned, from one run (`TrimMode=partial`):

| Reader | HTTP handler | Android emulator | Linux desktop, same code |
|--------|--------------|:---:|:---:|
| SDK `Stream(...)` (LaunchDarkly.EventSource 3.3.2) | platform default | 51.0 s | 1.8 s |
| SDK `Stream(...)` | `SocketsHttpHandler` | 2.7 s | 1.8 s |
| `HttpClient` + `Stream.ReadAsync` | platform default | 1.6 s | 1.8 s |
| `HttpClient` + `StreamReader.ReadLineAsync` | platform default | `id:` line 1.6 s, `data:` line 51.6 s | 1.8 s |
| `HttpClient` + `StreamReader.ReadLineAsync` | `SocketsHttpHandler` | 2.5 s | 1.6 s |

The delay needs both `StreamReader` and the platform default handler (`AndroidMessageHandler` in a MAUI app). Raw `ReadAsync` over that handler gets the whole event at once, and so does `StreamReader` over `SocketsHttpHandler`. `StreamReader` over the default handler returns the event's short `id:` line promptly, then holds the long `data:` line (about 1.5 KB for an operation) until more bytes arrive. LaunchDarkly.EventSource 3.3.2 reads with `StreamReader.ReadLineAsync` (`EventSourceStreamReader`) through `new HttpClient()`, i.e. the platform default handler, unless it is given a handler. Horizon sends no `Content-Encoding`. The cause below `StreamReader` was not isolated further.

#### Workaround: `SocketsHttpHandler`

The request builders accept a custom event source, so an app can give a stream `SocketsHttpHandler` without any SDK change:

```csharp
// One handler for the app's streams: LaunchDarkly.EventSource does not dispose a handler it is given.
private static readonly SocketsHttpHandler SseHandler = new();

var payments = server.Payments.ForAccount(accountId).Cursor(savedPagingToken);
payments.EventSource = new SseEventSource(payments.BuildUri(),
    config => config.MessageHandler(SseHandler));
var stream = payments.Stream((_, payment) => HandlePayment(payment));
var running = stream.Connect(); // completes after stream.Shutdown(), possibly after a reconnect wait
```

`SocketsHttpHandler` is .NET's managed HTTP stack, not Android's, so settings that only the Android handler applies do not carry over. It was validated here only against Horizon testnet on the emulator.

#### Events can be lost

The SDK does not reconnect from its own cursor. `Stream(...)` updates the builder's cursor and `EventSource.Url` on every event (`RequestBuilderStreamable`), but the LaunchDarkly connection never reads that URL again. A reconnect resumes only through the `Last-Event-ID` header, which LaunchDarkly takes from the last `id:` line it parsed and which Horizon honours. That has two consequences:

- **Android, default handler: a dropped connection loses the event being held.** LaunchDarkly records the id as soon as it parses the `id:` line (`EventSource.ProcessField`), and on Android that line arrives about 50 s before the event's data (table above). If the connection drops in between, for example on a network change, the reconnect asks for the events after the held one, and the held one is never delivered. Reproduced on the emulator by cutting the first connection 8 s after the submit: the payment was not delivered within 150 s, although Horizon lists it. With `SocketsHttpHandler` the event had already arrived (after 1.3 s) when the connection was cut, and without a cut it arrived after 51 s.
- **All platforms: a stream opened with `Cursor("now")` that has not received an event yet** reconnects with `cursor=now` and no `Last-Event-ID`, so payments that land between Horizon closing the stream and the reconnect are skipped. On desktop, 4 of 4 payments submitted in reconnect gaps never reached a `now` stream, while a stream opened at an earlier cursor delivered all 4.

Reconnects also slow down over time. LaunchDarkly resets its backoff only after a connection that lasted at least a minute, and Horizon closes connections sooner, so the wait before each reconnect grows up to 30 s (measured on desktop). An event that lands during a wait arrives with the next connection, and on Android with the default handler that connection holds it back again, so the worst-case delay is above 55 s.

The connection-cut and reconnect-gap measurements come from one-off probes that are not part of the validation app.

Consequences and options:

- Consumers on Android today: give streams `SocketsHttpHandler` (above). On every platform, store the paging token of the last processed record, open streams with `Cursor(savedToken)` instead of `"now"` (use `"now"` only on the very first start, then save the token of each record you process), and reconcile with a query after a reconnect or when the app resumes: `server.Payments.ForAccount(id).Cursor(savedToken).Execute()` returns up to 10 records by default, so repeat it from the last record's token until a page comes back empty.
- SDK fix (follow-up): make the default `SseEventSource` avoid the stall, either by using `SocketsHttpHandler` on Android (no public API change) or by reading the response as bytes. For bytes, LaunchDarkly.EventSource 6.0.0 has a byte-level reader (`ProcessResponseFromUtf8StreamAsync`, enabled with `PreferDataAsUtf8Bytes`), but moving to it changes `SseEventSource`'s public constructor, which exposes LaunchDarkly 3.x's `ConfigurationBuilder`; a small in-SDK reader over `Stream.ReadAsync` is the alternative. Separately, the SDK should resume from its own cursor so that `Cursor("now")` streams do not skip events. Any of these must be re-validated with this app.

Starting an SDK stream (`Connect()`) from the UI thread works; the check asserts that it runs there. LaunchDarkly then reads the response on thread-pool threads, so this covers starting a stream on the UI thread, not reading on it. While the harness was being written, a bare `HttpClient` that read a response stream on the UI thread failed with a Java `RuntimeException` from `AndroidMessageHandler`; the committed app does not repeat that experiment. The SDK's HTTP calls started from the UI thread all passed.

## 4. Residual risk: what was not validated

- **iOS was not exercised at all.** There was no macOS host, so no simulator, no device and no Mac Catalyst run. The iOS conclusion in §1 comes from package inspection only. In particular it is **unverified** that the .NET for iOS build statically links `runtimes/ios-arm64/native/libsodium.a` from the libsodium package, and that the `__Internal` P/Invokes of NSec's iOS assembly resolve under full AOT. Even a simulator run would not cover AOT-only behaviour, and the simulator cannot run the crypto at all (§1). **A physical iOS device run is needed** before claiming iOS support.
- **No physical Android device.** Only the x86_64 emulator ran; the arm64-v8a build was packaged but never executed. The emulator image is API 28; current Android versions (API 35/36, including 16 KB page-size devices) were not run. The binaries are 16 KB aligned (§1), but that was checked statically only.
- **Mono only.** The CoreCLR runtime for Android (opt-in in .NET 10) was not tested.
- **The iOS and Mac Catalyst project files have never been built.** Their `Info.plist`, entitlements and platform floors (iOS 15, Mac Catalyst 15) are untested. NSec's iOS and Mac Catalyst assemblies declare `SupportedOSPlatform` iOS 18.0 / Mac Catalyst 18.0; what that means for devices on iOS 15 to 17 is unverified.
- **The `SocketsHttpHandler` workaround (§3.3)** was validated only against Horizon testnet on the emulator.
- **Scope of the checks.** Only the listed flows ran. Other Horizon endpoints and response types, SEP-1/6/9/10/24/45 flows, federation (Nett TOML parsing) and Soroban transaction submission were not exercised on a device. Under `TrimMode=full` they depend entirely on the descriptor in §3.2, and their trim warnings are listed there but unverified at runtime.
- **MAUI 9 on Android** was not built. The same `net8.0` dependency path was run on the emulator through the published 15.1.0 package on `net10.0-android`, with and without the NSec workaround (§3.1).
- **Secure memory.** NSec keeps signing keys in libsodium secure memory (`mlock`). On Android, `mlock` limits were not measured; libsodium degrades silently if locking fails.

## 5. Deliverable coverage (SCF Q3 2026, deliverable 2)

What the deliverable committed to, and what this report covers. Items not delivered are marked plainly.

| Commitment | Status |
|------------|--------|
| Desk check: do NSec and Sodium.Core load on iOS and Android; pick a fallback | Done (§1), on 2026-09-30 together with the validation, not in the first week of July as planned. Decision: no fallback needed for the dependency set on `main`. |
| Environment: Android SDK, emulator image, MAUI workloads, pinned versions, reproducible | Done for Android on a Linux host (§2), with one emulator image (API 28). |
| Environment: macOS host with Xcode and iOS simulators | **Not done.** No macOS host was available. |
| Environment: signing and provisioning configuration | **Not done** for iOS. Android uses the default debug signing. |
| Minimal MAUI validation app, Release builds with trimming | Done: `StellarDotnetSdk.MauiValidation/`, with partial and full trimming. |
| Core flows: key pair generation and signing, Horizon query, transaction submit, Soroban simulate | Done on the Android emulator (§3.1). |
| Ed25519 native library loading, HTTP/SSE behaviour, trimming compatibility | Done on the Android emulator (§3.1–3.3). The SSE validation found an SDK limitation, with a workaround. |
| Android emulator | Done (x86_64, API 28). |
| At least one physical Android device | **Not done.** |
| iOS simulator | **Not done.** Expected not to work with the current libsodium package (§1). |
| iOS device smoke test, if provisioning allows | **Not done.** Physical-iOS behaviour, where AOT is enforced and there is no JIT, remains unvalidated risk (§4). |
| Validation after the multi-target package is published | The multi-target build is not published yet. `main` was validated, and so was the published 15.1.0 package (§3.1). |
| Compatibility report with workarounds and residual risk | This document. |
