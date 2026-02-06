#!/usr/bin/env node
/**
 * Subagent Initializer - scaffolds a Codely CLI custom agent (subagent) config.
 *
 * Format: legacy Codely TOML ([agent] table) only.
 *
 * Usage:
 *   node init_subagent.mjs <agent-name> [--scope project|user] [--project-root <path>]
 *                          [--description <text>] [--force]
 */

import * as fs from 'node:fs/promises';
import * as os from 'node:os';
import * as path from 'node:path';

function printHelp() {
  // Keep this short; the goal is script-driven creation with minimal user involvement.
  process.stdout.write(`Usage:
  node init_subagent.mjs <agent-name> [--scope project|user] [--project-root <path>] [--description <text>] [--force]

Examples:
  node init_subagent.mjs testing-expert
  node init_subagent.mjs api-analyzer --scope user --description "Analyzes APIs"
`);
}

function titleCaseAgentName(baseName) {
  const parts = baseName.trim().split(/[-_]+/).filter(Boolean);
  if (parts.length === 0) return baseName;
  return parts.map((p) => p.charAt(0).toUpperCase() + p.slice(1)).join(' ');
}

function toSnakeCase(name) {
  return name.replace(/-/g, '_');
}

function validateBaseName(baseName) {
  if (!baseName) {
    throw new Error('Agent name is empty.');
  }
  if (baseName.length > 80) {
    throw new Error('Agent name is too long (max 80 characters).');
  }
  if (!/^[a-z0-9][a-z0-9-_]*$/.test(baseName)) {
    throw new Error(
      'Invalid agent name. Use lowercase letters, digits, hyphens, and underscores only (must start with a letter or digit).',
    );
  }
}

function escapeTomlBasicString(value) {
  return String(value ?? '')
    .replace(/\\/g, '\\\\')
    .replace(/"/g, '\\"')
    .replace(/\r?\n/g, ' ')
    .trim();
}

function buildLegacyTomlTemplate({ baseName, agentId, description }) {
  const displayName = titleCaseAgentName(baseName);
  const desc = escapeTomlBasicString(description);

  // Keep the generated file small; fill details by editing TODO sections.
  return `#### Template guide
#### - Lines starting with '####' are descriptive comments.
#### - Tool allowlist: empty list means ALL tools are allowed (default).
#### - Streaming: subagents default to non-stream (recommended).
#### - Reserved inputs: 'task' is always required; 'agent_name' is forbidden.

description = "${desc}"

[agent]
name = "${agentId}"
system_prompt = """
You are the ${displayName} agent.

TODO: Describe this agent's specialization (what it is best at, and what it should avoid).

You run as a non-interactive subagent (do NOT ask the user questions). Work autonomously using tools and report back to the caller with a crisp, evidence-based result.

When invoked:
- Restate the objective briefly (1–2 sentences).
- Make a short plan (2–5 bullets).
- Use tools to gather evidence and iterate until done.
- Prefer the smallest safe changes; inspect before editing.
- Finish by calling complete_task with: status, result (with evidence), next_steps.
"""

# Optional. If omitted, the runtime will inject only the task as the first user message.
# query = "\${task}"

[agent.run_config]
stream = false
# max_time_minutes = 10
# max_turns = 200

[agent.tools]
#### Tool allowlist (empty = ALL tools)
allowed_tools = []

[agent.skills]
#### Skills allowlist (empty = ALL skills)
#### Only relevant when the main session has --experimental-skills enabled.
allowed_skills = []

[validation]
#### Input schema for delegate_to_agent wrapper params
#### - Supported types: string | number | boolean | integer | string[] | number[]
#### - Optional: append "?" to make a param optional (e.g. "string?")
#### Note: 'task' is reserved and must be a required string if specified.
input_schema = { task = "string", context = "string?", constraints = "string?", output_format = "string?" }

[example]
inputs = { task = "Describe what to do", context = "optional background", constraints = "optional constraints", output_format = "optional format" }
description = "Example invocation"
`;
}

function parseArgs(argv) {
  const args = argv.slice(2);
  const out = {
    agentName: '',
    scope: 'project',
    projectRoot: '.',
    description: undefined,
    force: false,
    help: false,
  };

  const takeValue = (i) => {
    if (i + 1 >= args.length) {
      throw new Error(`Missing value for ${args[i]}`);
    }
    return args[i + 1];
  };

  // First non-flag token = agent name.
  for (let i = 0; i < args.length; i++) {
    const token = args[i];
    if (!token) continue;

    if (token === '--help' || token === '-h') {
      out.help = true;
      continue;
    }
    if (token === '--force') {
      out.force = true;
      continue;
    }
    if (token === '--scope') {
      out.scope = takeValue(i);
      i += 1;
      continue;
    }
    if (token === '--project-root') {
      out.projectRoot = takeValue(i);
      i += 1;
      continue;
    }
    if (token === '--description') {
      out.description = takeValue(i);
      i += 1;
      continue;
    }

    if (!token.startsWith('-') && !out.agentName) {
      out.agentName = token;
      continue;
    }
  }

  return out;
}

function resolveOutputDir(scope, projectRoot) {
  if (scope === 'user') {
    return path.join(os.homedir(), '.codely-cli', 'agents');
  }
  return path.join(projectRoot, '.codely-cli', 'agents');
}

async function main() {
  const opts = parseArgs(process.argv);
  if (opts.help || !opts.agentName) {
    printHelp();
    process.exit(opts.agentName ? 0 : 1);
  }

  const baseName = String(opts.agentName)
    .trim()
    .replace(/\.(toml|md|markdown|yaml|yml)$/i, '');
  validateBaseName(baseName);

  const agentId = toSnakeCase(baseName);
  const displayName = titleCaseAgentName(baseName);
  const description =
    typeof opts.description === 'string' && opts.description.trim().length > 0
      ? opts.description.trim()
      : `Specialized agent for ${displayName.toLowerCase()} tasks`;

  const projectRoot = path.resolve(String(opts.projectRoot ?? '.'));
  const scope = opts.scope === 'user' ? 'user' : 'project';
  const outDir = resolveOutputDir(scope, projectRoot);
  await fs.mkdir(outDir, { recursive: true });

  const outFile = path.join(outDir, `${baseName}.toml`);
  try {
    await fs.access(outFile);
    if (!opts.force) {
      process.stdout.write(
        `[ERROR] Agent file already exists: ${outFile}\n        Re-run with --force to overwrite.\n`,
      );
      process.exit(1);
    }
  } catch {
    // File doesn't exist, OK.
  }

  const content = buildLegacyTomlTemplate({ baseName, agentId, description });
  await fs.writeFile(outFile, content, 'utf8');

  process.stdout.write(`[OK] Agent scaffold created\n`);
  process.stdout.write(`     Scope: ${scope}\n`);
  process.stdout.write(`     File:  ${outFile}\n\n`);
  process.stdout.write(`Next steps:\n`);
  process.stdout.write(`1) Edit system_prompt / tools / input_schema as needed\n`);
  process.stdout.write(`2) In Codely CLI: run /agents reload\n`);
  process.stdout.write(`3) Select it with @${agentId} and delegate a real task\n`);
}

main().catch((error) => {
  process.stderr.write(`[ERROR] ${error instanceof Error ? error.message : String(error)}\n`);
  process.exit(1);
});

