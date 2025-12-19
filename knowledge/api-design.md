# API Design Best Practices

TRIGGER: API, REST, endpoint, HTTP, versioning, request, response, GraphQL, query, mutation, subscription, schema

## Overview

Well-designed APIs are consistent, intuitive, and maintainable. This guide covers REST and GraphQL API design principles, versioning strategies, error handling, and documentation standards.

## When to Use REST vs GraphQL

| Factor | REST | GraphQL |
|--------|------|---------|
| **Data requirements** | Fixed, known responses | Client-defined, flexible |
| **Clients** | Homogeneous (similar needs) | Heterogeneous (different needs) |
| **Caching** | HTTP caching built-in | Requires custom caching |
| **File uploads** | Native support | Requires extensions |
| **Learning curve** | Lower | Higher |
| **Over-fetching** | Common problem | Solved by design |
| **Under-fetching** | Multiple requests needed | Single request |
| **Real-time** | Requires WebSockets | Subscriptions built-in |

**Choose REST when**: Simple CRUD, public APIs, caching critical, team REST-experienced
**Choose GraphQL when**: Complex data relationships, multiple clients, rapid iteration, avoiding over/under-fetching

## Core Principles

### API-First Design
1. Design the API contract before writing code
2. Review with stakeholders (consumers, architects)
3. Use OpenAPI/Swagger for specification
4. Generate code from spec, not spec from code

### REST Constraints
- **Client-Server**: Separation of concerns
- **Stateless**: Each request contains all needed information
- **Cacheable**: Responses indicate cacheability
- **Uniform Interface**: Consistent resource addressing
- **Layered System**: Intermediaries transparent to client

---

## Resource Design

### URL Structure
Use nouns for resources, not verbs:
```
# GOOD - Resources as nouns
GET    /users           # List users
GET    /users/123       # Get user 123
POST   /users           # Create user
PUT    /users/123       # Update user 123
DELETE /users/123       # Delete user 123

# BAD - Verbs in URL
GET    /getUsers
POST   /createUser
POST   /deleteUser/123
```

### Naming Conventions
| Convention | Example |
|------------|---------|
| Plural nouns | `/users`, `/orders`, `/products` |
| Lowercase | `/user-profiles` not `/UserProfiles` |
| Hyphens for readability | `/user-accounts` not `/user_accounts` |
| No file extensions | `/users` not `/users.json` |

### Nested Resources
Use for clear parent-child relationships:
```
GET /users/123/orders           # User's orders
GET /users/123/orders/456       # Specific order
POST /users/123/orders          # Create order for user
```

Limit nesting depth to 2-3 levels maximum.

### Query Parameters
Use for filtering, sorting, pagination:
```
GET /users?status=active                    # Filter
GET /users?sort=created_at&order=desc       # Sort
GET /users?page=2&limit=20                  # Paginate
GET /users?fields=id,name,email             # Sparse fields
```

---

## HTTP Methods

### Method Semantics

| Method | Purpose | Idempotent | Safe | Request Body |
|--------|---------|------------|------|--------------|
| GET | Retrieve resource(s) | Yes | Yes | No |
| POST | Create resource | No | No | Yes |
| PUT | Replace resource | Yes | No | Yes |
| PATCH | Partial update | No | No | Yes |
| DELETE | Remove resource | Yes | No | Optional |

### Method Usage
```
# GET - Retrieve (never modify data)
GET /users/123
→ Returns user 123

# POST - Create (returns created resource)
POST /users
Body: {"name": "John", "email": "john@example.com"}
→ Returns created user with ID

# PUT - Full replacement (send complete resource)
PUT /users/123
Body: {"name": "John Doe", "email": "john@example.com", "status": "active"}
→ Returns updated user

# PATCH - Partial update (send only changed fields)
PATCH /users/123
Body: {"status": "inactive"}
→ Returns updated user

# DELETE - Remove
DELETE /users/123
→ Returns 204 No Content
```

---

## HTTP Status Codes

