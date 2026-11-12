#!/usr/bin/env node
// PreToolUse hook (matcher: Write|Edit): rejects writes to ai-logs/curated-log.jsonl whose new
// lines don't look like valid, schema-complete curated log entries. Exit code 2 blocks the tool
// call and returns this script's stderr to the model as feedback.
const fs = require('fs');

const REQUIRED_FIELDS = [
  'id', 'timestamp', 'stage', 'agent', 'agent_name', 'agent_type',
  'ai_tool', 'model', 'mode', 'context', 'request', 'status',
];

function readStdin() {
  try {
    return fs.readFileSync(0, 'utf8');
  } catch {
    return '';
  }
}

function targetsCuratedLog(filePath) {
  if (!filePath) return false;
  return filePath.replace(/\\/g, '/').endsWith('ai-logs/curated-log.jsonl');
}

function candidateText(toolName, toolInput) {
  if (toolName === 'Write') return toolInput.content || '';
  if (toolName === 'Edit') return toolInput.new_string || '';
  return '';
}

function validateLine(line) {
  let entry;
  try {
    entry = JSON.parse(line);
  } catch (error) {
    return `not valid JSON (${error.message})`;
  }
  const missing = REQUIRED_FIELDS.filter((field) => !(field in entry));
  if (missing.length > 0) return `missing required field(s): ${missing.join(', ')}`;
  return null;
}

function main() {
  let input = {};
  try {
    input = JSON.parse(readStdin());
  } catch {
    process.exit(0);
  }

  const toolName = input.tool_name;
  const toolInput = input.tool_input || {};

  if (toolName !== 'Write' && toolName !== 'Edit') process.exit(0);
  if (!targetsCuratedLog(toolInput.file_path)) process.exit(0);

  const lines = candidateText(toolName, toolInput)
    .split('\n')
    .map((line) => line.trim())
    .filter(Boolean);

  for (const line of lines) {
    const error = validateLine(line);
    if (error) {
      process.stderr.write(
        `Rejected write to ai-logs/curated-log.jsonl: ${error}\nOffending line: ${line}\n`
      );
      process.exit(2);
    }
  }

  process.exit(0);
}

main();
