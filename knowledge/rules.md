# System Rules Reference

TRIGGER: rule, RULE-, violation, compliance, block, warn, enforcement

## Overview

This file contains the complete definition of all 15 system rules. These rules are enforced by the orchestrator and checked by compliance-agent.

## Rule Format

Each rule has:
- **ID**: Unique identifier (RULE-XXX)
- **TRIGGER**: When to check this rule
- **CONDITION**: What must be true
- **ACTION**: What to do if violated
- **SEVERITY**: BLOCK (halt execution) or WARN (log and continue)

---

## BLOCK Severity Rules (Must Halt on Violation)

### RULE-001: Agent Spawn Required for Code Changes
- **ID**: RULE-001
- **TRIGGER**: Before any Write/Edit tool call on code files
- **CONDITION**: An appropriate agent has been spawned for this task
- **ACTION**: HALT. Spawn appropriate agent before editing code.
- **SEVERITY**: BLOCK

**Agent Mapping**:
- Testing code → test-agent
- Bug fixes → debug-agent
- Architecture decisions → architect-agent
- Security changes → security-agent
- Refactoring → refactor-agent

---

### RULE-002: TodoWrite for Multi-Step Tasks
- **ID**: RULE-002
- **TRIGGER**: After identifying task has 2+ steps
- **CONDITION**: TodoWrite has been called with task items
- **ACTION**: HALT. Create todo list before proceeding.
- **SEVERITY**: BLOCK

---

### RULE-003: Planning Phase Required
- **ID**: RULE-003
- **TRIGGER**: Before spawning any agent
- **CONDITION**: workspace/[task-id]/context.md exists with Plan section populated
- **ACTION**: HALT. Complete planning phase first.
- **SEVERITY**: BLOCK

**Planning Checklist** (ALL 7 domains):
1. Testing - New code, behavior changes, bug fixes?
2. Documentation - API changes, config changes, user features?
3. Security - Auth, user input, sensitive data, DB queries?
4. Architecture - New component, design decisions, integrations?
5. Performance - Large loops, DB queries, caching, hot paths?
6. Review - Code changes ready for merge?
7. Clarity - Vague request, missing acceptance criteria?

---

### RULE-004: Agent Status Validation
- **ID**: RULE-004
- **TRIGGER**: After any agent completes
- **CONDITION**: Agent output contains Status: COMPLETE | BLOCKED | NEEDS_INPUT
- **ACTION**: HALT. Request status if missing. Do not proceed without it.
- **SEVERITY**: BLOCK

**Status Handling**:
- COMPLETE → Continue to next step
- BLOCKED → Resolve blocker before continuing
- NEEDS_INPUT → Get user clarification

---

### RULE-005: Context Logging Required
- **ID**: RULE-005
- **TRIGGER**: After any agent action or orchestrator decision
- **CONDITION**: workspace/[task-id]/context.md updated with contribution
- **ACTION**: HALT. Update context.md before continuing.
- **SEVERITY**: BLOCK

**Required Log Fields**:
- Agent name and timestamp
- Task assigned
- Status returned
- Key findings
- Handoff notes

---

### RULE-010: Playwright MCP Tool Usage Required
- **ID**: RULE-010
- **TRIGGER**: When using Playwright for browser interaction
- **CONDITION**: Using mcp__playwright_* tools directly (NOT writing code)
- **ACTION**: HALT. Use MCP tools instead of writing Playwright code.
- **SEVERITY**: BLOCK

**Required Tools**:
- mcp__playwright_browser_navigate
- mcp__playwright_browser_click
- mcp__playwright_browser_type
- mcp__playwright_browser_snapshot

---

### RULE-012: Self-Reflection Required
- **ID**: RULE-012
- **TRIGGER**: Before any agent reports COMPLETE status
- **CONDITION**: Agent has performed self-reflection checklist
- **ACTION**: HALT. Run self-reflection, include confidence level.
- **SEVERITY**: BLOCK

**Required Output**:
- Confidence: HIGH | MEDIUM | LOW
- Confidence Reasoning: 1-2 sentences

---

