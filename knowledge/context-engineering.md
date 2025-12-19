# Context Engineering for Agents

TRIGGER: context, token, attention, memory, write, select, compress, isolate, scratchpad, note-taking

## Overview

Context engineering is "the art and science of filling the context window with just the right information at each step of an agent's trajectory." The goal: find the smallest set of high-signal tokens that maximize the likelihood of your desired outcome.

## The Four Pillars

### Pillar 1: Write (System Prompts)

**Principle**: Find the "right altitude" - specific enough to guide behavior, flexible enough to provide strong heuristics.

**DO**:
- Organize into distinct sections using XML tags or Markdown headers
- Start minimal, add instructions based on failure modes
- Use direct, imperative language
- Provide concrete examples instead of abstract rules

**DON'T**:
- Hardcode brittle if-else logic
- Use overly vague guidance ("be helpful")
- Include every possible edge case
- Write essay-style prose

**Example - Right Altitude**:
```markdown
## Error Handling
When you encounter errors:
1. Parse the error message for root cause
2. Check if it's a known pattern (see error taxonomy)
3. Apply the appropriate recovery protocol
4. If stuck after 3 attempts, report BLOCKED

Don't silently ignore errors or proceed with broken state.
```

### Pillar 2: Select (Tool & Example Curation)

**Principle**: Design minimal viable toolsets. Tools are prominent in context - each unused tool wastes attention budget.

**Tool Design Rules**:
- Self-contained (no implicit dependencies)
- Robust to errors (handle edge cases internally)
- Extremely clear documentation
- Single purpose (avoid multi-function tools)

**Example Curation**:
- Use diverse, canonical examples (not exhaustive edge cases)
- For LLMs, "examples are the 'pictures' worth a thousand words"
- Curate 3-5 high-quality examples per behavior

**Token Cost Guide**:
| Item | Approximate Tokens |
|------|-------------------|
| Tool definition | 100-300 |
| Example (short) | 50-100 |
| Example (detailed) | 200-500 |
| Knowledge base (small) | 500-1000 |
| Knowledge base (large) | 2000-5000 |

### Pillar 3: Compress (Long-Horizon Management)

**Strategies**:

#### 3A: Compaction (Summarization)
When context nears limit:
- Distill critical details
- Discard redundant tool outputs
- Preserve: architectural decisions, unresolved bugs, implementation details
- Start by maximizing recall, iterate for precision

#### 3B: Structured Note-Taking (Scratchpad)
Agents write persistent notes outside context window:

```markdown
## Scratchpad Pattern

### When to Use
- During multi-step reasoning
- When tracking multiple items
- For discoveries that need persistence

### How to Implement
1. Create workspace/[task-id]/scratchpad.md
2. Write key findings as you discover them
3. Structure for quick retrieval:

## Scratchpad: [Task ID]

### Key Discoveries
- [timestamp] [finding]

### Intermediate Results
- [step] [result]

### Questions to Resolve
- [question]

### Decisions Made
- [decision]: [reasoning]
```

#### 3C: Sub-Agent Architecture
Specialized agents return condensed summaries (1,000-2,000 tokens) instead of full context:

```
Main Agent Context: Full task
     │
     ├── Sub-agent A (Research)
     │   └── Returns: 500-token summary
     │
     ├── Sub-agent B (Implementation)
     │   └── Returns: 1000-token summary
     │
     └── Sub-agent C (Testing)
         └── Returns: 500-token summary
```

### Pillar 4: Isolate (Dynamic Retrieval)

**Principle**: Use "just-in-time" context with lightweight identifiers.

**Patterns**:

#### 4A: Lightweight References
```
INSTEAD OF: Pasting entire file content
USE: "Read src/auth/login.ts for implementation details"
```

#### 4B: Progressive Disclosure
```
Initial context: File paths + descriptions
On-demand: Read specific files when needed
Never: Load everything upfront
```

#### 4C: Metadata-Guided Exploration
Use file structure, naming conventions, timestamps to guide what to read:
```
Looking for auth logic?
→ Check src/auth/ folder (naming convention)
→ Look at recently modified files (timestamp)
→ Read README in that folder first (metadata)
```

## Context Budget Guidelines

### Per-Agent Budgets

| Agent Type | Max Init Tokens | Max Output Tokens | Rationale |
|------------|-----------------|-------------------|-----------|
| Lightweight (explore) | 3,000 | 1,000 | Quick operations |
| Standard (test, debug) | 8,000 | 3,000 | Normal tasks |
| Heavy (architect, workflow) | 15,000 | 5,000 | Complex reasoning |
| Orchestrator | 25,000 | 5,000 | Coordination overhead |

### Token Efficiency Techniques

1. **READ vs Paste**: 50 tokens instruction vs 2000 tokens content
2. **Reference vs Inline**: Link to files vs paste file content
3. **Summary vs Full**: Key findings vs complete output
4. **Selective Loading**: Load relevant sections vs entire knowledge base

## Anti-Patterns (What NOT to Put in Context)

### Context Dumping
```
BAD:  Paste entire file contents into chat history
GOOD: Read file, extract relevant section, summarize
```

### Kitchen Sink Tools
```
BAD:  One tool that does 20 different operations
GOOD: 20 focused tools that each do one thing well
```

### Example Overload
```
BAD:  50 examples covering every edge case
GOOD: 5 diverse examples showing key patterns
```

### Outdated Context
```
BAD:  Keep old file contents in context after editing
GOOD: Re-read files after modifications
```

### Unstructured Notes
```
BAD:  Stream of consciousness notes
GOOD: Structured format with clear sections
```

## Context Quality Checklist

Before any agent action, verify:

- [ ] **Relevant**: Is all context necessary for this task?
- [ ] **Fresh**: Is information current (not outdated)?
- [ ] **Structured**: Is context organized for easy retrieval?
- [ ] **Minimal**: Could we accomplish this with less context?
- [ ] **Referenced**: Are we using pointers vs copies?

## Integration with Workspace

```
workspace/[task-id]/
├── context.md          # Primary task state (structured)
├── scratchpad.md       # Working notes (append-only)
├── outputs/            # Generated artifacts (on disk, referenced)
└── snapshots/          # Point-in-time captures (compressed)
```

### When to Use Each

| File | Purpose | Update Frequency |
|------|---------|------------------|
| context.md | Orchestration state | After each agent |
| scratchpad.md | Working memory | During task execution |
| outputs/ | Artifacts | On generation |
| snapshots/ | Recovery points | At checkpoints |

## Attention Management

### High-Value Context (Keep in Focus)
- Current task requirements
- Immediate next step
- Critical constraints
- Recent errors/blockers

### Medium-Value Context (Reference as Needed)
- Full task plan
- Agent contributions
- File contents
- Test results

### Low-Value Context (Archive/Compress)
- Superseded plans
- Old tool outputs
- Verbose logs
- Historical decisions (keep summary only)

---

*"The context window is not just memory—it's an attention budget that depletes with each new token."*
