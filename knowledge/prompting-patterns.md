# Prompting Patterns & Quality Rules

TRIGGER: prompt, quality, better, improve, response, output, rules, patterns, chain of thought, reasoning

## Overview

This knowledge base contains proven patterns and rules that improve AI response quality. Use these when you need better outputs, clearer reasoning, or more reliable results.

## Core Quality Rules

### Rule 1: Be Specific, Not Vague
```
BAD:  "Make it better"
GOOD: "Reduce latency by optimizing the database query in getUserOrders()"

BAD:  "Fix the bug"
GOOD: "Fix the null pointer exception on line 45 when user.email is undefined"
```

### Rule 2: Provide Context First
```
BAD:  "Write a function to calculate tax"
GOOD: "We're building an e-commerce checkout for US customers.
       Sales tax varies by state (0-10%).
       Write a function to calculate tax given cart total and state code."
```

### Rule 3: Specify Output Format
```
BAD:  "Analyze this code"
GOOD: "Analyze this code and provide:
       1. Security vulnerabilities (list each with severity)
       2. Performance issues (with line numbers)
       3. Suggested fixes (code snippets)"
```

### Rule 4: Use Examples (Few-Shot)
```
"Convert these sentences to past tense:

Example 1: 'I run' → 'I ran'
Example 2: 'She eats' → 'She ate'

Now convert: 'They swim'"
```

### Rule 5: Break Complex Tasks into Steps
```
BAD:  "Build me an authentication system"
GOOD: "Let's build authentication step by step:
       1. First, design the user schema
       2. Then, implement password hashing
       3. Next, create login/logout endpoints
       4. Finally, add JWT token handling

       Start with step 1."
```

## Reasoning Patterns

### Chain of Thought (CoT)
Force step-by-step reasoning:
```
"Think through this step by step:
1. What is the current behavior?
2. What is the expected behavior?
3. What could cause the difference?
4. How can we fix it?"
```

### Self-Critique
Ask for self-evaluation:
```
"After providing your solution:
1. What are potential weaknesses?
2. What edge cases might fail?
3. How confident are you (1-10)?
4. What would make you more confident?"
```

### Adversarial Thinking
Consider failure modes:
```
"Before implementing, consider:
- How could this fail?
- What would an attacker try?
- What happens under heavy load?
- What if the input is malformed?"
```

### Rubber Duck Debugging
Explain to understand:
```
"Explain this code line by line as if teaching a junior developer.
Then identify where the bug might be."
```

## Output Quality Patterns

### Structured Output
Request specific structure:
```
"Provide your analysis in this format:

## Summary
[2-3 sentence overview]

## Findings
- Finding 1: [description]
- Finding 2: [description]

## Recommendations
1. [Priority 1 action]
2. [Priority 2 action]

## Code Changes
[specific code with file:line references]"
```

### Confidence Scoring
Get uncertainty estimates:
```
"For each recommendation, include:
- Confidence: HIGH/MEDIUM/LOW
- Reasoning: Why this confidence level
- Alternatives: Other approaches if confidence is low"
```

### Verification Steps
Build in checks:
```
"After writing the code:
1. Trace through with example input
2. Identify edge cases
3. Check error handling
4. Verify it matches requirements"
```

## Task-Specific Patterns

### For Code Review
```
"Review this code for:
1. Bugs (logic errors, off-by-one, null safety)
2. Security (injection, auth, data exposure)
3. Performance (N+1 queries, unnecessary loops)
4. Maintainability (naming, complexity, DRY)

For each issue:
- Severity: CRITICAL/HIGH/MEDIUM/LOW
- Line number
- Problem description
- Suggested fix"
```

### For Debugging
```
"Debug this issue using this approach:
1. Reproduce: What steps trigger it?
2. Isolate: What's the smallest failing case?
3. Identify: What line/function causes it?
4. Fix: What change resolves it?
5. Verify: How do we know it's fixed?"
```

### For Design
```
"Design this system considering:
1. Requirements: What must it do?
2. Constraints: What limits exist?
3. Trade-offs: What are we optimizing for?
4. Alternatives: What other approaches exist?
5. Decision: Which approach and why?"
```

### For Estimation
```
"Estimate this task by:
1. Breaking into subtasks
2. Identifying unknowns/risks
3. Comparing to similar past work
4. Providing range (best/likely/worst)
5. Listing assumptions"
```

## Anti-Patterns to Avoid

### Vague Instructions
```
BAD: "Make it good"
BAD: "Improve this"
BAD: "Fix the issues"
```

### Missing Context
```
BAD: "Write a sort function" (what language? what data? what constraints?)
BAD: "Debug this" (what's the error? what's expected?)
```

### Overloading
```
BAD: "Write the backend, frontend, tests, docs, and deploy script"
GOOD: Break into separate focused requests
```

### Ambiguous Success Criteria
```
BAD: "Make it faster"
GOOD: "Reduce response time from 2s to under 200ms"
```

## Meta-Prompting Techniques

### Role Assignment
```
"You are a senior security engineer conducting a penetration test.
Review this code for vulnerabilities."
```

### Constraint Setting
```
"Provide a solution that:
- Uses only standard library
- Works in O(n) time
- Handles empty input
- Is thread-safe"
```

### Output Priming
```
"Your response should be:
- Concise (under 200 words)
- Actionable (specific next steps)
- Prioritized (most important first)"
```

### Iterative Refinement
```
"First, give a quick solution.
Then, critique your own solution.
Finally, provide an improved version addressing the critique."
```

### Think Tool Pattern (Mid-Task Reflection)
Stop and reflect during complex multi-step tasks:

**When to use**:
- After processing tool outputs before next action
- In policy-heavy environments requiring compliance checking
- During sequential decision-making where errors compound

**Implementation**:
```
"Before proceeding to the next step:
1. PAUSE and review what we've learned
2. Check: Does this align with our goal?
3. Check: Are there policy/rule violations?
4. Check: Do we have all information needed?
5. Then proceed or request clarification"
```

**Research Result**: 54% relative improvement on complex airline domain tasks (Anthropic research)

## Quality Checklist

Before finalizing any response, verify:

- [ ] **Specific**: No vague language
- [ ] **Structured**: Clear organization
- [ ] **Complete**: All parts addressed
- [ ] **Accurate**: Facts verified
- [ ] **Actionable**: Clear next steps
- [ ] **Appropriate**: Matches request complexity

## Using These Patterns

### In Agent Prompts
Include relevant patterns when spawning agents:
```
"## Quality Requirements
- Use chain-of-thought reasoning
- Include confidence scores
- Provide verification steps"
```

### In Task Instructions
Add structure requirements:
```
"Output your findings in structured format with:
- Summary section
- Detailed findings with severity
- Prioritized recommendations"
```

### For Self-Improvement
When output quality is low:
```
"Let me reconsider this using chain-of-thought:
Step 1: [reasoning]
Step 2: [reasoning]
...
Revised answer: [better output]"
```

---
*These patterns are language and task agnostic - apply them to any request for better results.*
