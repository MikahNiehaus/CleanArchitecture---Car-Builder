# Tool Design for AI Agents

TRIGGER: tool, MCP, tool definition, API, function, parameter, tool use, tool call

## Overview

Tools are the primary building blocks of agent execution. Well-designed tools dramatically improve agent performance; poorly designed tools waste tokens and cause errors.

## Core Principles

### Principle 1: Clear Naming and Descriptions

**DO**: Use explicit, unambiguous names
```json
{
  "name": "search_orders_by_customer_id",
  "description": "Search for orders placed by a specific customer. Returns order summaries with IDs, dates, and totals."
}
```

**DON'T**: Use vague or ambiguous names
```json
{
  "name": "search",
  "description": "Search for things"
}
```

### Principle 2: Self-Documenting Parameters

**DO**: Include type hints, examples, and constraints
```json
{
  "parameters": {
    "customer_id": {
      "type": "string",
      "description": "Customer UUID (e.g., 'cust_abc123'). Must be 12 characters.",
      "pattern": "^cust_[a-z0-9]{8}$"
    },
    "status_filter": {
      "type": "string",
      "enum": ["pending", "shipped", "delivered", "cancelled"],
      "description": "Filter orders by status. Defaults to all statuses."
    }
  }
}
```

**DON'T**: Leave parameters underdocumented
```json
{
  "parameters": {
    "id": { "type": "string" },
    "status": { "type": "string" }
  }
}
```

### Principle 3: Actionable Error Messages

**DO**: Return specific, correctable errors
```json
{
  "error": true,
  "message": "Invalid date format. Expected YYYY-MM-DD, got '12/25/2024'",
  "suggestion": "Use '2024-12-25' instead",
  "example": "search_orders(start_date='2024-12-25')"
}
```

**DON'T**: Return opaque error codes
```json
{
  "error": true,
  "code": "E_INVALID_PARAM"
}
```

### Principle 4: Token-Efficient Responses

**DO**: Return only high-signal information
```json
{
  "orders": [
    {
      "order_id": "ord_123",
      "customer_name": "John Doe",
      "total": "$99.00",
      "status": "shipped"
    }
  ],
  "total_count": 42,
  "has_more": true
}
```

**DON'T**: Return everything
```json
{
  "orders": [
    {
      "order_uuid": "550e8400-e29b-41d4-a716-446655440000",
      "customer_uuid": "...",
      "internal_status_code": 3,
      "created_at_timestamp_ms": 1702483200000,
      "updated_at_timestamp_ms": 1702569600000,
      "shipping_carrier_internal_id": "...",
      // ... 50 more fields
    }
  ]
}
```

## Response Design

### Pagination and Filtering

For large result sets, implement:

```markdown
## Pagination Pattern

Parameters:
- `limit`: Max results (default: 20, max: 100)
- `offset`: Skip N results (default: 0)
- `cursor`: For cursor-based pagination

Response includes:
- `results`: Array of items
- `total_count`: Total available
- `has_more`: Boolean for more pages
- `next_cursor`: If cursor-based
```

### Response Format Control

Offer format options for different needs:

```json
{
  "response_format": {
    "type": "string",
    "enum": ["concise", "detailed"],
    "description": "concise: names and IDs only. detailed: full metadata",
    "default": "concise"
  }
}
```

### Semantic vs Technical Identifiers

| Use Case | Prefer | Avoid |
|----------|--------|-------|
| Display to user | `name`, `title` | `uuid`, `internal_id` |
| Type classification | `file_type: "image"` | `mime_type: "image/jpeg"` |
| Status | `status: "active"` | `status_code: 1` |
| Dates | `created: "2024-12-12"` | `created_ms: 1702358400000` |

Only include technical identifiers when needed for downstream API calls.

## Tool Scope Design

### Right-Sizing Tools

**Anti-pattern**: One tool per API endpoint
```
- create_user
- get_user
- update_user_email
- update_user_name
- update_user_settings
- delete_user
- list_users
- search_users
... (20 more tools)
```

**Better**: Consolidated workflow-oriented tools
```
- manage_user (create, update, delete)
- search_users (list, filter, paginate)
```

### When to Split vs Combine

| Split When | Combine When |
|------------|--------------|
| Different authentication needed | Same entity, different operations |
| Very different use cases | Logically sequential operations |
| Complex parameters per operation | Simple, related queries |
| Different error handling needed | Same error patterns |

### Meaningful Namespacing

Group related tools with prefixes:

```
# Good namespacing
github_repos_search
github_repos_create
github_issues_list
github_issues_create

# Poor namespacing
searchRepos
createRepo
listIssues
create_issue
```

## Error Handling Design

### Error Categories

| Category | HTTP Code | Agent Action |
|----------|-----------|--------------|
| Invalid Input | 400 | Fix parameters, retry |
| Not Found | 404 | Try different search |
| Rate Limited | 429 | Wait, retry |
| Auth Failed | 401/403 | Report BLOCKED |
| Server Error | 500+ | Retry with backoff |

### Steering Through Errors

Errors can guide agents toward better behavior:

```json
{
  "error": true,
  "message": "Search returned 10,000+ results",
  "suggestion": "Add filters to narrow results. Try adding date_range or status filters.",
  "tip": "Smaller, targeted searches are more efficient than broad searches"
}
```

## Token Budget Awareness

### Tool Definition Costs

| Component | Approximate Tokens |
|-----------|-------------------|
| Tool name + description | 50-100 |
| Each parameter | 20-50 |
| Enum values | 5-10 each |
| Examples in description | 30-50 each |

### Optimization Strategies

1. **Prune unused parameters**: Remove rarely-used optional params
2. **Collapse similar tools**: Combine tools with overlapping functionality
3. **Default sensible values**: Reduce required parameters
4. **Lazy loading**: Only include advanced tools when needed

### Multi-Server Concerns

Multiple MCP servers can consume significant context before conversations start:

| Setup | Approximate Token Cost |
|-------|----------------------|
| 1 server, 5 tools | ~5K tokens |
| 3 servers, 15 tools | ~25K tokens |
| 5 servers, 30+ tools | ~55K tokens |

**Mitigation**: Only connect servers needed for current task.

## Evaluation-Driven Improvement

### Testing Tools

Create realistic test scenarios:

```markdown
## Tool Test Scenario

Task: Find all failed orders from last week
Expected tool sequence:
1. search_orders(status="failed", date_range="last_7_days")
2. If >20 results: paginate with cursor

Success criteria:
- Finds all failed orders (precision)
- No unnecessary calls (efficiency)
- Handles pagination correctly
```

### Analyzing Tool Usage

Watch for these patterns in transcripts:

| Pattern | Problem | Fix |
|---------|---------|-----|
| Many retries with param changes | Unclear parameter spec | Add examples |
| Redundant tool calls | Missing pagination | Add pagination |
| Wrong tool selection | Ambiguous names | Improve descriptions |
| Large response handling | Missing truncation | Add limits |

### Iterative Refinement

1. Run evaluation scenarios
2. Analyze failure patterns
3. Identify tool rough edges
4. Refine descriptions/parameters
5. Re-evaluate
6. Repeat until metrics improve

---

*"Even small refinements to tool descriptions can yield dramatic improvements."* - Anthropic

**Source**: [Writing Tools for Agents](https://www.anthropic.com/engineering/writing-tools-for-agents)
