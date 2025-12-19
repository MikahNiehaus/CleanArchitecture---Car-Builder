# Workspace Organization

TRIGGER: organize, workspace, task folder, artifact, snapshot, context

## Overview

All task-related artifacts should be organized in the `workspace/` folder using task-specific subfolders. Each task has its own isolated context for agent collaboration.

## Folder Structure

```
workspace/
└── [task-id]/
    ├── mockups/       # Design references, input images, specifications
    ├── outputs/       # Generated artifacts, final deliverables
    ├── snapshots/     # Screenshots, progress captures, debugging images
    └── context.md     # Task context, notes, agent contributions, handoffs
```

## Task ID Naming Convention

### When Ticket Number Exists
Use the ticket number directly:
- `ASC-914`
- `JIRA-123`
- `GH-456`
- `BUG-789`

### When No Ticket Number
Auto-generate using format: `YYYY-MM-DD-short-description`
- `2025-12-04-auth-refactor`
- `2025-12-04-fix-login-bug`
- `2025-12-04-add-caching`

Keep descriptions short (2-4 words), lowercase, hyphen-separated.

## What Goes Where

### mockups/
- Design mockups and wireframes
- UI specifications
- Reference images
- Input files for the task

### outputs/
- Generated code snippets
- Final deliverables
- Exported reports
- Completed artifacts

### snapshots/
- Screenshots during testing
- Progress captures
- Debugging images
- Before/after comparisons

### context.md
- Task status and description
- Notes and findings
- Agent contributions and handoffs
- Open questions
- Session history

## context.md Template

Every task folder should have a `context.md` file:

