#!/usr/bin/env node
// One-off import: extract session 1's transcript into a plain, readable form so curated
// AI-interaction log entries for it can be composed from real transcript data instead of
// memory. Session 1 ran before this repo's hooks existed, from a different project folder.
//
// Usage: node scripts/import-session1-transcript.mjs <path-to-transcript.jsonl> [out-path]
//
// Output is a draft extract only (ai-logs/session1-transcript-extract.jsonl by default) — it is
// not written into ai-logs/curated-log.jsonl directly, since composing a curated entry still
// needs a human-reviewed judgment call (stage, human_decision, status), not just raw turn text.

import fs from 'node:fs';
import path from 'node:path';
import readline from 'node:readline';

const TRANSCRIPT_PATH = process.argv[2];
const OUT_PATH = process.argv[3] || 'ai-logs/session1-transcript-extract.jsonl';

if (!TRANSCRIPT_PATH) {
  console.error('Usage: node scripts/import-session1-transcript.mjs <path-to-transcript.jsonl> [out-path]');
  process.exit(1);
}

function extractText(content) {
  if (typeof content === 'string') return content;
  if (!Array.isArray(content)) return null;
  const text = content
    .filter((block) => block && block.type === 'text' && typeof block.text === 'string')
    .map((block) => block.text)
    .join('\n')
    .trim();
  return text || null;
}

function extractToolCalls(content) {
  if (!Array.isArray(content)) return [];
  return content
    .filter((block) => block && block.type === 'tool_use')
    .map((block) => ({
      tool: block.name,
      input_summary: JSON.stringify(block.input || {}).slice(0, 300),
    }));
}

async function main() {
  const rl = readline.createInterface({
    input: fs.createReadStream(TRANSCRIPT_PATH, 'utf8'),
    crlfDelay: Infinity,
  });

  const turns = [];
  for await (const line of rl) {
    if (!line.trim()) continue;

    let entry;
    try {
      entry = JSON.parse(line);
    } catch {
      continue;
    }

    const role = entry.type;
    if (role !== 'user' && role !== 'assistant') continue;

    const message = entry.message || {};
    const text = extractText(message.content);
    const toolCalls = extractToolCalls(message.content);
    if (!text && toolCalls.length === 0) continue;

    turns.push({
      timestamp: entry.timestamp || null,
      role,
      text: text ? text.slice(0, 2000) : null,
      tool_calls: toolCalls,
    });
  }

  fs.mkdirSync(path.dirname(OUT_PATH), { recursive: true });
  fs.writeFileSync(OUT_PATH, turns.map((turn) => JSON.stringify(turn)).join('\n') + '\n');
  console.log(`Wrote ${turns.length} turns to ${OUT_PATH}`);
}

main();
