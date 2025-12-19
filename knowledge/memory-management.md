# Memory & Context Management Knowledge Base

TRIGGER: memory, context, compact, compaction, session, persist, remember, forget, context window

## Overview

Claude Code has limited context windows. When approaching capacity (~95%), auto-compaction summarizes the conversation. This knowledge base provides strategies for preserving critical context across compaction events and sessions.

## How Compaction Works

### Trigger Points
- **Auto-compact**: Triggered at ~95% context capacity
- **Manual compact**: User runs `/compact` command
- **Clear**: User runs `/clear` to reset conversation

### What Happens During Compaction
1. Claude analyzes entire conversation history
2. Creates concise summary of:
   - What was accomplished
   - Current work in progress
   - Files involved
   - Next steps
   - Key user constraints
3. Summary replaces old conversation messages
4. Session continues with preserved context

### What Survives Compaction
- CLAUDE.md files (loaded fresh each session)
- workspace/[task-id]/context.md (task-specific state)
- Git history
- All files on disk

### What Does NOT Survive
- Detailed conversation history (summarized but lost)
- Working memory of file contents (needs re-reading)
- Agent's "knowledge" of discussed topics
- Nuanced instructions from early in conversation

## Memory Hierarchy

Claude Code loads memories in order of precedence:

| Level | Location | Scope | Loaded |
|-------|----------|-------|--------|
| **Enterprise** | Org-wide policy | All teams | Always |
| **Project** | `./CLAUDE.md` | Team/shared | Always |
| **User** | `~/.claude/CLAUDE.md` | Your projects | Always |
| **Local** | `./CLAUDE.local.md` | Personal | Always |
| **Task** | `workspace/[task-id]/context.md` | Per-task | On access |

## Persistence Strategies

### 1. CLAUDE.md Optimization

**Keep it minimal but reference-rich**:
```markdown
# Project: Name

## Critical Rules
[Only absolute non-negotiables]

## Quick Reference
[One-liner for each key concept]

## File Structure
[Where to find things]

## Import Extended Context
@./agents/_orchestrator.md (for routing)
@./knowledge/organization.md (for workspace)
```

**Anti-patterns**:
- Don't include entire documentation
- Don't include code examples (put in knowledge/)
- Don't include conversation history

### 2. Per-Task Context (workspace/[task-id]/context.md)

**Critical for compaction recovery**:

```markdown
# Task: [ID]

## Quick Resume
[1-2 sentences: What is this task and current status]

## Status: [ACTIVE/BLOCKED/COMPLETE]

## Key Files
[List of relevant files for quick re-reading]

## Agent Contributions
[What each agent discovered/produced]

## Next Steps
[What to do when resuming]
```

### 3. Compaction-Friendly Updates

**After each significant milestone**:
1. Update task context.md with current state
2. Ensure Next Steps are current

**This ensures that after compaction, you can**:
1. Read CLAUDE.md (always loaded)
2. List workspace/ folders for active tasks
3. Read relevant task context.md
4. Resume with full understanding

## Custom Compaction Hints

Guide compaction with specific instructions:

```
/compact preserve the agent decisions and task status
/compact keep the architectural decisions we made
/compact focus on the current implementation plan
/compact prioritize the bug fix we're working on
```

## Session Start Recovery Protocol

When starting a new session or recovering from compaction:

```
1. Read CLAUDE.md (automatic)
2. List workspace/ folders to find active tasks
3. For each active task:
   - Read workspace/[task-id]/context.md
   - Re-read key files mentioned
4. Resume from recorded "Next Steps"
```

## Hooks for Memory Management

### PreCompact Hook

Runs before compaction happens:

```json
{
  "hooks": {
    "PreCompact": [
      {
        "matcher": "auto",
        "hooks": [
          {
            "type": "command",
            "command": "echo 'Compaction triggered at $(date)' >> .claude/compaction-log.txt"
          }
        ]
      }
    ]
  }
}
```

**Use cases**:
- Log compaction events
- Backup current state
- Update task context files

### SessionStart Hook

Runs when session starts or resumes:

```json
{
  "hooks": {
    "SessionStart": [
      {
        "matcher": "resume",
        "hooks": [
          {
            "type": "command",
            "command": "echo 'Session resumed at $(date)'"
          }
        ]
      }
    ]
  }
}
```

## Best Practices Summary

### DO
1. Keep CLAUDE.md lean (< 500 lines)
2. Update task context.md after each milestone
3. Structure context.md for quick resume
4. Record Next Steps explicitly
5. Include file paths in all notes
6. Use workspace/ folders for task isolation

