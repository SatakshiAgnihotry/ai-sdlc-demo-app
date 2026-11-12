# Proposal

## Why

The invoice approval endpoint currently authorizes by role alone (Approver or Admin) with no
connection between a user's authority and the invoice amount — anyone who can reach the endpoint
can approve an invoice of any size. This change adds spending limits by role so approval
authority scales with amount, closing that gap for two new roles (Manager, Director) while
leaving all existing behavior untouched.

## What Changes

- Add two new business roles, **Manager** and **Director**, used specifically for amount-based
  approval limits.
- A Manager may approve invoices up to and including 50,000.
- A Director may approve invoices up to and including 500,000.
- Invoices above 500,000 require two distinct approvals: the first may come from a Manager or a
  Director; the second must come from a Director.
- The same user may never be recorded as both approvers on one invoice.
- **BREAKING** (within this codebase): add a new `PendingSecondApproval` status to
  `InvoiceStatus`, representing "one approval recorded, awaiting a second, distinct Director."
  Any existing code that switches/pattern-matches over `InvoiceStatus` must handle it.
- Calling the existing **Reject** action always wins: it moves an invoice straight to the
  terminal `Rejected` status from either `PendingApproval` or `PendingSecondApproval`, regardless
  of any recorded first approval. The two-approver rule only constrains the approve path.
- A declined approve attempt (wrong role/limit, same-user-twice, invalid second approver) returns
  a clear reason via the existing 409 (`InvalidOperationException`) path and leaves every field on
  the invoice unchanged.
- UI: a declined approve attempt is shown as inline error text next to the Approve action.
- **Approver and Admin are explicitly unchanged** — confirmed with the user rather than assumed.
  They keep their current unlimited, single-approver behavior exactly as today; the new limits and
  the two-approver rule apply only to Manager/Director. **Known, accepted consequence:** because
  Approver/Admin's path doesn't check for a pending second approval, an Approver or Admin can
  still finalize a >500,000 invoice in one step even after a Manager has recorded a first
  approval on it — this is a deliberate product decision, not an oversight, and is called out here
  so it isn't mistaken for one later.
- Invoice creation/submission is out of scope — unchanged by this feature (confirmed with the
  trainer as a documented assumption).

## Capabilities

### New Capabilities
- `invoice-approval`: the invoice approval lifecycle and its role-based amount limits — who may
  approve or reject an invoice, under what conditions, and what state the invoice moves through.

### Modified Capabilities
(none — no specs exist yet in this project; this is the first)

## Impact

- `backend/src/InvoiceApp.Domain/ApprovalService.cs`, `Invoice.cs`, `InvoiceStatus.cs`
- `backend/src/InvoiceApp.Api/Controllers/ApprovalsController.cs` (pass caller's role through;
  add Manager/Director to `[RequireRole(...)]`)
- `backend/src/InvoiceApp.Api/appsettings.json` (demo users for Manager/Director)
- `backend/src/InvoiceApp.Api/wwwroot/app.js` (inline error display for a declined attempt)
- `backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs` and any new pure-policy test file
- `AGENTS.md` already documents these rules; this is their authoritative spec