### Success Codes (2xx)
| Code | Meaning | Use Case |
|------|---------|----------|
| 200 | OK | Successful GET, PUT, PATCH |
| 201 | Created | Successful POST, resource created |
| 204 | No Content | Successful DELETE |

### Client Error Codes (4xx)
| Code | Meaning | Use Case |
|------|---------|----------|
| 400 | Bad Request | Invalid request body/parameters |
| 401 | Unauthorized | Missing/invalid authentication |
| 403 | Forbidden | Authenticated but not authorized |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | State conflict (duplicate, version) |
| 422 | Unprocessable Entity | Validation errors |
| 429 | Too Many Requests | Rate limit exceeded |

### Server Error Codes (5xx)
| Code | Meaning | Use Case |
|------|---------|----------|
| 500 | Internal Server Error | Unexpected server error |
| 502 | Bad Gateway | Upstream service error |
| 503 | Service Unavailable | Temporary overload/maintenance |
| 504 | Gateway Timeout | Upstream timeout |

### Anti-Pattern
```json
// BAD - 200 with error in body
HTTP 200 OK
{"success": false, "error": "User not found"}

// GOOD - Proper status code
HTTP 404 Not Found
{"error": "User not found"}
```

---

## Error Handling

### RFC 9457 Problem Details Format
Standard error response format:
```json
{
  "type": "https://api.example.com/errors/validation-error",
  "title": "Validation Error",
  "status": 422,
  "detail": "The email address format is invalid",
  "instance": "/users/123",
  "errors": [
    {
      "field": "email",
      "message": "Must be a valid email address"
    }
  ]
}
```

### Error Response Best Practices
1. **Use appropriate HTTP status codes** - Don't wrap errors in 200
2. **Be consistent** - Same error format across all endpoints
3. **Be specific** - Tell clients what went wrong
4. **Be secure** - Don't expose internal details or stack traces
5. **Be actionable** - Help clients fix the problem

### Validation Errors
```json
{
  "type": "https://api.example.com/errors/validation-error",
  "title": "Validation Error",
  "status": 422,
  "errors": [
    {"field": "email", "message": "Required field"},
    {"field": "age", "message": "Must be positive integer"}
  ]
}
```

---

## API Versioning

### Strategies

#### URL Path Versioning (Most Common)
```
GET /v1/users
GET /v2/users
```
**Pros**: Explicit, easy to route, cache-friendly
**Cons**: URL changes between versions

#### Header Versioning
```
GET /users
Accept: application/vnd.api.v2+json
```
**Pros**: Clean URLs
**Cons**: Harder to test, less visible

#### Query Parameter Versioning
```
GET /users?version=2
```
**Pros**: Simple to implement
**Cons**: Clutters query string

### Recommendation
- **Use URL path versioning** for public APIs (most widely understood)
- Version at major breaking changes only
- Provide migration documentation
- Set deprecation timeline with sunset headers

### Deprecation Headers
```
Sunset: Sat, 31 Dec 2025 23:59:59 GMT
Deprecation: Sun, 01 Jan 2025 00:00:00 GMT
Link: <https://api.example.com/v3/users>; rel="successor-version"
```

---

## Pagination

### Offset-Based (Simple)
```
GET /users?offset=40&limit=20
```
**Response**:
```json
{
  "data": [...],
  "pagination": {
    "total": 100,
    "limit": 20,
    "offset": 40,
    "next": "/users?offset=60&limit=20",
    "prev": "/users?offset=20&limit=20"
  }
}
```

### Cursor-Based (Scalable)
```
GET /users?cursor=eyJpZCI6MTIzfQ&limit=20
```
**Response**:
```json
{
  "data": [...],
  "pagination": {
    "next_cursor": "eyJpZCI6MTQzfQ",
    "has_more": true
  }
}
```
**Pros**: Works well with real-time data, no page drift
**Cons**: No random access to pages

---

## Rate Limiting

### Headers
```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1640995200
Retry-After: 60
```

