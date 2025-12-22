---
applyTo: "**/*.cs"
---
# C# Code Style

**Naming**: Files/classes = `PascalCase.cs` | Public members = `PascalCase` | Private fields = `_camelCase` | Parameters/local = `camelCase` | Constants = `PascalCase`

**Functions**: Start with verbs (`Get`, `Create`, `Submit`) | Booleans use `Is`/`Has`/`Can` prefix

**Types**: Always explicit, nullable reference types enabled (`string?`, `T?`). Use `object` for "any type".

**Namespaces**: File-scoped (`namespace StellarDotnetSdk;`) preferred over block-scoped.

**Style**:
- Expression-bodied members for single-line (`=>`)
- Early returns over nesting
- `using` statements for `IDisposable`
- Always `{}` braces for if/else
- No blank lines inside method bodies
- `async`/`await` with `CancellationToken` for long ops
- `ConfigureAwait(false)` in library code

**Enums**: Use enum type, add extension methods for helpers if needed.

**Architecture**: Separate domain (accounts/assets/operations/XDR) from transport (HTTP/resilience/SSE) from examples/tests. Avoid infrastructure leaks into public APIs. Favor DI over global config.

**Async**: Prefer async for I/O, expose `CancellationToken`, pass through layers. Avoid blocking async code.

**Optimization**: Avoid micro-optimizing/churning simple correct code (e.g., `Math.Pow` vs bit shifting) without concrete bug/perf issue/clarity benefit.
