# Agent Self-Reflection Protocol

TRIGGER: reflection, confidence, verify, check output, hallucination, accuracy

## Overview

Self-reflection is a critical step that all agents MUST perform before finalizing output. Research shows "all types of self-reflection improve performance of LLM agents" and helps catch hallucinations and errors.

## When to Self-Reflect

**ALWAYS** - before producing any final output, run through this checklist.

## Self-Reflection Checklist

### 1. Task Alignment
- Does my output address the ACTUAL task requested?
- Have I stayed within my domain scope?
- Did I answer what was asked, not what I assumed was asked?

### 2. Assumption Check
- What assumptions have I made?
- Are any assumptions unverified?
- Should I state assumptions explicitly in my output?

### 3. Error Analysis
- What could be wrong with my analysis?
- What edge cases might I have missed?
- Would a senior engineer spot any issues?

### 4. Completeness Check
- Have I addressed all parts of the request?
- Are there any gaps in my response?
- Did I forget any required output sections?

### 5. Confidence Assessment

| Level | Criteria | Action |
|-------|----------|--------|
| **HIGH** | All assertions verified, sources checked, no significant assumptions | Proceed with COMPLETE |
| **MEDIUM** | Some unverified assumptions, minor uncertainty | Note assumptions in handoff |
| **LOW** | Significant uncertainty, guessing required | Consider NEEDS_INPUT instead |

## Required Output Format

Every agent status MUST include confidence:

```markdown
## Agent Status

**Status**: [COMPLETE | BLOCKED | NEEDS_INPUT]
**Confidence**: [HIGH | MEDIUM | LOW]
**Confidence Reasoning**: [1-2 sentences explaining why]
```

## Rules

1. **LOW confidence + critical task → Report NEEDS_INPUT**, not COMPLETE
2. **If you find errors during reflection → FIX** before finalizing
3. **Confidence without reasoning = invalid output**
4. **Never skip self-reflection** for "simple" tasks

## Anti-Hallucination Patterns

### Pattern 1: Source Verification
- Did I cite specific file paths and line numbers?
- Did I verify the information by reading the actual source?
- Am I stating facts or making inferences?

### Pattern 2: Uncertainty Expression
- Use "likely", "possibly", "based on X" when uncertain
- Never state uncertain things as facts
- Clearly mark inferences vs observations

### Pattern 3: Knowledge Boundaries
- Am I operating within my training knowledge?
- Should I recommend web research for current information?
- Am I making up details I don't actually know?

## Integration with Agent Output

Reference this file from `agents/_shared-output.md`:
- Behavioral Guideline #9: Self-reflect before finalizing
- Behavioral Guideline #10: Report confidence level

## Examples

### Good Self-Reflection
```markdown
## Self-Reflection

### Task Alignment
- Task asked to find authentication bugs
- I reviewed auth files and identified 3 potential issues
- Stayed within security domain scope

### Assumption Check
- Assumed JWT tokens are used (verified in config.ts:45)
- Assumed standard OWASP patterns apply
- Should verify: is rate limiting required?

### Error Analysis
- Might have missed auth flows in newer modules
- Should grep for additional auth patterns

### Confidence Assessment
**Confidence**: MEDIUM
**Reasoning**: Found clear issues but only reviewed 4 of 12 auth-related files
```

### Bad Self-Reflection (Missing Reasoning)
```markdown
## Self-Reflection

**Confidence**: HIGH
```
This is invalid - confidence must have reasoning.

---

## Model Selection Guidance

Different models are suited for different tasks. Claude 4 models form a tiered, interconnected family designed for orchestrated agentic work.

### Model Characteristics

| Model | Strength | Context | Cost ($/M tokens) |
|-------|----------|---------|-------------------|
| **Opus 4.5** | Deep reasoning, complex analysis | 200K | $5/$25 |
| **Sonnet 4.5** | Balanced intelligence, coding | 200K (1M beta) | $3/$15 |
| **Haiku 4.5** | Speed, high-volume tasks | 200K | $1/$5 |

### When to Use Each Model

| Use Case | Model | Rationale |
|----------|-------|-----------|
| Architecture decisions | Opus | Deep reasoning needed |
| Complex bug analysis | Opus | Multi-step investigation |
| Final code review (pre-merge) | Opus | Catches subtle bugs |
| Standard development tasks | Sonnet | Best balance of capability/cost |
| Frontend/UI implementation | Sonnet | Excels at pixel-perfect layouts |
| Research synthesis | Sonnet | Good at combining sources |
| Parallel subtask execution | Haiku | Fast, cost-effective at scale |
| Quick file exploration | Haiku | Speed matters more than depth |
| High-volume validation | Haiku | 90% of Sonnet performance, 5x speed |

### Orchestration Pattern

The recommended pattern for complex tasks:
1. **Sonnet** creates plan and decomposes into subtasks
2. **Haiku** instances execute subtasks in parallel
3. **Opus** reviews critical/complex results

### Context Length Considerations

| Scenario | Recommendation |
|----------|---------------|
| Short conversation (<10K tokens) | Any model suitable |
| Medium (10K-50K tokens) | Sonnet preferred |
| Long session (>50K tokens) | Sonnet or Opus (Haiku "loses track fast") |
| Ultra-long (>200K tokens) | Sonnet with 1M context beta |

When spawning agents, orchestrator should specify model based on:
1. Task complexity (simple → haiku, complex → opus)
2. Required depth (shallow → haiku, deep → opus)
3. Session length (short → any, long → sonnet/opus)
4. Cost sensitivity (budget-constrained → haiku)