### Response on Limit Exceeded
```json
HTTP 429 Too Many Requests

{
  "type": "https://api.example.com/errors/rate-limit-exceeded",
  "title": "Rate Limit Exceeded",
  "status": 429,
  "detail": "You have exceeded the rate limit of 1000 requests per hour",
  "retry_after": 60
}
```

---

## Authentication & Security

### Methods
| Method | Use Case |
|--------|----------|
| API Key | Simple, server-to-server |
| OAuth 2.0 | User authorization, third-party apps |
| JWT | Stateless authentication |

### Security Best Practices
1. **Always use HTTPS** (TLS 1.3 preferred)
2. **Authenticate in headers** - Never in URL query params
3. **Use short-lived tokens** - Refresh tokens for long sessions
4. **Implement rate limiting** - Prevent abuse
5. **Validate all input** - Don't trust client data
6. **Log security events** - Authentication failures, unusual patterns

### Authorization Header
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## Request/Response Format

### Content-Type
```
Content-Type: application/json
Accept: application/json
```

### Request Body Standards
```json
// Dates: ISO 8601
{"created_at": "2024-01-15T10:30:00Z"}

// Enums: lowercase strings
{"status": "active"}

// IDs: strings (avoid integer overflow)
{"user_id": "123456789"}

// Nulls: explicit null for clearing values
{"middle_name": null}
```

### Response Envelope (Optional)
```json
{
  "data": {...},
  "meta": {
    "request_id": "abc123",
    "timestamp": "2024-01-15T10:30:00Z"
  }
}
```

---

## Documentation

### OpenAPI/Swagger
```yaml
openapi: "3.1.0"
info:
  title: User API
  version: "1.0.0"
paths:
  /users:
    get:
      summary: List users
      responses:
        '200':
          description: Successful response
```

### Documentation Must Include
- [ ] Authentication requirements
- [ ] Request/response examples
- [ ] Error responses and codes
- [ ] Rate limits
- [ ] Pagination details
- [ ] Versioning policy

---

## API Design Checklist

### Resource Design
- [ ] Nouns for resources (not verbs)
- [ ] Plural names for collections
- [ ] Consistent naming conventions
- [ ] Sensible nesting depth

### HTTP Semantics
- [ ] Correct methods for operations
- [ ] Appropriate status codes
- [ ] Idempotent operations where expected

### Error Handling
- [ ] Consistent error format (RFC 9457)
- [ ] Specific, actionable messages
- [ ] No internal details exposed

### Versioning & Evolution
- [ ] Versioning strategy in place
- [ ] Deprecation policy defined
- [ ] Backward compatibility considered

### Security
- [ ] HTTPS enforced
- [ ] Authentication in headers
- [ ] Rate limiting implemented
- [ ] Input validation

### Documentation
- [ ] OpenAPI spec maintained
- [ ] Examples for all endpoints
- [ ] Changelog for versions

---

# GraphQL API Design

## GraphQL Fundamentals

GraphQL is a query language and runtime for APIs that gives clients the power to ask for exactly what they need. Unlike REST where the server defines the response structure, GraphQL lets clients specify their data requirements.

### Core Concepts
- **Schema**: Contract defining types, queries, mutations, subscriptions
- **Queries**: Read operations (like GET)
- **Mutations**: Write operations (like POST/PUT/DELETE)
- **Subscriptions**: Real-time updates via WebSocket
- **Resolvers**: Functions that fetch data for each field

---

## Schema Design

### Type Definitions
```graphql
# Scalar types: ID, String, Int, Float, Boolean
# Custom scalars for domain types
scalar DateTime
scalar Email

# Object types
type User {
  id: ID!                    # ! means non-nullable
  email: Email!
  name: String!
  createdAt: DateTime!
  orders: [Order!]!          # Non-null list of non-null orders
  profile: Profile           # Nullable relation
}

type Order {
  id: ID!
  user: User!
  items: [OrderItem!]!
  total: Float!
  status: OrderStatus!
  createdAt: DateTime!
}

# Enum types
enum OrderStatus {
  PENDING
  PROCESSING
  SHIPPED
  DELIVERED
  CANCELLED
}
```

