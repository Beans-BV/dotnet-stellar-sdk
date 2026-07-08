# Stellar .NET SDK — Roadmap & Backlog

**Priority:** .NET Backend APIs → MAUI + Wallet SDK → Unity → Tizen

---

## How this document works

This is an ordered backlog, not a set of pre-allocated quarter plans. The top
of the list is the next thing to pick up.

- **At each quarter start** (process in Appendix C): reserve the recurring
  overhead below, estimate hours for the items at the top of the backlog, and
  pull them top-down into the quarter's public-goods submission until the
  quarterly budget is full. The submission (`docs/public-goods/<quarter>.md`)
  is the authoritative commitment for that quarter and is where hours get
  calculated; this file is the queue behind it and carries no hour estimates.
- **New work** (bugs, protocol updates, peer-SDK moves, new ideas) is inserted
  at whatever position is logical. Nothing downstream needs recalculating.
- Items list their prerequisites; an item is only pickable when those are done.

## Recurring per-quarter overhead (reserved before picking)

- Developer support & maintenance (triage, Discord)
- Release & verification (changelogs, matrices, NuGet tags, gating suites)
- Capacity buffer (quarter-specific risks; unused buffer pulls the next
  backlog item forward)

Continuous maintenance (protocol updates, bug fixes, SEP upkeep, CI) lives in
these buckets — it is never a backlog item.

---

## Backlog (ordered by pickup)

### App developer platform

1. **SEP-30: Account Recovery + matrix**
   Before the Wallet SDK recovery module that consumes it. Thinnest ecosystem
   adoption of the wallet-parity SEPs, which is why it sits behind the others.
