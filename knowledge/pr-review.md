# PR Review Guidelines

> **TRIGGER**: Use this documentation when reviewing pull requests, code reviews, providing feedback on code changes, or when asked about PR best practices.

## Core Principles

Code review serves three primary purposes:
- **Knowledge transfer and learning** (top benefit across Google, Microsoft, Meta research)
- **Maintaining code health** and spreading expertise
- **Creating historical records** of technical decisions

**The single most important principle**: Approve code that definitely improves overall code health, even if it isn't perfect. Continuous improvement matters more than perfection.

### Review Constraints
- Effectiveness drops dramatically beyond **400 lines of code**
- Reviews should take **60-90 minutes maximum**
- One thorough reviewer is typically sufficient
- Optimal inspection rate: **300-500 LOC/hour**

## Technical Review Criteria

### Functionality and Correctness
- Verify code actually solves the stated problem
- Check for logic errors: off-by-one, incorrect conditionals, inverted boolean logic
- Examine edge cases: empty inputs, null values, boundary conditions, min/max values
- Verify error handling with meaningful error messages
- Confirm proper null pointer handling in chained method calls
- Review test coverage for edge cases, error paths, boundary conditions

### Code Quality and Maintainability
- Functions should be small and focused
- Flag code with >3-4 levels of nesting
- Flag excessive branching (if/else chains, nested loops, large switch statements)
- Check for over-engineering (solve current problems, not speculative future ones)
- Variable, function, class names must be clear and descriptive
- Comments explain WHY, not WHAT
- Remove commented-out code (version control serves that purpose)
- Replace magic numbers with named constants
- Look for DRY violations to extract into reusable functions

### Architecture and Design (SOLID Principles)

| Principle | Red Flags |
|-----------|-----------|
| **SRP** | Classes mixing business logic with persistence, presentation, or logging |
| **OCP** | Long if/else chains checking types, frequent instanceof usage |
| **LSP** | Derived classes requiring explicit casting or causing unexpected exceptions |
| **ISP** | "Fat" interfaces where clients implement unused functionality |
| **DIP** | Direct dependencies on concrete classes, liberal use of `new` keyword |

Also check:
- Appropriate coupling between modules
- Abstractions at right level (neither too specific nor too generic)
- Alignment with existing system architecture
- Avoid speculative generality

### Security Vulnerabilities (Always Blocking)

**Input Validation (OWASP Top 10)**:
- SQL injection: Use parameterized queries, not string concatenation
- Command injection: Handle safely
- Path traversal: Validate and sanitize file paths
- All user inputs: Validate, sanitize, escape appropriately for context

**Authentication & Authorization**:
- Robust, secure authentication mechanisms
- Proper session handling with appropriate timeouts
- Password storage: bcrypt or Argon2 (never plaintext)
- Authorization checks at all entry points, not just UI
- CSRF protection with anti-CSRF tokens

**Data Exposure**:
- No passwords, connection strings, API tokens, credentials in code or logs
- Error messages must not expose system details, stack traces, or sensitive info
- Sensitive data encrypted at rest and in transit

**XSS Prevention**:
- All user-generated content properly encoded for context
- Avoid unsafe innerHTML usage
- URL validation prevents malicious redirects

**Configuration**:
- No default credentials
- Debug mode disabled in production
- Proper CORS configuration

### Performance Considerations

**Algorithmic Complexity**:
- Flag O(n²) or worse without clear justification
- Check space complexity for large datasets
- Nested loops require careful analysis
- Look for better data structures (HashMap for lookups vs linear searches)

**Database Queries**:
- N+1 query problem (queries inside loops) - severely impacts performance
- Request query plans for complex queries
- Verify appropriate indexes for WHERE and JOIN clauses
- Large result sets: use pagination or streaming
- Database migrations: reversible, performant at scale, properly tested

**Resource Management**:
- File handles, database connections, streams properly cleaned up
- Verify optimizations are actually needed and measured
- Caching strategies need proper invalidation with appropriate TTL

### API Design and Compatibility

**Breaking Changes** (require major version bumps):
- Removing fields or endpoints
- Changing field types
- Renaming (equivalent to remove and add)
- Making optional parameters required
- Changing HTTP response code meanings
- Altering response structures (objects to arrays or vice versa)

