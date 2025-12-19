# Code Refactoring Best Practices

TRIGGER: refactor, code smell, technical debt, clean code, legacy code, extract method, duplication

## Overview

Refactoring is the process of changing code structure without changing behavior. The goal is to make code easier to understand, maintain, and extend while preserving its functionality.

## Why Refactor?

- **Reduce technical debt** - Developers spend 13.5 hours/week on technical debt (Stack Overflow 2023)
- **Enable feature development** - Clean code is easier to extend
- **Improve understanding** - Refactored code documents intent
- **Reduce bugs** - Simpler code has fewer hiding places for bugs
- **Faster development** - Elite teams deploy 208x more frequently

---

## When to Refactor

### Trigger Thresholds
| Metric | Threshold | Action |
|--------|-----------|--------|
| Method length | >40 lines | Extract Method |
| Cyclomatic complexity | >10 | Simplify conditionals |
| Parameter count | >4 | Introduce Parameter Object |
| Class size | >300 lines | Extract Class |
| Duplication | >3 occurrences | Extract common code |

### Refactoring Opportunities
- **Before adding features** - Clean foundation first
- **When fixing bugs** - "Boy Scout Rule" - leave code cleaner
- **During code review** - Spot improvement opportunities
- **When understanding code** - Refactor to clarify
- **Scheduled tech debt sprints** - Dedicated improvement time

### When NOT to Refactor
- No test coverage (write tests first)
- Close to deadline (risky timing)
- Code being replaced soon
- Working code that won't change
- During active feature development in same code

---

## The Refactoring Process

### Red-Green-Refactor (TDD)
1. **Red**: Write a failing test
2. **Green**: Write minimal code to pass
3. **Refactor**: Improve structure, keep tests green

### Safe Refactoring Steps
1. **Ensure test coverage** - Can't refactor safely without tests
2. **Commit current state** - Rollback point
3. **Make one change** - Single refactoring at a time
4. **Run tests** - Verify behavior preserved
5. **Commit** - Save progress
6. **Repeat** - Next refactoring

### Strangler Fig Pattern (Legacy Systems)
For large-scale modernization:
1. Identify a small piece to modernize
2. Build new implementation alongside old
3. Redirect traffic to new code
4. Remove old code when no longer used
5. Repeat for next piece

---

## Code Smells Catalog

### Bloaters
Code that has grown too large.

#### Long Method
**Signs**: Method >20-30 lines, needs comments to explain sections
**Fix**: Extract Method - pull sections into named methods
```python
# Before: Long method with comments
def process_order(order):
    # Validate
    if not order.items: raise Error()
    if not order.customer: raise Error()
    # Calculate
    total = 0
    for item in order.items:
        total += item.price * item.quantity
    # Discount
    if order.customer.premium:
        total *= 0.9
    return total

# After: Small methods with descriptive names
def process_order(order):
    validate(order)
    total = calculate_total(order.items)
    return apply_discount(total, order.customer)
```

#### Large Class
**Signs**: Class >300 lines, multiple responsibilities
**Fix**: Extract Class - split into focused classes

#### Long Parameter List
**Signs**: >3-4 parameters, related parameters travel together
**Fix**: Introduce Parameter Object
```python
# Before
def create_user(name, email, street, city, state, zip_code):
    pass

# After
@dataclass
class Address:
    street: str
    city: str
    state: str
    zip_code: str

def create_user(name, email, address: Address):
    pass
```

#### Primitive Obsession
**Signs**: Using primitives for domain concepts (string for phone number)
**Fix**: Replace Primitive with Object
```python
# Before
phone = "555-123-4567"

# After
class PhoneNumber:
    def __init__(self, number):
        self.validate(number)
        self.number = number
```

### Dispensables
Code that could be removed.

#### Duplicate Code
**Signs**: Same code in multiple places
**Fix**: Extract Method or Extract Class
```python
# Before: Duplicated validation
def process_a():
    if not data: raise Error()
    if len(data) > 100: raise Error()
    # ... process A

def process_b():
    if not data: raise Error()
    if len(data) > 100: raise Error()
    # ... process B

# After: Extracted validation
def validate_data(data):
    if not data: raise Error()
    if len(data) > 100: raise Error()

def process_a():
    validate_data(data)
    # ... process A
```

#### Dead Code
**Signs**: Unreachable code, unused variables/methods
**Fix**: Delete it - version control has history

#### Comments Explaining What
**Signs**: Comments describing what code does (not why)
**Fix**: Refactor code to be self-documenting
```python
# Before: Comment explains what
# Loop through users and check if active
for user in users:
    if user.status == 'active':
        active_users.append(user)

# After: Code explains itself
active_users = [u for u in users if u.is_active()]
```