2. **.NET Wallet SDK — `StellarDotnetSdk.Wallet`**
   New NuGet package following the
   [stellar_wallet_flutter_sdk](https://github.com/Soneso/stellar_wallet_flutter_sdk)
   architecture: workflow orchestration *on top of* the base SDK's SEP
   implementations (SEPs stay in `StellarDotnetSdk`). Modules:
   `Wallet.Stellar()` (accounts, trustlines, fee-bump, submit-with-retry),
   `Wallet.Anchor(homeDomain)` (SEP-1 → 10 → 12 → 6/24 chained),
   `Wallet.Anchor().Sep38()`, `Wallet.Recovery()` (SEP-30 multi-server),
   `Wallet.ParseSep7Uri()`. Encodes the non-idempotency rule: no Horizon retry
   policies on SEP endpoints.
   *Prereqs: SEP-7/12/38/30, MAUI validation.*
3. **MAUI sample app**
   Full demo on the Wallet SDK: create wallet, fund via Friendbot, anchor
   auth, deposit, balance, payment. *Prereqs: Wallet SDK, MAUI environment.*
4. **JSON modernization → 17.0.0 (breaking)**
   `SnakeCaseLower` naming policy (~810 of 905 `[JsonPropertyName]` attributes
   removed, ~90 documented exceptions), `[JsonDerivedType]` replaces the two
   polymorphic converters (verified on STJ 9 with out-of-order
   discriminators), `JsonStringEnumMemberNameAttribute` replaces the
   SendTransactionStatus converter (LiquidityPoolType stays: xdrgen-generated
   enum). Migrates any attribute-authored SEP models shipped in the meantime.
   Spike-verified. Sequence *after* additive SEP minors so a regression cannot
   block them. *Prereqs: PR #201 floor decision (lands with 16.0.0).*
5. **Unity validation**
   netstandard2.1 in Unity 2022.3 LTS + Unity 6: IL2CPP/AOT, managed
   stripping, libsodium native plugins (reuses the MAUI crypto-fallback
   decision). Compatibility report only; sample is a separate item.
6. **Test infrastructure: coverage in CI + Priority-2 integration tests**
   Coverlet upgrade, AppVeyor removal, coverage in GitHub Actions; the four
   uncovered RPC methods (`GetTransactions`, `GetLedgers`, `GetVersionInfo`,
   `GetFeeStats`). Remaining Priority-2 areas stay on the overflow list below.

### Maturity & ecosystem

7. **Wallet SDK hardening** — production feedback, anchor integration
   tests, error-handling and edge-case fixes. *Prereq: Wallet SDK shipped one
   quarter earlier.*
8. **Native AOT + JSON source generation** —
   `<IsAotCompatible>true</IsAotCompatible>`, `JsonSerializerContext`
   (simplified by the attribute-free 17.0.0 models), per-enum
   `JsonStringEnumConverter<T>`, full trimming validation. *Prereq: JSON
   modernization.*
9. **Tizen validation + sample** — netstandard2.1 on Tizen 5.5+;
   sample only if validation passes.
10. **Unity sample** — `Examples/Unity/`. *Prereq: Unity validation.*
11. **SEP catch-up: SEP-47, SEP-48, SEP-53 + new** — the deferred
    peer-parity SEPs (SEP-53 Final since 2026-06-18, unshipped by peers) plus
    anything peers ship in the meantime.
12. **SseParser migration** — replace LaunchDarkly.EventSource with
    built-in `SseParser` on net10.0/net8.0.
13. **XML documentation completion** — 382 → 0 missing items, `CS1591`
    as error.
14. **Drop net8.0 target** — EOL Nov 2026; evaluate and execute if
    safe. Simplifies to net10.0 + netstandard2.1.
15. **OpenZeppelin smart-account desk check** (decision gate)
    OZ contract audit/release status, .NET WebAuthn binding maturity on MAUI
    (ASAuthorization / CredentialManager), relayer + indexer availability.
    Decides whether item 16 gets committed — same pattern as the Q3 libsodium
    desk check.

### Gated / stretch (picked only when the gate passes or capacity allows)

16. **OpenZeppelin smart account support** *— gated on item 15*
    Passkey-based contract wallets (peer precedent: Soneso grant deliverable):
    wallet lifecycle with WebAuthn passkey registration, context rules and
    policies with configurable signers, token ops and contract calls with
    automatic auth-entry signing, multi-signer auth (passkey / delegated
    Stellar account / Ed25519), fee sponsoring via relayer proxy, credential
    discovery via indexer, platform WebAuthn through MAUI bindings with secure
    storage adapters (browser via Blazor/WASM evaluated separately),
    cross-platform demo, docs, test suite.
    *Prereqs: Wallet SDK, MAUI sample; SEP-45 already shipped. Larger than a
    single quarter — split across quarters when committed.*
17. **SBOM generation + supply-chain workflow in CI** (small; peer SDKs ship this)
18. **Public SDK usage/statistics dashboard** (peer precedent: soneso-sdk-stats)
19. **Performance benchmark suite** (BenchmarkDotNet, `SearchValues<T>`,
    hot-path tuning)
20. **Priority-2 integration-test overflow list** — remaining Horizon queries
    (Assets, ClaimableBalances, Effects, Ledgers, Offers, OrderBook, Trades,
    TradeAggregations, FeeStats, LiquidityPools, Paths, Health), remaining
    operations (AccountMerge, ManageData, BumpSequence,
    CreatePassiveSellOffer, ClaimableBalance CRUD, Sponsoring, Clawback,
    SetTrustlineFlags, LiquidityPool deposit/withdraw), SEP-1 real domain,
    SEP-10 full auth, multi-op transactions, Federation.

---

## Current State (July 2026)

- **Target:** net10.0 + net8.0 + netstandard2.1 (merged, PR #195; published as 16.0.0-beta, stable pending)
- **Version:** 15.1.0 stable / 16.0.0-beta on NuGet; 16.0.0 stable ships early July as Q2 carry-over
- **Coverage:** Horizon 100% (50/50), Stellar RPC 100% (12/12)
- **SEPs:** 6 implemented (SEP-1, 6, 9, 10, 24, 45)
- **Tests:** ~1,930 unit test cases per target framework + live-Testnet integration suite (17 Priority-1 areas, gating releases)
- **Protocol:** Protocol 27 (CAP-71) support merged and KAT-verified; Mainnet vote July 8
- **XML docs:** 382 missing items
- **JSON:** Reflection-based System.Text.Json, 905 manual `[JsonPropertyName]` attributes, hand-written polymorphic converters (modernization spike-verified, backlog #4)
- **SSE:** Third-party LaunchDarkly.EventSource dependency (backlog #12)

### SEP Parity vs Peer SDKs

| SEP | Flutter | iOS | Java | .NET (us) |
|-----|---------|-----|------|-----------|
| SEP-1 (TOML) | ✅ | ✅ | ✅ | ✅ |
| SEP-6 (Deposit/Withdraw) | ✅ | ✅ | ✅ | ✅ |
| SEP-7 (URI Scheme) | ✅ | ✅ | — | committed (Q3) |
| SEP-9 (KYC Fields) | ✅ | ✅ | ✅ | ✅ |
| SEP-10 (Web Auth) | ✅ | ✅ | ✅ | ✅ |
| SEP-12 (KYC API) | ✅ | ✅ | — | committed (Q3) |
| SEP-24 (Interactive) | ✅ | ✅ | ✅ | ✅ |
| SEP-30 (Recovery) | ✅ | ✅ | — | backlog #1 |
| SEP-38 (Quotes) | ✅ | ✅ | — | committed (Q3, overflow candidate) |
| SEP-45 (Contract WebAuth) | ✅ | ✅ | ✅ | ✅ |
| SEP-47 (Contract ID) | ✅ | ✅ | — | backlog #11 |
| SEP-48 (Contract Events) | ✅ | ✅ | — | backlog #11 |
| SEP-53 (Message Signing) | ❌ | ❌ | — | backlog #11 (Final 2026-06-18, unshipped by peers) |

### Protocol Timeline

| Date | Event |
|------|-------|
| April 16, 2026 | Protocol 26 "Yardstick" — Testnet upgrade (shipped) |
| May 6, 2026 | Protocol 26 — Mainnet vote (shipped) |
| June 17, 2026 | Testnet reset (handled by integration suite re-provisioning) |
| July 8, 2026 | Protocol 27 "Zipper" — Mainnet vote (CAP-71 support already merged) |
| Nov 2026 | .NET 8 end of support |

Sources: [Protocol 26 Guide](https://stellar.org/blog/foundation-news/stellar-yardstick-protocol-26-upgrade-guide), [.NET Support Policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core), [Stellar Networks](https://developers.stellar.org/docs/networks)

---

## Risk Register

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|-----------|
| 16.0.0 carry-over slips past mid-July | Low | Medium | Squeezes Q3 calendar (MAUI validation), not budget; feature code merged, close-out only |
| MAUI libsodium fails on iOS/Android | Medium | High | Week-1 desk check decides fallback (build libsodium vs managed Ed25519) before dependent work; quarter buffer |
| Physical-iOS (AOT, no JIT) unvalidated if provisioning blocks | Medium | Medium | Compatibility report states residual risk explicitly; revisited with the MAUI sample app |
| NSec ↔ Sodium.Core Ed25519 differences | Medium | High | RFC 8032 test vectors; FsCheck property tests |
| Wallet SDK scope creep | Medium | High | Follow Flutter SDK architecture strictly; wallet-parity SEPs completed before pickup |
| Backlog density around the Wallet SDK (items 1–6 exceed one quarter) | Medium | Medium | Backlog order is the priority order; test infra (item 6) is the natural flex item |
| Unity IL2CPP strips required types | Medium | Medium | link.xml; test IL2CPP early when item 5 is picked |
| SnakeCaseLower mismatch with Horizon fields | Low | Medium | Spike-verified: 813/905 attributes match the real naming policy; ~90 documented exceptions kept |
| Testnet reset / SDF test-anchor change stalls integration-gated releases | Medium | Medium | Friendbot re-provisioning; quarter buffer absorbs delay |
| Protocol 26 undocumented breaking changes | Resolved | — | Shipped in Q2 |
| Protocol 27 lands unexpectedly | Resolved | — | Implemented ahead of schedule (PR #187); Mainnet vote July 8 tracked in Q2 carry-over |
| System.Linq.AsyncEnumerable conflict on net10.0 | Resolved | — | Handled in Q2 multi-target work |

---

## Appendix A: Crypto Abstraction Design (shipped in Q2)

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
4. Move what shipped to **Shipped**; roll what slipped back to the top of the backlog
5. Re-order the backlog if priorities changed; insert any new items
6. Estimate hours for the items about to be picked (hours live in the
   quarterly submission, not here)
7. Reserve the recurring overhead, pull items top-down into the new
   `docs/public-goods/<quarter>.md`, and update **Now — committed**

**Refresh log:**
- 2026-07-08 (Q3): Q2 delivered; 16.0.0 close-out runs as Q2 carry-over. Q3
  committed to SEP-7/12/38 + MAUI validation at realistic per-item costs;
  JSON modernization, SEP-30, test infra, SseParser, AOT, Tizen, Unity sample
  pushed down the backlog; benchmarks moved to stretch. OpenZeppelin
  smart-account support added as a gated item behind the Wallet SDK.
- 2026-07-08 (later): converted from per-quarter allocations to this ordered
  backlog; SEP-38 marked as Q3's designated overflow item. Hour estimates
  removed — hours are calculated per quarter in the public-goods submissions.