```markdown
# Task: [Task ID]

## Quick Resume
[1-2 sentences: Current state for session recovery after compaction]

**NORMAL mode example**: "Implementing auth refactor. debug-agent found root cause in token.ts:45, waiting for test-agent to write regression tests."

**PERSISTENT mode example**: "**MODE: PERSISTENT** | Progress: 23/45 files | Next: Convert src/services/auth.js | Criteria: [1] All .js -> .ts [NOT MET: 22 remaining] [2] tsc passes [NOT MET]"

## Status
- **State**: [PLANNING/ACTIVE/BLOCKED/COMPLETE]
- **Current Phase**: [Planning | Execution | Review]
- **Last Agent**: [Most recent agent that contributed]
- **Created**: [YYYY-MM-DD]
- **Updated**: [YYYY-MM-DD]

## Execution Mode

### Mode Configuration
- **Mode**: [NORMAL/PERSISTENT]
- **Set By**: [User request / Auto-detected from patterns: "all", "until", "entire", "every", "complete"]
- **Set At**: [Timestamp]

### Completion Criteria (PERSISTENT mode only)
| # | Criterion | Verification Command | Threshold | Status |
|---|-----------|---------------------|-----------|--------|
| 1 | [e.g., All .js files converted to .ts] | [find . -name "*.js" \| wc -l] | [= 0] | [pending/met/failed] |
| 2 | [e.g., Test coverage >= 90%] | [npm run coverage --json] | [>= 90] | [pending/met/failed] |
| 3 | [e.g., No TypeScript errors] | [npx tsc --noEmit; echo $?] | [= 0] | [pending/met/failed] |

### Progress Tracker (PERSISTENT mode only)
- **Items Total**: [N or "counting..."]
- **Items Completed**: [M]
- **Completion %**: [M/N * 100]%
- **Last Checkpoint**: [Timestamp]
- **Last Item Processed**: [e.g., src/utils/auth.js]
- **Next Item**: [e.g., src/utils/crypto.js]

### Continuation Protocol
- **Continue If**: All criteria NOT met AND status not BLOCKED
- **Stop If**: All criteria met OR BLOCKED OR user interrupts
- **Checkpoint Frequency**: Every [N] items (default: 10)

## Plan (Populated by Planning Phase)

### Planning Checklist Results
| Domain | Needed? | Criteria Met | Agent |
|--------|---------|--------------|-------|
| Testing | [Yes/No] | [Specific criteria that triggered this] | test-agent |
| Documentation | [Yes/No] | [Specific criteria] | docs-agent |
| Security | [Yes/No] | [Specific criteria] | security-agent |
| Architecture | [Yes/No] | [Specific criteria] | architect-agent |
| Performance | [Yes/No] | [Specific criteria] | performance-agent |
| Review | [Yes/No] | [Specific criteria] | reviewer-agent |
| Clarity | [Yes/No] | [Specific criteria] | ticket-analyst-agent |

### Subtasks
| # | Subtask | Agent | Dependencies | Status |
|---|---------|-------|--------------|--------|
| 1 | [Name] | [agent] | None | [pending/in_progress/complete] |
| 2 | [Name] | [agent] | Task 1 | [pending/in_progress/complete] |

### Execution Strategy
- **Pattern**: [Sequential | Parallel | Hybrid]
- **Rationale**: [Why this pattern was chosen]

### Approval Status
- **Plan Mode**: [Active/Inactive]
- **Approved**: [Yes/No/Pending]
- **Approved By**: [User/Auto]
- **Modifications**: [Any changes requested before approval]

### Model Usage
| Agent | Model | Rationale |
|-------|-------|-----------|
| [agent-name] | [opus/sonnet] | [why this model: always-opus agent, complexity trigger, etc.] |

**Summary**:
- **Opus Usage**: [N] agents ([list])
- **Sonnet Usage**: [N] agents ([list])
- **Escalations**: [count and reasons, or "None"]

## Blocked Resolution (if BLOCKED)
- **Blocked By**: [Specific blocker description]
- **To Unblock**: [Required action or input needed]
- **Owner**: [Who needs to act: user/specific-agent/external]
- **Attempted**: [What has been tried so far]

## Key Files
[Critical files for this task - read these first when resuming]
- `path/to/file1.ts` - [why important]
- `path/to/file2.ts` - [why important]

## Task Description
[What this task is about, requirements, goals]

## Orchestrator Decisions

### Request Analysis - [Timestamp]
- **User Request**: [Original request]
- **Domains Identified**: [testing, debugging, architecture, etc.]
- **Agents Considered**: [List all agents that could apply]
- **Agents Spawned**: [List agents actually spawned and WHY]
- **Rationale**: [Why these specific agents]

## Notes & Findings
[Human notes, discoveries, key decisions made]

## Agent Contributions

### [Agent Name] - [Timestamp]
- **Task**: What agent was asked to do
- **Status**: COMPLETE/BLOCKED/NEEDS_INPUT
- **Key Findings**: Main discoveries
- **Output**: What was produced (or path to outputs/ folder)
- **Handoff Notes**: What next agent needs to know

## Parallel Findings (Updated by concurrent agents)

> When agents run in parallel, they add findings here so other agents can see discoveries in real-time.

| Agent | Finding | Impact | Timestamp |
|-------|---------|--------|-----------|
| - | - | - | - |
<!-- Parallel agents: Add your row IMMEDIATELY when you discover something significant -->

## Handoff Queue
| Next Agent | Reason | Priority |
|------------|--------|----------|
| [agent]    | [why]  | [P0-P2]  |

## Open Questions
- [ ] Question 1
- [ ] Question 2

## Next Steps
1. [First thing to do when resuming]
2. [Second thing to do]
3. [Third thing to do]

## Session History
| Time | Agent | Action | Result |
|------|-------|--------|--------|
| - | - | - | - |
```

## Status Definitions

### Task Status (in context.md)

| Status | Meaning | Actions |
|--------|---------|---------|
| **PLANNING** | Planning phase in progress | Complete checklist, generate plan |
| **ACTIVE** | Execution in progress | Continue with next steps |
| **BLOCKED** | Cannot proceed | Check "Blocked Resolution" section |
| **COMPLETE** | Task finished | Archive or clean up |

### Agent Status (in agent outputs)

| Status | Meaning | Orchestrator Action |
|--------|---------|---------------------|
| **COMPLETE** | Agent finished successfully | Continue to next agent or synthesize |
| **BLOCKED** | Cannot proceed | Check blocker, route to unblocking agent or ask user |
| **NEEDS_INPUT** | Requires user clarification | Present question to user |

## Context Lifecycle

### When to Create context.md
Create when starting a task that:
- Involves multiple steps
- Will have multiple agents collaborate
- Spans multiple sessions
- Needs progress tracking

### When to Update context.md
Update after:
- Each agent completes work (add to Agent Contributions)
- Key decisions are made (add to Notes & Findings)
- Status changes (update State)
- Questions arise (add to Open Questions)

### State Transitions
```
PLANNING → Running Planning Checklist, generating plan
ACTIVE → Executing plan, agents contributing
BLOCKED → Cannot proceed, see "Blocked By" field
COMPLETE → Task finished, final summary in Notes
```

