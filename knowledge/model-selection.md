# Model Selection Knowledge Base

## Overview

This system uses a **two-tier model selection** approach:
- **Sonnet**: Default for most agents (15 of 18)
- **Opus**: Reserved for critical checkpoints and complex reasoning

**Priority**: Quality > Accuracy > Token Cost (no Haiku)

---

## Quick Reference

### Always Opus (3 agents)

| Agent | Rationale |
|-------|-----------|
| **architect-agent** | Design decisions cascade everywhere - bad architecture propagates through entire codebase |
| **ticket-analyst-agent** | Wrong understanding = building wrong thing entirely - first step errors cascade |
| **reviewer-agent** | Final quality gate before shipping - catches architecture, security, subtle bugs |

### Default Sonnet (15 agents)

| Agent | Notes |
|-------|-------|
| test-agent | Test writing; escalate for complex mocking strategies |
| debug-agent | Root cause analysis; escalate for architectural bugs |
| workflow-agent | Implementation; escalate for complex integrations |
| docs-agent | Documentation |
| refactor-agent | Code cleanup; escalate for large-scale refactors |
| research-agent | Web research |
| explore-agent | Codebase exploration |
| estimator-agent | Story pointing |
| ui-agent | Frontend work |
| performance-agent | Profiling; escalate for system-wide optimization |
| security-agent | Security audits; escalate for auth/payment logic |
| browser-agent | Interactive testing |
| evaluator-agent | Output verification |
| teacher-agent | Explanations |
| compliance-agent | Rule auditing |

---

## Decision Tree

```
Task arrives
    │
    ▼
Is agent architect-agent, ticket-analyst-agent, or reviewer-agent?
    │
    ├─ YES ──────────────────────────────► Use Opus
    │
    └─ NO
        │
        ▼
    Check Opus Escalation Triggers
        │
        ├─ ANY trigger matched? ──────────► Use Opus
        │
        └─ No triggers matched ───────────► Use Sonnet (default)
```

---

## Opus Escalation Triggers

Use Opus instead of Sonnet if **ANY** of these conditions match:

### By Agent Type (always Opus)
- architect-agent
- ticket-analyst-agent
- reviewer-agent

### By Complexity
- 4+ domains identified in Planning Checklist
- 10+ subtasks decomposed from request
- Multi-file architectural changes
- Cross-cutting concerns (logging, auth, caching across modules)

### By Stakes
- Production deployment or live system changes
- Payment processing logic
- Authentication/authorization code
- Sensitive data handling (PII, credentials, secrets)
- Security-critical paths

### By Ambiguity
- Vague requirements needing interpretation ("make it better", "improve performance")
- Conflicting constraints requiring trade-off decisions
- Architectural decisions with long-term implications

### By Reasoning Depth
- Multi-step autonomous decision chains
- Complex debugging requiring system-wide understanding
- Design pattern selection with trade-offs

### By Escalation (mid-task)
- Agent reports `Confidence: LOW` on critical subtask
- Agent reports `Status: BLOCKED` with capability-related reason
- Multiple failed tool calls (>3) on same operation
- Agent explicitly requests deeper reasoning

---

## Complexity Scoring (for edge cases)

When escalation decision is unclear, score the task:

| Dimension | Score 1 (Sonnet) | Score 2 (Opus) |
|-----------|------------------|----------------|
| **Domains** | 1-3 domains | 4+ domains |
| **Subtasks** | 1-9 subtasks | 10+ subtasks |
| **Ambiguity** | Clear requirements | Vague/interpretive |
| **Stakes** | Dev/test environment | Production/sensitive |
| **Reasoning** | Linear/sequential | Multi-step autonomous |

**Scoring**:
- Total **8+** → Use Opus
- Total **5-7** → Use Sonnet (default)

**Override**: Always-Opus agents (architect, ticket-analyst, reviewer) use Opus regardless of score.

---

## Confidence-Based Mid-Task Escalation

During agent execution, the orchestrator should escalate to Opus if:

1. **LOW Confidence on Critical Path**
   - Agent self-reflection reports LOW confidence
   - Subtask is on critical path (not exploratory)
   - Action: Retry same subtask with Opus

2. **BLOCKED with Capability Issue**
   - Status: BLOCKED
   - Reason suggests reasoning limitation (not missing info)
   - Action: Spawn Opus agent for same task

3. **Repeated Failures**
   - Same tool call fails 3+ times
   - Failures suggest reasoning issue (not API/permission)
   - Action: Escalate to Opus

4. **Explicit Request**
   - Agent output includes: "This task may benefit from deeper reasoning"
   - Action: Honor the request, spawn Opus

### Escalation Protocol

```markdown
## Escalation Record

**Original Agent**: [agent-name] with Sonnet
**Escalation Trigger**: [which trigger matched]
**New Agent**: [agent-name] with Opus
**Handoff Context**: [what the Sonnet agent discovered before escalation]
```

---

## Model Characteristics

| Model | Strength | Best For | Avoid For |
|-------|----------|----------|-----------|
| **Opus** | Deep reasoning, complex analysis, nuanced judgment | Architecture, requirements, final review, high-stakes | High-volume simple tasks |
| **Sonnet** | Balanced intelligence, strong coding, efficient | Implementation, testing, debugging, documentation | Tasks requiring deep architectural reasoning |

### Token Context Considerations

- Both models support 200K context
- Sonnet has 1M context beta (if enabled)
- For ultra-long sessions (>100K tokens accumulated), prefer Opus for synthesis tasks

---

## Cost Tracking Template

Add to `workspace/[task-id]/context.md`:

```markdown
## Model Usage

| Agent | Model | Rationale | Est. Tokens |
|-------|-------|-----------|-------------|
| [agent] | [opus/sonnet] | [why this model] | ~Xk |

**Opus Usage**: X agents ([list])
**Sonnet Usage**: X agents ([list])
**Escalations**: [count and reasons]
```

---

## Anti-Patterns

| Anti-Pattern | Why It's Wrong | Correct Approach |
|--------------|----------------|------------------|
| "Use Opus to be safe" | Wastes usage, no quality benefit for simple tasks | Follow decision tree |
| "Sonnet for everything" | Misses quality benefits on critical tasks | Always-Opus agents exist for a reason |
| No visibility into usage | Can't optimize what you can't measure | Track in context.md |
| Ignoring escalation signals | Agent confidence exists for a reason | Honor LOW confidence, BLOCKED |
| "Quick task, skip the check" | All tasks deserve appropriate resources | Run decision tree every time |

---

## Integration Points

### Where Model Selection Happens

1. **Planning Phase** (in orchestrator)
   - Identify which agents needed
   - Apply decision tree to each agent
   - Document in Plan section of context.md

2. **Agent Spawning** (Task tool)
   - Include `model: "opus"` or `model: "sonnet"` parameter
   - Include rationale in agent prompt

3. **Mid-Task Monitoring** (during execution)
   - Watch for escalation triggers
   - Update context.md if escalation occurs

### Task Tool Spawn Template

```markdown
Use the Task tool with:
- subagent_type: "general-purpose"
- model: "[opus|sonnet]"  ← REQUIRED
- prompt: [agent instructions including model rationale]
```

---

## Related Knowledge Bases

- `knowledge/self-reflection.md` - Confidence levels and self-assessment
- `knowledge/error-recovery.md` - Error handling and escalation
- `knowledge/context-engineering.md` - Token budget management
- `knowledge/rule-enforcement.md` - Compliance checking
