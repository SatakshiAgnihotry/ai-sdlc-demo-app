# Project conventions for AI agents

Invoice approval demo app: ASP.NET Core 8 Web API with a small static web UI.

## Layout

- `backend/src/InvoiceApp.Domain`: business rules, no web or storage code. `ApprovalService` owns the invoice lifecycle (Draft, PendingApproval, Approved, Rejected).
- `backend/src/InvoiceApp.Api`: controllers, sign-in (`Auth/`), in-memory repository, and the web UI under `wwwroot/`.
- `backend/tests/InvoiceApp.Tests`: xUnit tests, one test class per class under test, named `<ClassUnderTest>Tests`.
- `e2e`: Playwright end-to-end tests (`tests/`) and page objects (`pages/`).
- `docs/use-cases`: short briefs for features we may build.

## Conventions

- Controllers live in `Controllers/`, one per resource, route `api/[controller]`. Request bodies are `record` types declared inside the controller.
- Controllers inherit `ApiControllerBase` and use `Handle(...)` so service errors map to 404, 409 and 400.
- Every endpoint is behind sign-in. Restrict actions with `[RequireRole("...")]`: destructive actions are Admin only, approvals are Approver or Admin.
- Business rules go in the Domain project, never in controllers.
- Register services in `Program.cs`. Repositories are interfaces in Domain with an in-memory implementation in Api.
- Every new rule or endpoint needs tests. Use `FakeInvoiceRepository` in tests.
- Use clear names; no one-letter variables or `flag` parameters.
- End-to-end tests use page objects and role-based locators (`getByRole`, `getByLabel`), never CSS or XPath selectors.

## Approval rules (role-based limits)

These apply inside `ApprovalService.Approve(...)` and must be enforced in the Domain layer —
never in `ApprovalsController` or `Program.cs`.

- A **Manager** may approve invoices up to and including 50,000.
- A **Director** may approve invoices up to and including 500,000.
- Invoices **above** 500,000 need two distinct approvals. The first approval may come from a
  Manager or a Director (whoever could approve it under their own limit); the **second**
  approval must come from a Director. *(The written requirement doesn't state this explicitly —
  confirmed with the trainer as an assumption to document; see `ASSIGNMENT.md`.)*
- The same user can never count as both approvers on one invoice.
- A rejected approval attempt returns a clear reason, and the invoice is left completely
  unchanged. Check every relevant field (status, DecidedBy, DecidedAt, DecisionNote, and any
  recorded first approval) before and after — not just that the repository wasn't called.
  `FakeInvoiceRepository.Update` is a no-op, so a test that only checks "Update wasn't called" can
  still pass even if the service mutated the invoice object in place first.
- The existing `Approver` and `Admin` roles are kept, not replaced. The exact relationship
  between `Approver`/`Admin` and the new `Manager`/`Director` roles is defined in the OpenSpec
  change under `openspec/changes/` — read that before touching role checks; do not assume one.
- Invoice creation/submission is untouched by this feature: any signed-in user who can create an
  invoice today still can, at any amount, even if they could never approve it themselves.
  *(Confirmed out of scope with the trainer; also an assumption to document.)*
- The two rules above about same-user-twice and nothing-changes-on-rejection are exactly where an
  agent is most likely to produce something that looks right and is not. Tests must specifically
  target them, not just the happy path.

## Commands

- Run the app: `dotnet run --project backend/src/InvoiceApp.Api` then open http://localhost:5000
- Run the tests: `dotnet test backend/InvoiceApp.sln`
- Run end-to-end tests: `cd e2e && npm install && npx playwright install chromium && npm test`
- Run mutation tests (from `backend/tests/InvoiceApp.Tests`):
  `dotnet-stryker --project InvoiceApp.Domain.csproj --reporter cleartext --reporter json --reporter html --output ../../../StrykerOutput/<label>`
  Compare the new run's overall and per-file scores against the previous one — a rule with
  surviving mutants in its own logic is not done.

## Review expectations

- Before a PR is opened, run the self-review prompt against the full diff:
  > Review this PR against our checklist. Report findings by severity with file and line, suggest
  > a fix for each, and say what you are unsure about.
- Use the `pr-review` skill or the `reviewer` subagent for this — it has no memory of writing the
  code, so it reviews the diff itself rather than the intent behind it.
- Every finding gets a verdict in the PR description: **fixed**, **dismissed with a reason**, or
  **deferred with a reason**. A human assigns the verdict; an agent does not dismiss or defer its
  own findings.

## Commit message format

- One commit per implementation step (e.g. single-approver limits, then the two-approver rule) —
  never the whole feature in one commit.
- Format:
  ```
  <one line: what changed>

  Why: <one or two lines: why, referencing the rule or spec item it satisfies>
  ```

## Demo accounts (not real credentials)

- `demo.approver` / `ChangeMe123!` (role Approver)
- `demo.admin` / `ChangeMe123!` (role Admin)
- `demo.manager` / `ChangeMe123!` (role Manager)
- `demo.director` / `ChangeMe123!` (role Director)

## Never

- Commit real credentials, tokens or customer data.
- Add business logic to `Program.cs` or controllers.
