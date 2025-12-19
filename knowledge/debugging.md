# Claude Code Debugging Configuration

> **TRIGGER**: Use this documentation when debugging errors, fixing bugs, troubleshooting issues, analyzing stack traces, or investigating unexpected behavior.

## Core Debugging Philosophy

- Claude achieves **78% accuracy** on complex multi-file debugging issues
- Claude maintains context **73% longer** than alternatives (50 min vs 17 min)
- **Limitation**: 81% failure rate when code undergoes semantic-preserving changes (variable renames, comment changes)
- Success hinges on structured prompts, proper context, and systematic workflows

## Structured Prompt Format

Use XML tags for optimal debugging accuracy:

```xml
<error_report>
Exact error message with line numbers
Stack trace with preserved formatting
Error codes
</error_report>

<context>
Recent changes that preceded the error
Environment details (OS, runtime versions)
Whether bug appears consistently or intermittently
</context>

<relevant_code>
The failing function
Immediate dependencies
Type definitions
File paths and line numbers
</relevant_code>

<instructions>
Analyze systematically
Identify root cause
Then propose a fix
</instructions>
```

## Chain-of-Thought Debugging

Always ask Claude to show its work:

```
"Analyze this bug systematically. First, examine the error message and identify
the immediate cause. Then trace the execution flow backward to find the root
cause. Finally, suggest fixes with rationale."
```

Use `<thinking>` tags for complex analysis requiring substantial reasoning.

## ReAct Framework (Reasoning + Action)

Force iterative hypothesis testing:

```
Thought 1: I need to understand the expected behavior
Action 1: Review function specification
Observation 1: [spec details]
Thought 2: Now check actual output...
```

This reduces guessing and grounds conclusions in evidence.

## Self-Ask Decomposition

Before solving, decompose into diagnostic questions:

- What is the expected behavior?
- What is the actual behavior?
- Where does execution diverge?
- What changed recently?

## Role-Based Priming

Use this role for debugging:

```
"You are a senior Site Reliability Engineer debugging a production incident.
Your priorities:
- Understand system behavior completely
- Identify root causes, not symptoms
- Minimize speculation
- Base conclusions on evidence
- Consider system-wide implications

Do NOT:
- Jump to solutions without diagnosis
- Make assumptions about environment
- Ignore edge cases
- Provide fixes that only address symptoms"
```

## Context Management

### Token Budget Priority (High to Low)

**Always include (500-1000 tokens)**:
- Exact error messages
- Relevant code sections
- Stack traces
- Recent changes
- Expected vs actual behavior

**Selectively include**:
- Full file contents (when directly relevant)
- Configuration files
- Test cases
- Related documentation

**Omit or summarize**:
- Third-party library code
- Boilerplate
- Extensive comments
- Historical context beyond recent changes

### Lost-in-the-Middle Effect

LLMs weigh information at the **beginning and end** more heavily than the middle.

**Structure your prompts**:
1. START: Task instructions + specific error
2. MIDDLE: Code and context
3. END: Reiterate key constraints

### Multi-File Context

Provide hierarchical organization:

1. High-level architecture overview
2. Dependency flows (how files interact)
3. Relevant code with clear file paths and line numbers

Example data flow mapping:
```
User clicks login → LoginForm.tsx
→ dispatches loginUser → userSlice.ts
→ calls POST /api/auth/login → authService.ts
→ Response updates Redux state
→ UserProfile.tsx re-renders
Issue occurs at step 4 where state isn't updating
```

## Systematic Debugging Workflow

### Plan-Then-Execute (4 Steps)

1. **Explore**: Read relevant files without coding
2. **Plan**: Create detailed step-by-step approach
3. **Implement**: Execute with verification at each step
4. **Commit**: Document changes

### Five-Step Iterative Process

When initial fixes fail:

1. **Replication**: Copy exact prompts/parameters to reproduce consistently
2. **Problem Identification**: Ask Claude to EXPLAIN the incorrect output (not justify)
3. **Iteration**: Update prompts to handle edge cases with precise wording
4. **Double Checking**: Regenerate multiple times to verify consistency
5. **Benchmarking**: Test against full test suite

### Multi-Stage Prompt Chaining

**Phase 1 - Information Gathering**:
```
Analyze this bug report. Extract:
- Observable symptoms
- Known facts
- Information gaps
Do NOT propose solutions yet.
```

**Phase 2 - Hypothesis Generation**:
```
Based on the analysis, generate hypotheses ranked by:
- Likelihood
- Testability
- Impact
Specify what evidence would confirm/refute each.
```

**Phase 3 - Evidence Evaluation**:
```
Evaluate evidence against hypotheses.
Provide confidence ratings for each.
```

**Phase 4 - Solution**:
```
Propose solutions addressing confirmed root causes.
Include validation approaches.
```

## Root Cause Analysis (Five Whys)

```
Problem: [symptom]
Why is this happening? [Answer 1]
Why does Answer 1 occur? [Answer 2]
Why does Answer 2 occur? [Answer 3]
Why does Answer 3 occur? [Answer 4]
Why does Answer 4 occur? [Answer 5 - Root Cause]
```

