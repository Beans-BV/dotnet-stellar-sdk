---
applyTo: "**"
---
# Code Generation Guidelines

**Methods**: Small, composable, well-named over monoliths.

**Examples**: Use public SDK surface correctly, respect nullability/async, use `HttpResilienceOptions`/`DefaultStellarSdkHttpClient` properly.

**Dependencies**: Avoid heavy new deps without strong justification.

**Encourage**: Tests/docs updates with changes, discuss trade-offs (perf vs resilience vs simplicity).

**Unclear intent**: Ask for clarification in PR/comments, don't assume.
