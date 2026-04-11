# Stellar .NET SDK — Roadmap 2026 Q2 – 2027 Q1

**Date:** 2026-04-11
**Budget:** 200 hours per quarter (800 hours total)
**Grant:** Stellar Public Goods Award
**Priority:** .NET Backend APIs → MAUI + Wallet SDK → Unity → Tizen

---

## Executive Summary

| Quarter | Theme | Key Outcomes |
|---------|-------|-------------|
| **Q2 2026** | **Backend: Reliability & Protocol** | Protocol 26, integration tests (MUST+SHOULD), multi-target (net10.0+net8.0+netstandard2.1), SEP-45 + SEP matrices |
| **Q3 2026** | **Modernization & SEP Expansion** | JSON modernization, 7 new SEPs (7/12/30/38/47/48/53), SseParser, MAUI validation |
| **Q4 2026** | **App Developer Platform** | .NET Wallet SDK (new package), MAUI sample app, Unity validation + sample, Tizen validation |
| **Q1 2027** | **Maturity & Ecosystem** | Wallet SDK hardening, Tizen sample, AOT source gen, performance benchmarks, XML docs completion |

---

## Current State (April 2026)

- **Target:** net8.0 only
- **Version:** v15.0.0
- **Coverage:** Horizon 100% (50/50), Stellar RPC 100% (12/12)
- **SEPs:** 5 implemented (SEP-1, 6, 9, 10, 24)
- **Tests:** 186 unit test files (mocked responses only, no integration tests)
- **XML docs:** 382 missing items
- **JSON:** Reflection-based System.Text.Json, ~900 manual `[JsonPropertyName]` attributes
- **SSE:** Third-party LaunchDarkly.EventSource dependency
- **No multi-platform support**

### SEP Parity vs Peer SDKs

| SEP | Flutter | iOS | Java | .NET (us) |
|-----|---------|-----|------|-----------|
| SEP-1 (TOML) | ✅ | ✅ | ✅ | ✅ |
| SEP-6 (Deposit/Withdraw) | ✅ | ✅ | ✅ | ✅ |
| SEP-7 (URI Scheme) | ✅ | ✅ | — | ❌ |
| SEP-9 (KYC Fields) | ✅ | ✅ | ✅ | ✅ |
| SEP-10 (Web Auth) | ✅ | ✅ | ✅ | ✅ |
| SEP-12 (KYC API) | ✅ | ✅ | — | ❌ |
| SEP-24 (Interactive) | ✅ | ✅ | ✅ | ✅ |
| SEP-30 (Recovery) | ✅ | ✅ | — | ❌ |
| SEP-38 (Quotes) | ✅ | ✅ | — | ❌ |
| SEP-45 (Contract WebAuth) | ✅ | ✅ | ✅ | ❌ |
| SEP-47 (Contract ID) | ✅ | ✅ | — | ❌ |
| SEP-48 (Contract Events) | ✅ | ✅ | — | ❌ |
| SEP-53 (Message Signing) | planned | planned | — | ❌ |

### Protocol Timeline

| Date | Event |
|------|-------|
| April 16, 2026 | Protocol 26 "Yardstick" — Testnet upgrade |
| May 6, 2026 | Protocol 26 — Mainnet vote |
| June 17, 2026 | Testnet reset |
| Nov 2026 | .NET 8 end of support |

