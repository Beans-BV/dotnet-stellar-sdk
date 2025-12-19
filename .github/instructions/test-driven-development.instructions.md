---
applyTo: "**"
---
# Test-Driven Development

For NEW code, follow Red-Green-Refactor:

1. **Write failing test** (Red)
2. **Get approval**
3. **Write minimal code to pass** (Green)
4. **Run tests** — `dotnet test` in project
5. **Get approval**
6. **Refactor if needed**
7. **Run tests again**
8. **Repeat**

One test at a time. Stop after each step for verification.

> For existing code without tests, use `retroactive-testing.mdc` instead.
