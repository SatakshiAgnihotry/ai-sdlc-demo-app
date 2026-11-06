# AI interaction log — summary

Human-readable rollup of `ai-logs/curated-log.jsonl`. One bullet per curated entry, newest last.

Session 1 ran from `C:\projects\inspect\Web Applications` before this repo's hooks existed, so it
has no raw events; its curated entries are backfilled from its real transcript via
`scripts/import-session1-transcript.mjs` (AI tool: Claude Code VS Code extension, model: Claude
Opus 5.5).

---

## Session 1 (`C:\projects\inspect\Web Applications`, 2026-11-05, Claude Opus 5.5 → Claude Sonnet 5)

- **11:45** — User shared the 3 workshop decks and the assignment PDF, asked for a fresh start
  with no hallucination/assumptions, and asked for AI-interaction logging per the trainer's format.
- **11:45** — Read the decks, the PDF, and the `ai-sdlc-demo-app` reference repo; checked available
  tooling.
- **11:48** — Asked the user which AI tool to use; **human decision:** Claude Code.
- **12:52** — Cloned the demo repo, renamed remote to `upstream`, ran `dotnet test` (7/7 passed),
  installed `dotnet-stryker 4.13.0`, ran the baseline mutation run (**30.77% overall**;
  `ApprovalService.cs` 60.00%, `DiscountCalculator.cs` 0.00%, `Invoice.cs` 0.00%).
- **12:55** — User interrupted a running tool call: *"don't commit anything make this a rule and
  first explain me what are you doing what's the plan, get approval from me for everything."*
  **Human decision:** set the standing no-commit / explain-first-approval rule — a correction of
  the unapproved actions just above.
- **13:01** — User asked for the assignment explained in plain language.
- **13:56** — User asked for a context file in `ai-sdlc-demo-app` so a new VS Code session could
  continue; `SESSION-HANDOFF.md` written.
- **14:01** — User ran `/model` directly; **human decision:** switched to `claude-sonnet-5`.

## Session 2 (`C:\projects\ai-sdlc-demo-app`, from 2026-11-05, Claude Sonnet 5)

- **2026-11-06** — Extended `AGENTS.md` (Step 2): added approval-rules, mutation-test command,
  review-expectations and commit-message-format sections. User shared a Teams screenshot showing
  the trainer had no answer for two spec gaps (first-approver role above 500,000; whether invoice
  creation should be role-restricted) and said to proceed with a documented assumption.
  **Human decision:** first approval above 500,000 may come from a Manager or a Director, second
  must be a Director; invoice creation is left untouched by this feature. Both recorded in
  `AGENTS.md` as assumptions to also document in `ASSIGNMENT.md`.
