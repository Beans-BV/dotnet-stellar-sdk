## dotnet-stellar-sdk – Copilot Repository Instructions

These instructions guide GitHub Copilot Chat, completions, and Copilot code review when working in this repository.  
They are specific to the `stellar-dotnet-sdk` / `dotnet-stellar-sdk` library and should be treated as the single source of truth for automated assistance.

---

## 1. About this repository

- **Purpose**: A production-grade .NET SDK for the Stellar network, targeting both the Horizon API and Soroban RPC.
- **Primary package**: `stellar-dotnet-sdk` (`StellarDotnetSdk` project), versioned using **SemVer** and published to NuGet.
- **Framework & language**: `net8.0`, C# 12, nullable reference types **enabled**.
- **Key external dependencies**: `Polly` for HTTP resilience, cryptography libraries (`NSec.Cryptography`, BIP32/39), and `LaunchDarkly.EventSource` for SSE.

When generating code or reviewing PRs, assume this is a **widely-used public SDK** where **backward compatibility, robustness, and clarity** are more important than micro-optimizations.

---

## 2. Project layout (high level)

- **`StellarDotnetSdk`**
  - Main public SDK surface: accounts, assets, operations, transactions, requests/responses, Soroban support, HTTP resilience, etc.
  - Everything in this project must preserve a clean and coherent public API.
- **`StellarDotnetSdk.Xdr`**
  - Generated XDR model types; tightly coupled to the Stellar protocol definitions.
  - Treat this as **generated infrastructure**: do not introduce hand-written business logic here.
  - Manual edits should be rare and carefully justified.
- **`StellarDotnetSdk.Tests`**
  - Unit and integration tests for the SDK.
  - New behaviors, bug fixes, and regression scenarios should be covered with tests that follow existing patterns.
- **`Examples/Horizon` and `Examples/Soroban`**
  - Small, focused example apps demonstrating real-world usage.
  - Keep examples simple, idiomatic, and aligned with current best practices and documentation.
- **`docs/`**
  - DocFX-based documentation and tutorials.
  - Changes to public behavior or significant internal behavior (e.g., HTTP resilience) must be reflected here.

When Copilot suggests or reviews changes, it should **reuse existing patterns from the appropriate project** instead of inventing new architectures ad hoc.

---

## 3. Coding style and design principles

When generating or reviewing code in this repo, Copilot should assume:

- **C# style**
  - Prefer readability and vertical alignment over extreme conciseness.
  - Use idiomatic C# naming: `PascalCase` for types, methods, and public properties; `camelCase` for locals and parameters.
  - Respect nullable reference types and avoid suppressing nullability unless clearly justified.
  - Prefer pure functions and immutable data where practical; allow mutability when it materially improves clarity.
  - Avoid unnecessary global mutable state and static singletons.
  - Avoid overengineering or micro-optimizing simple, correct code; do not churn between equivalent implementations (e.g., `Math.Pow` vs. bit shifting for small retry counts) without a concrete bug, measurable perf issue, or clarity benefit.

- **Architecture**
  - Maintain a clear separation between:
    - Domain concepts (accounts, assets, operations, XDR types).
    - Transport/infrastructure (HTTP client, resilience, SSE event sources).
    - Examples and tests.
  - Favor dependency injection and explicit configuration over hidden or global configuration.
  - Avoid leaking infrastructure concerns into high-level public APIs unless they are part of the official SDK surface.

- **Error handling & resilience**
  - Treat unexpected exceptions as **non-silent**; they should be either:
    - Propagated, or
    - Translated into well-defined SDK exceptions with clear semantics.
  - Distinguish between **recoverable** and **non-recoverable** errors.
  - Do not swallow exceptions silently, especially in network and transaction paths.

- **Async and cancellation**
  - Prefer async APIs where I/O is involved; avoid blocking calls on async code.
  - Where feasible, expose `CancellationToken` parameters and pass them through to lower layers.
  - Timeouts and retries should be explicit and configurable (see HTTP resilience section).

Copilot suggestions should **match these principles** and Copilot reviews should **call out deviations**.

---

## 4. Semantic Versioning, public API, and compatibility

This SDK is a NuGet package used in many external applications. Copilot must be conservative with public API changes.

- **Versioning**
  - The SDK uses **SemVer** (see `CONTRIBUTING.md`).
  - Breaking changes require:
    - A major version bump, and
    - Clear documentation in `README.md`, release notes, and relevant docs.
- **Public API**
  - Types and members in `StellarDotnetSdk` and `StellarDotnetSdk.Xdr` that are `public` or `protected` should be treated as **user-facing contracts**.
  - Before introducing or approving changes that:
    - Remove public types or members,
    - Change method signatures,
    - Change return types or exception semantics,
    - Alter serialization/XDR behavior,
    Copilot code review should:
    - Explicitly flag the change as **potentially breaking**.
    - Suggest tests and documentation updates.
    - Recommend review from maintainers before merging.

When in doubt, Copilot should err on the side of **highlighting potential breaking changes** in PR reviews.

---

## 5. HTTP resilience and retry behavior (v12+)

The HTTP resilience layer is a critical cross-cutting concern. It is implemented primarily in:

- `StellarDotnetSdk/Requests/HttpResilienceOptions.cs`
- `StellarDotnetSdk/Requests/RetryingHttpMessageHandler.cs`
- `docs/tutorials/examples/http-retry.md`

