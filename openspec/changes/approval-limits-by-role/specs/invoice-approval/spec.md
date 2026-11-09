# Spec Delta

## Purpose

The invoice-approval capability governs who may approve or reject an invoice, enforcing
role-based spending limits and ensuring a declined approval attempt never mutates the invoice.

## ADDED Requirements

### Requirement: Manager approval limit
A user with the Manager role SHALL be permitted to approve an invoice with an amount less than
or equal to 50,000, and SHALL be declined, with a clear reason, from approving a greater amount.

#### Scenario: Manager approves within limit
- **WHEN** a user with role Manager approves an invoice of amount less than or equal to 50,000
- **THEN** the invoice SHALL move to the Approved status

#### Scenario: Manager declined above limit
- **WHEN** a user with role Manager attempts to approve an invoice of amount greater than 50,000
- **THEN** the attempt SHALL be declined with a clear reason and the invoice SHALL remain unchanged

### Requirement: Director approval limit
A user with the Director role SHALL be permitted to approve an invoice with an amount less than
or equal to 500,000 in a single step. A Director's approval on a greater amount SHALL NOT finalize
the invoice alone; it SHALL instead be recorded as the first of the two approvals required by the
Two-approver rule below.

#### Scenario: Director approves within limit
- **WHEN** a user with role Director approves an invoice of amount less than or equal to 500,000
- **THEN** the invoice SHALL move to the Approved status

#### Scenario: Director above limit does not finalize alone
- **WHEN** a user with role Director approves, as the first approver, an invoice of amount greater
  than 500,000
- **THEN** the invoice SHALL move to the PendingSecondApproval status rather than Approved, per
  the Two-approver rule below

### Requirement: Two-approver rule above 500,000
An invoice with amount greater than 500,000 SHALL require two distinct approvals before moving
to Approved: a first approval from a user with role Manager or Director, and a second approval
from a user with role Director.

#### Scenario: First approval records pending state
- **WHEN** a user with role Manager or Director gives the first approval on an invoice of amount
  greater than 500,000
- **THEN** the invoice SHALL move to the PendingSecondApproval status and SHALL NOT move to Approved

#### Scenario: Second approval by a Director completes approval
- **WHEN** a user with role Director, distinct from the first approver, approves an invoice
  already in PendingSecondApproval
- **THEN** the invoice SHALL move to the Approved status

#### Scenario: Second approval declined if not a Director
- **WHEN** a user without role Director attempts the second approval on an invoice in
  PendingSecondApproval
- **THEN** the attempt SHALL be declined with a clear reason and the invoice SHALL remain unchanged

### Requirement: Same user cannot be both approvers
The system SHALL decline any attempt where the user attempting a second approval is the same
user recorded as the first approver on that invoice, regardless of role.

#### Scenario: Same Director attempts both approvals
- **WHEN** the same user who gave the first approval, holding role Director, attempts the second
  approval on the same invoice
- **THEN** the attempt SHALL be declined with a clear reason and the invoice SHALL remain unchanged

### Requirement: Declined approval attempt leaves the invoice unchanged
When an approval attempt is declined under any rule in this capability, the system SHALL leave
every field of the invoice exactly as it was before the attempt, and SHALL return a clear reason.

#### Scenario: Status and prior-approval fields unchanged after a decline
- **WHEN** an approval attempt by a Manager or Director is declined for any reason
- **THEN** the invoice's status, decision fields, and any recorded first approval SHALL be
  identical to their values immediately before the attempt

### Requirement: Rejection always wins over a pending second approval
Calling the existing Reject action on an invoice that is PendingApproval or PendingSecondApproval
SHALL move the invoice directly to the terminal Rejected status, regardless of any recorded first
approval.

#### Scenario: Reject after a first approval
- **WHEN** an eligible user calls Reject on an invoice in PendingSecondApproval
- **THEN** the invoice SHALL move to the Rejected status

### Requirement: Existing Approver and Admin behavior is unchanged
Users with the Approver or Admin role SHALL continue to approve an invoice of any amount in a
single step, exactly as before this capability existed, with no amount limit and no
two-approver requirement applied to them.

#### Scenario: Approver approves a large invoice in one step
- **WHEN** a user with role Approver approves an invoice of any amount, including one already in
  PendingSecondApproval
- **THEN** the invoice SHALL move to the Approved status in that single step

### Requirement: UI shows the decline reason
The web UI SHALL display the clear reason returned by a declined approval attempt to the user
who attempted it.

#### Scenario: Manager sees the reason for a declined approval
- **WHEN** a Manager's approval attempt on an invoice is declined
- **THEN** the UI SHALL show the returned reason as inline text next to the Approve action
