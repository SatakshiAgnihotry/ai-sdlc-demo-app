# Design

## Context

See proposal.md - Why. Today `ApprovalService.Approve(invoiceId, approver)` only knows a
username, not a role. `InvoiceStatus` has four values (Draft, PendingApproval, Approved,
Rejected). `FakeInvoiceRepository.Update` is a no-op and `GetById` returns the same object
reference it was given, so tests must snapshot fields to prove nothing mutates on a decline.

## Goals / Non-Goals

**Goals:**
- Enforce the role-based limits and two-approver rule entirely in `InvoiceApp.Domain`.
- Make "nothing changes on a declined attempt" verifiable by construction (validate-before-mutate
  ordering), not just by how the test happens to be written.
- Keep `ApiControllerBase`'s existing exception-to-status-code mapping; no new plumbing.

**Non-Goals:**
- Changing Approver/Admin behavior (explicitly out of scope per the user's decision).
- Any change to invoice creation/submission.
- Any new UI state for "pending second approval" beyond showing the decline reason inline.

## Decisions

1. **New `PendingSecondApproval` status**, not a fields-only representation — chosen by the user
   because the two-approver wait is a first-class lifecycle state, not just metadata.
   *Alternative considered:* infer the pending state from `FirstApprovedBy`/`FirstApprovedAt`
   being set, with no new status. Rejected per the user's choice — leaves the state implicit.
2. `Invoice` also gains two nullable fields, `FirstApprovedBy` and `FirstApprovedAt`, to record
   *who* gave the first approval — needed for the same-user-cannot-be-both-approvers rule and for
   an auditable record. These exist alongside the new status, not instead of it.
3. Role-checking logic is a pure policy (limit/role checks with no side effects) consumed by
   `ApprovalService.Approve(...)`, which does the state-transition orchestration.
   *Alternative considered:* inline every check directly in `Approve`. Rejected — conflates "is
   this allowed" with "what state transition happens," making both harder to test precisely and
   to hit exhaustively with mutation testing.
4. **Validate before mutate**: every rule check happens before any field on the `Invoice` is
   written and before `_repository.Update` is called. This is what makes the "nothing changes on
   a declined attempt" requirement actually true rather than incidentally true, given that
   `FakeInvoiceRepository.Update` is a no-op.
5. Reuse `InvalidOperationException` → 409 for every declined-attempt case, per the user's
   choice, rather than a new exception type/status code — smallest change, consistent with how
   `EnsurePending` already signals invalid state transitions today.
6. **`Approver`/`Admin` keep their current code path entirely untouched** — no role/amount check
   is added to it, per the user's explicit instruction. This includes the known consequence
   (documented in the proposal) that Approver/Admin can finalize a >500,000 invoice in one step
   even if it is `PendingSecondApproval`.
7. **Reject always wins**: checked ahead of approve-path logic, state-independent — accepts
   `PendingApproval` or `PendingSecondApproval` and always moves straight to `Rejected`.

## Risks / Trade-offs

- [Approver/Admin can bypass the two-approver rule for large invoices] → Mitigation: documented
  explicitly in `proposal.md` and here as a deliberate, user-confirmed decision; restate it again
  in the PR description so a reviewer doesn't mistake it for an oversight.
- [New `PendingSecondApproval` enum value could break an existing exhaustive switch over
  `InvoiceStatus` with no default case] → Mitigation: grep every existing switch/pattern-match
  over `InvoiceStatus` as part of implementation, not discovered after.
- [`FakeInvoiceRepository.Update`-is-a-no-op trap silently passing a bad test] → Mitigation: the
  unchanged-invoice tests snapshot every field before and after the attempt, not just whether
  `Update` was called.
- [Boundary off-by-one at 50,000/500,000] → Mitigation: explicit boundary tests at the limit and
  limit+0.01 for both roles, plus mutation testing to confirm no surviving mutant flips `<=` to
  `<` undetected.

## Migration Plan

No data migration needed — in-memory repository, no state persisted across deploys. Rollout is
the implementation steps themselves (AGENTS.md's commit convention): single-approver limits
first, then the two-approver rule, each its own commit, each leaving `dotnet test` green.

## Open Questions

None — every ambiguity that would have changed the specs, the approach, or the task breakdown
was resolved with the user before writing this design.
