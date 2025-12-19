# Claude Code Testing Configuration

> **TRIGGER**: Use this documentation when writing tests, implementing TDD, creating test suites, or when the user asks for tests.

## Core Testing Philosophy

- **ALWAYS** write tests that verify BEHAVIOR, not implementation
- Tests must answer: "What should this code do?" NOT "What does this code currently do?"
- When generating tests from existing code, FIRST identify the intended behavior from requirements/comments/function names
- A test that passes when there's a bug is worse than no test at all

**Critical insight**: Tests generated FROM code mirror that code's bugs; tests generated FROM requirements validate correctness.

## Test-Driven Development Workflow

When implementing new features or fixing bugs, follow this sequence:

1. **Write failing tests FIRST** based on requirements or expected behavior
   - Do NOT look at implementation when writing test assertions
   - Be explicit that you're doing TDD to avoid mock implementations

2. **Run tests and confirm they FAIL for the right reason**
   - Do NOT write implementation code at this stage
   - Verify the failure message describes missing/incorrect behavior

3. **Write minimal code to make tests pass**
   - Do NOT modify the tests during this phase

4. **Run tests and confirm they PASS**

5. **Refactor while keeping tests green**

6. **Never mark a task complete until all tests pass**

## Required Test Categories

Every test file MUST include tests for:

1. Happy path with valid inputs (normal operation)
2. Edge cases: empty inputs, null/undefined, boundary values, maximum values
3. Error conditions: invalid inputs, exceptions, permission errors
4. State transitions and side effects
5. Async behavior edge cases if applicable

If you find yourself only writing happy-path tests, STOP and explicitly add edge case tests.

## Test Structure (AAA Pattern - Universal)

Structure EVERY test using Arrange-Act-Assert:

```
// ARRANGE: Set up test data and dependencies
// ACT: Execute the code being tested (ONE action)
// ASSERT: Verify expected outcomes
```

Keep the "Act" section to a single operation. Multiple actions = multiple tests.

## What TO Mock

- External HTTP APIs and third-party services
- Database connections in unit tests
- File system operations
- Time/date functions for determinism
- Environment-specific resources

## What NOT TO Mock

