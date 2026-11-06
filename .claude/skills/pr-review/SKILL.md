---
name: pr-review
description: Run the workshop's exact PR review prompt against a diff and return findings in a fixed table format (severity, file:line, finding, suggested fix, confidence). Use when self-reviewing the pull request before the human assigns verdicts.
---

# pr-review

Use the review prompt verbatim — do not paraphrase or soften it:

> Review this PR against our checklist. Report findings by severity with file and line, suggest a
> fix for each, and say what you are unsure about.

## Checklist to review against

- Every rule in AGENTS.md's approval-rules section is implemented and tested, including both
  negative rules (same user can't be both approvers; a rejected attempt changes nothing).
- Business logic lives in `InvoiceApp.Domain`, not in controllers or `Program.cs`.
- Every new endpoint or action is behind sign-in and the correct `[RequireRole(...)]`.
- Controllers use `Handle(...)` so service errors map to the right HTTP status.
- Commit messages say what changed and why.
- Tests use `FakeInvoiceRepository`, not a real store.

## Output format

Return every finding as a row in this table, most severe first, then one line per finding on what
you are unsure about if anything:

| Severity | File:Line | Finding | Suggested fix |
|---|---|---|---|

This output is handed to the human, who assigns one of three verdicts per finding — fixed,
dismissed with a reason, or deferred with a reason. The skill does not assign verdicts itself.
