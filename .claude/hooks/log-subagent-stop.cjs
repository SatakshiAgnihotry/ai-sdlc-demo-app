#!/usr/bin/env node
// SubagentStop hook: appends one raw event per finished subagent run to ai-logs/raw-events.jsonl.
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
    type: 'subagent_stop',
    session_id: input.session_id || null,
    agent_name: input.agent_name || input.subagent_type || null,
    transcript_path: input.transcript_path || null,
  };

  fs.appendFileSync(path.join(logDir, 'raw-events.jsonl'), JSON.stringify(event) + '\n');
  process.exit(0);
}

main();