### When to Archive
- Mark COMPLETE when task is done
- Keep folder for reference if artifacts may be needed
- Clean up during project completion

## When to Create Task Folders

**ALWAYS create a task folder when:**
- Task involves ANY code changes
- Task involves multiple steps
- ANY agent is being spawned
- You'll generate artifacts (screenshots, outputs)
- Multiple agents will collaborate
- Work spans multiple sessions
- You need to track progress

**ONLY skip task folder for:**
- Single read-only questions (no file modifications)
- Simple explanations that fit in one response
- Codebase navigation questions

**When in doubt → create the folder.**

## Workflow Integration

### Before Starting Work
1. Determine task ID (ticket number or generate one)
2. Create `workspace/[task-id]/` folder structure
3. Create `context.md` from template
4. Store any input materials in `mockups/`

### During Work
1. Save screenshots to `snapshots/`
2. Update `context.md` with findings and agent contributions
3. Store generated artifacts in `outputs/`

### After Completion
1. Ensure all artifacts are in proper folders
2. Update `context.md` state to COMPLETE
3. Add final summary to Notes & Findings
4. Clean up any temporary files

## Cleanup Guidelines

### Active Tasks
- Keep all folders for active/recent tasks
- Context provides collaboration history

### Completed Tasks
- Keep for reference if artifacts may be needed later
- Archive or remove after project completion
- Final summary should be in context.md before cleanup

### Root Folder Rules
- Keep workspace root clean
- Only `.gitkeep` at root level
- All task work in subfolders

## Example Task Setup

```
# For ticket ASC-914 (UI button fix)
workspace/
└── ASC-914/
    ├── mockups/
    │   └── button-design.png
    ├── outputs/
    │   └── button-component.tsx
    ├── snapshots/
    │   ├── before-fix.png
    │   └── after-fix.png
    └── context.md

# For ad-hoc refactoring task
workspace/
└── 2025-12-04-auth-refactor/
    ├── mockups/
    ├── outputs/
    │   └── auth-service.ts
    ├── snapshots/
    └── context.md
```

## Integration with Multi-Agent System

When agents collaborate on a task:
1. All agents reference the same task folder
2. Each agent reads/updates `context.md` for handoffs
3. Agent contributions are logged with timestamps
4. Artifacts are accessible to all collaborating agents
5. Orchestrator updates context after each agent completes

---

## Context Size Management

Keep context.md files manageable for efficient agent collaboration.

### Size Limits

| Metric | Target | Warning | Action |
|--------|--------|---------|--------|
| File size | < 30 KB | > 30 KB | Archive old contributions |
| Agent contributions | < 10 active | > 10 | Move resolved to archive |
| Parallel findings | < 20 rows | > 20 | Consolidate into summary |

### What to Keep (Active)
- Current task status and blockers
- Unresolved open questions
- Recent agent contributions (last 3-5)
- Next steps
- Key files list

### What to Archive
- Resolved agent contributions (move to `outputs/archive/context-history.md`)
- Completed parallel findings (consolidate to summary)
- Old session history entries

### Archiving Process
When context.md exceeds 30 KB:
1. Create `outputs/archive/` if it doesn't exist
2. Move resolved Agent Contributions to `outputs/archive/context-history.md`
3. Keep only last 3-5 contributions in main context.md
4. Add summary line: "See `outputs/archive/` for [N] prior agent contributions"

---

## Quick Resume Auto-Update Protocol

The Quick Resume section MUST always reflect current state for compaction recovery.

### Update Rule (MANDATORY)

**After EVERY agent completes**, orchestrator MUST update Quick Resume:

```markdown
## Quick Resume
[agent-name] completed [task] at [HH:MM]. Next: [immediate next action].
```

### Examples

```markdown
## Quick Resume
debug-agent completed root cause analysis at 14:30. Next: spawn test-agent for regression tests.
```

```markdown
## Quick Resume
security-agent + reviewer-agent completed parallel review at 15:45. Next: synthesize findings for user.
```

```markdown
## Quick Resume
BLOCKED: test-agent needs user input on test framework preference. Waiting for user response.
```

### Validation

- Quick Resume should NEVER be more than 1 agent behind
- If compaction occurs, Quick Resume is the first thing read
- Stale Quick Resume = confused recovery = wasted tokens

### Auto-Update Trigger

Orchestrator updates Quick Resume:
1. Immediately after receiving agent status
2. Before spawning next agent
3. Before any user-facing response
4. When status changes to BLOCKED
