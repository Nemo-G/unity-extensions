#!/usr/bin/env node
/**
 * Quick validation for Codely CLI legacy TOML subagents.
 *
 * This validator is intentionally lightweight (regex-based), so it can run in
 * any Node 20+ environment without dependencies.
 *
 * Usage:
 *   node quick_validate.mjs <path/to/agent-file.toml>
 */

import * as fs from 'node:fs/promises';
import * as path from 'node:path';

function printUsage() {
  process.stdout.write('Usage: node quick_validate.mjs <path/to/agent-file.toml>\n');
}

function parseInlineTableStringValues(block) {
  // Extract key="value" pairs (string values only).
  const out = {};
  const re = /([A-Za-z0-9_]+)\s*=\s*"([^"]*)"/g;
  for (const match of block.matchAll(re)) {
    out[match[1]] = match[2];
  }
  return out;
}

async function validateAgentConfig(filePath) {
  const resolved = path.resolve(filePath);
  const stat = await fs.stat(resolved).catch(() => null);
  if (!stat) return { ok: false, message: `File not found: ${resolved}` };
  if (!stat.isFile()) return { ok: false, message: `Not a file: ${resolved}` };
  if (path.extname(resolved).toLowerCase() !== '.toml') {
    return {
      ok: false,
      message: 'Only legacy TOML agent files (.toml) are supported by this validator.',
    };
  }

  const text = await fs.readFile(resolved, 'utf8');

  if (!text.includes('[agent]')) {
    return { ok: false, message: 'Missing required [agent] table.' };
  }

  if (!/^\s*description\s*=\s*".+?"\s*$/m.test(text)) {
    return {
      ok: false,
      message: "Missing required top-level 'description' (non-empty string).",
    };
  }

  if (!/^\s*name\s*=\s*".+?"\s*$/m.test(text)) {
    return { ok: false, message: 'Missing required [agent].name (non-empty string).' };
  }

  if (!text.includes('system_prompt = """')) {
    return { ok: false, message: 'Missing required [agent].system_prompt (triple-quoted string).' };
  }

  // Reserved inputs / schema.
  const schemaMatch = text.match(/input_schema\s*=\s*\{([\s\S]*?)\}/m);
  const warnings = [];
  if (schemaMatch) {
    const schema = parseInlineTableStringValues(schemaMatch[1] ?? '');

    if (Object.prototype.hasOwnProperty.call(schema, 'agent_name')) {
      return {
        ok: false,
        message:
          'Invalid input_schema: input name "agent_name" is reserved. Rename it to something else.',
      };
    }

    if (Object.prototype.hasOwnProperty.call(schema, 'task')) {
      const normalized = String(schema.task ?? '').trim().toLowerCase();
      if (!normalized) {
        return {
          ok: false,
          message:
            "Invalid input_schema: 'task' is reserved and must be a required string (use task = \"string\").",
        };
      }
      if (normalized.endsWith('?')) {
        return {
          ok: false,
          message:
            "Invalid input_schema: 'task' is reserved and must be required (do not use '?'). Use task = \"string\".",
        };
      }
      if (normalized !== 'string' && normalized !== 'str') {
        return {
          ok: false,
          message: "Invalid input_schema: 'task' is reserved and must be a string. Use task = \"string\".",
        };
      }
    }
  }

  // Soft checks (warnings).
  const allowedToolsMatch = text.match(/allowed_tools\s*=\s*\[([\s\S]*?)\]/m);
  if (allowedToolsMatch) {
    const inside = allowedToolsMatch[1] ?? '';
    if (/\bdelegate_to_agent\b/.test(inside)) {
      warnings.push(
        "Warning: 'delegate_to_agent' appears in allowed_tools. It will be removed automatically (anti-recursion).",
      );
    }
  }

  if (/^\s*stream\s*=\s*true\s*$/m.test(text)) {
    warnings.push('Warning: stream=true. Subagents are recommended to run non-streaming (stream=false).');
  }

  const msg = warnings.length > 0 ? `Agent is valid! ${warnings.join(' ')}` : 'Agent is valid!';
  return { ok: true, message: msg };
}

async function main() {
  const filePath = process.argv[2];
  if (!filePath) {
    printUsage();
    process.exit(1);
  }

  const res = await validateAgentConfig(filePath);
  process.stdout.write(`${res.message}\n`);
  process.exit(res.ok ? 0 : 1);
}

main().catch((error) => {
  process.stderr.write(`[ERROR] ${error instanceof Error ? error.message : String(error)}\n`);
  process.exit(1);
});

