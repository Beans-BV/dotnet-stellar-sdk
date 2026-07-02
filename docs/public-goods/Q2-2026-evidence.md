# Q2 '26 Deliverables — Evidence Package

**Submission:** SCF Public Goods Q2 '26 — .NET SDK
**Repo:** [`Beans-BV/dotnet-stellar-sdk`](https://github.com/Beans-BV/dotnet-stellar-sdk)
**Verification target:** `main` @ [`f065324f`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/f065324f) · CI: green — Pack and Test ([run 28585099714](https://github.com/Beans-BV/dotnet-stellar-sdk/actions/runs/28585099714)), Integration Tests against live Testnet ([run 28585099826](https://github.com/Beans-BV/dotnet-stellar-sdk/actions/runs/28585099826), 9m8s)

---

## 0. One-command full verification

Every claim below is reproducible in ~1 minute (excluding the live-network integration suite):

```bash
git clone https://github.com/Beans-BV/dotnet-stellar-sdk.git
cd dotnet-stellar-sdk

# Deliverable 5 — Unit test suite (expect 1927 passed, 0 failed)
dotnet test StellarDotnetSdk.Tests/StellarDotnetSdk.Tests.csproj -c Release --nologo 2>&1 | tail -3

# Deliverable 2 — Integration test suite (52 test methods; runs against live Testnet in CI)
grep -rE '^\s*\[Test\]' --include='*.cs' StellarDotnetSdk.IntegrationTests | wc -l

# Deliverable 4 — SEP-45 unit tests (expect 82 passed) + 6 SEP matrices at 100%
dotnet test StellarDotnetSdk.Tests --filter "FullyQualifiedName~Sep0045" --nologo 2>&1 | tail -3
grep -h "Total Coverage" StellarDotnetSdk/Compatibility/sep/*.md

# XML doc gate carried over from Q1 (expect 0 CS1591)
dotnet build StellarDotnetSdk/StellarDotnetSdk.csproj -c Release --nologo 2>&1 | grep -c "CS1591"
```

Expected output verbatim:

```
Passed!  - Failed:     0, Passed:  1927, Skipped:     1, Total:  1928   # unit suite (net8.0)
52                                                                      # integration [Test] methods
Passed!  - Failed:     0, Passed:    82, Skipped:     0, Total:    82   # SEP-45 tests
**Total Coverage:** 100.0%  (× 6 — SEP-1, 6, 9, 10, 24, 45)
0                                                                       # CS1591 count
```

The one skipped unit test is `AuthorizeEntry_AgainstP27Testnet_SubmitsSuccessfully` (network-gated by design).

---

## 1. Deliverable-by-deliverable evidence

### Deliverable 1 — Protocol 26 "Yardstick" Support

**Closing issue:** [#155 — SDK Updates for Protocol 26 Compatibility](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/155) (closed 2026-06-07, together with the 15.1.0 stable release).

**Delivery PRs:**

| PR | Commit | Magnitude |
|---|---|---|
| [#169](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/169) migrate XDR generator from xdrgen | [`67ca1e48`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/67ca1e48) | 82 files, +8,305 / −34 |
| [#170](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/170) regenerate XDR classes with the new generator | [`80761a3e`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/80761a3e) | **478 files, +17,043 / −3,176** |
| [#176](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/176) bump stellar-xdr to v26 | [`945633a2`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/945633a2) | 29 files, +559 / −56 |
| [#177](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/177) SDK types for v26 frozen ledger keys + trustline-frozen results | [`80ae353c`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/80ae353c) | 43 files, +1,109 / −59 |

**Plan scorecard** (every Protocol 26 item from the submission, verified in code at `f065324f`):

| Planned item | Status | Where |
|---|---|---|
| 5 new frozen-ledger-key XDR types (CAP-77) | ✅ 5/5 | `EncodedLedgerKey`, `FreezeBypassTxs`, `FreezeBypassTxsDelta`, `FrozenLedgerKeys`, `FrozenLedgerKeysDelta` (all in `StellarDotnetSdk.Xdr/`, added by #176) |
| 4 new ConfigSettingID values | ✅ 4/4 | `ConfigSettingID.cs` — values 17–20 (`CONFIG_SETTING_FROZEN_LEDGER_KEYS` … `FREEZE_BYPASS_TXS_DELTA`) |
| 16 new BN254 ContractCostType entries (CAP-80) | ✅ 16/16 | `ContractCostType.cs` — `Bn254EncodeFp`=70 … `Bn254G1Msm`=85 |
| 4 new result codes | ✅ 4/4 | `txFROZEN_KEY_ACCESSED`, `CLAIM_CLAIMABLE_BALANCE_TRUSTLINE_FROZEN`, `LIQUIDITY_POOL_DEPOSIT_TRUSTLINE_FROZEN`, `LIQUIDITY_POOL_WITHDRAW_TRUSTLINE_FROZEN` (XDR enums + SDK result wrappers + tests) |
| 7 contract-spec unbounded-array changes | ✅ 7/7 | `SCSpecEventV0`, `SCSpecFunctionV0`, `SCSpecUDTEnumV0`, `SCSpecUDTErrorEnumV0`, `SCSpecUDTStructV0`, `SCSpecUDTUnionCaseTupleV0`, `SCSpecUDTUnionV0` (all touched by #176) |
| Matrices updated to v26 | ✅ Done (2026-07-02) | `horizon_matrix.md` pins Horizon v27.0.0, `rpc_matrix.md` pins RPC v26.0.1 — verified against upstream release notes: no new endpoints or RPC methods in either version (Horizon v26/v27 changes are result codes + effects the SDK already ships via #177/#179) |
| `getLatestLedger` v26 response fields | ✅ Done (2026-07-02) | `CloseTime` / `HeaderXdr` / `MetadataXdr` added to `GetLatestLedgerResponse` (6/6 response fields, verified against the `stellar-rpc` v26.0.1 handler source) with unit tests |

**Timeline vs plan:** 15.1.0-beta with full Protocol 26 support shipped **2026-04-22** — six days after the Testnet upgrade (Apr 16) and two weeks **before** the Mainnet vote (May 6). No .NET integrator broke on the Mainnet upgrade. Stable [15.1.0](https://github.com/Beans-BV/dotnet-stellar-sdk/releases/tag/15.1.0) (2026-06-07) contains exactly #169, #170, #176, #177 (verified via release notes).

**Bonus — Protocol 27 "Zipper" (CAP-71), pulled forward from Q3/Q4:**
[PR #187](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/187) (merged **2026-06-18, the day of the Protocol 27 Testnet upgrade**; commit [`deb388b7`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/deb388b7), 18 files, +3,277 / −165) delivers `SorobanAddressCredentialsV2`, delegated credentials (`SOROBAN_CREDENTIALS_ADDRESS_WITH_DELEGATES`), and signing helpers (`AuthorizeEntry`, `AuthorizeEntryWithDelegates`, `BuildAuthorizationEntryPreimageHash`), KAT-verified against `@stellar/stellar-sdk` 16.0.0-rc.1. Shipped in [16.0.0-beta](https://github.com/Beans-BV/dotnet-stellar-sdk/releases/tag/16.0.0-beta) (2026-06-25). Tracking issues [#186](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/186)/[#188](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/188) stay open until the stable 16.0.0 release closes out the remaining RPC-flag follow-up.

---

### Deliverable 2 — Integration Test Suite

| Metric | Planned | Delivered |
|---|---|---|
| Priority 1 MUST areas | 17 | **17/17 covered** |
| Test methods | est. 30–40 | **52** (33 live-network + 19 offline config-hardening) |
| CI gating | release tags only | release tags **+ every push to `main`** + manual dispatch (superset of plan) |
| Testnet-reset resilience | required (June 17 reset) | all tests self-provision via Friendbot; suite green post-reset ([run 28585099826](https://github.com/Beans-BV/dotnet-stellar-sdk/actions/runs/28585099826)) |

**Delivery PRs:**

| PR | Commit | Magnitude |
|---|---|---|
| [#185](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/185) integration test suite (phase 1) | [`539530e4`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/539530e4) | 14 files, +647 / −4 |
| [#196](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/196) integration test suite (phase 2) | [`f065324f`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/f065324f) | 30 files, +1,258 / −15 |

**All 17 Priority-1 MUST areas, each with a named test class** in [`StellarDotnetSdk.IntegrationTests/`](https://github.com/Beans-BV/dotnet-stellar-sdk/tree/main/StellarDotnetSdk.IntegrationTests):

| # | Area | Test class |
|---|---|---|
| 1 | Friendbot funding | `FriendbotTests` |
| 2 | `Server.RootAsync()` | `RootTests` |
| 3 | SubmitTransaction — sync / async / fee bump | `SubmitTransactionTests` (3 tests) |
| 4 | CheckMemoRequired (SEP-29) | `CheckMemoRequiredTests` (4 tests) |
| 5 | AccountsRequestBuilder | `AccountsRequestBuilderTests` |
| 6 | TransactionsRequestBuilder + pagination | `TransactionsRequestBuilderTests` |
| 7 | PaymentsRequestBuilder | `PaymentsRequestBuilderTests` |
| 8 | CreateAccountOperation | `CreateAccountOperationTests` |
| 9 | PaymentOperation (native + non-native) | `PaymentOperationTests` |
| 10 | PathPayment StrictReceive + StrictSend (real orderbook) | `PathPaymentStrictReceiveTests`, `PathPaymentStrictSendTests` |
| 11 | ManageSellOffer + ManageBuyOffer | `ManageOffersTests` |
| 12 | ChangeTrust + SetOptions | `ChangeTrustOperationTests`, `SetOptionsOperationTests` |
| 13 | InvokeHostFunctionOperation (Soroban) | `InvokeHostFunctionTests` |
| 14 | ExtendFootprint + RestoreFootprint | `FootprintTests` |
| 15 | Soroban RPC full flow (all 8 planned methods) | `SorobanRpcFlowTests` |
| 16 | SSE streaming (live Horizon events) | `SseStreamingTests` |
| 17 | SEP-10 full auth flow vs real anchor (testanchor.stellar.org) | `Sep10AuthTests` |

The CI workflow ([`integration_tests.yml`](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/main/.github/workflows/integration_tests.yml), typical wall clock ~9 min) uses env-configurable endpoints with public-Testnet defaults and secrets-based tokens, and uploads a TRX result artifact.

Writing the tests also surfaced and fixed **2 real SDK bugs** shipped inside #196: `ExtendFootprintOperation.cs` and `RestoreFootprintOperation.cs` — exactly the class of "mocked tests pass while production breaks" defect this deliverable was funded to catch.

**Priority-2 SHOULD tests:** none implemented in Q2. Per the submission's explicit rule ("Any items not completed in Q2 move to Q3"), the full SHOULD list carries into Q3.

---

### Deliverable 3 — Multi-Platform Preparation: Multi-Target + .NET Modernization

**Part B — Modern .NET APIs: all 6 PRs merged** (closing issues [#164](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/164), [#165](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/165), [#166](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/166), [#167](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/167), [#168](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/168) — all closed):

| PR | Commit | Magnitude | Landed at |
|---|---|---|---|
| [#180](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/180) `FrozenDictionary` for static lookup tables | [`360e040f`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/360e040f) | 6 files, +464 / −279 | `OperationResponseJsonConverter`, `EffectResponseJsonConverter`, 2 enum converters |
| [#181](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/181) `AllowDuplicateProperties = false` | [`1cadace0`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/1cadace0) | 3 files, +108 / −0 | `Converters/JsonOptions.cs:51` |
| [#182](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/182) `RespectNullableAnnotations` | [`e03da676`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/e03da676) | 2 files, +75 / −0 | `Converters/JsonOptions.cs:54` |
| [#183](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/183) `JsonSerializerOptions.MakeReadOnly()` | [`f0eb9987`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/f0eb9987) | 2 files, +94 / −33 | `Converters/JsonOptions.cs:85` |
| [#189](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/189) `Stream.ReadExactly()` in XDR decoding | [`f33f13f2`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/f33f13f2) | 22 files, +688 / −752 | `XdrDataInputStream.cs` (7 call sites) + generator template |
| [#184](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/184) HTTP retry overhaul (`ForSoroban`/`ForHorizon` presets, POST retry on 408/429/5xx, `Retry-After` honored) | [`d72fa82c`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/d72fa82c) | 20 files, +2,591 / −370 | `Requests/HttpResilienceOptions.cs`, new `RetryingHttpMessageHandler`, `RetryAfterParser` |

Every planned Part B item from the submission (FrozenDictionary, ReadExactly, AllowDuplicateProperties, RespectNullableAnnotations, MakeReadOnly) is merged and verifiable by `grep` at the file/line references above.

**Part A — Multi-target `net10.0 + net8.0 + netstandard2.1`: work complete, in final review as [PR #195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195)** (tracking issue [#162](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/162)):

| Criterion | Status |
|---|---|
| Scope | 63 files, +1,402 / −248, opened 2026-06-26 |
| Both packages retargeted | `StellarDotnetSdk` and `StellarDotnetSdk.Xdr` → `<TargetFrameworks>net10.0;net8.0;netstandard2.1</TargetFrameworks>` |
| Crypto abstraction | internal `Ed25519` facade (`Crypto/Ed25519.cs`): NSec.Cryptography on net8.0/net10.0, Sodium.Core 1.4.1 on netstandard2.1, with cross-provider equivalence tests (`Ed25519CrossProviderTest.cs`) |
| Polyfills / compat | `CompilerPolyfills.cs`, `Throw.cs` (ThrowIfNull/ThrowIfNullOrEmpty), `NetstandardCompat.cs` (ReadExactly shim), `DateOnly` conditional handling for SEP-9 |
| Dedicated netstandard2.1 test host | new `StellarDotnetSdk.NetStandard21.Tests` project; CI packs and tests all three TFMs |
| CI | **7/7 checks green** (`gh pr checks 195`) |
| Merge status | ⚠️ Open — `reviewDecision: REVIEW_REQUIRED`, active maintainer review rounds through 2026-06-30 |

We report Part A honestly as *delivered into review, not merged*: `main` still single-targets net8.0 until #195 lands, which is the first scheduled action of Q3.

---

### Deliverable 4 — SEP-45 Implementation + SEP Compatibility Matrices

**Closing issues:** [#160 — SEP-45 Implementation](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/160) (closed 2026-06-25), [#161 — SEP Compatibility Matrices](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/161) (closed 2026-06-24).

**Delivery PRs:** [#190](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/190) SEP-45 implementation (merged 2026-06-24, commit [`32f72f11`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/32f72f11), 39 files, +5,540 / −0) and [#191](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/191) SEP matrices (6 files, +1,320, merged into the feature branch 2026-06-23 and landed on `main` via #190 — the two PRs' line counts overlap and must not be summed).

| Criterion | Evidence |
|---|---|
| Implementation | `StellarDotnetSdk.Sep.Sep0045` — 28 files: `ClientWebAuthContract` (toml discovery, challenge, validation, auth-entry signing, JWT), `Sep45Challenge` helpers, 22 typed exceptions |
| Security hardening | 512 KiB response cap, https-only auth endpoint, no cross-origin credential forwarding, network-passphrase fail-fast |
| Unit tests | **82 passed / 0 failed** (`dotnet test --filter "FullyQualifiedName~Sep0045"`) |
| Peer-SDK gap closed | Flutter, iOS, and Java all shipped SEP-45 before us (issue #158's own framing: "we are the only SDK without it") — no longer true |
| Matrices | 6 published in [`StellarDotnetSdk/Compatibility/sep/`](https://github.com/Beans-BV/dotnet-stellar-sdk/tree/main/StellarDotnetSdk/Compatibility/sep) — exactly the promised set |

| Matrix | Coverage |
|---|---|
| SEP-0001 | 100.0% (70/70 fields) |
| SEP-0006 | 100.0% (95/95 fields) |
| SEP-0009 | 100.0% (76/76 fields) |
| SEP-0010 | 100.0% (22/22 applicable fields) |
| SEP-0024 | 100.0% (94/94 fields) |
| SEP-0045 | 100.0% (35/35 applicable fields) — `jwt_token_generation` marked N/A (server-side anchor responsibility; unimplemented in Flutter/Java/Python/JS/Go SDKs too) |

**Demo snippet using the new surface:**

```csharp
using StellarDotnetSdk.Sep.Sep0045;

// Discover config from the anchor's stellar.toml
using var webAuth = await ClientWebAuthContract.FromDomainAsync(
    "anchor.example.com", Network.Test(), "https://soroban-testnet.stellar.org");

// End-to-end SEP-45: GET challenge → validate → sign auth entries → POST → JWT
string jwt = await webAuth.JwtTokenAsync(
    clientAccountId: "C...CONTRACT_ADDRESS",
    signers: new[] { KeyPair.FromSecretSeed("S...") });
```

---

### Deliverable 5 — Release & Verification

| Criterion | Evidence |
|---|---|
| Releases shipped | [15.0.0](https://github.com/Beans-BV/dotnet-stellar-sdk/releases/tag/15.0.0) (2026-04-09) · [15.1.0-beta](https://github.com/Beans-BV/dotnet-stellar-sdk/releases/tag/15.1.0-beta) (2026-04-22) · [15.1.0](https://github.com/Beans-BV/dotnet-stellar-sdk/releases/tag/15.1.0) (2026-06-07, current Latest) · [16.0.0-beta](https://github.com/Beans-BV/dotnet-stellar-sdk/releases/tag/16.0.0-beta) (2026-06-25) |
| Unit test suite | **1,663 → 1,927 passed (+264, +15.9%)**, 0 failed |
| Integration suite | 52 tests, green in CI against live Testnet on every `main` push and release tag |
| XML doc gate (Q1 carry-over) | 0 × CS1591, still enforced via `<WarningsAsErrors>CS1591</WarningsAsErrors>` |
| CI on `main` @ `f065324f` | all green: Pack and Test, Integration Tests, CodeQL |
| Endpoint matrices | Horizon 100.0% (50/50), RPC 100% — parity maintained |

Stable **16.0.0** (Protocol 27 + SEP-45 + modernization + multi-target) is staged as a draft and ships early Q3 once [#195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195) merges — tracked in [#159](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/159).

---

### Non-deliverable — Developer Support & Maintenance Responsiveness

Operational metrics across the Q2 '26 window (2026-04-01 → 2026-07-02), reproducible via `gh`/`git`:

| Metric | Count | Command |
|---|---|---|
| Commits on `main` | **26** | `git rev-list --count --since=2026-04-01 main` |
| PRs merged | **21** | `gh pr list --state merged --search "merged:2026-04-01..2026-07-02"` |
| Issues closed | **15** | `gh issue list --state closed --search "closed:2026-04-01..2026-07-02"` (12 via search; #157/#158/#163 verified via direct API — GitHub's search index omits them) |
| Goal-closing issues | 9 — [#155](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/155), [#160](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/160), [#161](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/161), [#164](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/164)–[#168](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/168) | |
| Bug fixes | 2 ([#179](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/179) missing `contract_credited`/`contract_debited` handling, closing [#172](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/172); [#178](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/178) docs build) | |
| Releases shipped | **4** (2 stable, 2 beta) | `gh release list` |
| Author split | cuongph87: 18 commits · jopmiddelkamp: 8 commits | `git shortlog -sn --since=2026-04-01` |

**Continuity backlog already scoped for Q3:** [#162](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/162) multi-target (PR #195 in review), [#188](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/188) Protocol 27 close-out, [#156](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/156) integration-test umbrella (Priority-2), plus newly triaged bugs [#193](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/193) (pagination drops auth/resilience config) and [#197](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/197) (RPC error-response mapping).

---

## 2. Cross-reference: Q1 reviewer expectations → Q2 evidence

| Expectation from Q1 review | Addressed by |
|---|---|
| Explicit proof links per deliverable | Every deliverable above lists PRs with merge commits and +/− magnitudes |
| Quantitative before/after | Tests 1,663 → 1,927; SEPs 5 → 6; SEP matrices 0 → 6 (all 100% field coverage); integration tests 0 → 52; targets net8.0 → three TFMs (in review) |
| Concrete issue/PR links per objective | Closing issues cited per deliverable (#155, #160, #161, #164–#168) |
| SEP compatibility matrices (peers have them, we had 0) | Deliverable 4 — 6 matrices published in-tree |
| Automated test evidence | Unit suite + live-Testnet integration suite in CI ([run 28585099826](https://github.com/Beans-BV/dotnet-stellar-sdk/actions/runs/28585099826)) |

---

## 3. Honest gaps & carry-over (pre-empting follow-ups)

- **Multi-target not merged.** D3 Part A is fully built with green CI as [PR #195](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/195), but review completes in early Q3. `main` is still net8.0-only today.
- **Two D1 sub-items landed at window close (2026-07-02).** The Horizon/RPC matrix version bump (now v27.0.0 / v26.0.1) and the `getLatestLedger` response fields (`closeTime`, `headerXdr`, `metadataXdr`) were completed on the last day of the window, after the rest of this evidence was gathered. The research confirmed Horizon v26/v27 added no new endpoints (result codes and effects were already covered by #177/#179), so endpoint coverage remains 50/50.
- **Priority-2 SHOULD integration tests: 0 of the stretch list.** Priority 1 landed 17/17; the SHOULD list moves to Q3 exactly as the submission's overflow rule specified.
- **16.0.0 stable not yet published.** Protocol 27 + SEP-45 are live in 16.0.0-beta; the stable major follows the #195 merge to avoid two back-to-back majors.
- **Protocol 27 tracking issues (#186/#188) still open** although the CAP-71 code is merged and beta-shipped — they close with the stable release.
- **~175 non-CS1591 build warnings remain** (CS1572/1573/1574 doc-tag hygiene, some in the new SEP-45 files). The CS1591 missing-doc gate from Q1 stays at zero; tag hygiene continues under the capacity buffer.
