---
name: subagent-creator
description: Guide for creating and updating Codely CLI custom agents (subagents) using the legacy Codely TOML format in .codely-cli/agents or ~/.codely-cli/agents.
license: Complete terms in LICENSE.txt
---

# Subagent Creator

## Overview

Use this skill to create, validate, and iterate on Codely CLI custom agents (a.k.a. subagents). Subagents are loaded from agent config files and invoked via the single delegation entry point (`delegate_to_agent`) and the `@agent_id` selector.

## Key Facts (Codely CLI specifics)

### Discovery locations (priority order)

- Project: `.codely-cli/agents/` (higher priority; share with team)
- User: `~/.codely-cli/agents/` (fallback; personal)

### Templating rules

- Legacy TOML agents support `${var}` interpolation from tool inputs.

### Reserved inputs

- `task` is reserved and always required.
  - If `task` is present in `input_schema`, it MUST be a required string (`"string"`). Do not make it optional.
- `agent_name` is reserved for routing in `delegate_to_agent`. Never define it in `input_schema`.

### Anti-recursion

- `delegate_to_agent` is automatically removed from an agent tool list (to prevent subagents calling subagents).

### Streaming default

- Prefer `stream = false` for subagents (helps avoid provider streaming + tool-calling edge cases).

## When to Use This Skill

Activate this skill when asked to:

- Create a new subagent/custom agent for a focused workflow (tests, refactors, docs, investigations, etc.)
- Update or harden an existing agent prompt/tools/inputs

## Subagent Creation Workflow (recommended)

### Step 1: Derive creation params from the user's context (offer options; don't ask for free-form input)

When the user provides a folder and/or documentation (attachments, paths, pasted docs), treat it as candidate subagent context. Use it to propose a small set of concrete creation parameter options instead of asking the user to invent names/descriptions.

Do:

- Inspect the provided context quickly (list key directories, read the main docs/README, grep for keywords).
- Infer what the subagent should specialize in (single responsibility).
- Produce 2-3 parameter options (A/B/C) as user-readable choices.
- Ask the user to choose A/B/C (or proceed automatically if the user's intent clearly matches one option). Avoid asking "what should I name it?" style questions.

IMPORTANT:

- Do NOT show the actual script/CLI command line to the user.
- Options must be readable: show only `name`, `description`, and optionally `scope` (project/user) in plain language.
- After the user chooses an option, run the script internally to create the agent file.

Parameter options MUST include:

- `agent-name` (kebab-case base name; becomes snake_case `agent_id` automatically)
- `--description` (one concise sentence)
- `--scope` (project by default; user only when it's personal/non-repo specific)

After the user picks an option (or one is chosen automatically), proceed to create the file and then edit it to:

- Make the agent's scope explicit (what it does / does not do).
- Keep inputs minimal and stable:
  - Always include `task`
  - Prefer optional: `context`, `constraints`, `output_format`
- Restrict tools only after the prompt is stable (start with `allowed_tools = []` and tighten later).

#### Few-shot examples (offer options based on provided context)

##### Example 1: User provides a folder with security docs

User: "I attached a folder with security checklist/OWASP docs. Create a security scanning subagent."

Assistant (offer options, not questions):

- Option A (project, repo-wide):
  - name: `security-scanner`
  - description: Scans the repository for common security issues (secrets, injection, insecure configs) and reports actionable findings.
- Option B (project, secrets-focused):
  - name: `secrets-hunter`
  - description: Finds hardcoded secrets and credential leaks with minimal false positives.
- Option C (user, personal workflow):
  - name: `security-audit-helper`
  - description: Assists with security review checklists and writeups across projects.
  - scope: user

Then: create the chosen file, update `system_prompt` to follow the checklist, and keep `allowed_tools = []` initially.

##### Example 2: User provides an API spec / docs bundle

User: "I attached an API docs/spec bundle (endpoints/schema/examples). Create a subagent that reads it and produces an integration plan."

Assistant (offer options):

- Option A (project integration planner):
  - name: `api-integration-planner`
  - description: Reads API docs/specs and produces an implementation plan with endpoints, auth, and error handling.
- Option B (project API summarizer):
  - name: `api-doc-summarizer`
  - description: Summarizes API docs into a concise reference and highlights risky/unclear parts.
- Option C (user, reusable across repos):
  - name: `api-onboarding-helper`
  - description: Helps onboard new APIs by extracting auth, endpoints, and example calls.
  - scope: user

##### Example 3: User provides a code folder for a narrow workflow

User: "I attached `packages/core/src/agents/`. Create a subagent that analyzes the agents/subagents system and proposes safe changes."

Assistant (offer options):

- Option A (project investigator):
  - name: `agents-architecture-investigator`
  - description: Analyzes the agents/subagents system code and proposes minimal, safe changes with evidence.
- Option B (project refactor helper):
  - name: `agents-refactor-helper`
  - description: Plans and executes small refactors in the agents subsystem while keeping behavior stable.

### Step 2: Create the Agent File (script-driven; legacy TOML only)

This skill is meant to run in interactive mode, but the user should NOT need to type `/agents create` or hand-write TOML.

Instead:

- Collect the minimal inputs (agent name, one-line description, scope).
- Run the bundled script to generate the legacy TOML file.

From project root:

```bash
node .codely-cli/skills/subagent-creator/scripts/init_subagent.mjs agent-name
```

Optional (global/user scope):

```bash
node .codely-cli/skills/subagent-creator/scripts/init_subagent.mjs agent-name --scope user
```

Validate quickly (optional):

```bash
node .codely-cli/skills/subagent-creator/scripts/quick_validate.mjs <agent-file-path>
```

### Step 3: Fill Prompt and Config Safely

- Keep the system prompt focused on:
  - Role + domain expertise
  - Working loop (inspect -> plan -> act -> verify)
  - Evidence-based handoff for the caller
- Do not mention tool names in the system prompt.
- If optional inputs are included in `input_schema`, reference them in prompts only when they are always provided.

### Step 4: Reload and Smoke Test

- Run `/agents reload`
- Verify with `/agents list` and `/agents info agent_id` (legacy TOML typically uses snake_case ids)
- Trigger delegation by selecting `@agent_id` and giving a real task
- Iterate prompt/tools/inputs until stable and reusable

## Resources

- `scripts/init_subagent.mjs`: scaffold a legacy TOML agent file into the correct location
- `scripts/quick_validate.mjs`: minimal validator for reserved-field constraints and basic structure

