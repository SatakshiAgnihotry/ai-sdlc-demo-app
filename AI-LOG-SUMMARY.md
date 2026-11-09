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
- **2026-11-06** — Step 4, "show context matters": ran the identical planning-only ticket through
  two fresh Plan sub-agents, one reading a temporary worktree at the pre-session commit
  (original `AGENTS.md`), one reading the current repo (extended `AGENTS.md`). Both caught the
  `FakeInvoiceRepository.Update` no-op trap on their own, but only the extended-context agent
  checked for the OpenSpec change `AGENTS.md` now points to, found it doesn't exist yet, and
  explicitly declined to guess the Approver/Admin vs Manager/Director role relationship in the
  controller wiring rather than silently assuming it. It also quoted the exact mutation command,
  the exact review prompt/verdict system, and the exact commit format — all newly added to
  `AGENTS.md`. Nothing from either plan was implemented; the worktree was removed afterward.
- **2026-11-06** — Step 5: installed OpenSpec 1.16.0 via `npx` (no global install), ran
  `openspec init`, and wrote the `approval-limits-by-role` change (proposal, spec delta, design,
  tasks), asking the user to resolve every remaining open question first. **Human decisions:**
  pending-second-approval is a new `InvoiceStatus` value; declined attempts reuse the existing
  409 path; Reject always wins over a pending second approval; the UI shows the reason inline;
  and Approver/Admin keep their current unlimited behavior completely unchanged, with the
  resulting two-approver bypass documented as a deliberate, accepted consequence. One UI question
  had to be re-asked after the user caught it conflating a declined attempt with the separate
  Reject action. `openspec validate --strict` passes.
- **2026-11-06** — Step 6: implemented single-approver limits (Manager ≤50,000, Director
  ≤500,000, Approver/Admin unrestricted) via a new `ApprovalLimitPolicy`, threaded through
  `ApprovalService`/`ApprovalsController`, with demo accounts and tests. `dotnet test`: 26 passed
  (up from 7). Live-smoke-tested against the running API: a Manager's 80,000 approval attempt was
  declined at 409 with the exact reason text; a Director then approved the same invoice at 200.
  The two-approver rule is explicitly deferred to Step 7.
- **2026-11-09** — Step 7: implemented the two-approver rule above 500,000. New
  `PendingSecondApproval` status and `FirstApprovedBy`/`FirstApprovedAt` fields; a Manager or
  Director's approval on an invoice over 500,000 now registers as a first approval instead of
  being declined; a second, distinct Director completes it; Approver/Admin still finalize any
  amount, including a pending-second-approval invoice, in one step; Reject always wins. Caught a
  real contradiction between two already-approved spec.md requirements for a Director approving
  alone above 500,000 (hard decline-and-unchanged vs. succeed-as-first-approval) and asked the
  user rather than guessing. **Human decision:** register it as a first approval; the old Step 6
  decline test at that boundary was rewritten accordingly. `dotnet test`: 33 passed (up from 26).
  Live-smoke-tested the full flow against the running API: first approval, second-approver-not-
  Director decline, same-user decline, and Admin one-step finalization all behaved exactly as
  designed.
- **2026-11-09** — Step 8 (mutation testing): ran `dotnet-stryker` against `InvoiceApp.Domain`
  after Step 7. First run: overall 56.25% (baseline 30.77%); `ApprovalLimitPolicy.cs` 62.50%;
  `ApprovalService.cs` 77.78% (baseline 60.00%). Classified every surviving mutant in the two
  files this feature owns as either a real gap or the known `FakeInvoiceRepository.Update`-no-op
  trap AGENTS.md itself says not to chase. Strengthened the decline-reason tests (exact text
  instead of a not-empty check that let half a concatenated message mutate away undetected) and
  added two missing-coverage tests (unknown role above the Director limit; Reject on a Draft
  invoice). Second run: overall 66.25%, `ApprovalLimitPolicy.cs` 100.00% (0 survivors),
  `ApprovalService.cs` 82.22%, with every remaining survivor being the accepted Update-no-op trap
  or pre-existing message text in untouched code. `dotnet test`: 38 passed (up from 33).
- **2026-11-09** — Step 8 (UI, task 4.1): changed `wwwroot/app.js`'s decline-reason display from a
  single shared paragraph below the whole pending-approvals list to a per-invoice alert next to
  each invoice's Approve button, matching spec.md's literal wording. Verified live via the
  Playwright MCP browser tool (not just by reading the code): a Manager's declined approval on an
  80,000 invoice shows the exact reason next to that invoice only, with the other pending
  invoices unaffected. This was a prerequisite the user and I identified before the Playwright
  e2e test, since the test needs a real reason displayed to assert against.
