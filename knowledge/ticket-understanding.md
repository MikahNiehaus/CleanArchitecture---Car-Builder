# Ticket Understanding Methodology

> **TRIGGER**: Use this documentation when analyzing tickets, understanding requirements, clarifying vague requests, decomposing tasks, or preparing work for delegation to other agents.

## Core Philosophy

**37% of project failures stem from unclear requirements** (PMI). The cost of fixing a requirement error increases 100x when discovered in production versus during analysis. Invest time upfront to understand fully before acting.

**Key Principle**: The user's stated request is rarely the complete requirement. Your job is to uncover the TRUE need beneath the stated want.

## The Understanding Framework

### Level 1: Literal Understanding
What did the user literally say? Capture the exact words.

### Level 2: Intent Understanding
What do they actually want to accomplish? The goal behind the request.

### Level 3: Context Understanding
Why do they need this? What problem are they solving? Who benefits?

### Level 4: Implicit Understanding
What did they NOT say but will definitely expect? Industry standards, common patterns, obvious features.

### Level 5: Constraint Understanding
What limitations exist? Time, budget, technology, resources, dependencies.

## Chain-of-Thought Analysis

For every request, think through systematically:

```markdown
## Thinking Through Request: "[Original Request]"

### Step 1: Parse the Request
- Key nouns (objects/entities): [list]
- Key verbs (actions): [list]
- Qualifiers/modifiers: [list]
- Ambiguous terms: [list]

### Step 2: Identify the Goal
- Immediate goal: [what they asked for]
- Underlying goal: [why they need it]
- Business value: [what problem it solves]

### Step 3: Map Stakeholders
- Direct users: [who will use this]
- Indirect beneficiaries: [who else is affected]
- Decision makers: [who approves completion]

### Step 4: Define Success
- Observable outcome: [what will be different]
- Measurable criteria: [how to verify]
- Edge cases: [what could go wrong]

### Step 5: Identify Gaps
- Missing information: [what we don't know]
- Assumptions made: [what we're guessing]
- Clarifications needed: [questions to ask]
```

## The Five Whys Technique

Dig deeper to find root needs:

```
Request: "Add a button to export data"
Why? → Need to share data with finance team
Why? → Finance needs to reconcile with their system
Why? → Currently they manually copy data (error-prone)
Why? → No automated integration exists
Why? → Systems were built separately

ROOT NEED: Finance needs reliable data transfer (button is just ONE solution)
```

**This reveals**: Maybe an API integration or scheduled export would serve them better than a manual button.

## Requirements Elicitation Questions

### The Essential Seven (Ask These Always)

1. **WHAT**: "What specific behavior/outcome do you expect?"
2. **WHY**: "What problem does this solve? What happens if we don't do this?"
3. **WHO**: "Who will use this? Who else is affected?"
4. **WHEN**: "How urgent is this? When is it needed by?"
5. **WHERE**: "Where in the system/workflow does this fit?"
6. **HOW**: "How will we know it's working correctly?"
7. **HOW MUCH**: "What volume/scale do we need to support?"

### Clarification Question Categories

#### For Vague Terms
- "When you say '[term]', what specifically do you mean?"
- "Can you give me an example of '[term]'?"
- "How would you define '[term]' for someone unfamiliar with it?"

#### For Missing Context
- "What currently happens in this situation?"
- "What's the current workaround?"
- "How does this interact with [related feature]?"

#### For Implicit Requirements
- "Should this work on mobile devices as well?"
- "What should happen if [edge case]?"
- "Who needs to be notified when [action] happens?"

#### For Priority/Scope
- "If we could only deliver part of this, what's most essential?"
- "Is [related feature] part of this scope or future work?"
- "What's the minimum viable version?"

## INVEST Criteria Validation

Validate that requirements are well-formed:

| Criterion | Question | Warning Signs |
|-----------|----------|---------------|
| **I**ndependent | Can this be done without other tasks completing first? | "After we do X..." or dependent on unstarted work |
| **N**egotiable | Is there flexibility in how this is implemented? | Overly prescriptive ("must use React") vs outcome-focused |
| **V**aluable | Does completing this deliver user/business value? | Technical debt, refactoring with no user benefit |
| **E**stimable | Can we roughly estimate the effort? | "Build AI" - too vague to estimate |
| **S**mall | Can this be done in a reasonable timeframe? | Multi-week epics, multiple distinct features bundled |
| **T**estable | Can we verify when it's done? | "Make it better", "Improve performance" (unmeasurable) |

### Fixing Non-INVEST Requirements

