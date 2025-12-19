# Multi-Agent System Failure Patterns

TRIGGER: multi-agent, failure, cascade, coordination, misalignment, handoff failure, agent conflict

## Overview

Research shows multi-agent systems have 14 unique failure modes clustered into 3 categories. Understanding these patterns helps prevent failures before they cascade.

## MAST Taxonomy (Multi-Agent System Failure Types)

### Category 1: System Design Issues (32%)

Failures where agents don't know what they're supposed to do.

| Failure Mode | Description | Prevention |
|--------------|-------------|------------|
| **Unclear Specification** | Agent role/boundaries ambiguous | Use explicit JSON schemas for agent inputs/outputs |
| **Missing Context** | Agent lacks information to decide | Require context.md reading before action |
| **Wrong Agent Selection** | Task routed to agent without expertise | Use explicit routing table in _orchestrator.md |
| **Scope Creep** | Agent does more than assigned | Define explicit boundaries in agent definition |
| **Overlapping Responsibilities** | Multiple agents claim same work | Clear domain ownership in collaboration matrix |

### Category 2: Inter-Agent Misalignment (28%)

Most common failure mode - agents talk past each other.

| Failure Mode | Description | Prevention |
|--------------|-------------|------------|
| **Communication Mismatch** | Output format doesn't match expected input | Standardize output format in _shared-output.md |
| **Lost Handoff** | Context dropped between agents | Mandatory context.md updates after each agent |
| **Duplicate Effort** | Agents redo each other's work | Parallel Findings table for discovery sharing |
| **Conflicting Actions** | Agents make incompatible changes | Conflict resolution rules in _orchestrator.md |
| **Responsibility Amnesia** | Agent forgets its role mid-task | Constitutional principles in spawn prompt |

### Category 3: Task Verification Gaps (24%)

Nobody checks if the work is actually good.

| Failure Mode | Description | Prevention |
|--------------|-------------|------------|
| **Premature Completion** | Task marked done before criteria met | Evaluator-agent quality gate |
| **Missing Validation** | Output not verified against requirements | Pre-completion verification protocol |
| **Acceptance Criteria Drift** | Final output doesn't match original request | Re-read original task before completion |
| **Silent Failures** | Errors hidden or ignored | Mandatory error reporting in status |

### Category 4: Infrastructure Issues (16%)

Technical constraints causing failures.

| Failure Mode | Description | Prevention |
|--------------|-------------|------------|
| **Token Exhaustion** | Context window overflow | Checkpoint protocol, scratchpad pattern |
| **Rate Limiting** | API throttling | Retry with exponential backoff |
| **Timeout** | Long operations exceeding limits | Break into smaller operations |
| **Tool Errors** | Tool execution failures | Error recovery protocol |

## Error Cascading Prevention

Error cascading is the most dangerous failure mode - one small mistake compounds through the system.

### The Cascade Pattern

```
Agent A makes error
    ↓
Agent B receives bad output
    ↓
Agent B builds on bad output (compounds error)
    ↓
Agent C receives even worse output
    ↓
Final output is catastrophically wrong
```

### Prevention Strategies

#### 1. Validation at Boundaries
```markdown
## Handoff Validation Checklist

Before accepting input from previous agent:
- [ ] Output format matches expected schema?
- [ ] All required fields present?
- [ ] Values within expected ranges?
- [ ] No obvious errors in reasoning?

If ANY check fails → Report BLOCKED, don't propagate error
```

#### 2. Error Isolation
```markdown
## Error Isolation Pattern

When error detected:
1. STOP processing immediately
2. Log error with full context
3. DO NOT pass error downstream
4. Report BLOCKED with specifics
5. Wait for orchestrator to resolve
```

#### 3. Result Validation
```markdown
## Result Validation

Before marking COMPLETE:
1. Re-read original requirements
2. Compare output against requirements
3. Run verification commands
4. Check for inconsistencies
5. Self-reflect on confidence

If validation fails → Fix or report BLOCKED
```

