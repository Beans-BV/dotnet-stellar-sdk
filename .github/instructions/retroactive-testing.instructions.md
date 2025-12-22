---
applyTo: "**"
---
# Retroactive Testing

When adding tests to existing code:

1. **Understand intent** — what should code do, not what it currently does
2. **Test the contract** — public API, inputs→outputs, not implementation details
3. **Flag suspicious behavior** — don't blindly test potential bugs as features
4. **Atomic steps** — one test at a time, verify before next

**Workflow**: Analyze code → identify intent → list test cases → get approval → write test → run → discuss → repeat

**Avoid**: Copying code logic into assertions, testing private methods, hardcoding current output without understanding why.
