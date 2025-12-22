---
applyTo: "**/PULL_REQUEST_TEMPLATE.md,**/.github/PULL_REQUEST_TEMPLATE.md"
---

# PR Summary Auto-Generation

When working with pull request templates, **automatically generate and update** the summary section.

## Summary Generation Rules

1. **Always generate a summary** when you encounter the placeholder `<!-- Copilot: generate the summary here -->` in a PR template.

2. **Summary format**:
   - **Concise description** (1-2 sentences) of what the PR does in user-oriented terms
   - **Affected areas** (e.g., Horizon requests, Soroban support, HTTP resilience, XDR types, tests, docs)
   - **Change type** (bug fix, new feature, breaking change, refactoring, documentation)
   - **Key files/modules** touched (list 3-5 most important)

3. **When updating an existing summary**:
   - If a summary already exists, **replace it** with an updated version based on the current PR changes
   - Analyze the diff/changes to ensure accuracy
   - Keep the same format but refresh the content

4. **Content guidelines**:
   - Focus on **what** changed and **why** (if clear from context)
   - Use clear, professional language
   - Avoid implementation details unless critical
   - Highlight breaking changes explicitly
   - Mention if tests/docs were added/updated

5. **Example structure**:
   ```
   This PR [action verb] [what] to [why/benefit]. 
   
   **Affected areas**: [area1], [area2], [area3]
   **Change type**: [bug fix/new feature/breaking change/etc.]
   **Key changes**: [file1], [file2], [file3]
   ```

6. **Always replace** the entire content between the comment markers, don't append or merge.