Sources: [Protocol 26 Guide](https://stellar.org/blog/foundation-news/stellar-yardstick-protocol-26-upgrade-guide), [.NET Support Policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core), [Stellar Networks](https://developers.stellar.org/docs/networks)

---

## Q2 2026 — Backend: Reliability & Protocol (200h)

Focus: make the SDK reliable and future-proof for .NET backend developers.

### D1 — Protocol 26 "Yardstick" Support (25h)

- **Specific:** Update XDR schemas from stellar-xdr v26.0, regenerate C# types
  via `xdrgen`, update RPC response models (`getLatestLedger` gains
  `ledgerCloseTime`, `headerXdr`, `metadataXdr`), add new result codes, update
  compatibility matrices.
- **Measurable:** SDK builds and passes tests against Protocol 26 Testnet;
  matrices updated to v26.0.
- **Time-bound:** Before April 16 (Testnet) and May 6 (Mainnet).

Protocol 26 changes: 5 new XDR types (frozen ledger keys — CAP-77), 4 new
ConfigSettingID values (17–20), 16 new BN254 ContractCostType entries (CAP-80),
4 new result codes, 7 contract spec unbounded arrays, SCP quorum nesting 2→4,
RPC `getLatestLedger` new fields.

### D2 — Integration Test Suite (65h)

Test infrastructure (12h) + Priority 1 MUST tests (35h) + Priority 2 SHOULD
tests as stretch (18h).

**Priority 1 — MUST tests (17 areas):**

| # | Area | What it validates |
|---|---|---|
| 1 | Friendbot funding | Foundational for all other tests |
| 2 | Server.RootAsync() | Connectivity + protocol version |
| 3 | SubmitTransaction (sync) | Core transaction submission |
| 4 | SubmitTransactionAsync (async) | Separate `/transactions_async` endpoint |
| 5 | SubmitTransaction (FeeBump) | Fee bump wrapping + submission |
| 6 | CheckMemoRequired | SEP-29 enforcement on real accounts |
| 7 | AccountsRequestBuilder | Fetch account, account data |
| 8 | TransactionsRequestBuilder | Query + paginate transactions |
| 9 | PaymentsRequestBuilder | Query payments |
| 10 | CreateAccountOperation | Create new accounts on-chain |
| 11 | PaymentOperation | Native + non-native payments |
| 12 | PathPaymentStrictReceive + StrictSend | Path payments with real orderbook |
| 13 | ManageSellOffer + ManageBuyOffer | DEX operations |
| 14 | ChangeTrust + SetOptions | Trustlines, signers, thresholds, flags |
| 15 | Soroban: InvokeHostFunction, ExtendFootprint, RestoreFootprint | Smart contract lifecycle |
| 16 | RPC full flow | GetHealth → GetAccount → Simulate → Send → GetTx → GetLedgerEntries → GetEvents |
| 17 | SSE streaming | Real Horizon event parsing on ≥1 endpoint |

**Priority 2 — SHOULD tests (stretch, overflow → Q3):**

- Remaining Horizon queries: Assets, ClaimableBalances, Effects, Ledgers, Offers,
  OrderBook, Trades, TradeAggregations, FeeStats, LiquidityPools, Paths, Health
- Remaining operations: AccountMerge, ManageData, BumpSequence,
  CreatePassiveSellOffer, ClaimableBalance CRUD, Sponsoring (begin/end/revoke),
  Clawback, SetTrustlineFlags, LiquidityPool deposit/withdraw
- Remaining RPC: GetTransactions, GetLedgers, GetVersionInfo, GetFeeStats
- SEP-1 (real domain), SEP-10 full auth, multi-op transactions, Federation

### D3 — Multi-Platform Preparation: Multi-Target + .NET Modernization (42h)

Prework for Q3's MAUI validation and Wallet SDK.

**Part A — Multi-target: net10.0 + net8.0 + netstandard2.1**

- Crypto abstraction: `ICryptoProvider` interface, `NSecCryptoProvider` (net8.0+),
  `SodiumCoreCryptoProvider` (netstandard2.1). Validated with RFC 8032 Ed25519
  test vectors.
- Compiler polyfills: `IsExternalInit`, `RequiredMemberAttribute`,
  `CompilerFeatureRequiredAttribute`
- Conditional compilation: `DateOnly` (4), `ThrowIfNull` (11),
  `ThrowIfNullOrEmpty` (20), `ReadOnlySpan<T>` (3), Index/Range (4)
- `System.Text.Json` NuGet ref for netstandard2.1

**Part B — .NET 7–10 Modernization**

- `FrozenDictionary` for static lookup tables (~47% faster reads)
- `Stream.ReadExactly()` in XDR decoding (eliminates partial-read bugs)
- `AllowDuplicateProperties = false` (prevents silent data corruption)
- `RespectNullableAnnotations` (catches malformed API responses)
- `JsonSerializerOptions.MakeReadOnly()` (explicit freeze)
- Free JIT wins from net10.0: stack alloc, array devirtualization, PGO

### D4 — SEP-45 + SEP Compatibility Matrices (27h)

SEP-45 implementation (Contract WebAuth) + 6 per-SEP compatibility matrices
(SEP-1, 6, 9, 10, 24, 45). Closes the most visible gap vs all peer SDKs.

### D5 — Developer Support (16h)

Issue triage, Discord (~1.5h/week). Overflow → D2.

### D6 — Capacity Buffer (15h)

Absorbs unexpected complexity or upstream changes. Overflow → D2 SHOULD tests.

### D7 — Release & Verification (10h)

| Deliverable | Hours |
|---|---|
| D1 — Protocol 26 | 25h |
| D2 — Integration tests | 65h |
| D3 — Multi-target + modernization | 42h |
| D4 — SEP-45 + matrices | 27h |
| D5 — Developer support | 16h |
| D6 — Capacity buffer | 15h |
| D7 — Release & verification | 10h |
| **Total** | **200h** |

**After Q2:** 6 SEPs, multi-target ships, integration tests gate releases

---

## Q3 2026 — Modernization & SEP Expansion → MAUI Ready (200h)

Focus: modernize SDK internals, reach SEP parity with peers, validate MAUI.

### D1 — JSON Modernization (40h)

| Change | Effort |
|--------|--------|
| `JsonNamingPolicy.SnakeCaseLower` — remove ~900 `[JsonPropertyName]` attributes | 14h |
| `[JsonDerivedType]` — replace hand-written EffectResponse/OperationResponse converters | 12h |
| `JsonStringEnumMemberNameAttribute` — replace custom enum converters | 4h |
| `FrozenDictionary` in remaining spots | 2h |
| `AllowDuplicateProperties`, `RespectNullableAnnotations` fine-tuning | 3h |
| Validation & regression testing | 5h |

### D2 — SEP Expansion: 7 new SEPs (55h)

| SEP | Name | Effort | Why |
|-----|------|--------|-----|
| SEP-7 | URI Scheme | 8h | Wallet SDK prereq, Flutter/iOS have it |
| SEP-12 | KYC API | 10h | Wallet SDK prereq, Flutter/iOS have it |
| SEP-30 | Account Recovery | 10h | Wallet SDK prereq, Flutter/iOS have it |
| SEP-38 | Quotes/RFQ | 8h | Wallet SDK prereq, Flutter/iOS have it |
| SEP-47 | Contract SEP ID | 7h | Flutter/iOS have it |
| SEP-48 | Contract Events | 7h | Flutter/iOS have it |
| SEP-53 | Message Signing | 5h | Flutter/iOS planning it |

All with unit tests and per-SEP compatibility matrices.

### D3 — SseParser Migration (15h)

Replace LaunchDarkly.EventSource with built-in .NET 9 `SseParser` on
net10.0/net8.0 targets. One fewer third-party dependency.

### D4 — MAUI Validation (20h)

Validate SDK on iOS simulator + Android emulator via .NET MAUI. Fix
crypto/libsodium, HTTP stack, linker trimming issues. Minimal validation
app — not a full sample yet (that's Q4 with the Wallet SDK).

### D5 — Test Coverage CI + Runtime Modernization + Q2 SHOULD Overflow (15h)

- Test coverage tracking in CI (Coverlet + GitHub Actions)
- `Base64Url` for SEP-10 JWT handling
- Regex source generator for validation
- Complete remaining Q2 SHOULD integration tests

### D6 — Protocol Evolution (15h)

Protocol 27 may land Q3/Q4 — capacity reserved.

### D7 — Developer Support (25h)

Issue triage, Discord (~2h/week).

### D8 — Buffer + Release (15h)

| Deliverable | Hours |
|---|---|
| D1 — JSON modernization | 40h |
| D2 — SEP-7/12/30/38/47/48/53 | 55h |
| D3 — SseParser migration | 15h |
| D4 — MAUI validation | 20h |
| D5 — Test coverage CI + modernization + Q2 overflow | 15h |
| D6 — Protocol evolution | 15h |
| D7 — Developer support | 25h |
| D8 — Buffer + release | 15h |
| **Total** | **200h** |

**After Q3:** 13 SEPs, JSON modernized, MAUI validated, SseParser on modern targets

---

## Q4 2026 — App Developer Platform: MAUI → Unity → Tizen (200h)

Focus: ship the Wallet SDK, sample apps, validate Unity and Tizen.

### D1 — .NET Wallet SDK (50h)

New `StellarDotnetSdk.Wallet` NuGet package following
[stellar_wallet_flutter_sdk](https://github.com/Soneso/stellar_wallet_flutter_sdk)
architecture.

**Modules:**
- `Wallet.Stellar()` — Account creation (incl. sponsored), trustlines,
  fee-bump, transaction submission with retry
- `Wallet.Anchor(homeDomain)` — SEP-1 → SEP-10 → SEP-12 → SEP-6/24
  orchestrated into simple method chains
- `Wallet.Anchor().Sep38()` — Exchange quotes and cross-asset pricing
- `Wallet.Recovery()` — SEP-30 multi-server account recovery
- `Wallet.ParseSep7Uri()` — SEP-7 URI scheme for delegated signing

```
┌─────────────────────────────┐
│  StellarDotnetSdk.Wallet    │  ← NEW
│  (workflow orchestration)   │
├─────────────────────────────┤
│  StellarDotnetSdk           │  ← existing
│  (Horizon + RPC + SEPs)     │
├─────────────────────────────┤
│  StellarDotnetSdk.Xdr       │  ← existing
└─────────────────────────────┘
```

### D2 — MAUI Sample App (20h)

Full MAUI demo using Wallet SDK: create wallet, fund via Friendbot,
authenticate with anchor, deposit, check balance, send payment.

### D3 — Unity Validation + Sample (35h)

Validate netstandard2.1 in Unity 2022.3 LTS and Unity 6. Fix IL2CPP/AOT,
managed stripping, libsodium native plugins. Sample project in
`Examples/Unity/`.

### D4 — Native AOT Readiness (20h)

`<IsAotCompatible>true</IsAotCompatible>`, `JsonSerializerContext` source
generation, per-enum `JsonStringEnumConverter<T>`.

### D5 — Tizen Validation (10h)

Validate netstandard2.1 on Tizen 5.5+. Compatibility report. Determines
Q1 2027 investment.

### D6 — Protocol Evolution (15h)

Protocol 27 expected in this window.

### D7 — Developer Support (25h)

Issue triage, Discord (~2h/week).

### D8 — .NET 8 EOL Prep + Buffer + Release (25h)

Migration guide for consumers. Evaluate dropping net8.0 target.

| Deliverable | Hours |
|---|---|
| D1 — .NET Wallet SDK | 50h |
| D2 — MAUI sample app | 20h |
| D3 — Unity validation + sample | 35h |
| D4 — Native AOT readiness | 20h |
| D5 — Tizen validation | 10h |
| D6 — Protocol evolution | 15h |
| D7 — Developer support | 25h |
| D8 — .NET 8 EOL + buffer + release | 25h |
| **Total** | **200h** |

**After Q4:** 3 NuGet packages, MAUI app, Unity sample, Tizen validated

---

## Q1 2027 — Maturity & Ecosystem (200h)

Focus: harden everything shipped in 2026, fill remaining gaps.

### D1 — Wallet SDK Hardening (30h)

Production feedback, additional anchor integration tests, error handling
improvements, edge case fixes from real-world usage.

### D2 — Tizen Sample App (25h)

If Q4 validation passed, build full Tizen sample (Samsung TV/wearable demo).

### D3 — JSON Source Generation Completion (25h)

Complete `JsonSerializerContext` for all types, full AOT trimming validation,
remove reflection-based JSON fallback on net10.0 target.

### D4 — Performance Benchmarks (20h)

BenchmarkDotNet suite, `SearchValues<T>`, `IUtf8SpanFormattable`,
`params ReadOnlySpan<T>`, `Utf8JsonReader.CopyString()` in hot paths.

### D5 — SEP Catch-Up (20h)

Evaluate any new SEPs shipped by peer SDKs in Q3/Q4, implement as needed.

### D6 — XML Documentation Completion (25h)

382 → 0 missing items. Enforce `CS1591` as error. Full IntelliSense coverage.

### D7 — Drop net8.0 Target (10h)

EOL Nov 2026. Evaluate and execute if safe. Simplifies to net10.0 +
netstandard2.1.

### D8 — Protocol Evolution + Developer Support + Release (45h)

| Deliverable | Hours |
|---|---|
| D1 — Wallet SDK hardening | 30h |
| D2 — Tizen sample app | 25h |
| D3 — JSON source gen completion | 25h |
| D4 — Performance benchmarks | 20h |
| D5 — SEP catch-up | 20h |
| D6 — XML docs completion | 25h |
| D7 — Drop net8.0 target | 10h |
| D8 — Protocol + support + release | 45h |
| **Total** | **200h** |

---

## Full Timeline

| Quarter | Theme | Hours | SEPs | Platforms | Packages |
|---------|-------|-------|------|-----------|----------|
| Q2 2026 | Reliability & Protocol | 200h | 5→6 | multi-target ships | 2 |
| Q3 2026 | Modernization & SEPs | 200h | 6→13 | MAUI validated | 2 |
| Q4 2026 | App Developer Platform | 200h | +Wallet SDK | MAUI app, Unity, Tizen validated | 3 |
| Q1 2027 | Maturity & Ecosystem | 200h | catch-up | Tizen app, all hardened | 3 |
| **Total** | | **800h** | | | |

## End State (March 2027)

- **Packages:** `StellarDotnetSdk` + `StellarDotnetSdk.Xdr` + `StellarDotnetSdk.Wallet`
- **Targets:** net10.0 + netstandard2.1 (net8.0 dropped after EOL)
- **Platforms:** Backend (.NET 10), MAUI (iOS/Android), Unity (2022.3+/6), Tizen (5.5+)
- **SEPs:** 13+ in base SDK + Wallet SDK orchestration layer
- **Quality:** Integration tests (MUST+SHOULD) gate releases, test coverage in CI,
  Native AOT compatible, benchmarked
- **JSON:** SnakeCaseLower, source-generated, strict validation, no hand-written
  polymorphic converters
- **SSE:** Built-in SseParser (no third-party dependency on modern targets)
- **Docs:** 100% XML doc coverage, full SEP compatibility matrices
- **Competitive:** Feature parity with Flutter/iOS SDKs on SEPs, plus unique
  multi-platform reach (backend + mobile + games + TV)

---

## Risk Register

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|-----------|
| Protocol 26 undocumented breaking changes | Medium | High | Early Testnet testing; 15h Q2 buffer |
| NSec ↔ Sodium.Core Ed25519 differences | Medium | High | RFC 8032 test vectors; FsCheck property tests |
| Wallet SDK scope creep | Medium | High | Follow Flutter SDK architecture strictly; SEPs done in Q3 |
| MAUI libsodium fails on iOS AOT | Medium | Medium | Managed crypto fallback; validate in Q3 before Wallet SDK |
| Unity IL2CPP strips required types | Medium | Medium | link.xml; test IL2CPP early in Q4 |
| Protocol 27 lands unexpectedly | Low | Medium | 15h reserved Q3/Q4 |
| SnakeCaseLower mismatch with Horizon fields | Low | Medium | Audit all 900 attributes; keep exceptions |
| System.Linq.AsyncEnumerable conflict on net10.0 | High | Low | Remove System.Linq.Async NuGet |

---

## Appendix A: Crypto Abstraction Design (Q2 D3)

NSec.Cryptography targets net8.0+ only. netstandard2.1 needs an alternative.

1. Define `ICryptoProvider` with: `Sign`, `Verify`, `GenerateKeypair`, `DeriveSharedSecret`
2. `NSecCryptoProvider` — default on net8.0+ (existing behavior)
3. `SodiumCoreCryptoProvider` — for netstandard2.1 via
   [Sodium.Core](https://www.nuget.org/packages/Sodium.Core) (targets netstandard2.0)
4. Auto-select via `#if NETSTANDARD`
5. Validate with [RFC 8032 test vectors](https://datatracker.ietf.org/doc/html/rfc8032#section-7.1)

## Appendix B: Breaking Changes (net8.0 → net10.0)

| Change | Action | Source |
|--------|--------|--------|
| JSON property name conflict checking | Full test suite on net10.0 | [docs](https://learn.microsoft.com/en-us/dotnet/core/compatibility/serialization/10/property-name-validation) |
| `System.Linq.AsyncEnumerable` in core | Remove System.Linq.Async NuGet | [docs](https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/10.0/asyncenumerable) |
| C# 14 span overload resolution | Review compiler warnings | [docs](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14#implicit-span-conversions) |
| OpenSSL 1.1.1+ required on Unix | Document in release notes | [docs](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10) |

## Appendix C: Quarterly Refresh Process

At the start of each quarter, open a fresh conversation and:

1. Review protocol timeline — any new upgrades announced?
2. Check peer SDK releases — any new SEPs or features to match?
3. Review grant reviewer feedback from the previous quarter
4. Assess what shipped vs what slipped — adjust the current quarter
5. Update this spec with revised deliverables and hour allocations