- The code being tested (never mock the subject under test)
- Framework features (don't test that React renders or Angular DI works)
- Your own simple utility functions
- Data models and interfaces

## Test Quality Checklist

Before completing test generation, verify:

- [ ] Tests would FAIL if the core functionality broke
- [ ] Edge cases (null, empty, max values) are covered
- [ ] Error handling paths are tested
- [ ] Test names describe WHAT should happen, not HOW
- [ ] No tests that just assert `toBeTruthy()` or `!= null`
- [ ] Mocks verify they were called with correct arguments
- [ ] Tests are isolated (can run in any order)

## Iterative Test Execution Rules

- **ALWAYS** run tests after writing them - never assume they pass
- Run the specific test file first, then the full suite
- When a test fails:
  1. Read the COMPLETE error message
  2. Identify: Is the test wrong or the implementation?
  3. Make the SMALLEST change to fix
  4. Re-run immediately
- Continue iterating until ALL tests pass with zero errors
- Use `--watch` or equivalent for rapid iteration when available

## Test Commands by Framework

- Python: `pytest -xvs` (stop on first failure, verbose)
- JavaScript/TypeScript: `npm test -- --watch` or `jest --watch`
- .NET: `dotnet test --logger "console;verbosity=detailed"`
- Java: `mvn test -Dtest=ClassName#methodName`
- Go: `go test -v -run TestName`

Prefer running single test files during iteration for speed.

## Naming Conventions

Use descriptive names that document expected behavior:

- `test_withdraw_with_insufficient_funds_raises_exception`
- `should throw error when input is null`
- `CalculateTotal_EmptyCart_ReturnsZero`

Test names should explain what broke when they fail.

## CRITICAL: Preventing Superficial Tests

Do NOT write tests that:

- Just assert that a function returns without error
- Copy implementation logic into expected values
- Test that mocks return what you configured them to return
- Only verify type correctness (unless that's the requirement)
- Assert implementation details like "called private method X"

Instead, write tests that would FAIL if:

- The function returns the wrong value
- An edge case is mishandled
- An error condition isn't caught
- Business logic changes incorrectly

## Azure Testing Configuration

### Emulator Setup

- Use Azurite for Azure Storage: `AzureWebJobsStorage=UseDevelopmentStorage=true`
- Use Cosmos DB Emulator: `AccountEndpoint=https://localhost:8081/;AccountKey=...`
- Use Service Bus Emulator via Docker (requires SQL Edge dependency)
- **NEVER** deploy `UseDevelopmentStorage=true` to production

### Azure SDK Mocking

All Azure SDK clients support mocking via:

- Protected constructors for inheritance
- Virtual methods for overriding
- ModelFactory classes for creating response objects

Example pattern:

```csharp
var mockClient = new Mock<BlobClient>();
mockClient.Setup(x => x.UploadAsync(...)).ReturnsAsync(Mock.Of<Response<BlobContentInfo>>());
```

### Azure Functions Testing

- Use dependency injection (DI) - inject services, don't create them in functions
- Mock `DurableTaskClient` and `TaskOrchestrationContext` for Durable Functions
- Activity functions require no special handling - test like normal methods
- Use `TestStartup.cs` to override services for integration tests

### Connection String Safety

```json
// local.settings.json (NEVER commit with real credentials)
{
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "CosmosDBConnection": "AccountEndpoint=https://localhost:8081/;..."
  }
}
```

### Common Azure Testing Pitfalls to Avoid

- Timer triggers fail without AzureWebJobsStorage configured
- Static methods block testability - always use DI
- Blob lease conflicts under load - use 30-second time-bound leases
- Port 10000-10002 conflicts - check Azurite isn't already running

## Production Safety Rules (CRITICAL)

### Environment Protection

- Tests MUST use dedicated test databases/services, NEVER production
- Add environment validation at test startup:

```python
assert os.getenv('APP_ENV') != 'production', "Cannot run tests in production"
if 'production' in db_url.lower():
    raise EnvironmentError("Test attempting production database access!")
```

- Use `.env.test` files separate from production configuration

### Database Isolation Strategies

Choose ONE pattern per project:

1. **Transaction Rollback**: Wrap each test in transaction, rollback after
2. **Testcontainers**: Spin up fresh database container per test class
3. **Schema Per Test**: Create unique schema, drop after test
4. **Truncate Pattern**: Clear all data between tests

### Network Isolation

- **ALL** external HTTP calls MUST be mocked (WireMock, nock, responses, etc.)
- Tests should never depend on external service availability
- Use contract testing (Pact) for API integration verification

### Secrets Handling

- NEVER use real production credentials in tests
- Test credentials should be obviously fake: `test_api_key_fake_12345`
- Use different secret sources per environment (env vars for test, Vault for prod)
- Implement pre-commit hooks to detect accidentally committed secrets

### Testcontainers Best Practices

- NEVER use fixed ports (causes conflicts in parallel runs)
- NEVER use `:latest` tags (causes flaky tests)
- Always pin specific versions matching production
- Allow Resource Reaper (Ryuk) to auto-cleanup containers

### PROHIBITED in Tests

- Production URLs, connection strings, or credentials
- Real API keys matching production patterns (sk_live_, pk_live_)
- DELETE/TRUNCATE without WHERE clauses
- DROP TABLE/DATABASE statements without environment checks
- Tests that modify shared state between runs

## Test Execution Workflow

### Before Any Code Change

1. Run existing tests to establish baseline
2. Identify which tests cover the code being modified
3. Understand why tests exist before changing them

### After Writing Tests

1. Run the new tests IMMEDIATELY
2. For TDD: Confirm tests FAIL for the right reason
3. Review failure messages - do they describe the missing behavior?

### After Writing Implementation

1. Run affected tests first (faster feedback)
2. Fix any failures before running full suite
3. Run full test suite before considering complete
4. NEVER skip the final full-suite verification

### Failure Iteration Loop

```
┌─ WHEN A TEST FAILS ─────────────────────────────┐
│ 1. READ: Parse complete error message           │
│    - Expected value vs actual value             │
│    - Stack trace location                       │
│ 2. ANALYZE: Root cause                          │
│    - Is the test correct?                       │
│    - Is the implementation wrong?               │
│    - Is setup/mocking incorrect?                │
│ 3. FIX: Smallest targeted change                │
│ 4. RUN: Immediately re-execute test             │
│ 5. REPEAT until all pass                        │
└─────────────────────────────────────────────────┘
```

### Completion Criteria

A task is ONLY complete when:

- [ ] All new tests pass
- [ ] All existing tests pass (no regressions)
- [ ] Tests cover the primary functionality AND edge cases
- [ ] No skipped or pending tests
- [ ] Coverage meets project threshold (if defined)

## Universal Testing Rules

### FIRST Principles (Apply to ALL Frameworks)

- **Fast**: Tests run in milliseconds; suites in seconds
- **Isolated**: Tests don't depend on each other or external state
- **Repeatable**: Same results every time in any environment
- **Self-Validating**: Tests produce pass/fail without manual inspection
- **Thorough**: Cover happy paths, error cases, and boundaries

### Test Organization

- Python: `tests/test_*.py` or `*_test.py`
- JavaScript/TypeScript: `__tests__/*.test.js` or `*.spec.ts`
- .NET: `*.Tests.cs` in separate test project
- Java: `*Test.java` in `src/test/java/`
- Go: `*_test.go` alongside source
- Ruby: `spec/*_spec.rb`

### Framework Detection

Before writing tests, check for existing configuration:

- `pytest.ini`, `pyproject.toml` → pytest
- `jest.config.js`, `package.json:jest` → Jest
- `*.csproj` with test SDKs → xUnit/NUnit
- `pom.xml` or `build.gradle` → JUnit

Use the project's existing framework and conventions.

### Cross-Framework Assertions

| Type | Python | JavaScript | C# | Java |
|------|--------|------------|-----|------|
| Equal | `assert x == y` | `expect(x).toBe(y)` | `Assert.Equal(x, y)` | `assertEquals(x, y)` |
| Throws | `pytest.raises()` | `expect().toThrow()` | `Assert.Throws()` | `assertThrows()` |
| Contains | `assert x in y` | `expect().toContain()` | `Assert.Contains()` | `assertThat().contains()` |

### Determinism Requirements

- Never use `time.sleep()` or `setTimeout()` with real delays
- Mock time/date for tests that depend on current time
- Use fixed seeds for random number generators
- Avoid tests that depend on execution order

## Hooks Configuration

Use `.claude/settings.json` for hooks that automatically run tests after file edits:

```json
{
  "hooks": {
    "PostToolUse": [
      {
        "matcher": { "tool_name": "edit_file" },
        "command": "npm test -- --onlyChanged"
      }
    ]
  }
}
```
