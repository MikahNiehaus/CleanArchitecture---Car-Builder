# Error Handling Knowledge Base

TRIGGER: error, exception, handling, recovery, retry, fault tolerance, failure, catch, throw

## Overview

Good error handling makes systems reliable, debuggable, and maintainable. This knowledge base covers error design patterns, recovery strategies, and best practices across languages.

## Error Design Principles

### 1. Fail Fast
Detect and report errors as early as possible:
```python
# Good: Validate input immediately
def process_order(order):
    if not order.items:
        raise ValueError("Order must have at least one item")
    if order.total <= 0:
        raise ValueError("Order total must be positive")
    # ... process valid order

# Bad: Deep in processing
def process_order(order):
    # ... lots of processing
    for item in order.items:  # Fails here if items is None
        # ...
```

### 2. Be Specific
Use specific error types, not generic ones:
```python
# Good: Specific exceptions
class OrderNotFoundError(Exception): pass
class InsufficientInventoryError(Exception): pass
class PaymentDeclinedError(Exception): pass

# Bad: Generic exceptions
raise Exception("Something went wrong")
raise RuntimeError("Error")
```

### 3. Include Context
Errors should contain debugging information:
```python
# Good: Rich context
raise OrderProcessingError(
    f"Failed to process order {order_id}",
    order_id=order_id,
    user_id=user_id,
    items=order.items,
    original_error=e
)

# Bad: No context
raise Exception("Order failed")
```

### 4. Don't Swallow Errors
Never hide errors silently:
```python
# Bad: Silent failure
try:
    process_payment(order)
except Exception:
    pass  # Silently continues

# Good: Handle or propagate
try:
    process_payment(order)
except PaymentError as e:
    logger.error("Payment failed", extra={"order_id": order.id, "error": str(e)})
    raise
```

## Error Hierarchy Design

### Recommended Structure
```
ApplicationError (base)
├── ValidationError
│   ├── InvalidInputError
│   ├── MissingFieldError
│   └── InvalidFormatError
├── BusinessError
│   ├── InsufficientFundsError
│   ├── ItemNotAvailableError
│   └── PermissionDeniedError
├── IntegrationError
│   ├── DatabaseError
│   ├── ExternalServiceError
│   └── TimeoutError
└── SystemError
    ├── ConfigurationError
    └── ResourceExhaustedError
```

### Implementation Example
```python
class ApplicationError(Exception):
    """Base error for all application errors."""
    def __init__(self, message, code=None, details=None):
        super().__init__(message)
        self.code = code
        self.details = details or {}

class ValidationError(ApplicationError):
    """Input validation failed."""
    pass

class BusinessError(ApplicationError):
    """Business rule violation."""
    pass

class IntegrationError(ApplicationError):
    """External system interaction failed."""
    def __init__(self, message, service=None, **kwargs):
        super().__init__(message, **kwargs)
        self.service = service
```

## Exception vs Result Types

