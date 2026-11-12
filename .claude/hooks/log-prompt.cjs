#!/usr/bin/env node
// UserPromptSubmit hook: appends one raw event per prompt to ai-logs/raw-events.jsonl.
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
    type: 'prompt',
    session_id: input.session_id || null,
    prompt: typeof input.prompt === 'string' ? input.prompt.slice(0, 4000) : null,
  };

  fs.appendFileSync(path.join(logDir, 'raw-events.jsonl'), JSON.stringify(event) + '\n');
  process.exit(0);
}

main();