| Problem | Example | Solution |
|---------|---------|----------|
| Too dependent | "After API is done, build UI" | Identify mock/stub approach or reorder |
| Too prescriptive | "Build with React hooks" | Reframe as outcome: "Display user dashboard" |
| No clear value | "Refactor utils folder" | Ask: "What problem does this solve?" |
| Not estimable | "Add AI features" | Decompose into specific capabilities |
| Too large | "Build authentication system" | Break into: login, logout, password reset, etc. |
| Not testable | "Make it user-friendly" | Define: "User can complete task in <3 clicks" |

## Acceptance Criteria Definition

Use Given-When-Then (Gherkin) format for clarity:

```gherkin
Feature: [Feature Name]

Scenario: [Specific scenario]
  Given [precondition/context]
  And [additional context if needed]
  When [action/trigger]
  Then [expected outcome]
  And [additional outcomes]
```

### Example Transformations

**Vague**: "Users should be able to search"

**Clear**:
```gherkin
Scenario: Basic search returns matching results
  Given I am on the search page
  And products exist with "laptop" in their name
  When I enter "laptop" in the search box
  And I click the search button
  Then I see a list of products containing "laptop"
  And results are sorted by relevance

Scenario: Empty search shows helpful message
  Given I am on the search page
  When I enter "xyznonexistent123" in the search box
  And I click the search button
  Then I see "No results found for 'xyznonexistent123'"
  And I see suggested search terms
```

## Scope Boundary Definition

### The Scope Triangle

```
        In Scope
           /\
          /  \
         /    \
        /      \
       /________\
   Out of Scope
```

**In Scope**: Explicit deliverables for THIS task
**Out of Scope**: Related work that is NOT included
**Boundaries**: The lines that separate them

### Boundary Documentation Template

```markdown
## Scope for: [Task Name]

### Explicitly In Scope
- [ ] [Specific deliverable 1]
- [ ] [Specific deliverable 2]

### Explicitly Out of Scope
- [ ] [Related item NOT included]
- [ ] [Future enhancement NOT included]

### Boundary Rules
1. This task covers [X] but NOT [Y]
2. We will modify [Component A] but NOT [Component B]
3. Support for [User Type 1] is included; [User Type 2] is future work

### Scope Change Protocol
If scope change is requested:
1. Document the requested change
2. Assess impact on timeline/effort
3. Get explicit approval before proceeding
4. Update this scope document
```

## Scope Creep Prevention

### Warning Signs
- "While we're at it, can we also..."
- "That reminds me, we should..."
- "It would be nice if..."
- "Users will probably want..."
- "Let's just quickly add..."

### Response Strategy

1. **Acknowledge**: "That's a good idea."
2. **Document**: "Let me note that for future work."
3. **Redirect**: "For this task, we're focused on [original scope]."
4. **Defer**: "We can evaluate that as a separate ticket."

### Scope Creep vs. Missing Requirement

| Scope Creep | Missing Requirement |
|-------------|---------------------|
| Nice to have | Must have for stated goal |
| Discovered during implementation | Should have been found in analysis |
| Adds new functionality | Completes existing functionality |
| Can be deferred | Cannot be deferred |

**If in doubt**: Ask "Can the original goal be achieved without this?"

## Task Decomposition

### Decomposition Principles

1. **Single Responsibility**: Each subtask does one thing
2. **Independence**: Minimize dependencies between tasks
3. **Testability**: Each subtask has clear completion criteria
4. **Assignability**: Each subtask can be given to one agent/person
5. **Parallelizability**: Identify what can be done simultaneously

### Decomposition Template

```markdown
## Task: [Main Task Name]

### Subtasks

| # | Subtask | Description | Depends On | Can Parallelize With | Agent |
|---|---------|-------------|------------|---------------------|-------|
| 1 | [Name] | [What] | None | 2, 3 | [agent] |
| 2 | [Name] | [What] | None | 1, 3 | [agent] |
| 3 | [Name] | [What] | None | 1, 2 | [agent] |
| 4 | [Name] | [What] | 1, 2, 3 | None | [agent] |

### Dependency Graph

```
[1]─┐
    ├──►[4]
[2]─┤
    │
[3]─┘
```

### Execution Order
1. Start: 1, 2, 3 (parallel)
2. After 1, 2, 3 complete: 4
```

## Common Ticket Types

### Bug Reports

**Must Extract**:
- Expected behavior
- Actual behavior
- Steps to reproduce
- Environment details
- Frequency (always/sometimes/rare)

**Clarifying Questions**:
- "What were you trying to accomplish when this happened?"
- "Does this happen consistently or intermittently?"
- "When did this start happening? What changed?"

### Feature Requests

**Must Extract**:
- User need/problem being solved
- Desired outcome
- Success criteria
- Edge cases
- Integration points

