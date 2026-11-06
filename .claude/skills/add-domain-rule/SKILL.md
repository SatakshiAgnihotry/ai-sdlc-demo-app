---
name: add-domain-rule
description: Add one new business rule to the Domain layer and AGENTS.md together, in a consistent shape (rule statement, implementing code, owning test, AGENTS.md entry). Use whenever a new approval/business rule is implemented so every rule addition looks the same and nothing is added to a controller by mistake.
---

# add-domain-rule

Use this whenever a new business rule is implemented for the invoice approval feature (or any
future domain rule).

## Steps

1. State the rule in one sentence, in the same plain language used in `AGENTS.md`'s "Conventions"
   section.
2. Implement it in `backend/src/InvoiceApp.Domain` only — never in a controller, never in
   `Program.cs`. If the rule needs data not currently on `Invoice`, say so and ask before adding
   fields.
3. Point to (or write) the unit test(s) in `InvoiceApp.Tests` that would fail without this rule.
   A rule with no test that can fail without it is not done.
4. Add one bullet to `AGENTS.md` describing the rule, under the approval-rules section, so a cold
   agent reading the file next time already knows it.
5. Note the rule, its test, and its AGENTS.md line in the next `ai-log` entry's `artifacts`.
