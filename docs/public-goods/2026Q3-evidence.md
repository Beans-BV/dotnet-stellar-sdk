# Q3 '26 Deliverables — Evidence Package

**Submission:** SCF Public Goods Q3 '26 — .NET SDK
**Proposal:** [PG Award Proposal: Stellar .NET SDK (Q3)](https://github.com/SCF-Public-Goods-Maintenance/scf-public-goods-maintenance.github.io/pull/125), merged 2026-08-04: 2 deliverables, 2 non-deliverables
**Repo:** [`Beans-BV/dotnet-stellar-sdk`](https://github.com/Beans-BV/dotnet-stellar-sdk)

**Verification target:** release tag
[`16.0.0`](https://github.com/Beans-BV/dotnet-stellar-sdk/releases/tag/16.0.0) @
[`9440e88e`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/9440e88e) on `main` (2026-10-03).
It sits one commit above [`51cc50c4`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/51cc50c4)
(2026-10-02, the last deliverable merged) and changes only the CHANGELOG, version numbers, matrix
headers and one publish-workflow flag, not code or tests · CI on the tag commit: green — Pack and Test
([run 37089117259](https://github.com/Beans-BV/dotnet-stellar-sdk/actions/runs/37089117259)),
Integration Tests against live Testnet
([run 37089117283](https://github.com/Beans-BV/dotnet-stellar-sdk/actions/runs/37089117283), 56/56),
XDR Generator Tests
([run 37089117258](https://github.com/Beans-BV/dotnet-stellar-sdk/actions/runs/37089117258)), CodeQL
([run 37089116601](https://github.com/Beans-BV/dotnet-stellar-sdk/actions/runs/37089116601)) ·
Published to NuGet as
[`stellar-dotnet-sdk` 16.0.0](https://www.nuget.org/packages/stellar-dotnet-sdk/16.0.0) and
[`stellar-dotnet-sdk-xdr` 16.0.0](https://www.nuget.org/packages/stellar-dotnet-sdk-xdr/16.0.0)
([publish run 37090782596](https://github.com/Beans-BV/dotnet-stellar-sdk/actions/runs/37090782596))
· **Activity window:** 2026-07-03 → 2026-10-02, starting the day after the Q2 report's cutoff so no
work is counted twice.

| Item                                      | Result                                                                                         |
| ----------------------------------------- | ---------------------------------------------------------------------------------------------- |
| Deliverable 1: SEP-7, SEP-12, SEP-38      | ✅ Delivered and released in 16.0.0                        |
| Deliverable 2: MAUI validation            | ✅ Delivered: Android (emulator + 4 physical devices), iPhone, iOS Simulator and Mac Catalyst |
| Non-deliverable 1: support & maintenance  | ✅ 13 issues closed (11 bugs), 30 PRs merged; two issues from Stellar's SDK team got no reply  |
| Non-deliverable 2: capacity buffer        | ✅ Fully used: Soroban RPC correctness, Protocol 27 follow-ups, Protocol 28                    |
| Release                                   | ✅ Completed: 16.0.0 available on [NuGet](https://www.nuget.org/packages/stellar-dotnet-sdk/16.0.0) (2026-10-03) |

## 0. One-command verification

Every deliverable is in the 16.0.0 release, so one checkout of the tag reproduces every number below
in a few minutes. The MAUI checks need network access to Stellar Testnet.

```bash
git clone https://github.com/Beans-BV/dotnet-stellar-sdk.git
cd dotnet-stellar-sdk
git checkout -q 16.0.0

# The NuGet package ships three target frameworks
curl -sL https://api.nuget.org/v3-flatcontainer/stellar-dotnet-sdk/16.0.0/stellar-dotnet-sdk.16.0.0.nupkg -o sdk.nupkg
unzip -l sdk.nupkg | grep -oE "lib/[^ ]+\.dll"

# Full unit suite (expect 3756 passed on net8.0; the Q2 report had 1927)
dotnet test StellarDotnetSdk.Tests -f net8.0 --nologo 2>&1 | tail -1

# Deliverable 1: SEP unit tests and matrix coverage
for sep in 0007 0012 0038; do
  dotnet test StellarDotnetSdk.Tests -f net8.0 --no-build --nologo \
    --filter "FullyQualifiedName~Tests.Sep.Sep${sep}" 2>&1 | tail -1
  grep -h "Total Coverage" "StellarDotnetSdk/Compatibility/sep/SEP-${sep}_COMPATIBILITY_MATRIX.md"
done
ls StellarDotnetSdk/Compatibility/sep/ | wc -l   # SEP matrices

# Deliverable 2: the six MAUI validation checks as a desktop app, built from source
dotnet run --project StellarDotnetSdk.MauiValidation/Desktop -c Release 2>&1 | grep RESULT

# Deliverable 2: the same six checks against the published NuGet package (run next to the clone)
cd ..
dotnet new console --framework net10.0 -o pkgcheck
cp dotnet-stellar-sdk/StellarDotnetSdk.MauiValidation/{ValidationRunner.cs,StreamSearch.cs,Desktop/Program.cs} pkgcheck/
dotnet add pkgcheck package stellar-dotnet-sdk --version 16.0.0
dotnet run --project pkgcheck 2>&1 | grep -E "assemblies|RESULT"
```

Expected output (unit test lines trimmed after `Total`):

```txt
lib/net10.0/StellarDotnetSdk.dll
lib/net8.0/StellarDotnetSdk.dll
lib/netstandard2.1/StellarDotnetSdk.dll
Passed!  - Failed:     0, Passed:  3756, Skipped:     1, Total:  3757   # full suite
Passed!  - Failed:     0, Passed:   589, Skipped:     0, Total:   589   # SEP-7
**Total Coverage:** 100.0% (31/31 fields)
Passed!  - Failed:     0, Passed:   368, Skipped:     0, Total:   368   # SEP-12
**Total Coverage:** 100.0% (90/90 fields)
Passed!  - Failed:     0, Passed:   284, Skipped:     0, Total:   284   # SEP-38
**Total Coverage:** 100.0% (75/75 fields)
9                                                                       # SEP matrices
STELLAR-MAUI-VALIDATION RESULT PASS 6/6                                 # from source
STELLAR-MAUI-VALIDATION INFO assemblies StellarDotnetSdk 16.0.0+9440e88e3cb953e4a2b7ed34d1bdef472533af14, NSec.Cryptography 26.4.0+…
STELLAR-MAUI-VALIDATION RESULT PASS 6/6                                 # from the NuGet package
```

The test counts include every `[DataRow]` case. The one skipped unit test is network-gated by design,
as in Q2. The suite also passes on `net10.0` (3,766) and on the `netstandard2.1` build (3,762).

---

## 1. Evidence per deliverable

### Deliverable 1: SEP Expansion (SEP-7, SEP-12, SEP-38, with matrices)

**Status: delivered and released.** All three PRs were opened on 2026-09-30 and merged on 2026-10-02,
after maintainer review and green CI, and shipped in
[16.0.0](https://github.com/Beans-BV/dotnet-stellar-sdk/releases/tag/16.0.0) on 2026-10-03. The plan
was to ship them "in one or more 16.x minors as they complete"; they shipped together in 16.0.0
instead.

| PR                                                                                       | Merge commit                                                                 | Magnitude              | SEP version | Matrix                                                                                                                                           | Unit tests |
| ---------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------- | ---------------------- | ----------- | ------------------------------------------------------------------------------------------------------------------------------------------------ | ---------- |
| [#238](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/238) SEP-7 URI scheme         | [`674980e0`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/674980e0) | 32 files, +7,392 / −5  | 2.1.0       | [100.0% (31/31)](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/9440e88e/StellarDotnetSdk/Compatibility/sep/SEP-0007_COMPATIBILITY_MATRIX.md) | 589 passed |
| [#239](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/239) SEP-12 KYC API client    | [`51cc50c4`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/51cc50c4) | 51 files, +7,810 / −58 | 1.15.0      | [100.0% (90/90)](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/9440e88e/StellarDotnetSdk/Compatibility/sep/SEP-0012_COMPATIBILITY_MATRIX.md) | 368 passed |
| [#243](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/243) SEP-38 Anchor RFQ client | [`756a589f`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/756a589f) | 45 files, +7,320 / −15 | 2.5.0       | [100.0% (75/75)](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/9440e88e/StellarDotnetSdk/Compatibility/sep/SEP-0038_COMPATIBILITY_MATRIX.md) | 284 passed |

| Criterion (from the proposal)                                | Evidence                                                                                                                                                                       |
| ------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 3 new SEP namespaces with passing unit tests                 | `Sep/Sep0007` (`UriScheme`, `Sep7Uri`), `Sep/Sep0012` (`KycService`, `KycCallbackSignature`), `Sep/Sep0038` (`QuoteService`, `AssetIdentifier`); 1,241 SEP unit tests, 0 failing |
| Matrix count grows from 6 to 9                               | 9 matrices in [`StellarDotnetSdk/Compatibility/sep/`](https://github.com/Beans-BV/dotnet-stellar-sdk/tree/9440e88e/StellarDotnetSdk/Compatibility/sep), each at 100% field coverage, in the Q2 format |
| SDK goes from 6 to 9 implemented SEPs                        | SEP-1, 6, 7, 9, 10, 12, 24, 38 and 45                                                                                                                                          |
| SEP-12 and SEP-38 reuse the SEP-10/45 WebAuth infrastructure | Both clients take the JWT that the existing SEP-10 and SEP-45 clients produce, and discover their endpoints from `stellar.toml` like SEP-6 and SEP-24                          |

With SEP-6 and SEP-24 already in the SDK, a .NET client can now run the standard client-side anchor
flow with SDK calls only: SEP-1 (discover) → SEP-10/45 (authenticate) → SEP-12 (KYC) → SEP-38 (quote)
→ SEP-6/24 (deposit or withdraw).

The matrices use the same field-level format as the other Stellar SDKs, so the percentages are
comparable. For SEP-38 v2.5.0 our matrix lists 75 fields where the Flutter SDK's lists 58; ours also
covers the buy-side `/prices` fields added in v2.3.0 and the delivery-method fields.

All three clients carry the hardening established for SEP-45 in Q2: https-only endpoints (plain http
only for an explicit loopback host), capped response bodies (512 KiB for SEP-7's `stellar.toml` and
callback reads, 1 MiB for SEP-12 and SEP-38), no automatic redirect following on the SDK's own HTTP
client (SEP-7's `stellar.toml` fetch allows up to five https redirects), rejection of duplicate JSON
properties, JWTs and KYC data redacted from request `ToString()`, and bounded, sanitized exception
messages.

**Demo snippet** (compiles against the NuGet package 16.0.0 with warnings as errors):

```csharp
using StellarDotnetSdk.Sep.Sep0007;
using StellarDotnetSdk.Sep.Sep0012;
using StellarDotnetSdk.Sep.Sep0012.Requests;
using StellarDotnetSdk.Sep.Sep0038;
using StellarDotnetSdk.Sep.Sep0038.Requests;

// SEP-7: build a payment request, then validate it on the wallet side
string uri = UriScheme.GeneratePayOperationUri(
    destination: "GDR6DXASQP4XMGFBCU4NQEU63WY3WEOMPQAQLBJN533GKGBV3MOWIJTL",
    amount: "120.5",
    assetCode: "USDC",
    assetIssuer: "GBBD47IF6LWK7P7MDEVSCWR7DPUWV3NY3DTQEVFL4NAT4AQH3ZLLFLA5");
Sep7ValidationResult check = UriScheme.ValidateUri(uri); // check.IsValid == true

// SEP-12: the customer's KYC status (the JWT comes from SEP-10 or SEP-45)
var kyc = await KycService.FromDomainAsync("testanchor.stellar.org");
var customer = await kyc.GetCustomerInfoAsync(new GetCustomerInfoRequest { Jwt = jwt });
// customer.Status: Accepted | Processing | NeedsInfo | Rejected

// SEP-38: a firm quote, USD by wire -> USDC
var quotes = await QuoteService.FromDomainAsync("testanchor.stellar.org");
var quote = await quotes.PostQuoteAsync(new QuoteRequest
{
    Context = QuoteContext.Sep6,
    SellAsset = AssetIdentifier.Iso4217("USD"),
    BuyAsset = AssetIdentifier.Stellar("USDC", "GBBD47IF6LWK7P7MDEVSCWR7DPUWV3NY3DTQEVFL4NAT4AQH3ZLLFLA5"),
    SellAmount = 100m,
    SellDeliveryMethod = "WIRE",
    CountryCode = "US",
    Jwt = jwt,
});
```

---

### Deliverable 2: MAUI Validation (incl. environment setup)

**Status: delivered.** Two PRs:

- [PR #242](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/242) (merge commit [`ed597e26`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/ed597e26), 2026-10-02, 27 files,
  +1,829 / −3): the validation app
  ([`StellarDotnetSdk.MauiValidation/`](https://github.com/Beans-BV/dotnet-stellar-sdk/tree/17450661/StellarDotnetSdk.MauiValidation)), the
  desk check, the Android environment and runs, and the compatibility report
  ([`docs/maui-compatibility.md`](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/17450661/docs/maui-compatibility.md)).
- [PR #256](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/256) (merge commit [`17450661`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/17450661), 2026-10-03, 8 files,
  +228 / −22): the macOS environment and the Apple runs on iPhone, iOS Simulator and Mac Catalyst,
  against both `main` and the published NuGet package 16.0.0. It changes only the validation app and
  the report, not SDK code.

The report's own coverage table ([§5](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/17450661/docs/maui-compatibility.md#5-deliverable-coverage-scf-q3-2026-deliverable-2)) maps
every commitment to its status:

| Commitment | Status |
| --- | --- |
| Desk check: do NSec and Sodium.Core load on iOS and Android; pick a fallback | ✅ Done on 2026-09-30, not in the first week of July as planned. Decision: keep NSec as the only backend on the MAUI path, with no libsodium bundling and no managed fallback ([§1](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/17450661/docs/maui-compatibility.md#1-desk-check-does-the-ed25519-backend-load-on-ios-and-android)). The Apple runs confirmed its predictions |
| Environment for Android: SDK, emulator, MAUI workloads, pinned versions | ✅ Reproducible from a script: .NET SDK 10.0.401, workload set 10.0.401.1 (MAUI 10.0.110, Android 36.1.69), android-36 platform, API 28 x86_64 emulator image ([§2](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/17450661/docs/maui-compatibility.md#2-environment-setup)) |
| Environment for iOS: macOS host, Xcode, simulators, signing and provisioning | ✅ Apple silicon Mac, macOS 26.6, Xcode 27.0, iOS 27.0 simulator runtime, Apple Development certificate with a team provisioning profile, with build and run commands ([§2](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/17450661/docs/maui-compatibility.md#2-environment-setup)) |
| Validation app, Release build with trimming | ✅ `TrimMode=partial` (the MAUI default, no changes needed) and `TrimMode=full` (needs a documented linker descriptor; without it the first Horizon call fails) ([§3.2](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/17450661/docs/maui-compatibility.md#32-trimming)) |
| Core flows: key pair generation and signing, Horizon query, transaction submit, Soroban simulate | ✅ All pass against Testnet on the Android emulator, four physical Android devices, an iPhone, the iOS Simulator and Mac Catalyst |
| Ed25519 native library loading, HTTP/SSE, trimming | ✅ libsodium loads on every platform: dynamically on Android, statically linked on iOS. Found an Android-only SDK limitation: the default Android HTTP handler holds SSE events back by about 47 s; a documented `SocketsHttpHandler` workaround delivers them in under 4 s ([§3.3](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/17450661/docs/maui-compatibility.md#33-sse-streaming-on-android-the-default-http-handler-holds-events-back)). Apple platforms deliver SSE events in about 2–4 s with the default handler |
| Android emulator | ✅ x86_64, API 28 |
| At least one physical Android device | ✅ Four arm64 devices on 2026-10-02: Pixel XL (Android 10, API 29), Galaxy S21 Ultra (Android 15, API 35), Find X9 Pro (Android 16, API 36), Galaxy S23 Ultra (Android 16, API 36), all with full trimming ([§3.1](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/17450661/docs/maui-compatibility.md#31-android-results-mono-runtime-release)) |
| iOS simulator | ✅ 6/6 on iOS 27.0 (arm64). The libsodium package has no simulator build, so the link fails, as the desk check predicted; the new `build-libsodium-simulator.sh` builds one from the signed upstream release ([§3.4](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/17450661/docs/maui-compatibility.md#34-ios-and-mac-catalyst-results-release-trimmodefull--workaround-2026-10-03)) |
| iOS device smoke test | ✅ 6/6 on an iPhone 15 Pro (iOS 26.6.2) with `TrimMode=full`, AOT-only (`dynamicCode=False`, no JIT), with the SDK from `main` and with NuGet 16.0.0 ([§3.4](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/17450661/docs/maui-compatibility.md#34-ios-and-mac-catalyst-results-release-trimmodefull--workaround-2026-10-03)) |
| Mac Catalyst (beyond the plan) | ✅ 6/6 on macOS 26.6 (arm64), with `main` and with NuGet 16.0.0 |
| Validation after the multi-target package is published | ✅ On iPhone and Mac Catalyst with NuGet 16.0.0, and on desktop (osx-arm64). The Android runs used `main` @ `83303a27`, before the release; it resolves the same crypto dependencies as 16.0.0 (NSec 26.4.0, libsodium 1.0.22) |
| Compatibility report with workarounds and residual risk | ✅ [`docs/maui-compatibility.md`](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/17450661/docs/maui-compatibility.md) |

The validation also produced concrete findings for app developers:

- **15.1.0 does not work on Android.** It fails at the first crypto call
  (`PlatformNotSupportedException`), because its libsodium has no Android binaries. The multi-target
  16.0.0 package fixes this for .NET 10 MAUI apps; a .NET 9 MAUI app resolves the `net8.0` target and
  still needs an explicit `NSec.Cryptography` 26.4.0 reference.
- **iOS 27 stops MAUI apps without the UIScene lifecycle at launch.** This is a MAUI template gap,
  not an SDK one; the report documents the fix (a scene manifest and a `SceneDelegate`).
- **The iOS Simulator needs a simulator build of libsodium**, which the validation app shows how to
  build and link. Devices and Mac Catalyst need nothing.

The report lists the remaining risk plainly
([§4](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/17450661/docs/maui-compatibility.md#4-residual-risk-what-was-not-validated)): one physical iOS device (iOS 26.6.2) and no iOS
15–17 or physical iOS 27 device, the MAUI default trim mode and NativeAOT on iOS not run, and the
Android runs not repeated against the 16.0.0 package.

---

### Non-deliverable 1: Developer Support & Maintenance Responsiveness

Operational metrics for the activity window (2026-07-03 → 2026-10-02), reproducible via `gh` and
`git`:

| Metric                        | Count | Command |
| ----------------------------- | ----- | ------- |
| Commits on `main`             | **30** (cuongph87 24, Jop Middelkamp 6) | `git rev-list --count --since=2026-07-03T00:00Z --until=2026-10-03T00:00Z main` |
| PRs merged                    | **30** (28 into `main`, 2 into stacked PR branches), of which 9 merged on 2026-10-02, including all four deliverable PRs (#238, #239, #242, #243) | `gh pr list --state merged --search "merged:2026-07-03..2026-10-02"` |
| Issues opened / closed        | **23** / **13** (11 of the 13 are bugs) | `gh issue list --state all --search "created:2026-07-03..2026-10-02"`, and `--state closed --search "closed:2026-07-03..2026-10-02"` |
| Unit tests on `main` (net8.0) | **1,927 → 2,311** at the last September merge (`83303a27`), **3,756** at the 16.0.0 tag | see §0 |
| Integration test methods      | **52 → 56** (CAP-71 v2 and Protocol 28 live tests); 29 of 30 runs on `main` pushes green, the red one ([run 33148039960](https://github.com/Beans-BV/dotnet-stellar-sdk/actions/runs/33148039960)) was three tests hitting Horizon Testnet 502/503 errors | `grep -rE '^\s*\[Test\]' --include='*.cs' StellarDotnetSdk.IntegrationTests \| wc -l` |
| Releases published            | **1**: [16.0.0](https://github.com/Beans-BV/dotnet-stellar-sdk/releases/tag/16.0.0) (2026-10-03), available on NuGet | `gh release list` |
| NuGet downloads (lifetime)    | stellar-dotnet-sdk 566,582; stellar-dotnet-sdk-xdr 481,996 | NuGet search API, 2026-10-03 |

Two issues came from Stellar's SDK team this quarter, and both were acted on but neither got a direct
reply. [#206](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/206) (the `useUpgradedAuth`
flag) was fixed 15 days after it was filed (2026-08-28).
[#207](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/207) (Protocol 28) is addressed by
[#252](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/252), merged 2026-10-02 and released in
16.0.0.

---

### Non-deliverable 2: Capacity Buffer

The buffer, and more than the buffer, went to unplanned work, so SEP-30 (what an unused buffer would
have funded) was not started. About half of it fixed SDK features that did not work for users; the
rest tightened behaviour that mostly worked and could have waited for a later release:

- **Soroban RPC correctness (15 PRs).** Fixes for features that did not work: `authMode` sent in a
  form stellar-rpc rejects (a number in 14.x, upper case in 15.x), broken in every release since 14.0.0
  ([#209](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/209)); `getEvents` pagination silently
  dropped ([#219](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/219)); JSON-RPC errors swallowed
  ([#217](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/217),
  [#218](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/218)), which also closes the Q2
  carry-over [#197](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/197);
  `RestorePreamble.SorobanTransactionData` always throwing
  ([#222](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/222)); contract return values missing
  from V3 transaction metas ([#228](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/228)); and
  legitimate fees failing on the `MinResourceFee` type
  ([#221](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/221)). Stricter handling that could
  have waited: nullability, field presence and enum wire formats
  ([#220](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/220),
  [#225](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/225),
  [#234](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/234),
  [#235](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/235),
  [#236](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/236)), bounded converter exception
  messages ([#232](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/232)), and wire-format tests
  ([#216](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/216),
  [#233](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/233)). Most of the stricter items are
  breaking changes, so they also lengthen the 16.0.0 migration notes.
- **Protocol 27 follow-up:** the `useUpgradedAuth` flag (#209) and CAP-71 v2 credentials as the
  default ([#244](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/244)), as the proposal's
  "post-Mainnet-vote Protocol 27 follow-ups" anticipated.
- **Multi-target hardening after #195:** the Q2 JSON protections (`AllowDuplicateProperties = false`,
  `RespectNullableAnnotations`), which #195 had limited to `net10.0`, restored on all target
  frameworks ([#201](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/201)); post-review fixes to
  the Ed25519 signer, retry handler and date converters
  ([#202](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/202)); and NuGet Trusted Publishing for
  the release pipeline ([#203](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/203)), which
  16.0.0 is the first release to use.
- **Protocol 28 support:** [#252](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/252) (59 files,
  +3,414 / −75), covering CAP-85 external executable references and CAP-83, with byte-exact handling
  of binary executable tags, known-answer tests against `@stellar/stellar-sdk` 17.2.0, and two live
  Testnet tests.
- **XML doc-tag warnings (Q2 gap):** the 61 remaining doc warnings fixed, and every doc-tag warning
  now fails the build ([#254](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/254)).

**Protocol timeline**, with dates read from the first ledger of each protocol version:

| Protocol            | Testnet                                                           | Mainnet                                                     | On NuGet              |
| ------------------- | ----------------------------------------------------------------- | ----------------------------------------------------------- | --------------------- |
| 27 (CAP-71)         | [2026-06-18](https://horizon-testnet.stellar.org/ledgers/3157753) | [2026-07-08](https://horizon.stellar.org/ledgers/63386819) | 16.0.0 (2026-10-03)   |
| 28 (CAP-83, CAP-85) | [2026-08-27](https://horizon-testnet.stellar.org/ledgers/4365284) | [2026-09-16](https://horizon.stellar.org/ledgers/64458446) | 16.0.0 (2026-10-03)   |
| 29                  | [2026-09-29](https://horizon-testnet.stellar.org/ledgers/4935524) | [2026-10-01](https://horizon.stellar.org/ledgers/64717645) | 16.0.0 (no XDR change) |

Protocol 29 needs no SDK change: stellar-core
[v29.0.0](https://github.com/stellar/stellar-core/tree/v29.0.0-internal/src/protocol-curr) builds on
the same stellar-xdr commit
([`9c9c145`](https://github.com/stellar/stellar-xdr/commit/9c9c145953e80990d6ff1ae3a6a973a0ce6d0694))
as Protocol 28.

---

## 2. Q2 carry-over and review: what was promised and what happened

| Q2 report or review said                                                                                                   | Outcome |
| -------------------------------------------------------------------------------------------------------------------------- | ------- |
| Stable 16.0.0 ships early in Q3; the review accepted v16 as "staged, pending a small P27 compatibility fix"                 | ✅ Completed. The P27 fix merged on 2026-08-28 (#209); 16.0.0 is released (2026-10-03) and available on NuGet |
| `getLatestLedger` fields and matrix re-pins in [#198](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/198) (in review) | ✅ Merged 2026-10-02, also adding the `getHealth` close times; both matrices pinned to v28.0.1 at 100% (Horizon 50/50, RPC 12/12) |
| Bug [#197](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/197) (RPC error-response mapping)                         | ✅ Fixed in #217 |
| Bug [#193](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/193) (pagination drops auth and resilience config)        | ⏭️ Moved to Q4. Bug-fix time went first to the Soroban RPC client, where bugs broke calls outright: every `simulateTransaction` that set `authMode` failed, and `getEvents` could not page (11 bugs fixed, see Non-deliverable 2). #193 affects Horizon reads from page 2 onward (`NextPage()`/`PreviousPage()`): those requests drop the bearer token, default headers and retry policy. |
| Protocol 27 tracking issues #186 and #188 close with the stable release                                                    | ✅ Shipped in 16.0.0 and closed |
| Priority-2 integration tests move to Q3                                                                                    | ⏭️ Moved to Q4. These were not part of the Q3 proposal; the Q2 report had moved them here. The Q3 integration-test work went to the two protocol upgrades that reached the network this quarter: 4 new live Testnet tests for Protocol 27 (CAP-71 v2 auth) and Protocol 28 (CAP-85), taking the suite from 52 to 56. |
| ~175 non-CS1591 warnings (doc-tag hygiene) remain                                                                          | ✅ Doc-tag warnings at 0 and gated (#254) |
| The multi-target work was "harder to verify for the reviewer as a non-.NET developer"                                      | Both Q3 deliverables can be checked without .NET: the matrices are plain tables, the MAUI report's §5 maps every commitment to a status, and the NuGet package lists its three target frameworks |