### When to Use Exceptions
- Truly exceptional conditions (shouldn't happen in normal flow)
- Errors that can't be handled locally
- Crossing module boundaries
- I/O errors, system failures

### When to Use Result Types
- Expected failure cases (validation, not found)
- Errors that caller should handle
- Functional programming style
- When you want to force error handling

### Result Type Pattern
```python
from dataclasses import dataclass
from typing import TypeVar, Generic, Union

T = TypeVar('T')
E = TypeVar('E')

@dataclass
class Ok(Generic[T]):
    value: T

@dataclass
class Err(Generic[E]):
    error: E

Result = Union[Ok[T], Err[E]]

# Usage
def divide(a: float, b: float) -> Result[float, str]:
    if b == 0:
        return Err("Division by zero")
    return Ok(a / b)

result = divide(10, 0)
match result:
    case Ok(value):
        print(f"Result: {value}")
    case Err(error):
        print(f"Error: {error}")
```

## Error Recovery Patterns

### Retry with Backoff
```python
import time
import random

def retry_with_backoff(func, max_retries=3, base_delay=1.0, max_delay=60.0):
    """Retry with exponential backoff and jitter."""
    for attempt in range(max_retries):
        try:
            return func()
        except (ConnectionError, TimeoutError) as e:
            if attempt == max_retries - 1:
                raise

            delay = min(base_delay * (2 ** attempt), max_delay)
            jitter = random.uniform(0, delay * 0.1)
            time.sleep(delay + jitter)

            logger.warning(f"Retry {attempt + 1}/{max_retries} after {delay:.1f}s")
```

### Circuit Breaker
```python
import time
from enum import Enum

class CircuitState(Enum):
    CLOSED = "closed"      # Normal operation
    OPEN = "open"          # Failing, reject calls
    HALF_OPEN = "half_open"  # Testing if recovered

class CircuitBreaker:
    def __init__(self, failure_threshold=5, recovery_timeout=30):
        self.failure_threshold = failure_threshold
        self.recovery_timeout = recovery_timeout
        self.failures = 0
        self.state = CircuitState.CLOSED
        self.last_failure_time = None

    def call(self, func):
        if self.state == CircuitState.OPEN:
            if time.time() - self.last_failure_time > self.recovery_timeout:
                self.state = CircuitState.HALF_OPEN
            else:
                raise CircuitOpenError("Circuit breaker is open")

        try:
            result = func()
            self._on_success()
            return result
        except Exception as e:
            self._on_failure()
            raise

    def _on_success(self):
        self.failures = 0
        self.state = CircuitState.CLOSED

    def _on_failure(self):
        self.failures += 1
        self.last_failure_time = time.time()
        if self.failures >= self.failure_threshold:
            self.state = CircuitState.OPEN
```

### Fallback
```python
def get_user_with_fallback(user_id):
    """Try primary source, fall back to cache if unavailable."""
    try:
        return database.get_user(user_id)
    except DatabaseError:
        logger.warning(f"Database unavailable, using cache for user {user_id}")
        return cache.get_user(user_id)
    except CacheError:
        logger.error(f"Both database and cache failed for user {user_id}")
        raise ServiceUnavailableError("Unable to retrieve user")
```

### Graceful Degradation
```python
def get_product_details(product_id):
    """Return full details if available, partial if some services fail."""
    result = {"id": product_id}

    # Core data (required)
    result["name"] = database.get_product_name(product_id)
    result["price"] = database.get_product_price(product_id)

    # Enhanced data (optional, degrade gracefully)
    try:
        result["reviews"] = review_service.get_reviews(product_id)
    except ExternalServiceError:
        result["reviews"] = []  # Degrade: no reviews
        result["reviews_unavailable"] = True

    try:
        result["recommendations"] = ml_service.get_recommendations(product_id)
    except ExternalServiceError:
        result["recommendations"] = []  # Degrade: no recommendations

    return result
```

## HTTP Error Handling

### REST API Error Response (RFC 9457)
```json
{
  "type": "https://api.example.com/errors/validation",
  "title": "Validation Failed",
  "status": 400,
  "detail": "The request body contains invalid data",
  "instance": "/orders/12345",
  "errors": [
    {
      "field": "email",
      "message": "Must be a valid email address"
    },
    {
      "field": "quantity",
      "message": "Must be at least 1"
    }
  ]
}
```

### HTTP Status Code Guide
| Code | When to Use |
|------|-------------|
| **400** | Client error - bad request, validation failure |
| **401** | Authentication required |
| **403** | Authenticated but not authorized |
| **404** | Resource not found |
| **409** | Conflict (e.g., duplicate resource) |
| **422** | Unprocessable entity (semantic error) |
| **429** | Rate limit exceeded |
| **500** | Server error - unexpected failure |
| **502** | Bad gateway - upstream service failed |
| **503** | Service unavailable - temporary overload |
| **504** | Gateway timeout - upstream timeout |

## Error Logging Best Practices

### What to Log
```python
try:
    result = process_order(order)
except OrderProcessingError as e:
    logger.error(
        "Order processing failed",
        extra={
            # Context
            "order_id": order.id,
            "user_id": order.user_id,
            "total": order.total,

            # Error details
            "error_type": type(e).__name__,
            "error_message": str(e),
            "error_code": getattr(e, 'code', None),

            # Tracing
            "trace_id": request.trace_id,
            "span_id": current_span_id(),
        },
        exc_info=True  # Include stack trace
    )
```

### What NOT to Log
- Passwords, tokens, API keys
- Full credit card numbers
- Personal identifiable information (PII)
- Sensitive business data

## Language-Specific Patterns

### Python
```python
# Context managers for cleanup
class DatabaseConnection:
    def __enter__(self):
        self.conn = connect()
        return self.conn

    def __exit__(self, exc_type, exc_val, exc_tb):
        self.conn.close()
        return False  # Don't suppress exceptions

# Exception chaining
try:
    parse_config()
except ParseError as e:
    raise ConfigurationError("Invalid config file") from e
```

### JavaScript/TypeScript
```typescript
// Custom error classes
class ApplicationError extends Error {
  constructor(message: string, public code?: string, public details?: object) {
    super(message);
    this.name = this.constructor.name;
    Error.captureStackTrace(this, this.constructor);
  }
}

// Async error handling
async function fetchUser(id: string): Promise<User> {
  try {
    const response = await fetch(`/api/users/${id}`);
    if (!response.ok) {
      throw new HttpError(response.status, await response.text());
    }
    return response.json();
  } catch (error) {
    if (error instanceof HttpError) throw error;
    throw new NetworkError('Failed to fetch user', { cause: error });
  }
}
```

### Go
```go
// Custom error types
type ValidationError struct {
    Field   string
    Message string
}

func (e *ValidationError) Error() string {
    return fmt.Sprintf("validation error on %s: %s", e.Field, e.Message)
}

// Error wrapping
func ProcessOrder(id string) error {
    order, err := db.GetOrder(id)
    if err != nil {
        return fmt.Errorf("failed to get order %s: %w", id, err)
    }
    // ...
}

// Checking wrapped errors
if errors.Is(err, sql.ErrNoRows) {
    // Handle not found
}
```

## Anti-Patterns to Avoid

### 1. Pokemon Exception Handling
```python
# Bad: Catch 'em all
try:
    do_something()
except:  # Catches everything including KeyboardInterrupt
    pass

# Good: Specific exceptions
try:
    do_something()
except (ValueError, TypeError) as e:
    handle_error(e)
```

### 2. Exception as Control Flow
```python
# Bad: Using exceptions for expected cases
def find_user(username):
    for user in users:
        if user.name == username:
            return user
    raise UserNotFoundError()

# Good: Return None or use Result type
def find_user(username):
    for user in users:
        if user.name == username:
            return user
    return None
```

### 3. Rethrowing Without Context
```python
# Bad: Loses original context
try:
    process()
except Exception:
    raise RuntimeError("Processing failed")

# Good: Preserve chain
try:
    process()
except Exception as e:
    raise RuntimeError("Processing failed") from e
```

### 4. Logging and Rethrowing
```python
# Bad: Double logging (will be logged again up the stack)
try:
    process()
except Exception as e:
    logger.error(f"Error: {e}")
    raise

# Good: Log once at the handling point, or just rethrow
try:
    process()
except Exception:
    raise  # Let caller handle and log
```

## Error Handling Checklist

### Design Phase
- [ ] Defined error hierarchy for the application
- [ ] Identified recoverable vs non-recoverable errors
- [ ] Designed error response format for APIs
- [ ] Planned retry/fallback strategies

### Implementation
- [ ] Specific exception types used
- [ ] Errors include relevant context
- [ ] No swallowed exceptions
- [ ] Proper cleanup in finally/defer/using
- [ ] Async errors properly handled

### Observability
- [ ] Errors logged with context
- [ ] Sensitive data excluded from logs
- [ ] Error metrics tracked
- [ ] Alerts configured for critical errors

### Testing
- [ ] Error paths tested
- [ ] Recovery mechanisms tested
- [ ] Timeout behavior tested
- [ ] Concurrent error handling tested
