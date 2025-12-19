# Shared Agent Output Format

> All agents reference this for consistent output structure. Saves ~200 tokens per agent definition.

## Required Status

Every agent response MUST end with:

```markdown
---
## Agent Status

**Status**: [COMPLETE | BLOCKED | NEEDS_INPUT]
**Confidence**: [HIGH | MEDIUM | LOW]
**Confidence Reasoning**: [1-2 sentences why this confidence level]

**If BLOCKED**:
- Blocked by: [What's preventing progress]
- Need: [What would unblock]

**If NEEDS_INPUT**:
- Question: [What clarification is needed]
- Options: [If applicable, list choices]

**Handoff Notes**: [Key findings for next agent or orchestrator]
```

## Status Definitions

| Status | Meaning | Orchestrator Action |
|--------|---------|---------------------|
| COMPLETE | Task finished successfully | Continue to next agent or synthesize |
| BLOCKED | Cannot proceed | Route to unblocking agent or ask user |
| NEEDS_INPUT | Need clarification | Ask user, then resume or re-spawn |

## Context Acknowledgment (REQUIRED for collaborative tasks)

When working on a task with a task ID, you MUST confirm context was read:

```markdown
---
## Context Acknowledgment

- **Context Read**: YES / NO / N/A (no prior context)
- **Context Path**: `workspace/[task-id]/context.md`
- **Prior Agents**: [List agents who contributed before you, or "None"]
- **Key Context Used**: [1-2 sentences: What you learned from prior work]
```

**Rules**:
- If prior agents contributed → you MUST read context.md first
- Report what you learned from prior context in your output
- If you skip reading context, orchestrator will reject your output

---

## Behavioral Guidelines (All Agents)

1. **Read context FIRST**: For collaborative tasks, read `workspace/[task-id]/context.md` before anything
2. **Read your definition**: Load your agent file for role clarity
3. **Read knowledge base**: Load domain expertise before acting
4. **Stay in scope**: Only handle your domain, escalate others
5. **Be explicit**: State assumptions, don't guess silently
6. **Document findings**: Future agents may need your discoveries
7. **Fail fast**: Report BLOCKED early, don't spin on impossible tasks
8. **Update parallel findings**: If spawned in parallel, add your findings to context immediately
9. **Self-reflect**: Run the self-reflection checklist before finalizing (see `knowledge/self-reflection.md`)
10. **Report confidence**: Include confidence level (HIGH/MEDIUM/LOW) with reasoning in status

---

## Self-Reflection Reference

Before finalizing output, run through `knowledge/self-reflection.md` checklist:
- Task alignment check
- Assumption check
- Error analysis
- Confidence assessment

**Status must include**:
```markdown
**Confidence**: [HIGH | MEDIUM | LOW]
**Confidence Reasoning**: [Why this level]
```

See `knowledge/self-reflection.md` for full protocol.

---

## Model Selection Reference

**Two-Tier System** (No Haiku - quality over speed):

| Agent | Model | Rationale |
|-------|-------|-----------|
| architect-agent | **Opus** | Design decisions cascade |
| ticket-analyst-agent | **Opus** | Requirements understanding critical |
| reviewer-agent | **Opus** | Final quality gate |
| All others (15) | Sonnet | Escalate on LOW confidence or triggers |

**Escalation**: If you report LOW confidence or BLOCKED, orchestrator may retry with Opus.

See `knowledge/model-selection.md` and RULE-013 in CLAUDE.md for full details.
