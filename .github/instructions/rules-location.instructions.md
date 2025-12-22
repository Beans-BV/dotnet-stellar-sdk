---
applyTo: "**/*.mdc,**/.cursor/rules/**/*"
---
# Cursor Rules Location

Place rules in `.cursor/rules/your-rule-name.mdc`

**Naming**: kebab-case, `.mdc` extension.

**Frontmatter options**:
- `alwaysApply: true` — always active
- `alwaysApply: false` + `description` — AI decides when relevant
- `globs: "pattern"` — active for matching files

**Sync requirement**: When updating rules, update BOTH `.cursor/rules/*.mdc` AND `.github/instructions/*.instructions.md` to keep them in sync.
