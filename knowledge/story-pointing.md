# Story Pointing Guide

> **TRIGGER**: Use this documentation when estimating tickets, story pointing, sprint planning, analyzing ticket complexity, or when asked about Fibonacci estimation.

## Core Concept

Story points measure **relative effort** combining three factors:
- **Complexity**: Technical difficulty, unknowns, integrations
- **Effort**: Volume of work, scope of changes
- **Risk**: Dependencies, uncertainty, legacy code

Story points are NOT time estimates. They're team-specific measures that improve through calibration.

## Fibonacci Scale Reference

| Points | Complexity | Effort | Risk | Typical Work |
|--------|-----------|--------|------|--------------|
| **1** | Minimal | Very low | None | Simple text changes, config updates, typo fixes |
| **2** | Low | Low | Minor | Single form field, basic CRUD, simple UI component |
| **3** | Moderate | Moderate | Some | Multi-field forms, standard API endpoints, auth additions |
| **5** | Medium | Medium-High | Moderate | Frontend + backend + DB work, integrations, search with filtering |
| **8** | High | High | High | Multi-provider systems, complex reporting, significant refactoring |
| **13** | Very High | Very High | Very High | **MUST BE SPLIT** - Major integrations, framework migrations |

## The Bucket Metaphor

Think of point values as buckets, not precise measures. If work doesn't fit in the 8-point bucket, use the 13-point bucket. **When uncertain between two values, round UP.**

## Five Questions Before Estimation

Before assigning points, answer these questions:

1. **Who and why?** - User persona and pain point being addressed
2. **What problem?** - Specific business outcome, not just features
3. **Success metrics?** - Testable, specific acceptance criteria
4. **Dependencies?** - Internal (your team) and external (other teams, APIs)
5. **Scope boundaries?** - What's explicitly OUT of scope

If you can't answer these confidently → **ticket needs clarification**

## Complexity Multipliers

These factors elevate estimates by 1-2 Fibonacci levels:

- First time working in codebase area
- No existing tests (need to create coverage)
- Third-party API integration
- Multiple team dependencies
- Unclear or changing requirements
- Legacy code refactoring
- Performance optimization needed
- Security-critical changes
- Data migration required

## When to Create a Spike

Create a time-boxed spike (1-3 days) when:

- Technical uncertainty is high
- Estimates diverge widely (team votes range 3 to 21)
- Adopting new technology
- Implementation approach is unclear

**Spikes produce information and recommendations, not working code.**

## When to Proceed with Estimation

Proceed directly when:

- Similar work has been completed before
- Team understands the technical approach
- Estimates converge (within 1-2 Fibonacci levels)
- Requirements are clear

## Story Decomposition Strategies

If a story exceeds 8 points, break it down:

### Separate by Concern
- Backend API (3 points)
- Frontend UI (3 points)
- Database/Integration (2 points)

### Extract Non-Functional Requirements
- Core feature (5 points)
- Performance optimization (3 points)
- Security hardening (2 points)

### Vertical Slices
Instead of "Build entire shopping cart" (13+):
- Add single item to cart (3)
- Display cart with quantity updates (3)
- Checkout integration (5)

## Writing Justifications

Template: **"[X] points because: [Volume], [Complexity], [Risk]. Similar to [reference story]."**

### Include:
- Specific complexity drivers (new integration, unfamiliar area)
- Identified risks (dependencies, brittle code)
- Scope indicators (affects 5 components vs 2)
- Comparisons to known reference stories

### Omit:
- Implementation details (emerge during sprint)
- Hour/day time conversions
- Lengthy explanations

### Example:
"5 points because: Changes authentication module (high risk), requires coordination with 2 teams (complexity), includes integration tests (volume). Similar to the OAuth integration from Sprint 12."

## Planning Poker Communication

When estimates diverge significantly (e.g., 3 vs 13):

1. Highest and lowest estimators explain reasoning first
2. Focus on **factors seen differently**, not defending numbers
3. Time-box to 2-3 minutes per story
4. If no consensus after 2 rounds → create spike or defer

### Good patterns:
- "I see authentication risk because we'll touch the SSO module"
- "This is twice the size of Story X we did last week"

### Avoid:
- "This should take me 2 days" (individual capacity)
- "I always give login features 5 points" (not relative to this story)

## Reference Stories

Maintain 2-3 reference stories for each point value as calibration anchors:

| Points | Reference Story | Why It's That Size |
|--------|----------------|-------------------|
| 1 | "Update footer copyright" | Single file, no logic |
| 2 | "Add email validation to signup" | One component, standard pattern |
| 3 | "User profile edit form" | Multiple fields, validation, API call |
| 5 | "Payment method selection" | Frontend + backend + external API |
| 8 | "Multi-currency support" | Multiple services, exchange rates, testing |

Update references quarterly based on retrospectives.

## Questions Requiring Clarification

**Don't estimate until these are answered:**

- Business value unclear ("Who is this for and why?")
- Acceptance criteria ambiguous ("Improve performance" - what's the target?)
- Technical feasibility uncertain ("Can our database handle this?")
- Critical dependencies unresolved ("Do we have API credentials?")

## Proceed with Documented Assumptions

**Can proceed when:**

- Implementation details to emerge during sprint
- Minor edge cases with low risk
- Standard technical decisions within team's purview
- Story meets Definition of Ready

**Document assumptions explicitly:**
"Assuming fewer than 1% of users have legacy accounts; will add migration path if data shows otherwise."

## Risk Assessment Framework

| Risk Level | Proceed? | Examples |
|------------|----------|----------|
| **High** | NO | External dependencies unclear, acceptance criteria undefined, technical feasibility unknown |
| **Medium** | Yes, with assumptions | Minor edge cases, optimization targets, nice-to-have features |
| **Low** | Yes | Refactoring approach, code organization, test strategy |

**Key question:** "If this assumption is wrong, does it invalidate our estimate or just change our approach?"

## AI Assistant Guidelines

When providing estimates:

### Provide Confident Assessment When:
- Patterns clearly recognizable from historical data (>80% accuracy)
- Technical complexity well-defined
- Similar tickets exist with known outcomes
- Scope is narrow and well-specified

### Ask Clarifying Questions When:
- Ambiguity could change estimate significantly
- Multiple valid interpretations exist
- Dependencies cross system boundaries
- Historical data shows high variance (>40% deviation)

### Output Format:
```
Summary: [One sentence description]
Estimate: [X] story points (Confidence: [High/Medium/Low])

Breakdown:
- Complexity: [Low/Medium/High] - [reason]
- Effort: [Low/Medium/High] - [reason]
- Risk: [Low/Medium/High] - [reason]

Similar Stories: [Reference 2-3 comparable tickets]

Assumptions: [List any assumptions made]

Recommendation: [Clear next action]
```

## Definition of Ready Checklist

Before estimation, verify:

- [ ] Clear enough for team to understand what and why
- [ ] Small enough to complete within a sprint
- [ ] Acceptance criteria are testable and specific
- [ ] Dependencies identified and documented
- [ ] Scope boundaries defined (what's OUT of scope)
- [ ] No blocking questions remain

## Red Flags - Ticket Not Ready

- Describes solution without explaining problem
- Vague success criteria ("improve", "enhance", "optimize")
- Missing user persona or business context
- Unknown external dependencies
- Estimate exceeds 13 points
- Team estimates diverge by more than 2 Fibonacci levels
