#!/usr/bin/env node
// PostToolUse hook: appends one raw event per tool call to ai-logs/raw-events.jsonl.
const fs = require('fs');
const path = require('path');

function readStdin() {
  try {
    return fs.readFileSync(0, 'utf8');
  } catch {
    return '';
  }
}

function summarize(value, maxLen) {
  if (value == null) return null;
  const str = typeof value === 'string' ? value : JSON.stringify(value);
  return str.length > maxLen ? str.slice(0, maxLen) + '…' : str;
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

  const toolResponse = input.tool_response || {};
  const event = {
    timestamp: new Date().toISOString(),
    type: 'tool_use',
    session_id: input.session_id || null,
    tool_name: input.tool_name || null,
    input_summary: summarize(input.tool_input, 500),
    outcome: toolResponse.error ? 'error' : 'ok',
  };

  fs.appendFileSync(path.join(logDir, 'raw-events.jsonl'), JSON.stringify(event) + '\n');
  process.exit(0);
}

main();