### Input Types
```graphql
# Use input types for mutation arguments
input CreateUserInput {
  email: Email!
  name: String!
  password: String!
}

input UpdateUserInput {
  email: Email
  name: String
  # Password update should be separate mutation
}

# Filter inputs for queries
input UserFilter {
  status: UserStatus
  createdAfter: DateTime
  searchTerm: String
}
```

### Interface and Union Types
```graphql
# Interface for shared fields
interface Node {
  id: ID!
}

interface Timestamped {
  createdAt: DateTime!
  updatedAt: DateTime!
}

type User implements Node & Timestamped {
  id: ID!
  createdAt: DateTime!
  updatedAt: DateTime!
  # ... other fields
}

# Union for polymorphic returns
union SearchResult = User | Order | Product

type Query {
  search(term: String!): [SearchResult!]!
}
```

---

## Query Design

### Basic Queries
```graphql
type Query {
  # Single resource by ID
  user(id: ID!): User

  # List with filtering, pagination
  users(
    filter: UserFilter
    first: Int
    after: String
  ): UserConnection!

  # Current authenticated user
  me: User

  # Search across types
  search(query: String!, types: [SearchType!]): [SearchResult!]!
}
```

### Pagination (Relay Cursor Style)
```graphql
# Connection pattern for pagination
type UserConnection {
  edges: [UserEdge!]!
  pageInfo: PageInfo!
  totalCount: Int!
}

type UserEdge {
  node: User!
  cursor: String!
}

type PageInfo {
  hasNextPage: Boolean!
  hasPreviousPage: Boolean!
  startCursor: String
  endCursor: String
}

# Usage
query {
  users(first: 10, after: "cursor123") {
    edges {
      node {
        id
        name
      }
      cursor
    }
    pageInfo {
      hasNextPage
      endCursor
    }
  }
}
```

### Avoiding N+1 with DataLoader
```javascript
// Problem: N+1 queries
// Query users -> for each user, query orders (N additional queries)

// Solution: DataLoader batches requests
const orderLoader = new DataLoader(async (userIds) => {
  // Single query: SELECT * FROM orders WHERE user_id IN (...)
  const orders = await db.orders.findByUserIds(userIds);
  return userIds.map(id => orders.filter(o => o.userId === id));
});

// Resolver uses loader
const resolvers = {
  User: {
    orders: (user, args, context) => context.loaders.orders.load(user.id)
  }
};
```

---

## Mutation Design

### Mutation Structure
```graphql
type Mutation {
  # Specific, action-oriented names
  createUser(input: CreateUserInput!): CreateUserPayload!
  updateUser(id: ID!, input: UpdateUserInput!): UpdateUserPayload!
  deleteUser(id: ID!): DeleteUserPayload!

  # Business operations (not just CRUD)
  placeOrder(input: PlaceOrderInput!): PlaceOrderPayload!
  cancelOrder(id: ID!, reason: String): CancelOrderPayload!

  # Batch operations
  bulkUpdateUsers(ids: [ID!]!, input: UpdateUserInput!): BulkUpdatePayload!
}
```

### Payload Pattern
```graphql
# Always return a payload type (not just the entity)
type CreateUserPayload {
  user: User                          # The created entity
  errors: [UserError!]!               # Validation/business errors
  success: Boolean!                   # Quick success check
}

type UserError {
  field: String                       # Which field had error
  message: String!                    # Human-readable message
  code: ErrorCode!                    # Machine-readable code
}

enum ErrorCode {
  INVALID_EMAIL
  EMAIL_TAKEN
  WEAK_PASSWORD
  NOT_FOUND
  UNAUTHORIZED
}
```

### Mutation Example
```graphql
mutation CreateUser($input: CreateUserInput!) {
  createUser(input: $input) {
    success
    user {
      id
      email
      name
    }
    errors {
      field
      message
      code
    }
  }
}

# Variables
{
  "input": {
    "email": "john@example.com",
    "name": "John Doe",
    "password": "SecurePass123!"
  }
}
```