### RULE-014: No Stopping in PERSISTENT Mode
- **ID**: RULE-014
- **TRIGGER**: When task mode is PERSISTENT and considering asking user
- **CONDITION**: All completion criteria are MET or tokens exhausted
- **ACTION**: HALT any question/stopping until criteria met. Auto-continue.
- **SEVERITY**: BLOCK

**Blocked Phrases in PERSISTENT Mode**:
- "Shall I continue?"
- "Would you like..."
- "Let me know if..."
- "Should I proceed..."

---

### RULE-015: Ask Before Migrations and Deployments
- **ID**: RULE-015
- **TRIGGER**: Before running migration, deployment, or database-altering command
- **CONDITION**: User has explicitly approved this specific operation
- **ACTION**: HALT. Ask user for confirmation before proceeding.
- **SEVERITY**: BLOCK

**Always Ask Before**:
- Database migrations (migrate, db push, prisma migrate, etc.)
- Database seeding
- Deployments (deploy, publish, release)
- Production operations
- Schema changes

---

## WARN Severity Rules (Log and Continue)

### RULE-006: Research Agent for Research Tasks
- **ID**: RULE-006
- **TRIGGER**: Task involves web search, fact verification, external info
- **CONDITION**: research-agent spawned (not direct WebSearch/WebFetch)
- **ACTION**: Log warning. Consider spawning research-agent.
- **SEVERITY**: WARN

---

### RULE-007: Security Agent for Security Tasks
- **ID**: RULE-007
- **TRIGGER**: Task involves auth, user input, sensitive data, DB queries
- **CONDITION**: security-agent spawned or explicitly consulted
- **ACTION**: Log warning. Add security-agent to plan.
- **SEVERITY**: WARN

**Triggers**:
- Authentication/authorization
- User input handling
- Sensitive data processing
- Database queries
- HTTP requests to external services
- File system operations
- Payment processing

---

### RULE-008: Token Efficient Agent Spawning
- **ID**: RULE-008
- **TRIGGER**: When spawning any agent via Task tool
- **CONDITION**: Agent prompt instructs to READ files, not paste content
- **ACTION**: Rewrite prompt to use READ pattern.
- **SEVERITY**: WARN

**Correct Pattern**:
```
## Your Role
You are [agent-name]. READ agents/[agent-name].md for your definition.

## Your Knowledge
READ knowledge/[topic].md for domain expertise.
```

---

### RULE-009: Browser URL Access Policy
- **ID**: RULE-009
- **TRIGGER**: Before any browser navigation via Playwright MCP
- **CONDITION**: URL matches access policy
- **ACTION**: Ask user before navigating to external URLs.
- **SEVERITY**: WARN

**Auto-Allowed**:
- localhost:*, 127.0.0.1:*
- OAuth providers (b2clogin.com, auth0.com, etc.)

**Requires Permission**:
- Production URLs
- External domains

---

### RULE-011: Windows File Edit Resilience
- **ID**: RULE-011
- **TRIGGER**: Edit/Write tool fails with "unexpectedly modified" error
- **CONDITION**: On Windows platform
- **ACTION**: Retry with workarounds.
- **SEVERITY**: WARN

**Workaround Priority**:
1. Use relative paths
2. Read immediately before edit
3. Fall back to Bash commands
4. Create new file + rename

---

### RULE-013: Model Selection for Agents
- **ID**: RULE-013
- **TRIGGER**: When spawning any agent via Task tool
- **CONDITION**: Model explicitly specified AND matches criteria
- **ACTION**: Apply decision tree, specify model in Task call.
- **SEVERITY**: WARN

**Always Opus**:
- architect-agent
- ticket-analyst-agent
- reviewer-agent

**Default Sonnet**: All other agents

**Opus Escalation Triggers**:
- 4+ domains in Planning Checklist
- 10+ subtasks identified
- Production/payment/auth code
- Agent reports LOW confidence or BLOCKED

---

## Quick Compliance Check

Before any action, ask:
1. Am I editing code without an agent? → RULE-001
2. Multi-step task without TodoWrite? → RULE-002
3. Spawning agent without planning? → RULE-003
4. Agent missing status field? → RULE-004
5. Did I update context.md? → RULE-005
6. Research task without research-agent? → RULE-006
7. Security task without security-agent? → RULE-007
