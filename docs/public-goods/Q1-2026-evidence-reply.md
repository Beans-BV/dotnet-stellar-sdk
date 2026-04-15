# Q1 '26 Deliverables — Evidence Package

**Submission:** SCF Public Goods Q1 '26 — .NET SDK
**Repo:** [`Beans-BV/dotnet-stellar-sdk`](https://github.com/Beans-BV/dotnet-stellar-sdk)
**Verification target:** `main` @ [`3e1e1917`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/3e1e1917) · CI: green ([run 24325675906](https://github.com/Beans-BV/dotnet-stellar-sdk/actions/runs/24325675906))

---

## 0. One-command full verification

Every claim below is reproducible in ~1 minute:

```bash
git clone https://github.com/Beans-BV/dotnet-stellar-sdk.git
cd dotnet-stellar-sdk

# Deliverable 1 — XML doc gate (expect 0 CS1591)
dotnet build StellarDotnetSdk/StellarDotnetSdk.csproj -c Release 2>&1 \
  | tee build.log | grep -c "CS1591"

# Deliverable 6 — Test suite (expect 1663 passed, 0 failed)
dotnet test -c Release --nologo 2>&1 | tail -3

# Deliverables 2 & 3 — Matrix coverage
grep "Coverage:" StellarDotnetSdk/Compatibility/horizon_matrix.md
grep "Coverage:" StellarDotnetSdk/Compatibility/rpc_matrix.md
```

Expected output verbatim:

```
0                                          # CS1591 (missing public XML doc) count
Passed!  - Failed:     0, Passed:  1663    # test result
**Coverage:** 100.0% (50/50 fully supported)   # Horizon
**Coverage:** 100%                             # Stellar RPC
```

---

## 1. Deliverable-by-deliverable evidence

### Deliverable 1 — Public API XML doc debt: 383 → 0

| Metric | Value | Proof |
|---|---|---|
| Baseline | 383 missing items | `missing-docs.txt` at [`b09739d9^`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/b09739d9) contained 382 enumerated items · `HealthRequestBuilder` (+1) resolved in [#147](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/147) = **383** |
| Current | 0 missing | `dotnet build -c Release` on `main` → **0 × CS1591, 0 errors** (verified locally and in CI) |
| Gate | Permanent | [`StellarDotnetSdk.csproj`](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/main/StellarDotnetSdk/StellarDotnetSdk.csproj) sets `<WarningsAsErrors>CS1591</WarningsAsErrors>` — any regression fails the build |
| CI enforcement | Every push & PR | [`.github/workflows/pack_and_test.yml`](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/main/.github/workflows/pack_and_test.yml) runs `dotnet pack -c Release` + `dotnet test -c Release` |

**Closing issue:** [#136 — Eliminate public API XML doc debt](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/136) (closed).

**Delivery PRs:**

| PR | Commit | Magnitude |
|---|---|---|
| [#141](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/141) update public API XML docs | [`34cadad5`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/34cadad5) | **315 files changed, +4,846 / −10** |
| [#144](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/144) added missing docs | [`04145188`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/04145188) | **194 files changed, +1,450 / −6** |
| [#147](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/147) missing XML docs for `HealthRequestBuilder` | [`043d30ea`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/043d30ea) | 1 file, +5 |
| [`b09739d9`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/b09739d9) cleanup missing docs text file | — | −382 lines (tracker retired) |

> Note on remaining warnings: the build emits ~170 non-CS1591 warnings (CS1572/1573 param-tag mismatches, CS8618 nullable init, CS1574 cref resolution). These are **doc-quality and nullable-hygiene** items, not missing documentation. Every public member has a summary — the CS1591 contract, which is what the 383-item backlog tracked, is at zero. Remaining items are being worked off continuously via the Deliverable 1 capacity buffer.

---

### Deliverable 2 — Horizon matrix parity: 92.0% → 100%

| Metric | Baseline | Current |
|---|---|---|
| Coverage | **92.0% (46/50)** | **100.0% (50/50)** |
| Fully supported | 46 | 50 |
| Partially supported | 1 | 0 |
| Not supported | 3 | 0 |

**Baseline verification** (byte-for-byte extract from the pre-parity matrix):

```bash
git show 34cadad5^:StellarDotnetSdk/Compatibility/horizon_matrix.md | head -25
# → Coverage: 92.0% (46/50 fully supported)
# → Partially Supported: 1/50 · Not Supported: 3/50
```

**Closing issue:** [#137 — Complete Horizon matrix parity](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/137) (closed).

**Delivery PR:** [#139 — chore: complete Horizon matrix parity](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/139), commit [`02bcf4a1`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/02bcf4a1).

**Current matrix on `main`:** [`horizon_matrix.md`](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/main/StellarDotnetSdk/Compatibility/horizon_matrix.md), pinned to Horizon v25.0.0.

**Every item called out in the original submission, now ✅:**

| Item | Status | SDK surface |
|---|---|---|
| `GET /health` | ✅ Fully Supported | `Server.Health.Execute()` via `HealthRequestBuilder` |
| `GET /liquidity_pools/{id}/effects` | ✅ Fully Supported (streamable) | `Server.Effects.ForLiquidityPool(id).Execute()` |
| `GET /liquidity_pools/{id}/operations` | ✅ Fully Supported (streamable) | `Server.Operations.ForLiquidityPool(id).Execute()` |
| `GET /liquidity_pools/{id}/trades` | ✅ Fully Supported | `Server.Trades.ForLiquidityPool(id).Execute()` |
| `GET /liquidity_pools/{id}/transactions` | ✅ Fully Supported (streamable) | `Server.Transactions.ForLiquidityPool(id).Execute()` |
| `GET /offers/{offer_id}/trades` | ✅ Fully Supported | `Server.Trades.ForOffer(offerId).Execute()` |

---

### Deliverable 3 — Stellar RPC matrix parity: 83.3% → 100%

| Metric | Baseline | Current |
|---|---|---|
| Coverage | **83.3% (10/12)** | **100% (12/12)** |
| Fully supported | 10 | 12 |
| Partially supported | 1 | 0 |
| Not supported | 1 | 0 |
| Methods with missing response fields | 2 | 0 |

**Baseline verification:**

```bash
git show 34cadad5^:StellarDotnetSdk/Compatibility/rpc_matrix.md | head -15
# → Coverage: 83.3%
# → Fully Supported: 10/12 · Partially Supported: 1/12 · Not Supported: 1/12
```

**Closing issue:** [#138 — Complete Soroban RPC matrix parity](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/138) (closed).

**Delivery PR:** [#140 — chore: complete RPC matrix parity](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/140), commit [`84492ecf`](https://github.com/Beans-BV/dotnet-stellar-sdk/commit/84492ecf). Also: [#143](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/143) aligned naming with the upstream `Soroban RPC → Stellar RPC` rename.

**Current matrix on `main`:** [`rpc_matrix.md`](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/main/StellarDotnetSdk/Compatibility/rpc_matrix.md), pinned to Stellar RPC v25.0.0. Response-field table shows `Missing Fields: -` for every method.

**Items called out in the original submission, now ✅:**

| Item | Status |
|---|---|
| `getLedgers` | ✅ 1/1 params, 6/6 response fields (previously absent) |
| `getTransactions` — `cursor` response field | ✅ 6/6 fields (was 5/6) |

**Demo snippet using the new surface:**

```csharp
var server = new SorobanServer("https://soroban-testnet.stellar.org");
var latest = await server.GetLatestLedger();
var ledgers = await server.GetLedgers(startLedger: latest.Sequence - 10);
// ledgers.Ledgers, ledgers.LatestLedger, ledgers.LatestLedgerCloseTime, ledgers.Cursor
```

Extended Soroban invocation examples: [PR #150 — chore: extend example app](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/150).

---

### Deliverable 4 — Continuous compatibility with protocol evolution

| Criterion | Status |
|---|---|
| Horizon target | v25.0.0 — fully tracked |
| Stellar RPC target | v25.0.0 — fully tracked |
| Unsupported breaking upstream changes | **0** |
| Upstream terminology alignment | Soroban RPC → Stellar RPC rename done in [#143](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/143) |

Both matrix files carry an explicit `**Horizon Version:** v25.0.0` / `**RPC Version:** v25.0.0` pin at the top, making upstream drift detection mechanical.

---

### Deliverable 5 — Developer support & maintenance responsiveness

Operational metrics across the Q1 '26 engagement window (reproducible via `gh` CLI):

| Metric | Count |
|---|---|
| Commits on `main` | **18** |
| PRs merged | **15** |
| Issues closed | **6** |
| Goal-closing issues | 5 ([#136](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/136), [#137](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/137), [#138](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/138), [#157](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/157), [#158](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/158), [#163](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/163)) |
| Bug issues closed | 1 ([#135](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/135)) |
| Releases shipped | **1** (v15.0.0) |
| Author split | cuongph87: 12 commits · jopmiddelkamp: 6 commits |

**Continuity backlog already scoped for next period:** [#157 Multi-Platform Preparation](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/157), [#158 SEP-45 + SEP compatibility matrices](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/158), [#163 .NET 7–10 modernization](https://github.com/Beans-BV/dotnet-stellar-sdk/issues/163).

---

### Deliverable 6 — Release & verification: v15.0.0

| Criterion | Evidence |
|---|---|
| Release tagged | [v15.0.0](https://github.com/Beans-BV/dotnet-stellar-sdk/releases/tag/15.0.0) |
| NuGet published | `StellarDotnetSdk` 15.0.0 via [`publish_nuget.yml`](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/main/.github/workflows/publish_nuget.yml) |
| Test suite | **1,663 passed / 0 failed / 0 skipped** (verified locally on `main`) |
| Matrices updated in-tree | Yes — [`horizon_matrix.md`](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/main/StellarDotnetSdk/Compatibility/horizon_matrix.md), [`rpc_matrix.md`](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/main/StellarDotnetSdk/Compatibility/rpc_matrix.md) |
| Parity PRs in release | [#139](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/139), [#140](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/140), [#141](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/141), [#143](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/143), [#144](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/144), [#147](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/147) |

**Full test-run evidence (reproducible in 10 seconds):**

```
$ dotnet test -c Release --nologo
...
Passed!  - Failed:     0, Passed:  1663, Skipped:     0, Total:  1663, Duration: 4 s
```

---

## 2. Cross-reference: Anke's review gaps → evidence

| Gap raised in review | Resolved by |
|---|---|
| "No quantitative proof Horizon 92.0% → 100%" | Section 1.2, baseline extract from `34cadad5^` vs current [`horizon_matrix.md`](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/main/StellarDotnetSdk/Compatibility/horizon_matrix.md) |
| "No quantitative proof RPC 83.3% → 100%" | Section 1.3, baseline extract from `34cadad5^` vs current [`rpc_matrix.md`](https://github.com/Beans-BV/dotnet-stellar-sdk/blob/main/StellarDotnetSdk/Compatibility/rpc_matrix.md) |
| "No CI log / badge / count showing 383 → 0" | Section 1.1, `dotnet build -c Release` returns **0 CS1591 warnings**, gate enforced via `WarningsAsErrors=CS1591` |
| "No PR links for /health, liquidity-pool subresources, offer-trades" | Section 1.2, PR [#139](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/139) |
| "No PR links for getLedgers, cursor field" | Section 1.3, PR [#140](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/140) |
| "No automated test evidence" | Section 1.6, **1,663 tests pass** in CI and locally |
| "No sample/snippet for Soroban new RPC" | Section 1.3, `GetLedgers` snippet + [example app #150](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/150) |
| "No v15.0.0 release notes summary" | Section 1.6, [Release 15.0.0](https://github.com/Beans-BV/dotnet-stellar-sdk/releases/tag/15.0.0) with explicit parity PR list |
| "No operational metrics (issues / PRs / releases)" | Section 1.5, reproducible via `gh` CLI commands |
| "No continuous-compatibility evidence" | Section 1.4, upstream v25.0.0 pins + upstream rename tracked in [#143](https://github.com/Beans-BV/dotnet-stellar-sdk/pull/143) |

---

## 3. Known non-gaps (pre-empting follow-ups)

- **Non-CS1591 build warnings (~170).** These are doc-quality (CS1572/1573) and nullable-hygiene (CS8618) warnings, not missing-doc warnings. The 383-item backlog tracked missing documentation specifically (`CS1591`); that count is now zero and gated. Other warning families are continuously reduced as part of the Deliverable 1 capacity buffer.
- **No per-endpoint live-network contract tests.** The matrix is hand-curated and validated by the 1,663-test unit suite plus manual release verification against testnet. A generated contract-test harness against Horizon/RPC is a candidate scope item for a future period; it wasn't part of Q1 '26 deliverables.
- **No NuGet download telemetry inline.** nuget.org publishes these; can be linked on request.