---

## Subscription Design

### Real-Time Updates
```graphql
type Subscription {
  # Entity-specific subscriptions
  orderUpdated(orderId: ID!): Order!

  # User-scoped subscriptions
  userNotifications: Notification!

  # Filtered subscriptions
  newOrders(status: OrderStatus): Order!
}

# Client usage
subscription {
  orderUpdated(orderId: "123") {
    id
    status
    updatedAt
  }
}
```

### Subscription Implementation
```javascript
// Using graphql-subscriptions
const { PubSub } = require('graphql-subscriptions');
const pubsub = new PubSub();

const resolvers = {
  Mutation: {
    updateOrder: async (_, { id, input }) => {
      const order = await db.orders.update(id, input);
      pubsub.publish(`ORDER_UPDATED_${id}`, { orderUpdated: order });
      return { order, success: true };
    }
  },
  Subscription: {
    orderUpdated: {
      subscribe: (_, { orderId }) =>
        pubsub.asyncIterator(`ORDER_UPDATED_${orderId}`)
    }
  }
};
```

---

## Error Handling

### Error Categories
```graphql
# 1. Top-level errors (schema validation, auth, server errors)
# Returned in "errors" array
{
  "errors": [
    {
      "message": "Not authenticated",
      "extensions": {
        "code": "UNAUTHENTICATED"
      }
    }
  ],
  "data": null
}

# 2. Field-level errors (business/validation errors)
# Returned in payload
{
  "data": {
    "createUser": {
      "success": false,
      "user": null,
      "errors": [
        {
          "field": "email",
          "message": "Email already registered",
          "code": "EMAIL_TAKEN"
        }
      ]
    }
  }
}
```

### Error Extensions
```graphql
# Custom error with extensions
{
  "errors": [
    {
      "message": "Rate limit exceeded",
      "path": ["createUser"],
      "extensions": {
        "code": "RATE_LIMITED",
        "retryAfter": 60,
        "limit": 100,
        "remaining": 0
      }
    }
  ]
}
```

### Error Handling Best Practices
1. **Use payload errors for expected failures** (validation, business rules)
2. **Use top-level errors for unexpected failures** (auth, server errors)
3. **Include error codes** for programmatic handling
4. **Never expose internal details** (stack traces, SQL errors)
5. **Provide actionable messages** for developers

---

## Schema Evolution & Deprecation

### Adding Fields (Non-Breaking)
```graphql
type User {
  id: ID!
  name: String!
  email: String!
  # New field - non-breaking, clients ignore unknown fields
  phoneNumber: String
}
```

### Deprecating Fields
```graphql
type User {
  id: ID!
  name: String!

  # Deprecated - provide migration path
  fullName: String @deprecated(reason: "Use 'name' instead. Will be removed 2025-06-01")

  # Old enum value
  status: UserStatus
}

enum UserStatus {
  ACTIVE
  INACTIVE
  SUSPENDED @deprecated(reason: "Use INACTIVE instead")
}
```

### Breaking Changes (Avoid!)
```graphql
# BREAKING: Don't do these without versioning
# - Removing fields/types
# - Changing field types (String -> Int)
# - Making nullable field non-nullable
# - Removing enum values
# - Renaming fields/types

# SAFE: These are backwards compatible
# - Adding new fields (nullable or with defaults)
# - Adding new types
# - Adding new enum values
# - Deprecating (not removing) fields
```

### Versioning Strategy
```
# GraphQL typically doesn't version like REST
# Instead: evolve schema, deprecate, then remove

Timeline:
1. Add new field alongside old
2. Mark old field @deprecated with date
3. Monitor usage of deprecated field
4. Remove after deprecation period (6+ months)

# If breaking change unavoidable:
# - Use field arguments for different behaviors
# - Create new types (UserV2) temporarily
# - Consider separate endpoint for major versions
```

---

## Security