## Inter-Agent Misalignment Prevention

### Communication Protocol

All agent-to-agent communication MUST use standardized format:

```markdown
## Handoff Notes (REQUIRED)

### What I Did
[Specific actions taken]

### What I Found
[Key discoveries]

### What I Produced
[Artifacts created, with paths]

### What Next Agent Needs
[Specific information for continuation]

### Assumptions Made
[Any assumptions that next agent should verify]
```

### Conflict Resolution

When agents have conflicting outputs:

1. **Security vs Performance**: Security wins
2. **Correctness vs Speed**: Correctness wins
3. **Multiple Valid Approaches**: Present options to user
4. **Unclear Priority**: Ask user for guidance

### Responsibility Enforcement

Each agent spawn MUST include constitutional principles:

```markdown
## Your Responsibilities (Enforce These)
1. Complete your assigned subtask only
2. Do NOT start tasks assigned to other agents
3. If you need another agent's expertise → Report NEEDS_INPUT
4. Do NOT mark COMPLETE unless YOUR task is done
5. Document everything for next agent
```

## Common Anti-Patterns

### Anti-Pattern 1: Over-Engineering

```markdown
BAD: Building "self-reflecting autonomous super-duper agents"
     for problems solvable with 3 API calls in sequence

GOOD: Start simple, add complexity only when needed
      Single-threaded loop + good tools = reliable agents
```

### Anti-Pattern 2: Prototype as Production

```markdown
BAD: "The prototype works, let's ship it!"
     Prototype optimized for speed/flexibility, not resilience

GOOD: Redesign for production:
      - Decomposition
      - Observability
      - Testing
      - Error handling
```

### Anti-Pattern 3: Ignoring Hallucinations

```markdown
BAD: Assuming 5% hallucination rate is acceptable
     Errors compound in multi-agent workflows

GOOD: Validate all agent outputs
      Use evaluator-agent for quality gate
      Require confidence scores
```

### Anti-Pattern 4: Implicit Contracts

```markdown
BAD: Agent A expects JSON, Agent B outputs markdown
     No one documented the interface

GOOD: Explicit schema definitions
      Standardized output formats
      Validation at boundaries
```

## Recovery Patterns

### Pattern 1: Checkpoint and Restart

```markdown
When cascade detected:
1. Stop all agents
2. Identify cascade origin point
3. Load last good checkpoint
4. Fix root cause
5. Resume from checkpoint
```

### Pattern 2: Graceful Degradation

```markdown
When agent fails:
1. Isolate failure (don't propagate)
2. Try simpler fallback approach
3. If fallback fails → Human escalation
4. Never continue with broken state
```

### Pattern 3: Human-in-the-Loop Gate

```markdown
For high-risk operations:
1. Agent proposes action
2. Present to user for approval
3. Only execute if approved
4. Log decision for audit
```

## Monitoring for Failures

### Warning Signs

| Signal | Meaning | Action |
|--------|---------|--------|
| Same error 3+ times | Agent stuck in loop | Break loop, try different approach |
| Confidence dropping | Agent uncertain | Consider model escalation |
| Context growing fast | Possible dumping | Trigger compression |
| Multiple BLOCKED | Systemic issue | Pause, analyze, redesign |

### Health Metrics

Track these to detect problems early:

| Metric | Healthy Range | Alert Threshold |
|--------|--------------|-----------------|
| First-try success | >90% | <80% |
| BLOCKED rate | <10% | >20% |
| Cascade rate | <2% | >5% |
| Human escalation | <5% | >15% |

---

*Based on MAST taxonomy research analyzing 1600+ multi-agent traces across 7 frameworks.*

**Sources**:
- [Why Do Multi-Agent LLM Systems Fail?](https://arxiv.org/abs/2503.13657)
- [Why AI Agents Fail in Production](https://softcery.com/lab/why-ai-agent-prototypes-fail-in-production-and-how-to-fix-it)