### Couplers
Excessive dependencies between classes.

#### Feature Envy
**Signs**: Method uses another class's data more than its own
**Fix**: Move Method to the class it envies
```python
# Before: Phone logic in Person
class Person:
    def format_phone(self):
        return f"({self.phone.area}) {self.phone.number}"

# After: Phone logic in Phone
class Phone:
    def format(self):
        return f"({self.area}) {self.number}"
```

#### Message Chains
**Signs**: a.getB().getC().getD()
**Fix**: Hide Delegate - add method to intermediate class

---

## Refactoring Techniques

### Extract Method
**When**: Code fragment that can be grouped together
**How**:
1. Create new method with descriptive name
2. Copy extracted code to new method
3. Replace original code with method call
4. Pass needed variables as parameters
5. Return results if needed

### Extract Class
**When**: Class doing too many things
**How**:
1. Identify subset of fields/methods that belong together
2. Create new class for that subset
3. Move fields and methods
4. Update original class to delegate to new class

### Inline Method
**When**: Method body is as clear as its name
**How**:
1. Ensure method isn't overridden
2. Replace all calls with method body
3. Delete the method

### Replace Temp with Query
**When**: Temp variable holds result of expression
**How**:
1. Extract expression into method
2. Replace temp references with method calls
3. Remove temp declaration

### Introduce Parameter Object
**When**: Group of parameters that travel together
**How**:
1. Create class for the parameter group
2. Update callers to pass object instead
3. Move behavior that operates on parameters to the object

### Replace Conditional with Polymorphism
**When**: Conditional logic based on type
**How**:
1. Create class hierarchy
2. Move each conditional branch to subclass method
3. Replace conditional with polymorphic call

---

## Measuring Refactoring Success

### Metrics to Track
| Metric | Tool | Target |
|--------|------|--------|
| Cyclomatic Complexity | SonarQube | <10 per method |
| Code Duplication | SonarQube | <3% |
| Method Length | Linter | <40 lines |
| Test Coverage | Coverage tools | >80% |
| Coupling | SonarQube | Low between modules |

### Qualitative Signs
- Easier to understand when reading
- Faster to make changes
- Fewer bugs in modified areas
- Tests are clearer
- Less "fear" when touching code

---

## Technical Debt Prioritization

### Debt Quadrant (Martin Fowler)
| | Reckless | Prudent |
|---|----------|---------|
| **Deliberate** | "We don't have time for design" | "We must ship now and deal with consequences" |
| **Inadvertent** | "What's layering?" | "Now we know how we should have done it" |

### Prioritization Matrix
| Priority | Criteria | Action |
|----------|----------|--------|
| P0 | Blocks feature work | Refactor immediately |
| P1 | Causes bugs regularly | Schedule soon |
| P2 | Slows development | Plan for tech debt sprint |
| P3 | Code smell, no impact | Address opportunistically |

---

## Tools

### Static Analysis
- **SonarQube** - Comprehensive code quality
- **ESLint/Pylint** - Language-specific linting
- **CodeClimate** - Automated code review
- **Semgrep** - Pattern-based analysis

### IDE Refactoring Support
Modern IDEs (VS Code, IntelliJ, PyCharm) provide:
- Extract Method/Variable/Class
- Rename with references
- Move/Copy
- Inline
- Change Signature

### AI-Assisted Refactoring
- AI tools can identify patterns and suggest refactorings
- 20-30% faster refactoring with AI assistance (McKinsey)
- Still requires human judgment on what to refactor

---

## Common Mistakes

### Refactoring Anti-Patterns
1. **Big Bang Rewrite** - Replacing everything at once
2. **Refactoring Without Tests** - No safety net
3. **Gold Plating** - Perfecting working code unnecessarily
4. **Mixing with Feature Work** - Confuses code review
5. **Not Committing Often** - Loses rollback points
6. **Ignoring Test Code** - Tests need refactoring too
7. **Premature Refactoring** - Optimizing code you don't understand

### How to Avoid
- Write tests first
- Commit after each successful refactoring
- Separate refactoring commits from feature commits
- Refactor test code with production code
- Understand before improving

---

## References

- [Refactoring: Improving the Design of Existing Code](https://martinfowler.com/books/refactoring.html) - Martin Fowler
- [Refactoring Guru](https://refactoring.guru/refactoring) - Online catalog
- [Working Effectively with Legacy Code](https://www.oreilly.com/library/view/working-effectively-with/0131177052/) - Michael Feathers
- [Industrial Logic Smells to Refactorings Cheatsheet](https://www.industriallogic.com/blog/smells-to-refactorings-cheatsheet/)
