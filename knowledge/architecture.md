# Software Architecture Best Practices

> **TRIGGER**: Use this documentation when designing systems, making architectural decisions, reviewing code structure, implementing design patterns, or when asked about SOLID, Clean Architecture, or best practices.

## SOLID Principles

### Single Responsibility Principle (SRP)
A class should have only one reason to change.

**Bad:** `ReportGenerator` that generates AND formats reports
**Good:** Separate `ReportGenerator` and `ReportFormatter`

### Open/Closed Principle (OCP)
Open for extension, closed for modification.

**Bad:** Modifying `Shape` class for each new shape type
**Good:** `Shape` interface with new implementations for each type

### Liskov Substitution Principle (LSP)
Subtypes must be substitutable for base types.

**Bad:** `Square` extending `Rectangle` but breaking setWidth/setHeight behavior
**Good:** Rethink hierarchy or use composition

### Interface Segregation Principle (ISP)
Many specific interfaces over one general-purpose interface.

**Bad:** Single `Worker` interface with developer, tester, and manager methods
**Good:** Separate `Developer`, `Tester`, `Manager` interfaces

### Dependency Inversion Principle (DIP)
Depend on abstractions, not concrete implementations.

**Bad:** `NotificationService` directly depends on `EmailSender`
**Good:** Depend on `IMessageSender` interface

## Clean Architecture

Organize code in concentric layers where dependencies only point inward:

```
┌─────────────────────────────────────────┐
│  Frameworks & Drivers (outermost)       │
│  ┌─────────────────────────────────┐    │
│  │  Interface Adapters              │    │
│  │  ┌─────────────────────────┐    │    │
│  │  │  Use Cases               │    │    │
│  │  │  ┌─────────────────┐    │    │    │
│  │  │  │  Entities       │    │    │    │
│  │  │  │  (innermost)    │    │    │    │
│  │  │  └─────────────────┘    │    │    │
│  │  └─────────────────────────┘    │    │
│  └─────────────────────────────────┘    │
└─────────────────────────────────────────┘
```

**The Dependency Rule:** Source code dependencies must only point inward.
- Inner circles contain policies
- Outer circles contain mechanisms
- Business logic never imports from frameworks

## Hexagonal Architecture (Ports & Adapters)

Core application communicates with outside world through:
- **Ports**: Interfaces defining interactions
- **Adapters**: Implementations connecting to actual technologies

**Driving Ports:** External actors interact with your app (REST APIs, CLI)
**Driven Ports:** Your app interacts with external systems (databases, queues)

## Domain-Driven Design (DDD) Concepts

| Concept | Description |
|---------|-------------|
| **Ubiquitous Language** | Shared vocabulary between developers and domain experts |
| **Entities** | Objects with unique identity (Customer, Order) |
| **Value Objects** | Defined by attributes, no identity (Money, Address) |
| **Aggregates** | Cluster of domain objects with consistency boundary |
| **Aggregate Root** | Entry point to aggregate (Order contains OrderItems) |
| **Bounded Context** | Explicit boundary where domain model applies |

## Design Patterns Quick Reference

### Creational
- **Factory Method**: Encapsulates instantiation
- **Builder**: Separates complex construction from representation
- **Singleton**: Single instance (prefer DI over singletons)

### Structural
- **Adapter**: Converts incompatible interfaces
- **Decorator**: Dynamically adds responsibilities
- **Facade**: Simplifies complex subsystems

### Behavioral
- **Strategy**: Encapsulates interchangeable algorithms
- **Observer**: One-to-many notification of state changes
- **Command**: Encapsulates requests as objects (undo/redo)

## Dependency Injection

**Always prefer constructor injection** - makes dependencies explicit and immutable.

```python
# Bad - creates own dependency
class OrderService:
    def __init__(self):
        self.repo = OrderRepository()  # tight coupling

# Good - dependency injected
class OrderService:
    def __init__(self, repo: IOrderRepository):
        self.repo = repo  # loose coupling, testable
```

## Core Principles

### DRY (Don't Repeat Yourself)
Every piece of knowledge has single representation. Focus on **knowledge** duplication, not just textual similarity.

### KISS (Keep It Simple, Stupid)
Simplest solution that works is often best. Avoid premature optimization.

### YAGNI (You Aren't Gonna Need It)
Don't implement functionality before it's needed. Balance with good design that makes adding features easy.

### Coupling & Cohesion
- **Low coupling**: Modules can be understood in isolation
- **High cohesion**: All elements contribute to single purpose

## Code Organization

### File Structure Options

**Layered (by technical role):**
```
src/
  controllers/
  models/
  services/
  repositories/
```

**Feature-based (by domain) - recommended for larger projects:**
```
src/
  users/
  products/
  orders/
  payments/
```

### Module Guidelines
- Single responsibility
- Loose coupling with other modules
- High internal cohesion
- Clear interfaces
- No circular dependencies

## Naming Conventions

| Type | Convention | Examples |
|------|------------|----------|
| Classes | PascalCase, nouns | `UserManager`, `PaymentProcessor` |
| Functions | camelCase, verbs | `calculateTotal`, `validateInput` |
| Booleans | Predicates | `isActive`, `hasPermission`, `canEdit` |
| Collections | Plurals | `users`, `orderItems` |
| Constants | SCREAMING_SNAKE | `MAX_RETRY_COUNT`, `API_TIMEOUT` |

