# Tasks

## 1. Data model and policy scaffolding

- [x] 1.1 Add the Manager/Director role concept and the pure limit/role-check policy (50,000 /
      500,000, second-approver-must-be-Director) to InvoiceApp.Domain; verify `dotnet build
      backend/InvoiceApp.sln` succeeds (Step 6 landed the 50,000/500,000 single-approver limits;
      Step 7 added the second-approver-must-be-Director check)
- [x] 1.2 Add `PendingSecondApproval` to `InvoiceStatus` and `FirstApprovedBy`/`FirstApprovedAt`
      to `Invoice`; verify the existing 7 pre-existing tests still compile and pass via
      `dotnet test backend/InvoiceApp.sln`

## 2. Single-approver limits (Step 6 commit)

- [x] 2.1 Thread role through `ApprovalService.Approve(invoiceId, approver, role)` and enforce
      Manager ≤50,000 / Director ≤500,000 for amounts ≤500,000; verify by new unit tests for
      success and decline-above-limit for each role, including boundary values (limit and
      limit+0.01)
- [x] 2.2 Unit test: a declined over-limit attempt leaves every `Invoice` field unchanged
      (snapshot before/after, not just "Update wasn't called"); verify via `dotnet test`
- [x] 2.3 Wire role through `ApprovalsController`, add Manager/Director to
      `[RequireRole(...)]`, add `demo.manager`/`demo.director` to `appsettings.json` and
      AGENTS.md's demo-accounts list; verify by running the app and signing in as each

## 3. Two-approver rule above 500,000 (Step 7 commit)

- [x] 3.1 Implement first-approval recording (→ `PendingSecondApproval`, records
      `FirstApprovedBy`/`At`) and second-approval completion (Director only, distinct user) in
      `ApprovalService`; verify with unit tests for both transitions
- [x] 3.2 Unit test: same user cannot be both approvers, isolated so only identity differs (same
      valid role on both attempts); verify `dotnet test` passes and the invoice is unchanged
      after the decline
- [x] 3.3 Unit test: second approval declined if the second approver is not a Director; verify
      the unchanged-invoice snapshot as well
- [x] 3.4 Unit test: Reject always wins — Reject on a `PendingSecondApproval` invoice moves
      straight to `Rejected`; verify via `dotnet test`
- [x] 3.5 Unit test: Approver/Admin's existing unlimited single-step behavior is unchanged,
      including finalizing a `PendingSecondApproval` invoice in one step; verify via `dotnet test`

## 4. UI: show the decline reason

- [x] 4.1 Update `wwwroot/app.js` to display the server's returned reason as inline text next to
      the Approve action on a 409 decline; verify manually by running the app and triggering a
      declined approval in the browser (verified live via the Playwright MCP browser tool: a
      Manager's declined approval on an 80,000 invoice shows the exact reason as an alert next to
      that invoice's Approve button, scoped only to that invoice)

## 5. Mutation testing ("after" run)

- [x] 5.1 Run the mutation command from AGENTS.md against `InvoiceApp.Domain` and compare against
      the baseline (`StrykerOutput/baseline`); verify by recording the new overall and per-file
      scores (baseline 30.77% overall → 56.25% after Step 7; see AI-LOG-SUMMARY.md for the full
      before/after breakdown)
- [x] 5.2 For any surviving mutant inside the new approval-limit logic, strengthen the specific
      test that should have caught it; verify by re-running Stryker and confirming it's killed
      (second run: 66.25% overall, ApprovalLimitPolicy.cs 100.00%, ApprovalService.cs 82.22%;
      every remaining survivor is the accepted Update-no-op trap or pre-existing untouched code)

## 6. Playwright e2e test

- [x] 6.1 Add a page object for the invoice/approval UI under `e2e/pages`, using role-based
      locators per AGENTS.md; verify it compiles/typechecks
- [x] 6.2 Add one Playwright test: a declined over-limit approval through the UI, asserting the
      clear reason is shown; verify via `npm test` in `e2e/` (1 passed)

## 7. Self-review and PR

- [x] 7.1 Run the `pr-review` skill / `reviewer` sub-agent with the workshop prompt against the
      full diff; verify by producing the findings table
- [x] 7.2 Assign a verdict (fixed / dismissed-with-reason / deferred-with-reason) to every
      finding with the user; verify the PR description lists every finding and verdict

## Workflow follow-up

- Archive this change once implementation, tests, mutation run, and review are complete and merged.
- Verify the archived result with `openspec list --specs`.
