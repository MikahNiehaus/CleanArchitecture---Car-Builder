# Code Documentation Guide

> **TRIGGER**: Use this documentation when writing docstrings, comments, README files, API documentation, or when the user asks for documentation.

## Core Philosophy

- Documentation explains the **WHY**, not the WHAT or HOW
- Code shows what it does; documentation explains reasoning invisible to code readers
- Treat prompts like engineering specifications with explicit constraints
- The verbatim copying trap: Never paraphrase code line-by-line

## XML Tag Structure for Documentation Prompts

```xml
<instructions>
Write comprehensive documentation for the provided code.
Focus on explaining the purpose, business logic, and "why" behind decisions.
Do NOT simply restate what the code does line-by-line.
</instructions>

<code>
[Your actual code here]
</code>

<requirements>
- Explain the purpose and business context
- Document parameters with types, constraints, and valid ranges
- Include practical usage examples showing common scenarios
- Note edge cases, error handling, and performance considerations
- Avoid verbatim code repetition or implementation narration
</requirements>

<output_format>
Use [language]-appropriate docstring format
Include: Summary, Parameters, Returns, Raises/Throws, Examples, Notes
</output_format>
```

## Chain-of-Thought for Documentation

Before writing documentation, analyze in `<thinking>` tags:

1. Identify the function's primary purpose and business value
2. Determine what problem it solves for users
3. Analyze the algorithm/approach and why this approach was chosen
4. Identify edge cases, error conditions, and performance characteristics
5. Note any non-obvious design decisions or constraints

## Critical Rules

```
DO NOT:
- Restate what the code does line-by-line
- Copy code patterns into prose ("multiplies X by Y")
- Describe implementation details visible in the code
- Write "this function does X" without explaining WHY

DO:
- Explain the business purpose and real-world use case
- Document the "why" behind design decisions and trade-offs
- Provide context a new team member would need
- Include practical usage examples with realistic scenarios
- Note edge cases, constraints, and error handling strategies
```

## Good vs Bad Documentation Example

**Bad (just restates code):**
```
"""Calculates discount.
This function takes a price and discount_rate parameter. It multiplies
the price by 1 minus the discount_rate. Then it returns the result.
"""
```

**Good (explains WHY):**
```
"""Calculate final price after applying a percentage discount.

This function implements the core pricing logic for promotional codes
in e-commerce checkout flows. The discount is applied multiplicatively
to support stacking with other promotions.

Args:
    price (float): Original item price in dollars, must be positive
    discount_rate (float): Discount as decimal between 0 and 1
        (e.g., 0.2 represents 20% off)

Returns:
    float: Final price after discount

Example:
    >>> calculate_discount(100.0, 0.2)
    80.0

Note:
    Does not handle currency conversion. Ensure all prices are in
    the same currency before applying discounts.
"""
```

## Role-Based Prompting for Documentation

```xml
<role>
You are a senior technical writer creating documentation for experienced
developers who can read and understand code. Your audience doesn't need you
to narrate what the code does—they can see that themselves.

Your job is to explain what they CAN'T see from reading code alone:
- The business reasoning and requirements driving this implementation
- Why this approach was chosen over alternatives
- What edge cases and gotchas matter in practice
- How this fits into the larger system architecture
- What assumptions and constraints apply

Assume your reader is competent but unfamiliar with this specific codebase.
</role>
```

## Audience-Specific Documentation

### For Junior Developers (0-2 years)
- Start with clear prerequisites
- Explain non-obvious design patterns
- Include "why" explanations for non-intuitive approaches
- Provide complete, runnable examples with expected output
- Anticipate common mistakes and misconceptions

### For Senior Developers (5+ years)
- Lead with architectural context
- Focus on design trade-offs and why alternatives weren't chosen
- Document performance characteristics and scalability
- Note subtle edge cases and threading/concurrency implications
- Skip obvious error cases; focus on non-trivial failure modes

### Layered Documentation (Progressive Disclosure)
1. **QUICK START**: One paragraph with purpose, signature, basic example
2. **DETAILED GUIDE**: Full parameters, return values, 2-3 usage examples
3. **DEEP DIVE**: Implementation notes, design rationale, edge cases, performance

## Language-Specific Conventions

### Python (Google-style docstrings)
- Use imperative mood ("Return X" not "Returns X")
- Type hints in signature, not docstring
- First line: summary under 79 characters
- Include: Args, Returns, Raises, Example, Note

### JavaScript/TypeScript (TSDoc/JSDoc)
- Do NOT repeat type information from TypeScript signature
- Use @param, @returns, @throws tags
- Include @example with executable code blocks
- Use @remarks for design rationale

### Java (Javadoc)
- First sentence becomes summary (ends at period + space)
- Tag order: @param, @return, @throws, @see, @since
- Use `<p>` for paragraph breaks
- Document all checked exceptions

### C++ (Doxygen)
- Use @brief for one-line summary
- Document template parameters with @tparam
- Include complexity analysis in @par Complexity
- Use @pre and @post for contracts

## Documentation Focus Balance

| Focus | % of Content | What to Include |
|-------|--------------|-----------------|
| WHY | 80% | Business purpose, design decisions, trade-offs |
| WHAT | 15% | High-level behavior, inputs/outputs, errors |
| HOW | 5% | Only when implementation is surprising/non-obvious |

## Inline Comments Rules

Write inline comments ONLY when:
- Explaining why a workaround exists (reference bug ticket)
- Noting performance optimizations that look suboptimal but are measured
- Documenting business rules not obvious from code
- Marking intentional deviations from best practices

NEVER write inline comments that:
- Restate what the code obviously does
- Duplicate information in docstrings
- Describe standard language features

## Technical Writing Fundamentals

### Active Voice (80-90% of the time)
- "The function validates input" NOT "Input is validated by the function"
- "Returns an error" NOT "An error is returned"

### Sentence Structure
- Keep most sentences under 25 words
- Front-load important information: conclusion first, then details
- Keep subject and verb close together (within 5-7 words)

### Terminology Consistency
- Choose one term per concept and use it everywhere
- Don't switch between "user", "customer", "account holder"
- Define technical terms on first use

## Code Examples Requirements

Every code example must:
- Be complete and immediately runnable (include imports, setup)
- Use realistic variable names and data
- Show both success and error cases
- Include expected output as comments
- Stay under 20 lines when possible
- Follow language best practices

## Documentation Maintenance

- Live in same Git repository as code (docs-as-code)
- Update in same PR/commit as code changes
- Include "last updated" dates for time-sensitive content
- Delete dead documentation aggressively
- Test examples automatically in CI/CD

## Quality Checklist

Before completing documentation:

- [ ] Can a new team member understand purpose in under 60 seconds?
- [ ] Can someone use the API correctly without reading implementation?
- [ ] Are code examples copy-pasteable and runnable?
- [ ] Does documentation answer "why" questions that code cannot?
- [ ] Is technical terminology consistent throughout?