### DON'T
1. Store conversation history in files
2. Include full code in CLAUDE.md
3. Rely on conversation memory for critical info
4. Forget to update task status
5. Use vague descriptions ("the thing we discussed")

## Quick Reference: Context Recovery Checklist

When recovering from compaction or new session:

- [ ] CLAUDE.md loads automatically
- [ ] List workspace/ folders to find active tasks
- [ ] Read context.md for each active task
- [ ] Re-read files mentioned in Key Files
- [ ] Review Next Steps for each task
- [ ] Continue from documented state

---

## PERSISTENT Mode Compaction Protocol

PERSISTENT mode tasks have special requirements to ensure they continue automatically after compaction.

### What is PERSISTENT Mode?

PERSISTENT mode is for tasks that should continue until explicit completion criteria are met:
- "Convert all files to TypeScript"
- "Test until 90% coverage"
- "Refactor entire module"

Unlike NORMAL mode (stop and check with user), PERSISTENT mode auto-continues.

### Auto-Checkpoint Protocol

To prevent token exhaustion and enable seamless compaction recovery:

#### When to Checkpoint

1. **Every N items** (default: 10 files/functions/tests)
2. **Before large operations** (>5K estimated tokens)
3. **At ~75% token capacity** (force checkpoint before compaction)
4. **After completing a logical phase**

#### What to Save at Checkpoint

Update context.md with:

```markdown
## Execution Mode
- **Mode**: PERSISTENT
- **Set By**: [source]
- **Set At**: [timestamp]

### Completion Criteria
| # | Criterion | Verification Command | Threshold | Status |
|---|-----------|---------------------|-----------|--------|
| 1 | [criterion] | [command] | [threshold] | [pending/met] |

### Progress Tracker
- **Items Total**: 45
- **Items Completed**: 23
- **Completion %**: 51%
- **Last Checkpoint**: [timestamp]
- **Last Item Processed**: src/utils/auth.js
- **Next Item**: src/utils/crypto.js

### Checkpoint History
| Time | Progress | Criteria Status |
|------|----------|-----------------|
| 14:30 | 10/45 | [1] 35 remaining |
| 15:15 | 23/45 | [1] 22 remaining |
```

#### Quick Resume Format for PERSISTENT Mode

The Quick Resume section MUST follow this format:

```markdown
## Quick Resume
**MODE: PERSISTENT** | Progress: 23/45 files | Next: Convert src/utils/crypto.js
Criteria: [1] All .js -> .ts [NOT MET: 22 remaining] [2] tsc passes [NOT MET]
```

This format enables immediate recognition of:
- Mode (PERSISTENT = auto-continue)
- Progress (how far along)
- Next action (exactly what to do)
- Criteria status (what's left)

### Compaction Recovery for PERSISTENT Mode

When SessionStart hook detects a PERSISTENT mode task that is ACTIVE:

1. **DO NOT ask user** whether to continue
2. **Read Quick Resume** for current state
3. **Run verification commands** to validate stored progress
4. **Compare actual vs stored**:
   - If actual > stored: Something changed externally, investigate
   - If actual < stored: Something reverted, investigate
   - If actual = stored: Resume normally
5. **Continue from Next Item** automatically

### Example Recovery Flow

```
Session starts
    │
    ▼
List workspace/ folders
    │
    ▼
Read context.md for each task
    │
    ▼
Check Execution Mode
    │
    ├── NORMAL → Summarize, ask user
    │
    └── PERSISTENT + ACTIVE → Auto-continue
              │
              ▼
        Read Quick Resume
        "MODE: PERSISTENT | Progress: 23/45 | Next: crypto.js"
              │
              ▼
        Run verification: find . -name "*.js" | wc -l
        Result: 22 (matches stored)
              │
              ▼
        Resume: "Converting src/utils/crypto.js..."
```

### Safeguards

#### Infinite Loop Prevention
If same item processed 3+ times:
1. Mark as BLOCKED
2. Log in context.md
3. Wait for user guidance

#### Progress Stall Detection
If 10+ iterations with no progress toward criteria:
1. Report current state
2. Ask user: "Progress stalled. Continue or adjust threshold?"

#### Max Iterations per Session
Optional limit to prevent runaway execution:
- Set in context.md: `Max Iterations: 100`
- After limit reached, checkpoint and pause

---

## Compaction Warning Signs

Take action when you notice:
- Claude forgetting earlier decisions
- Need to repeat instructions
- Claude re-reading files it already read
- Confusion about project structure

**Response**: Manually `/compact` with hints rather than waiting for auto-compact.