**Clarifying Questions**:
- "Who is the primary user of this feature?"
- "What's the current workaround?"
- "What's the minimum version that would be valuable?"

### Technical Tasks

**Must Extract**:
- Technical goal
- Success metrics
- Constraints/requirements
- Impact assessment
- Rollback plan

**Clarifying Questions**:
- "What problem does this solve for users (even indirectly)?"
- "What could go wrong? How do we detect/recover?"
- "What's the validation approach?"

## Red Flags in Requirements

### Ambiguous Language
- "Better", "improved", "enhanced" (compared to what?)
- "Fast", "quick", "responsive" (how fast?)
- "User-friendly", "intuitive" (for whom?)
- "Flexible", "scalable" (to what extent?)
- "Support for X" (what kind of support?)

### Missing Information
- No acceptance criteria
- No error handling specified
- No edge cases considered
- No user identification
- No priority indication

### Scope Bombs
- "Should be simple"
- "Just like [complex system]"
- "All the usual features"
- "Industry standard"
- "Obviously it should..."

## Definition of Done Template

```markdown
## Definition of Done: [Task Name]

### Functional Completion
- [ ] All acceptance criteria pass
- [ ] Edge cases handled
- [ ] Error states handled
- [ ] User feedback is clear

### Quality Completion
- [ ] Code reviewed and approved
- [ ] Tests written and passing
- [ ] No regressions introduced
- [ ] Performance acceptable

### Documentation Completion
- [ ] Code is self-documenting or commented
- [ ] User-facing docs updated (if applicable)
- [ ] API docs updated (if applicable)

### Deployment Completion
- [ ] Can be deployed independently
- [ ] Rollback plan exists
- [ ] Monitoring/alerting in place (if applicable)
```

## Orchestrator Handoff Format

When analysis is complete, provide to orchestrator:

```markdown
## Ready for Delegation: [Task Name]

### Executive Summary
[2-3 sentences: what needs to be done and why]

### Primary Agent Needed
**Agent**: [agent-name]
**Reason**: [why this agent]

### Task Specification
[Detailed requirements ready for agent]

### Acceptance Criteria
[Testable criteria for completion]

### Dependencies
- **Blockers**: [anything that must happen first]
- **Parallel Work**: [what can happen simultaneously]

### Risk Factors
- [Risk 1 and mitigation]

### Questions Resolved
[Summary of clarifications obtained]

### Questions Pending (if any)
[What still needs user input]
```

## Anti-Patterns in Ticket Analysis

1. **Assumption Cascade**: Making one assumption that leads to more assumptions
2. **Gold Plating**: Adding requirements the user didn't ask for
3. **Analysis Paralysis**: Asking infinite questions instead of reasonable clarity
4. **Literal Interpretation**: Taking requests at face value without probing
5. **Scope Blindness**: Not defining boundaries until scope creep occurs
6. **Happy Path Only**: Ignoring error cases and edge conditions
7. **Technical Tunnel Vision**: Focusing on implementation over user need
8. **Premature Decomposition**: Breaking down before understanding the whole

## Quality Checklist

Before marking analysis complete:

- [ ] Original request captured verbatim
- [ ] User intent understood (not just words)
- [ ] Business value identified
- [ ] All ambiguous terms clarified
- [ ] Acceptance criteria defined (Given-When-Then)
- [ ] Scope boundaries explicit
- [ ] Out of scope documented
- [ ] Task decomposed into subtasks
- [ ] Dependencies mapped
- [ ] Risks identified
- [ ] Definition of done specified
- [ ] Handoff notes prepared for orchestrator

## Sources and Further Reading

Research informing this methodology:
- [Underspecification in LLM Prompts](https://arxiv.org/html/2505.13360v1) - Managing vague requirements
- [Task Decomposition Techniques](https://learnprompting.org/docs/advanced/decomposition/introduction) - Breaking down complex tasks
- [Chain-of-Thought Prompting](https://www.ibm.com/think/topics/chain-of-thoughts) - Systematic reasoning
- [Requirements Elicitation](https://www.bridging-the-gap.com/what-questions-do-i-ask-during-requirements-elicitation/) - Question frameworks
- [INVEST Criteria](https://www.boost.co.nz/blog/2021/10/invest-criteria) - User story quality
- [Acceptance Criteria Best Practices](https://www.atlassian.com/work-management/project-management/acceptance-criteria) - Defining done
- [Scope Creep Prevention](https://asana.com/resources/what-is-scope-creep) - Managing boundaries
- [Five Whys Analysis](https://tulip.co/glossary/five-whys/) - Root cause identification