**Safe Changes**:
- Adding optional fields
- New endpoints
- Optional parameters with sensible defaults
- Deprecation (maintains function while signaling future removal)

**Checklist**:
- Clear contracts with well-defined inputs, outputs, behavior
- REST conventions or GraphQL best practices
- Correct HTTP verbs (GET read, POST create, PUT replace, PATCH update, DELETE remove)
- Appropriate HTTP status codes with meaningful messages
- Pagination for large result sets
- Rate limiting for public/resource-intensive endpoints

## Communication and Feedback

### Fundamental Principles

1. **Comment on the code, never on the developer** (Google's golden rule)
   - "This code does not close the socket connection" ✓
   - "You did not close the socket connection" ✗

2. **Use questions instead of commands**
   - "What do you think about calling this variable userId?" ✓
   - "This variable should be called userId" ✗

3. **Frame as personal observations (I-messages)**
   - "It's hard for me to understand this code" ✓
   - "This code is hard to understand" ✗

4. **Avoid condescending words**: Remove "just," "easy," "only," "obvious," "simply"

5. **Never use sarcasm** - people cannot reliably detect sarcasm in text

### Actionable Feedback

**Vague (bad)**:
- "This doesn't look right"
- "Improve this"
- "Fix this"
- "This could be better"

**Actionable (good)**:
"The calculateTotal function lacks error handling for invalid input. Please add checks for negative numbers to prevent incorrect calculations and potential system errors."

Three elements of effective feedback:
1. **Specificity** about what needs attention
2. **Reasoning** explaining why it matters
3. **Guidance** on how to improve

### Conventional Comments Labels

Format: `<label> [decorations]: <subject>`

**Critical/Blocking**:
- `blocker:` Critical issues preventing merge
- `issue:` Problems needing fixes, usually blocking

**Suggestions/Improvements**:
- `suggestion (non-blocking):` Proposes improvements
- `question:` Asks for clarification
- `thought:` Ideas for future consideration

**Minor/Informational**:
- `nit:` Trivial, preference-based (always non-blocking)
- `polish:` Quality improvements, cosmetic
- `convention:` Deviations from organizational standards
- `typo:` Spelling/grammar corrections
- `praise:` Highlights something genuinely positive
- `FYI:` Informational, no action required

**Decorations**:
- `(blocking)` - Must be resolved before merge
- `(non-blocking)` - Shouldn't prevent acceptance
- `(security)` - Security-related concern
- `(performance)` - Performance concern
- `(if-minor)` - Resolve only if changes are minor

### Language Patterns

**Use**:
- "What do you think about...?"
- "Could we consider...?"
- "Would it work if...?"
- "Have you considered...?"
- "How would this handle...?"
- "Can you help me understand...?"
- "I suggest [X] because..."
- "This could be improved by..."
- "It's hard for me to follow..."
- "I'm concerned about..."

**Avoid**:
- "You should..." / "You must..." / "You need to..."
- "Just do X" / "Simply..." / "Obviously..." / "Clearly..."
- "This is wrong" / "This doesn't work"
- "Why didn't you just...?"
- "You wrote bad code" / "You don't understand..."
- "You always..." / "You never..."

### Handling Disagreements

1. Ensure mutual understanding before defending position
2. Never respond in anger
3. Structure disagreement with trade-offs:
   > "I went with approach X because of [pros/cons]. My understanding is that Y would be worse because of [reasons]. Are you suggesting that Y better serves our goals?"
4. Focus on technical facts and data, not opinions
5. Defer to automated linters for style preferences
6. Escalation path: technical discussion → team standards/ADRs → synchronous discussion → tech leads → engineering management

### Balancing Criticism with Recognition

Give praise when:
- Something genuinely impresses beyond "good enough"
- Exemplary practices (excellent test coverage, clean refactoring)
- You learn something from the change
- First PRs from new team members
- Recognizing significant effort

Make praise specific:
> "praise: Excellent caching mechanism here—this will significantly improve our page load times by reducing database calls."

Avoid:
- False praise (damages trust)
- Praising basic requirements
- Using praise to soften every criticism

## Review Process

### Systematic Approach (C.L.E.A.R. Framework)

1. **Context**: Review requirements, understand integration points, identify constraints
2. **Layered examination**: Structure → architecture → logic → security → performance → style
3. **Explicit verification**: Mentally execute with sample data, test boundary conditions
4. **Alternative consideration**: Evaluate patterns, analyze trade-offs
5. **Refactoring recommendations**: Prioritize high/medium/low, provide actionable feedback

### Two-Pass Review
- **First pass**: Architecture, overall approach, design patterns, major logic flows
- **Second pass**: Detailed line-by-line, edge cases, error handling

### Timing and Response
- Respond within **one business day maximum** (Google's 24-hour rule)
- Keep sessions to **60-90 minutes maximum**
- Review at **300-500 LOC/hour** (not faster)
- Take breaks between reviews

### Managing Scope and Size
- Request PR splitting when changes exceed **400 lines**
- Ideal PR size: **200-400 lines**
- Splitting strategies:
  - By system boundaries (frontend/backend)
  - Separate concerns (refactoring first, new functionality second)
  - Stacked pull requests
  - Feature flags
  - Temporal separation

### Automation Integration

**Automate**:
- Code formatting and style
- Syntax errors and build success
- Test coverage thresholds
- Security vulnerabilities in dependencies
- License compliance
- File/PR size limits
- Duplicate code detection

**Reserve human review for**:
- Architecture decisions and design choices
- Business logic correctness
- Code readability and clarity
- Naming quality
- Performance optimization strategies
- Edge case identification
- Technical debt trade-offs
- Security threat modeling

## When to Approve vs Request Changes

### Approve When:
- Code definitely improves overall code health
- Functionality is correct and edge cases handled
- Security issues properly addressed
- Tests provide adequate coverage
- Remaining issues are truly minor
- Author has solid justification even if you'd do it differently

### Request Changes When:
- Security vulnerabilities exist
- Functionality is incorrect or missing critical edge cases
- Approach has fundamental flaws
- Tests are missing or inadequate
- Performance issues will impact users
- Breaking API changes aren't properly managed
- Significant technical debt without acknowledgment

### Needs Discussion When:
- Significant concerns but uncertain about best path
- Multiple valid approaches exist
- Architectural decisions require broader discussion
- Scope has grown beyond single PR

## Review Depth by Context

| Change Type | Focus |
|-------------|-------|
| **Small bug fixes (<50 lines)** | Correctness, edge cases, regression test |
| **New features (50-400 lines)** | Full review: architecture, security, performance, testing |
| **Refactoring** | Maintains behavior, improves quality, adequate tests |
| **Large architectural (>400 lines)** | Request split, or: walkthrough, multiple sessions, multiple reviewers |
| **Critical/security** | Maximum scrutiny, domain expert review, penetration testing |

## Anti-Patterns to Avoid

### Process Anti-Patterns
- **Marathon reviews**: Effectiveness drops after 60-90 minutes
- **Ping-pong reviews**: Provide all feedback in one pass
- **Rubber stamping**: Quick LGTM without thorough examination
- **Moving goalposts**: Suddenly objecting to previously accepted patterns

### Communication Anti-Patterns
- Harsh, hostile, or dismissive tone
- Vague feedback without actionable direction
- Excessive nitpicking on trivial style issues
- Over-helping/scope creep demanding unrelated refactoring

### Technical Anti-Patterns
- Ignoring tests (test quality matters equally)
- Reviewing exclusively bottom-up (understand forest before trees)
- Ignoring context (read PR description, linked issues first)

## Quick Reference Checklist

### Before Starting Review
- [ ] Read PR description thoroughly
- [ ] Check linked issues and design documents
- [ ] Understand problem being solved and constraints

### During Review
- [ ] First pass: Architecture and overall approach
- [ ] Second pass: Detailed line-by-line review
- [ ] Check security implications
- [ ] Verify test coverage
- [ ] Use conventional comment labels

### Feedback Quality
- [ ] Every comment is specific and actionable
- [ ] Reasoning explained for each suggestion
- [ ] Blocking vs non-blocking clearly indicated
- [ ] Praise for genuinely impressive work
- [ ] Professional, collaborative tone throughout
