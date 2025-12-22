---
applyTo: "**"
---
# Semantic Versioning & API Compatibility

**SemVer**: Breaking changes → major version bump + docs in `README.md`/release notes.

**Public API**: `public`/`protected` in `StellarDotnetSdk`/`StellarDotnetSdk.Xdr` = user-facing contracts.

**Breaking changes**: Removed members, signature changes, return type changes, serialization/XDR changes → flag as **potentially breaking**, suggest tests/docs, recommend maintainer review.

**Conservative approach**: Err on side of highlighting potential breaking changes in reviews.
