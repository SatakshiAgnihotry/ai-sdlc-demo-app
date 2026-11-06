---
name: ai-log
description: Turn recent raw AI-interaction events into one schema-validated curated log entry, and keep AI-LOG-SUMMARY.md in sync. Use after finishing a meaningful unit of work (a step, a subagent run, a user decision) so the assignment's AI-interaction log stays complete without relying on memory.
---

# ai-log

Use this after any meaningful unit of work in this assignment: a plan step, a subagent run, a
user decision or correction, a test/mutation run, a commit handed to the user.

## What this does

1. Reads the tail of `ai-logs/raw-events.jsonl` (the automatic, hook-written record) for the
   current session to recover accurate tool/subagent/prompt facts — do not reconstruct these
   from memory.
2. Fills in the fields a hook cannot infer: `stage`, `context`, `request`, `skills_used`,
   `subagents_used`, `mcp_tools_used`, `human_decision`, `suggestion`, `limitation`, `status`,
   `artifacts`. Use the reference schema in `SESSION-HANDOFF.md` — every field it lists must be
   present, even if `null`.
3. Appends exactly one JSON object per line to `ai-logs/curated-log.jsonl`. A PreToolUse hook
   validates the write and rejects it if a required field is missing — if rejected, fix the
   entry and retry, do not bypass the hook.
4. Appends a one-line, human-readable bullet to `AI-LOG-SUMMARY.md` describing what happened and
   which human decision (if any) shaped it.

## Field notes

- `id`: `<UTC timestamp, YYYYMMDDTHHMMSSZ>-<short-slug>`
- `agent_type`: `built-in`, `custom`, or `sub-agent` — never leave this as a placeholder
- `human_decision`: only non-null when the user actually made a call (approved, corrected,
  rejected, chose between options) — do not invent one
- `status`: `applied`, `partial`, or `rework` — matches the real outcome, not the intended one
