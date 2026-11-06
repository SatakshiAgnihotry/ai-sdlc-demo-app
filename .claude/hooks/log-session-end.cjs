#!/usr/bin/env node
// SessionEnd hook: appends one raw event marking the end of a session to ai-logs/raw-events.jsonl.
const fs = require('fs');
const path = require('path');

function readStdin() {
  try {
    return fs.readFileSync(0, 'utf8');
  } catch {
    return '';
  }
}

function main() {
  let input = {};
  try {
    input = JSON.parse(readStdin());
  } catch {
    process.exit(0);
  }

  const projectDir = process.env.CLAUDE_PROJECT_DIR || process.cwd();
  const logDir = path.join(projectDir, 'ai-logs');
  fs.mkdirSync(logDir, { recursive: true });

  const event = {
    timestamp: new Date().toISOString(),
    type: 'session_end',
    session_id: input.session_id || null,
    reason: input.reason || null,
  };

  fs.appendFileSync(path.join(logDir, 'raw-events.jsonl'), JSON.stringify(event) + '\n');
  process.exit(0);
}

main();