Key expectations:

- **Default behavior**
  - Retries are enabled by default (3 attempts with exponential backoff, jitter, and `Retry-After` support).
  - This is a **breaking change in v12** and must be preserved unless there is a compelling reason to change it.
- **Configuration**
  - `HttpResilienceOptions` and `HttpResilienceOptionsPresets` provide a clear configuration surface; changes should keep this API:
    - Intuitive,
    - Backwards compatible where possible,
    - Consistently documented.
- **Docs alignment**
  - Any PR that modifies HTTP resilience behavior (options, presets, retry conditions, circuit breaker, timeouts) **must also update**:
    - `docs/tutorials/examples/http-retry.md`
    - Relevant sections in `README.md` that describe features like “Automatic Retry”, “Retry-After Support”, etc.
  - Copilot code review should:
    - Compare code changes in the resilience layer with these docs.
    - Call out mismatches (e.g., defaults, presets, or retry policy descriptions that no longer match the code).
    - Prefer straightforward, well-clamped implementations for backoff and delay logic over complex rewrites aimed at hypothetical edge cases when simpler code is already correct.

For resilience-related PRs, Copilot reviews should pay extra attention to:

- Preservation of **reasonable defaults** for most users.
- Proper handling of timeouts, cancellation, and circuit breaker semantics.
- Impact on throughput and latency-sensitive use cases (e.g., trading bots).

---

## 6. Testing expectations

Copilot should assume:

- Every **bug fix** or **behavioral change** should be accompanied by:
  - At least one new or updated test in `StellarDotnetSdk.Tests`, preferably near existing tests for the same feature.
- New **public API** surface (methods, options, classes) should have:
  - Happy-path tests,
  - Edge-case tests (e.g., invalid parameters, boundary conditions),
  - When applicable, regression tests for previously reported issues.
- Tests should:
  - Use existing patterns in this repo (assert style, naming conventions, test helper utilities).
  - Avoid excessive mocking where straightforward integration-like tests are feasible.

During code review, Copilot should:

- Highlight when new behaviors lack tests.
- Suggest concrete areas where tests are missing (e.g., “no tests for `HttpResilienceOptions.FailureRatio` out-of-range values”).

---

## 7. Documentation and examples

Copilot should help keep docs and examples in sync with the code:

- **When code changes:**
  - Public API changes or behavior changes should be reflected in:
    - `README.md`,
    - `docs/` (especially tutorials and API docs),
    - Example projects under `Examples/` if they showcase the changed behavior.
- **For PR reviews:**
  - If a PR:
    - Introduces new public methods or classes,
    - Changes important defaults (e.g., retry behavior),
    - Alters recommended patterns of use,
    Copilot should:
    - Check whether relevant docs and examples are updated.
    - Call out documentation gaps explicitly.

Docs should prefer **clear, production-ready guidance** over minimal examples.

---

## 8. Guidance for Copilot code review

When acting as a **code reviewer on pull requests in this repo**, Copilot should:

1. **Summarize the change**
   - Provide a concise description of what the PR does in user-oriented terms.
   - Identify which areas are touched (e.g., Horizon requests, Soroban support, HTTP resilience, XDR types, tests, docs).
2. **Assess risk and impact**
   - Highlight any potential breaking changes or behavior changes, especially in public APIs.
   - Call out risks around transaction safety, correctness of XDR serialization, and network interactions.
3. **Enforce architectural and style guidelines**
   - Flag violations of separation of concerns (e.g., networking logic leaking into pure domain objects).
   - Call out unclear or overly clever code; suggest more readable alternatives.
4. **Verify tests**
   - Check that new or changed behavior is covered by tests in `StellarDotnetSdk.Tests`.
   - If tests are missing, suggest specific scenarios that should be tested.
5. **Verify docs and examples**
   - Ensure that any user-visible changes are documented, especially:
     - Retry/circuit-breaker behavior,
     - New configuration options,
     - Example usage in `Examples/`.
6. **Consider performance and resilience**
   - For hot paths or network-heavy areas, consider the impact of additional allocations, blocking calls, or unnecessary retries.
   - Ensure that resilience changes do not degrade typical user experience or introduce pathological retries.
7. **Be explicit and actionable**
   - Provide concrete, actionable feedback rather than vague suggestions.
   - Where possible, propose specific code-level improvements.

If the PR is small and obviously correct, Copilot may respond succinctly but should still mention:

- Whether it appears non-breaking,
- Whether tests and docs are sufficient,
- Any obvious follow-up improvements.

---

## 9. Guidance for Copilot Chat and code generation

When generating code or answering questions in this repo, Copilot should:

- Prefer **small, composable, and well-named methods** over large monoliths.
- Show examples that:
  - Use the public SDK surface as intended,
  - Respect nullability and async patterns,
  - Use `HttpResilienceOptions` and `DefaultStellarSdkHttpClient` correctly when discussing networking.
- Avoid introducing new heavy dependencies without strong justification.
- Encourage:
  - Adding or updating tests alongside changes,
  - Updating docs and examples when behavior changes,
  - Discussing trade-offs (performance vs. resilience vs. simplicity) when relevant.

When unclear about intent (e.g., whether a change should be breaking), Copilot should **ask for clarification in the PR discussion or comments** rather than assuming.


