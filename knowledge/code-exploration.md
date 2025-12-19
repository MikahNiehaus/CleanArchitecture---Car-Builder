# Code Exploration Knowledge Base

TRIGGER: explore, codebase, understand, find, where, how does, architecture, structure, dependencies, navigate, search code

## Overview

Systematic code exploration produces reliable understanding faster than ad-hoc browsing. This knowledge base provides techniques for efficiently navigating codebases of any size.

## Core Principles

1. **Read-only first**: Understand before modifying
2. **Structured exploration**: Use systematic patterns, not random browsing
3. **Pattern recognition**: Look for recurring structures across files
4. **Dependency awareness**: Understand relationships, not just individual files
5. **Incremental depth**: Start broad, drill down as needed
6. **Context preservation**: Always note file paths and line numbers

## Exploration Workflows

### Quick Codebase Overview (< 5 minutes)

```
1. Check README and docs/
2. List top-level directories
3. Find entry points (main, index, app, server)
4. Identify configuration (package.json, pyproject.toml, Cargo.toml)
5. Count and categorize source files
```

**Output**: High-level structure map, tech stack, entry points

### Feature Understanding (5-15 minutes)

```
1. Identify feature entry point (API route, UI component, command)
2. Trace imports and dependencies
3. Map call chain through execution path
4. Find related tests
5. Check for configuration/feature flags
```

**Output**: Execution flow diagram, key files list, test locations

### Dependency Analysis (10-20 minutes)

```
1. Start from target file/module
2. Map all direct imports
3. Recursively map transitive dependencies
4. Identify circular dependencies
5. Find common utilities/shared code
6. Calculate dependency depth
```

**Output**: Dependency graph, coupling analysis, shared code identification

### Architecture Discovery (20-30 minutes)

```
1. Identify layer boundaries (UI, API, business logic, data)
2. Map module/package structure
3. Find abstraction patterns (interfaces, base classes)
4. Trace cross-cutting concerns (logging, auth, errors)
5. Identify external integrations
6. Document architectural patterns
```

**Output**: Layer diagram, pattern catalog, integration points

## File Discovery Techniques

### Glob Pattern Reference

| Pattern | Finds | Use For |
|---------|-------|---------|
| `**/*.ts` | All TypeScript files | Language-specific search |
| `src/**/*` | All source files | Source vs. config separation |
| `**/test*/**` | Test directories | Finding test coverage |
| `**/*Controller*` | Controller files | MVC pattern discovery |
| `**/*.{ts,tsx}` | TS and TSX files | React project exploration |
| `!**/node_modules/**` | Exclude deps | Avoiding package bloat |

### Common Entry Point Patterns

```
# JavaScript/TypeScript
**/index.{js,ts}
**/main.{js,ts}
**/app.{js,ts}
**/server.{js,ts}

# Python
**/__main__.py
**/main.py
**/app.py
**/wsgi.py

# Go
**/main.go
**/cmd/**

# Rust
**/main.rs
**/lib.rs
```

## Content Search Techniques

### Grep Pattern Reference

| Goal | Pattern | Notes |
|------|---------|-------|
| Class definition | `class\s+ClassName` | Works for most languages |
| Function definition | `function\s+name` or `def\s+name` | JS vs Python |
| Import statements | `import.*ModuleName` | Find usage |
| TODO/FIXME comments | `TODO\|FIXME\|HACK` | Technical debt |
| API endpoints | `@(Get\|Post\|Put)` or `router\.(get\|post)` | Route discovery |
| Error handling | `catch\|except\|rescue` | Exception patterns |
| Configuration | `process\.env\|os\.environ` | Environment usage |

### Regex Tips for Code Search

```
# Match function calls (with arguments)
functionName\([^)]*\)

# Match method chains
\.methodName\(

# Match class inheritance
class\s+\w+\s+extends\s+(\w+)

# Match interface implementation
implements\s+(\w+)

# Match decorators/annotations
@\w+

# Match generic types
<\w+>
```

## Dependency Mapping

### Internal Dependency Analysis

```
For each file in scope:
  1. Extract import statements
  2. Resolve relative paths to absolute
  3. Group by module/package
  4. Build adjacency list
  5. Calculate incoming/outgoing connections
  6. Identify circular dependencies
```

### Dependency Metrics

| Metric | What It Measures | High Value Means |
|--------|------------------|------------------|
| Afferent coupling (Ca) | Incoming dependencies | Many dependents (stable) |
| Efferent coupling (Ce) | Outgoing dependencies | Many dependencies (unstable) |
| Instability (Ce/(Ca+Ce)) | Change likelihood | 0=stable, 1=unstable |
| Abstractness | Abstract vs concrete | High=abstract, Low=concrete |

### Circular Dependency Detection

```
1. Build directed graph of imports
2. Run DFS from each node
3. Track visited nodes in current path
4. Cycle found when visiting already-in-path node
5. Report cycle with full path
```

## Call Graph Construction

### Static Analysis Approach

```
1. Parse source files into AST
2. Identify function/method definitions
3. Walk AST for call expressions
4. Resolve callee to definition
5. Build caller → callee edges
6. Handle dynamic dispatch conservatively
```

### Practical Call Tracing

When full AST parsing isn't available:

```
1. Grep for function definition
2. Grep for function name followed by (
3. Filter to actual calls (not definitions)
4. Note file and line for each call
5. Repeat recursively for callees
```

## Pattern Recognition

### Common Code Patterns to Find

| Pattern | Search Strategy | Example |
|---------|-----------------|---------|
| Factory | `Factory`, `create`, `make` | `UserFactory.create()` |
| Singleton | `instance`, `getInstance` | `Logger.getInstance()` |
| Observer | `subscribe`, `on`, `emit` | `eventEmitter.on('event')` |
| Repository | `Repository`, `find`, `save` | `userRepository.find()` |
| Service | `Service`, suffixed classes | `AuthService.validate()` |
| Controller | `Controller`, route handlers | `UserController.list()` |
| Middleware | `use`, `next` | `app.use(authMiddleware)` |

### Layer Identification

```
Presentation Layer:
  - Components, Views, Templates
  - Route handlers, Controllers
  - Search: **/components/**, **/views/**, **/*Controller*

Application Layer:
  - Services, Use Cases
  - Orchestration logic
  - Search: **/services/**, **/usecases/**

Domain Layer:
  - Entities, Value Objects
  - Business rules
  - Search: **/domain/**, **/entities/**, **/models/**

Infrastructure Layer:
  - Repositories, Adapters
  - External integrations
  - Search: **/infrastructure/**, **/adapters/**, **/repositories/**
```

## Large Codebase Strategies

### Handling 100K+ Line Codebases

1. **Never read all files**: Use targeted search
2. **Start at boundaries**: Entry points, APIs, tests
3. **Follow data flow**: Input → processing → output
4. **Trust package boundaries**: Don't drill into every module
5. **Sample patterns**: Find 2-3 examples, assume pattern holds

### Chunking Strategies

```
1. Divide by module/package first
2. Explore each module independently
3. Map inter-module dependencies
4. Focus on integration points
5. Summarize, don't enumerate
```

### Information Overload Prevention

- Set explicit scope boundaries before starting
- Time-box exploration phases
- Write findings as you go (don't hold in memory)
- Stop when you have enough to answer the question
- Accept "good enough" understanding for initial exploration

## Output Standards

### Always Include

1. **File paths**: Absolute or repo-relative paths
2. **Line numbers**: Specific line references
3. **Code context**: Relevant snippets (not full files)
4. **Relationships**: How files/functions connect
5. **Next steps**: What to explore next

### Formatting for Clarity

```markdown
## Finding: [Title]

**Location**: `src/services/auth.ts:42`

**Code**:
\`\`\`typescript
function validateToken(token: string): boolean {
  // Token validation logic
}
\`\`\`

**Called by**:
- `src/middleware/auth.ts:15` - Auth middleware
- `src/routes/api.ts:88` - Protected routes

**Calls**:
- `src/utils/jwt.ts:decode()` - Token parsing
```

## Common Mistakes to Avoid

### Exploration Anti-Patterns

1. **Reading files linearly**: Code isn't a novel; jump around
2. **Ignoring tests**: Tests show intended usage
3. **Missing configuration**: Config often explains behavior
4. **Skipping README**: Often contains crucial context
5. **Over-exploring**: Stop when you have enough

### Search Anti-Patterns

1. **Too broad**: `*.*` returns too much noise
2. **Too literal**: Missing variations in naming
3. **Wrong language**: Searching Python patterns in JS
4. **Ignoring case**: `User` vs `user` both matter

## Integration with Other Agents

### Handoff to Debug Agent
```
"Exploration found the bug likely in these files:
- src/auth/validate.ts:42 - Token parsing
- src/middleware/auth.ts:15 - Middleware invocation
Call chain: request → middleware → validate → [error]"
```

### Handoff to Architect Agent
```
"Codebase uses layered architecture:
- Presentation: React components in /src/components
- Application: Services in /src/services
- Domain: Models in /src/domain
- Infrastructure: Repositories in /src/data

Current coupling issues found in /src/services/user.ts
which directly imports repository implementation."
```

### Handoff to Test Agent
```
"Test coverage analysis:
- Unit tests in __tests__/ directories (co-located)
- Integration tests in /tests/integration/
- Missing coverage: /src/services/payment.ts has no tests
- Test patterns: Jest, describe/it structure, mock factories"
```

## Quick Reference Card

### Start Exploration
```bash
# Structure overview
Glob: ** (top level)
Read: README.md, package.json

# Entry points
Glob: **/main.* **/index.* **/app.*
Grep: "createServer" or "listen("

# Key abstractions
Grep: "class\s+\w+" or "interface\s+\w+"
```

### Find Specific Code
```bash
# Definition
Grep: "function targetName" or "def targetName"
Grep: "class TargetName"

# Usage
Grep: "import.*TargetName"
Grep: "TargetName\("
```

### Trace Dependencies
```bash
# Imports in file
Read: target file, extract import statements

# Who imports target
Grep: "from ['\"]\./path/to/target"
Grep: "import.*from.*target"
```

### Architecture Mapping
```bash
# Layers
Glob: **/components/** **/services/** **/domain/**

# External deps
Read: package.json dependencies
Grep: "require\|import.*from ['\"]\w"

# Integration points
Grep: "fetch\|axios\|http\|database"
```
