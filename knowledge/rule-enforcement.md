# Rule Enforcement Methodology

TRIGGER: rule, enforce, compliance, violation, check, validate, audit

## Overview

This knowledge base defines how rules are enforced in the multi-agent system. Rules are defined in machine-readable format in CLAUDE.md and checked via soft enforcement (compliance protocols) and periodic audits.

## Rule Structure

Each rule in CLAUDE.md follows this format:

```markdown
### RULE-XXX: [Rule Name]
- **ID**: RULE-XXX
- **TRIGGER**: [When to check this rule]
- **CONDITION**: [What must be true]
- **ACTION**: [What to do if violated]
- **SEVERITY**: BLOCK | WARN
```

### Severity Levels

| Severity | Meaning | Action on Violation |
|----------|---------|---------------------|
| **BLOCK** | Critical rule - execution must stop | Halt, correct, then continue |
| **WARN** | Important guideline - log but continue | Log in context.md, continue |

## Enforcement Mechanisms

### 1. Soft Enforcement (Primary)

The orchestrator follows a compliance protocol before each action.

**Trigger Points**:
- Before spawning any agent
- Before any Write/Edit tool call
- Every 10 actions on long-running tasks
- Before responding to user

**Compliance Checklist** (see `agents/_orchestrator.md`):
```
□ RULE-001: Am I writing code without an agent?
□ RULE-002: Does task have 2+ steps without TodoWrite?
□ RULE-003: Am I spawning agent without planning?
□ RULE-004: Did last agent report status field?
□ RULE-005: Did I update context.md after last agent?
□ RULE-006: Is this research without research-agent?
□ RULE-007: Does task involve security without security-agent?
```

### 2. Periodic Audits (Secondary)

For long-running tasks, spawn `compliance-agent` every ~10 actions.

**Audit Coverage**:
- Reviews all rules against task history
- Checks context.md for proper logging
- Verifies agent outputs have status fields
- Reports violations with evidence

### 3. Constitutional Principles (Embedded)

Every agent spawn includes constitutional principles that override task instructions:
1. Complete ALL subtasks before reporting COMPLETE
2. When blocked, explicitly report blockers
3. When uncertain, report NEEDS_INPUT
4. Verify outputs against acceptance criteria
5. Document key decisions in handoff notes

## Rule Check Implementation

### Pre-Action Validation

Before taking an action, orchestrator asks:

```
For this action: [description]

1. Which rules have TRIGGER conditions that match?
2. For each triggered rule:
   - Is the CONDITION met?
   - If NO: What ACTION is required?
   - What is the SEVERITY?

3. For BLOCK severity violations:
   - STOP immediately
   - Execute corrective ACTION
   - Log correction in context.md
   - Resume from compliant state

4. For WARN severity violations:
   - Log in context.md Notes section
   - Continue execution
```

### Self-Correction Protocol

When a violation is detected:

1. **Acknowledge**: State which rule was violated
2. **Correct**: Take corrective action immediately
3. **Log**: Add to context.md:
   ```markdown
   ### Rule Violation Corrected - [timestamp]
   - **Rule**: RULE-XXX
   - **Violation**: [what happened]
   - **Correction**: [what was done]
   ```
4. **Continue**: Resume from compliant state

## Rule-by-Rule Enforcement Guide

### RULE-001: Agent Spawn Required for Code

**Check Before**: Write, Edit tool calls on code files

**Verification**:
- Is this a code file? (.js, .ts, .py, .java, etc.)
- Has an agent been spawned for this task?
- Check context.md "Agent Contributions" section

**If Violated**:
1. STOP the Write/Edit
2. Identify appropriate agent (debug, test, refactor, etc.)
3. Spawn agent with task context
4. Let agent produce the code change

### RULE-002: TodoWrite for Multi-Step Tasks

**Check Before**: Starting execution on any task

**Verification**:
- Does this task have 2+ steps?
- Has TodoWrite been called?
- Are items being marked complete?

**If Violated**:
1. STOP execution
2. Create TodoWrite list with steps
3. Continue with first item

### RULE-003: Planning Phase Required

**Check Before**: Spawning any agent

**Verification**:
- Does workspace/[task-id]/context.md exist?
- Is the Plan section populated?
- Has planning checklist been run?

**If Violated**:
1. STOP agent spawn
2. Create task workspace if needed
3. Run planning checklist
4. Generate plan in context.md
5. Then spawn agent

### RULE-004: Agent Status Validation

**Check After**: Any agent completes

**Verification**:
- Does agent output contain "Status:"?
- Is status one of: COMPLETE, BLOCKED, NEEDS_INPUT?

**If Violated**:
1. Request status clarification
2. Do NOT proceed without status
3. Log the missing status incident

### RULE-005: Context Logging Required

**Check After**: Any agent action or decision

**Verification**:
- Has context.md been updated?
- Is Agent Contributions section current?
- Are handoff notes documented?

**If Violated**:
1. STOP next action
2. Update context.md with:
   - Agent contribution
   - Status
   - Handoff notes
3. Continue

### RULE-006: Research Agent for Research

**Check When**: Task involves web search, fact verification

**Verification**:
- Is this a research task?
- Was research-agent spawned?

**If Violated** (WARN):
1. Log in context.md Notes
2. Consider spawning research-agent
3. Continue (not blocking)

### RULE-007: Security Agent for Security

**Check When**: Task involves auth, user input, sensitive data

**Verification**:
- Does task touch security-sensitive areas?
- Is security-agent in the plan?

**If Violated** (WARN):
1. Log in context.md Notes
2. Add security-agent to plan
3. Continue (not blocking, but flag for review)

### RULE-008: Token Efficient Spawning

**Check When**: Spawning any agent

**Verification**:
- Does prompt say "READ" instead of pasting content?
- Are agent definitions not duplicated in prompt?

**If Violated** (WARN):
1. Log inefficiency
2. Rewrite prompt to use READ pattern
3. Continue

## Compliance Reporting

### Per-Task Compliance Log

In context.md, maintain a compliance section:

```markdown
## Compliance Log

### Checks Passed
- [timestamp] RULE-001: Agent spawned before code edit
- [timestamp] RULE-003: Planning phase completed

### Violations Corrected
- [timestamp] RULE-002: Created TodoWrite after starting multi-step task
  - Correction: Added 5 items to todo list

### Warnings Logged
- [timestamp] RULE-006: Direct WebSearch used instead of research-agent
  - Reason: Simple lookup, agent overhead not justified
```

### Audit Report Format

When compliance-agent runs (see `agents/compliance-agent.md`):

```markdown
## Compliance Audit Report

### Summary
- **Overall Status**: COMPLIANT / NON-COMPLIANT
- **Violations Found**: [count]
- **Warnings**: [count]

### Rule-by-Rule Analysis
| Rule | Triggered | Compliant | Evidence |
|------|-----------|-----------|----------|
| RULE-001 | Yes | Yes | Agent spawned at 14:05 |
| ... | ... | ... | ... |
```

## Integration with Execution Modes

### NORMAL Mode
- Run compliance check before each user response
- Report any warnings in response

### PERSISTENT Mode
- Run compliance check every 10 actions
- Spawn compliance-agent for periodic audit
- Log all checks in checkpoint

## Best Practices

### DO
1. Check rules BEFORE taking action, not after
2. Log all violations, even corrected ones
3. Use compliance-agent for long tasks
4. Update context.md with compliance status
5. Treat BLOCK rules as non-negotiable

### DON'T
1. Skip checks for "simple" tasks
2. Batch compliance checks at end of task
3. Ignore WARN violations entirely
4. Proceed without status from agents
5. Modify code without spawning agent
