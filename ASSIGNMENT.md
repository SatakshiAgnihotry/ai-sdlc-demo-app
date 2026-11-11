# Assignment: Role-based invoice approval limits

## Feature
Role-based invoice approval limits (Manager/Director), including the two-approver rule for invoices above 500,000.

## Approach
Used OpenSpec (spec-driven workflow) to write the proposal, spec delta, design, and tasks before implementation; artifacts live under `openspec/changes/approval-limits-by-role/`.

## Assumptions confirmed with the trainer
The written requirement didn't specify these; confirmed via Teams with the trainer (Utsav Verma), who said: "The requirement doesn't mention any such details. You can proceed with your assumptions and mention the same in the readme."

1. **First approver above 500,000**: may be a Manager or a Director (whoever could approve it under their own limit); the second approval must come from a Director.
2. **Invoice creation/submission**: untouched by this feature — any signed-in user who can create an invoice today still can, at any amount, regardless of whether they could approve it themselves.

## Implementation steps
Implemented in the required minimum of two steps, each its own commit:
1. Single-approver limits (Manager ≤50,000, Director ≤500,000)
2. Two-approver rule above 500,000 (first approval by Manager/Director, second by a distinct Director)

Plus follow-up commits for mutation-driven test strengthening, the required UI/e2e work, and self-review fixes — one commit per step, never the whole feature in one commit.

## Testing
- Unit tests (xUnit): 38 tests, including both negative rules the assignment specifically calls out.
- Mutation testing (Stryker.NET 4.13.0) against `InvoiceApp.Domain`:

| File | Before | After |
|---|---|---|
| Overall | 30.77% | 66.25% |
| ApprovalLimitPolicy.cs | n/a (new this feature) | 100.00% |
| ApprovalService.cs | 60.00% | 82.22% |
| DiscountCalculator.cs | 0.00% | 0.00% (pre-existing, untouched) |
| Invoice.cs | 0.00% | 0.00% (pre-existing, untouched) |

- One Playwright e2e test covering a declined over-limit approval through the real UI.

## Self-review
Ran the self-review prompt ("Review this PR against our checklist...") against the full diff using a dedicated `reviewer` sub-agent with no memory of writing the code. 4 findings reported; verdicts and reasoning are in the PR description.

## AI tooling used
- Claude Code (VS Code extension), model Claude Sonnet 5 (session 2; session 1 used Claude Opus 5.5 → Sonnet 5).
- Custom sub-agents (`explorer`, `test-writer`, `reviewer`, `e2e-tester`) and skills (`ai-log`, `add-domain-rule`, `pr-review`) under `.claude/`.
- Hooks (`UserPromptSubmit`, `PreToolUse`, `PostToolUse`, `SubagentStop`, `SessionEnd`) for structured, schema-validated AI-interaction logging — see `ai-logs/` and `AI-LOG-SUMMARY.md`.
- OpenSpec for the spec-driven workflow — see `openspec/changes/approval-limits-by-role/`.

## Demo video
https://nagarro-my.sharepoint.com/:v:/p/satakshi_agnihotri/IQC4xGqB2x77QroX_TslEMSpAWq22kdFNHOY_I17iMmNlr8