Provide evidence citations at each step.

## Error-Type Specific Strategies

| Error Type | Strategy |
|------------|----------|
| Runtime errors | Trace data flow to identify where undefined values originate |
| Logic bugs | Step-by-step function walkthrough with sample data |
| Performance | Complexity analysis, Big O notation, bottleneck identification |
| Race conditions | Timing analysis, thread interleaving, lock strategy gaps |
| Memory leaks | Lifecycle analysis, unclosed resources, circular references |
| Intermittent bugs | Multi-iteration testing, statistical pattern gathering |

## Known Limitations

### When Claude Struggles

- Complex multi-system issues (microservices, APIs, distributed databases)
- Security vulnerabilities (attack vectors, exploitation scenarios)
- Architectural decisions (long-term maintainability, scalability)
- Performance optimization (production environment specifics)
- Configuration/infrastructure (environment-specific setups)
- Novel algorithms without training data parallels
- Race conditions and timing-dependent bugs

### Location Bias

- 60% of correctly localized faults appear in first 25% of code
- Only 13% detected in last 25%
- **Action**: Place critical code sections EARLIER in context

### Working Memory

- Effective working memory is much smaller than context window
- Tasks requiring tracking state across dozens of components will fail
- Claude excels at localized debugging, struggles with system-wide issues

## Failure Indicators (When to Stop)

Stop using Claude and escalate to human-led debugging when:

- After 2-3 iterations without meaningful progress
- Suggestions become repetitive without new information
- Solutions becoming more complex instead of simpler
- Confident but incorrect statements about APIs/methods
- Surface fixes that mask symptoms without addressing causes
- The "70% problem" - initial magic, then diminishing returns

### False Progress Patterns

- **Repetition loop**: Same suggestions reformulated
- **Complexity spiral**: Adding dependencies instead of fixing root cause
- **Hallucination pattern**: Confidently incorrect API/method info
- **Surface fix**: Masks symptoms without addressing design flaws

## Workarounds for Extended Reach

### Retrieval-Augmented Generation (RAG)

- Chunk large documents into 10-20 pages per chunk
- Use vector embeddings for semantic search
- Retrieve only relevant chunks based on error messages

### Debug-Gym Approach

Provide access to actual debugging tools:
- pdb, breakpoints, variable inspection
- Test function creation
- Studies show 30-182% performance improvement with tool access

### Multi-Agent Debugging

Run multiple Claude instances on different branches using git worktrees:
- Each tackles distinct components
- Eliminates blocking between debugging sessions

### Model Context Protocol (MCP)

Connect external knowledge sources:
- Google Drive docs with architectural decisions
- Slack conversations about similar bugs
- Internal wikis with troubleshooting guides
- Observability platforms (Sentry) with error traces

## Complexity Hierarchy

| Level | When to Use | Approach |
|-------|-------------|----------|
| 1 | Clear errors, single-file, syntax/logic | Claude alone |
| 2 | Multi-file with clear boundaries, domain knowledge needed | Claude + human guidance |
| 3 | System-wide, security implications, performance-critical | Human-led + Claude assistance |
| 4 | Distributed systems, production emergencies, security breach | Human only |

## Quality Indicators

### Strong Responses

- Explain WHY fixes work
- Reference specific error messages
- Provide context about the problem
- Suggest prevention approaches
- Acknowledge limitations/uncertainties

### Weak Responses

- Generic "try this" without rationale
- Ignore provided context
- Multiple random changes without reasoning
- Modify unrelated code
- Fail to address actual error messages

## Session Setup Checklist

Before debugging:

- [ ] CLAUDE.md updated with relevant project context
- [ ] Custom slash commands defined for recurring patterns
- [ ] Error messages prepared with full stack traces
- [ ] Recent changes documented
- [ ] Clear success criteria defined

## Validation Checklist

After debugging:

- [ ] Tests written that would have caught the original bug
- [ ] Solution verified across multiple scenarios including edge cases
- [ ] No regressions in related functionality
- [ ] Root cause documented (why it occurred, how fix addresses it)
- [ ] CLAUDE.md updated with patterns learned

## Time-Boxing

- Maximum 20-30 minutes before reassessing strategy
- If Claude repeatedly suggests same fixes → stop, switch approaches
- If more time explaining context than debugging → problem exceeds Claude's capabilities

## Integration Patterns

```bash
# Pipe logs for analysis
cat error.log | claude "explain these errors"

# Redirect dev server for pattern recognition
bun run dev > dev.log 2>&1

# Auto-generate commit messages
git diff | claude "generate commit message"
```

## CLAUDE.md Best Practices

Document in your CLAUDE.md:

- Common error patterns: "CORS errors on auth endpoints typically indicate API_URL misconfiguration"
- Stack-specific debugging: "State not updating after API calls: check if action is dispatched, verify reducer handles action type"
- Important constraints: "Never modify files in /generated"
- Edge cases to watch for
- Links to relevant documentation