### Query Complexity & Depth Limiting
```javascript
// Prevent expensive queries
const depthLimit = require('graphql-depth-limit');
const { createComplexityLimitRule } = require('graphql-validation-complexity');

const server = new ApolloServer({
  validationRules: [
    depthLimit(10),  // Max nesting depth
    createComplexityLimitRule(1000, {
      scalarCost: 1,
      objectCost: 10,
      listFactor: 10
    })
  ]
});
```

### Persisted Queries
```javascript
// Only allow pre-approved queries in production
// Prevents arbitrary query injection

// Client sends hash instead of query
POST /graphql
{
  "extensions": {
    "persistedQuery": {
      "sha256Hash": "abc123..."
    }
  },
  "variables": { "id": "123" }
}
```

### Authorization
```graphql
# Directive-based authorization
directive @auth(requires: Role!) on FIELD_DEFINITION

type Query {
  users: [User!]! @auth(requires: ADMIN)
  me: User @auth(requires: USER)
  publicProducts: [Product!]!  # No auth required
}

enum Role {
  USER
  ADMIN
  SUPER_ADMIN
}
```

```javascript
// Resolver-level authorization
const resolvers = {
  Query: {
    users: (_, args, context) => {
      if (!context.user?.roles.includes('ADMIN')) {
        throw new ForbiddenError('Admin access required');
      }
      return db.users.findAll();
    }
  }
};
```

---

## Performance

### Field-Level Caching
```graphql
# Cache hints in schema
type User @cacheControl(maxAge: 60) {
  id: ID!
  name: String!
  email: String! @cacheControl(maxAge: 0)  # No cache for PII
  publicProfile: Profile @cacheControl(maxAge: 3600)
}
```

### Batching with DataLoader
```javascript
// Always use DataLoader for relationships
// Prevents N+1 query problem

const loaders = {
  users: new DataLoader(ids => batchGetUsers(ids)),
  orders: new DataLoader(ids => batchGetOrders(ids)),
  products: new DataLoader(ids => batchGetProducts(ids))
};
```

### Query Planning
```javascript
// Use query info to optimize fetching
const resolvers = {
  Query: {
    users: (_, args, context, info) => {
      // Parse info to determine which fields are requested
      // Only fetch what's needed from database
      const requestedFields = getFieldsFromInfo(info);
      return db.users.findAll({ select: requestedFields });
    }
  }
};
```

---

## GraphQL Design Checklist

### Schema Design
- [ ] Use specific, domain-driven type names
- [ ] Implement Node interface for relay compatibility
- [ ] Use input types for mutation arguments
- [ ] Use enums for fixed value sets
- [ ] Add descriptions to types and fields

### Queries
- [ ] Implement cursor-based pagination
- [ ] Use DataLoader for all relationships
- [ ] Add filtering and sorting options
- [ ] Limit query depth and complexity

### Mutations
- [ ] Use payload types with errors array
- [ ] Use specific action verbs (createX, updateX)
- [ ] Validate input at resolver level
- [ ] Return affected entity in payload

### Security
- [ ] Implement authentication/authorization
- [ ] Add query depth limiting
- [ ] Add complexity analysis
- [ ] Consider persisted queries for production
- [ ] Sanitize all inputs

### Evolution
- [ ] Use @deprecated before removing
- [ ] Document deprecation timeline
- [ ] Monitor deprecated field usage
- [ ] Plan migration path for clients

---

## References

- [Microsoft Azure REST API Guidelines](https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-design)
- [Google Cloud API Design Guide](https://cloud.google.com/apis/design)
- [Zalando RESTful API Guidelines](https://opensource.zalando.com/restful-api-guidelines/)
- [RFC 9457 - Problem Details for HTTP APIs](https://www.rfc-editor.org/rfc/rfc9457)
- [JSON:API Specification](https://jsonapi.org/)
- [GraphQL Official Documentation](https://graphql.org/learn/)
- [Apollo GraphQL Best Practices](https://www.apollographql.com/docs/)
- [Relay GraphQL Server Specification](https://relay.dev/docs/guides/graphql-server-specification/)
- [Production Ready GraphQL](https://book.productionreadygraphql.com/)