### Best Practices
- Be descriptive (`numberOfActiveUsers` not `count`)
- Reveal intent (`timeoutInMilliseconds` not `timeout`)
- Avoid abbreviations (except `id`, `url`, etc.)
- Consistent vocabulary (pick `fetch`/`retrieve`/`get` and stick with it)
- Add business context (`premiumCustomer` not `type1Customer`)

## Testing Organization

### Testing Trophy (Kent C. Dodds)
- **Static Analysis (10%)**: TypeScript, ESLint
- **Unit Tests (20%)**: Individual functions in isolation
- **Integration Tests (70%)**: Multiple units working together - **main focus**
- **E2E Tests (10%)**: Complete user workflows

### Test Structure
- Mirror source structure in `tests/` directory
- Name: `component.test.js` or `user-flow.integration.test.js`
- Use Arrange-Act-Assert pattern
- Keep tests independent

## Structured Chain-of-Thought (SCoT) for Code Generation

Before implementing, break down using programming structures:

```
SEQUENCE: Operations in execution order
  validate input → check inventory → calculate price → process payment

BRANCH: Decision points and conditions
  if inventory low → backorder
  if payment fails → rollback

LOOP: Iterations needed
  for each order item → calculate line total
```

Then implement following this structure.

## Prompting for Architecture

### Context + Motivation Pattern

```xml
<context>
System: E-commerce platform
Scale: 50K daily users, 20% monthly growth
Stack: Python 3.11, FastAPI, PostgreSQL
</context>

<architectural_principles>
- Clean Architecture with separation of concerns
- Repository pattern for data access
- Dependency injection for testability
- Domain events for cross-service communication
</architectural_principles>

<task>
Design the inventory service following our patterns.
Explain architectural decisions and alignment with principles.
</task>
```

## Code Quality Patterns

### Code Clustering
```
domain/         # Pure functions (no side effects, deterministic)
infrastructure/ # Side effects (database, API, file I/O)
application/    # Orchestration (coordinates pure + side effects)
```

### Intermediate Abstraction Pattern
Protect business logic from third-party libraries:

```python
# Port (abstraction)
class EmailService(ABC):
    @abstractmethod
    def send(self, to: str, subject: str, body: str): pass

# Adapter (implementation)
class SendGridAdapter(EmailService):
    def send(self, to: str, subject: str, body: str):
        # SendGrid-specific implementation
```

## Anti-Patterns to Avoid

| Anti-Pattern | Problem | Solution |
|--------------|---------|----------|
| God Object | Class doing too much (>200 lines) | Split by responsibility |
| Anemic Domain | All logic in services | Put behavior in domain objects |
| Circular Dependencies | A imports B imports A | Dependency injection, events |
| Missing Error Handling | Only happy path | Handle all edge cases |
| Hardcoded Config | Magic strings/numbers | Environment variables, config files |
| Tight Coupling | Direct concrete dependencies | Depend on abstractions |
| Layer Violations | Domain calling infrastructure | Respect dependency direction |

## Quality Metrics

### Cyclomatic Complexity
- 1-10: Simple, low risk
- 11-20: Moderate complexity
- 21-50: Complex, high risk
- >50: Untestable - **refactor immediately**

### Size Guidelines
- Functions: <50 lines (ideal <20)
- Classes: <400 statements
- Files: <500-1000 lines

## Security Architecture

### Principles
- **Defense in depth**: Multiple security layers
- **Least privilege**: Minimum permissions necessary
- **Security by design**: Threat modeling during design
- **Zero trust**: Never trust, always verify

### Checklist
- [ ] Strong authentication (MFA where possible)
- [ ] Authorization checks on every protected resource
- [ ] All inputs validated server-side
- [ ] Output encoding to prevent XSS
- [ ] Sensitive data encrypted at rest and in transit
- [ ] Security events logged
- [ ] No sensitive data in logs or error messages

## API Design

### RESTful Principles
- Use nouns for resources, not verbs
- HTTP methods: GET (read), POST (create), PUT (replace), PATCH (update), DELETE (remove)
- Proper status codes: 200, 201, 400, 401, 404, 500

### API Checklist
- [ ] Consistent naming conventions
- [ ] Versioning included (`/api/v1/`)
- [ ] Pagination, filtering, sorting supported
- [ ] Rate limiting implemented
- [ ] HTTPS only
- [ ] Comprehensive documentation with examples

## Architecture Decision Records (ADRs)

Use for significant decisions. MADR format:

```markdown
# ADR-001: Authentication Approach

## Status
Accepted

## Context
Need to implement user authentication for e-commerce platform.
Team has limited DevOps capacity. 3-month timeline to MVP.

## Options Considered
1. Self-hosted (Keycloak)
2. Managed (Auth0)
3. Custom implementation

## Decision
Auth0 (managed service)

## Consequences
+ Faster implementation
+ Reduced operational burden
- Vendor lock-in
- Monthly cost
```

## Red Flags Requiring Immediate Action

### Code
- Hard-coded credentials
- SQL queries concatenated with user input
- No input validation on external data
- Exceptions caught and ignored
- Functions over 100 lines

### Architecture
- Circular dependencies between modules
- Business logic in presentation layer
- No abstraction for external services
- Single point of failure

### Security
- Authentication bypassable
- Sensitive data logged
- HTTPS not enforced
- Unpatched dependencies with known CVEs

## Validation Rules

- Functions should do one thing
- Keep abstractions at consistent levels
- Dependencies should point inward
- Fail fast on invalid state
- Prefer composition over inheritance
