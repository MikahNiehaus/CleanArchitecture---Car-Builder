# Development Workflow & Reliable Execution Guide

> **TRIGGER**: Use this documentation when implementing features, executing development tasks, working on complex multi-step implementations, or when you need to ensure thorough and safe code execution.

## Core Principle

Claude 4.x models follow instructions precisely but won't go "above and beyond" unless explicitly told to. Structure prompts like you're managing a brilliant but inexperienced developer who needs clear boundaries, systematic processes, and mandatory verification steps.

## Common Failure Patterns to Prevent

| Pattern | Problem | Solution |
|---------|---------|----------|
| Premature stopping | Completes part of task, moves on | Structured progress tracking with checkpoints |
| Test manipulation | Modifies tests to match buggy code | TDD workflow with tests committed first |
| Context drift | Forgets earlier instructions | Recursive self-display of rules |
| Scope creep | Makes "improvements" beyond request | Explicit scope boundaries |
| Skipping verification | Assumes tests pass without running | Mandatory test execution at each phase |

## The CLAUDE.md File

Create a CLAUDE.md file in your project root that establishes:

1. **Purpose section**: Why these rules exist
2. **Priority rules**: Using MUST, SHOULD, SHOULD NOT
3. **Implementation patterns**: Specific to your codebase
4. **Test requirements**: Exact commands and coverage expectations
5. **Blocked operations**: What Claude should never do
6. **Custom commands**: Reusable workflow templates

### Example Structure
```markdown
# Project Rules

## Purpose
Ensure maintainability, safety, and team velocity.

## Absolute Rules (MUST)
- MUST run tests after every code change
- MUST use dependency injection for all services
- MUST NOT modify tests to make them pass

## Patterns
- Repository pattern for data access
- Use existing error handling in /src/errors/

## Blocked Operations
- Never DELETE without WHERE clause
- Never access production database
```

## Plan-Before-Execute Workflow

**Never jump straight to coding.** Follow this sequence:

### Phase 1: Explore
- Read relevant files
- Understand current architecture patterns
- Identify relevant existing modules
- List integration points
- Identify potential risks

### Phase 2: Plan
- Create detailed step-by-step approach
- Document modules to create/modify with rationale
- Identify new abstractions needed
- Define testing strategy
- Create rollback plan
- **Save to plan.md and wait for approval**

### Phase 3: Implement
- Execute with verification at each stage
- Run tests and fix failures
- Check integration with existing code
- Update plan.md progress
- Ask for review before next phase

### Phase 4: Commit
- Document changes
- Create PR with updated documentation

## Test-Driven Development (TDD) Workflow

### The Correct Sequence

1. **Write tests FIRST** based on expected input-output pairs
   - Be explicit about following TDD to avoid mock implementations

2. **Run tests and confirm they FAIL**
   - Do NOT write implementation code yet
   - Verify failure message describes missing behavior

3. **Commit tests** when satisfied with coverage

4. **Implement** functionality to make tests pass
   - Do NOT modify tests during this phase

5. **Run tests and confirm they PASS**
   - Iterate on failures
   - Show final test output

6. **Commit** the working implementation

### Critical Rule
```
It is UNACCEPTABLE to remove, comment out, or edit tests to make them pass.
Tests define requirements. If a test fails, fix the CODE, never the test.
```

## Database Safety Protocol

### Before ANY Database Operation

1. **Identify environment** by checking environment variables
2. **Analyze connection strings** for production indicators
3. **Confirm database name** matches expected patterns
4. **Verify appropriate permissions**

### Environment Verification Output
```
Environment: DEVELOPMENT
Database: myapp_dev_john
Host: localhost
Safety Level: SAFE - Proceed with caution
```

### Operation Classification Matrix

| Operation Type | Development | Staging | Production |
|---------------|-------------|---------|------------|
| READ | Proceed | Proceed | Proceed |
| WRITE | Proceed | Warn | Warn |
| DESTRUCTIVE | Warn | Block | REFUSE |
| SCHEMA | Migrations only | Migrations only | Migrations only |

### Connection String Validation

Check for production indicators:
- `prod`, `production` in connection string
- `.rds.amazonaws.com`
- `.database.windows.net`

Require safe indicators:
- `localhost`, `127.0.0.1`
- `dev`, `test`, `_dev_`, `_test_`
- `.local`

## Explicit Thoroughness Requests

Claude 4.x requires explicit instructions for comprehensive solutions:

**Instead of:** "Create an analytics dashboard"

**Use:** "Create an analytics dashboard that:
- Includes filtering, sorting, export, and real-time updates
- Follows Clean Architecture with separation of concerns
- Uses dependency injection for testability
- Includes comprehensive error handling
- Goes beyond basics to create fully-featured implementation"

## Circuit Breakers

Prevent runaway token consumption:

```
If tests fail, iterate to fix them. Maximum 5 attempts.
If all 5 attempts fail without resolution:
- STOP
- Summarize what you tried
- Explain why it's failing
- Ask for human guidance
```

## State Tracking for Long Sessions

Create persistent files to maintain continuity:

### progress.json
```json
{
  "current_phase": "implementation",
  "completed_tasks": ["setup", "models", "api"],
  "current_task": "frontend components",
  "next_tasks": ["testing", "documentation"],
  "blockers": []
}
```

### For New Sessions
1. Verify directory (`pwd`)
2. Read progress.json
3. Review recent git history
4. Run tests
5. Continue from next task

## Context Management

### When to Compact/Reset
- Use `/compact` when above 150K tokens (75% capacity)
- Use `/clear` between unrelated tasks
- Start fresh sessions for completely new features

### Preventing Context Drift

Add to CLAUDE.md:
```
At the end of every response, reproduce the safety rules verbatim
within <rules_reminder> tags to keep them fresh in context.
```

## Scope Control

Prevent scope creep with explicit boundaries:

```
Fix ONLY the code directly related to [specific issue].
Do NOT:
- Refactor unrelated code
- "Improve" things outside this scope
- Make architectural changes

After making changes:
- Run ONLY the tests for [affected module]
- Report what changed and what tests you ran
```

## Iterative Development Phases

### Multi-Turn Conversation Pattern

**Turn 1 - Analysis:**
"I need to implement [FEATURE]. First, analyze WITHOUT writing code:
1. Read files in [DIRECTORY]
2. Understand current architecture patterns
3. Identify relevant existing modules
4. List integration points
5. Identify potential risks
Provide summary with recommendations."

**Turn 2 - Design:**
"Based on your analysis, create detailed plan:
1. Modules to create/modify (with rationale)
2. New abstractions needed
3. Database schema changes
4. API contract changes
5. Testing strategy
6. Rollback plan
Save to plan.md. Wait for approval before coding."

**Turn 3 - Implementation:**
"Implement Phase 1 of plan.md: [SPECIFIC STEPS].
After implementation:
1. Run tests and fix failures
2. Check integration with existing code
3. Update plan.md progress
4. Ask for review before Phase 2"

## XML Tag Structure for Prompts

Claude was trained with XML tags, making it exceptionally good at parsing structured prompts:

```xml
<context>
System description and constraints
</context>

<current_state>
Existing code and patterns
</current_state>

<desired_state>
Goal and requirements
</desired_state>

<safety_requirements>
Non-negotiable constraints
</safety_requirements>

<verification_steps>
How to confirm correctness
</verification_steps>
```

## Anti-Patterns to Explicitly Forbid

In CLAUDE.md, explicitly prohibit:

- **God Objects**: Classes over 200 lines doing multiple things
- **Anemic Domain Models**: All logic in services, none in domain
- **Circular Dependencies**: A imports B imports A
- **Missing Error Handling**: Only happy path implemented
- **Hardcoded Configuration**: Magic strings/numbers
- **Tight Coupling**: Direct concrete dependencies
- **Layer Violations**: Domain calling infrastructure

## Verification Checklist

After code generation, verify:

1. Architecture violations?
2. Missing error handling for edge cases?
3. Security vulnerabilities?
4. Performance bottlenecks?
5. Test coverage gaps?
6. Anti-patterns present?

## CI/CD Environment Parity

When creating configurations, specify both contexts:

"Create a deployment script that works both:
- Locally for developer testing (using local Docker)
- In CI/CD pipeline (using hosted agents)

Requirements:
- Detect execution context (local vs CI)
- Use environment-appropriate authentication
- Provide clear logging for debugging in both contexts
- Include validation steps that work locally
- Document how to test locally before committing"

## Quick Reference: Session Start Checklist

- [ ] Read CLAUDE.md for project rules
- [ ] Check progress.json for current state
- [ ] Review recent git commits
- [ ] Run existing tests to establish baseline
- [ ] Identify task scope and boundaries
- [ ] Create/update todo list for tracking
