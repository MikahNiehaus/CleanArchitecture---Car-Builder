# Error Recovery & Self-Healing Protocol

TRIGGER: error, failure, stuck, blocked, retry, recovery, debug agent, self-healing

## Overview

Research shows that structured error recovery with root cause analysis achieves 24% higher accuracy than ad-hoc debugging. This knowledge base provides the AgentDebug pattern for systematic failure recovery.

## Error Taxonomy

### Level 1: Memory Errors
| Error Type | Symptoms | Recovery Action |
|------------|----------|-----------------|
| Context Overflow | Forgetting earlier context, repeating work | Checkpoint + compact + resume |
| Stale Reference | Acting on outdated information | Re-read source files |
| Lost Handoff | Missing context from previous agent | Re-read workspace context.md |
| Assumption Drift | Working with incorrect assumptions | Verify assumptions explicitly |

### Level 2: Reflection Errors
| Error Type | Symptoms | Recovery Action |
|------------|----------|-----------------|
| Premature Completion | Marking done before criteria met | Run verification commands |
| Scope Creep | Doing more than asked | Re-read original task |
| Confidence Miscalibration | HIGH confidence on uncertain output | Force self-reflection checklist |
| Missing Self-Check | No verification performed | Run self-reflection protocol |

### Level 3: Planning Errors
| Error Type | Symptoms | Recovery Action |
|------------|----------|-----------------|
| Task Misunderstanding | Wrong interpretation of request | Ask user for clarification |
| Bad Decomposition | Subtasks don't cover full task | Re-decompose with completeness check |
| Wrong Agent Selection | Agent lacks required expertise | Re-route to correct agent |
| Missing Dependencies | Steps in wrong order | Rebuild dependency graph |

### Level 4: Action Errors
| Error Type | Symptoms | Recovery Action |
|------------|----------|-----------------|
| Tool Failure | Command returns error | Parse error, adjust, retry |
| File Not Found | Expected file missing | Search for alternate locations |
| Permission Denied | Can't access resource | Ask user for permissions |
| Syntax Error | Code doesn't compile | Review and fix syntax |

### Level 5: System Errors
| Error Type | Symptoms | Recovery Action |
|------------|----------|-----------------|
| Token Exhaustion | Near context limit | Force checkpoint + compact |
| Rate Limiting | API throttling | Wait + retry with backoff |
| External Service Down | API unavailable | Log + continue other work |
| Environment Mismatch | Different from expected | Verify environment state |

## Detect-Decide-Act Framework

### Step 1: Detect
Recognize error indicators:

```markdown
## Error Detection Checklist
- [ ] Did the last action succeed? (check return code/output)
- [ ] Does output match expected format?
- [ ] Are there error messages in output?
- [ ] Is progress being made? (not stuck in loop)
- [ ] Is confidence level appropriate? (not guessing)
```

### Step 2: Decide
Classify the error using the taxonomy above, then select recovery strategy:

```
Error Detected
     │
     ▼
What level is the error?
     │
     ├── Level 1 (Memory) → Context recovery actions
     ├── Level 2 (Reflection) → Self-check actions
     ├── Level 3 (Planning) → Re-planning actions
     ├── Level 4 (Action) → Retry/adjust actions
     └── Level 5 (System) → Infrastructure actions
```

### Step 3: Act
Execute recovery with logging:

```markdown
### Error Recovery Log
- **Error Detected**: [description]
- **Classification**: [Level-Type]
- **Recovery Action**: [what was done]
- **Result**: [success/still blocked]
- **Timestamp**: [when]
```

## Recovery Protocols by Error Type

### Protocol: Context Recovery (Level 1)
```
1. Checkpoint current state to workspace/[task-id]/context.md
2. Re-read all Key Files mentioned in context.md
3. Verify current understanding matches file contents
4. Update Quick Resume with current accurate state
5. Resume from verified state
```

### Protocol: Self-Check Recovery (Level 2)
```
1. STOP current action
2. Run full self-reflection checklist (knowledge/self-reflection.md)
3. Identify specific failure point
4. If confidence < MEDIUM → Report NEEDS_INPUT
5. If confidence >= MEDIUM → Proceed with corrections
```

### Protocol: Re-Planning Recovery (Level 3)
```
1. Re-read original user request
2. Compare current approach to request
3. If misaligned → Create new plan
4. If unclear → Ask user via AskUserQuestion
5. Update context.md with corrected plan
```

### Protocol: Action Recovery (Level 4)
```
1. Parse error message for root cause
2. Search for similar errors in codebase (grep patterns)
3. Adjust command/approach
4. Retry with adjustment
5. If fails 3x → Escalate to different approach
```

### Protocol: System Recovery (Level 5)
```
1. Log system error with timestamp
2. Checkpoint all progress
3. If token exhaustion → Allow compaction
4. If external service → Queue for later or skip
5. Continue with available resources
```

## Escalation Matrix

When recovery fails, escalate:

| Failure Count | Action |
|---------------|--------|
| 1 | Retry with same approach |
| 2 | Adjust approach slightly |
| 3 | Try different approach entirely |
| 4 | Ask user for guidance |
| 5+ | Mark BLOCKED, document fully |

### Model Escalation (for stuck reasoning)

When a task is stuck due to reasoning complexity:

| Current Model | Escalation To | Trigger |
|---------------|---------------|---------|
| haiku | sonnet | Complex reasoning needed |
| sonnet | opus | Architectural decisions, critical analysis |
| opus | User | Still stuck after opus attempt |

## Corrective Feedback Pattern

When recovering from errors, provide corrective context:

```markdown
## Corrective Feedback for Retry

### What Failed
[Specific description of the failure]

### Why It Failed
[Root cause analysis]

### Corrective Guidance
[Specific instructions to avoid repeating the error]

### Expected Outcome
[What success looks like this time]
```

## Integration with Agent Spawning

When spawning agents, include error recovery instructions:

```markdown
## Error Handling Instructions

If you encounter errors:
1. Classify using error taxonomy (READ knowledge/error-recovery.md)
2. Apply appropriate recovery protocol
3. Log recovery actions in your output
4. If stuck after 3 attempts → Report BLOCKED with full context

Do NOT:
- Silently ignore errors
- Proceed with broken state
- Report COMPLETE if any errors unresolved
```

## Anti-Patterns

### DON'T: Retry Without Analysis
```
BAD:  Command failed → run same command again
GOOD: Command failed → analyze error → adjust → retry
```

### DON'T: Ignore Warnings
```
BAD:  Warnings in output → proceed anyway
GOOD: Warnings in output → assess severity → address if significant
```

### DON'T: Assume Transient
```
BAD:  Error occurred → assume it's temporary → move on
GOOD: Error occurred → verify root cause → fix root cause
```

## Metrics for Self-Healing Effectiveness

Track these to assess recovery quality:

| Metric | Target | Meaning |
|--------|--------|---------|
| Recovery Rate | >80% | Errors resolved without escalation |
| First-Try Success | >90% | Actions succeed on first attempt |
| BLOCKED Rate | <10% | Tasks requiring user intervention |
| False COMPLETE | 0% | COMPLETE reported but task incomplete |

---

*This protocol is based on AgentDebug research showing 24% accuracy improvement with structured error recovery.*
