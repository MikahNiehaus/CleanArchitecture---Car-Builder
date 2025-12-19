# Observability Knowledge Base

TRIGGER: logging, metrics, tracing, monitoring, observability, alerts, telemetry, dashboard, APM

## Overview

Observability is the ability to understand the internal state of a system by examining its outputs. The three pillars are **Logs** (events), **Metrics** (aggregated measurements), and **Traces** (request flows). Together they answer: What happened? How often? Where in the system?

## The Three Pillars

### Logs
**What**: Discrete events with context
**When to use**: Debugging, audit trails, error details
**Examples**: Error messages, request logs, audit events

### Metrics
**What**: Numerical measurements over time
**When to use**: Alerting, dashboards, capacity planning
**Examples**: Request count, latency percentiles, CPU usage

### Traces
**What**: Request flow through distributed systems
**When to use**: Understanding dependencies, latency breakdown
**Examples**: API call → Service A → Database → Cache → Response

## Structured Logging

### Why Structure Matters
```
# Bad: Unstructured log
ERROR: Failed to process order for user john

# Good: Structured log (JSON)
{
  "level": "error",
  "message": "Failed to process order",
  "user_id": "user_123",
  "order_id": "order_456",
  "error_type": "PaymentDeclined",
  "timestamp": "2025-12-05T14:30:00Z",
  "trace_id": "abc123"
}
```

### Log Levels
| Level | Usage | Examples |
|-------|-------|----------|
| **TRACE** | Very detailed debugging | Variable values in loops |
| **DEBUG** | Development debugging | Function entry/exit, state changes |
| **INFO** | Normal operations | Request completed, job started |
| **WARN** | Potential problems | Retry attempted, deprecated API used |
| **ERROR** | Operation failed | Exception caught, request failed |
| **FATAL** | Application cannot continue | Startup failure, critical dependency down |

### Logging Best Practices

**DO**:
```python
# Include context
logger.error("Payment failed", extra={
    "user_id": user.id,
    "order_id": order.id,
    "amount": order.total,
    "error_code": e.code
})

# Use correlation IDs
logger.info("Processing request", extra={"trace_id": request.trace_id})

# Log at boundaries
logger.info("External API call", extra={
    "service": "payment-gateway",
    "method": "POST",
    "path": "/charge"
})
```

**DON'T**:
```python
# No context
logger.error("Something went wrong")

# Sensitive data
logger.info(f"User login: {password}")

# High-cardinality in messages
logger.info(f"Request from {ip_address}")  # Put in structured field instead

# Excessive logging in loops
for item in large_list:
    logger.debug(f"Processing {item}")  # Will flood logs
```

### Log Aggregation Tools
| Tool | Best For | Features |
|------|----------|----------|
| **ELK Stack** | Self-hosted, full control | Elasticsearch, Logstash, Kibana |
| **Datadog** | Unified platform | Logs + metrics + traces integrated |
| **Splunk** | Enterprise, compliance | Powerful search, security focus |
| **CloudWatch** | AWS native | Integrated with AWS services |
| **Loki** | Kubernetes, cost-effective | Prometheus-like labels, no indexing |

## Metrics

### Metric Types

**Counter**: Only increases (or resets)
```
http_requests_total{method="GET", path="/api/users", status="200"}
```

**Gauge**: Current value (can go up or down)
```
active_connections{service="api"}
memory_usage_bytes{instance="web-1"}
```

**Histogram**: Distribution of values
```
http_request_duration_seconds_bucket{le="0.1"}
http_request_duration_seconds_bucket{le="0.5"}
http_request_duration_seconds_bucket{le="1.0"}
```

**Summary**: Pre-calculated percentiles
```
http_request_duration_seconds{quantile="0.5"}
http_request_duration_seconds{quantile="0.99"}
```

### Key Metrics to Track

**RED Method** (Request-focused):
- **R**ate: Requests per second
- **E**rrors: Failed requests per second
- **D**uration: Request latency distribution

**USE Method** (Resource-focused):
- **U**tilization: % resource in use
- **S**aturation: Work queued/waiting
- **E**rrors: Error count

### Essential Application Metrics
```
# Request metrics
http_requests_total{method, path, status}
http_request_duration_seconds{method, path}
http_request_size_bytes{method, path}

# Business metrics
orders_processed_total{status}
revenue_dollars_total{product_type}
user_signups_total{source}

# System metrics
process_cpu_seconds_total
process_memory_bytes
process_open_fds
```

### Metrics Tools
| Tool | Type | Best For |
|------|------|----------|
| **Prometheus** | Pull-based TSDB | Kubernetes, microservices |
| **Datadog** | Push-based SaaS | Full stack monitoring |
| **CloudWatch** | AWS native | AWS services |
| **InfluxDB** | Push-based TSDB | IoT, high-cardinality |
| **Grafana** | Visualization | Dashboards, any data source |

## Distributed Tracing

### Concepts

**Trace**: Full journey of a request through the system
**Span**: Single operation within a trace
**Context Propagation**: Passing trace IDs across services

### Span Anatomy
```json
{
  "trace_id": "abc123",
  "span_id": "span456",
  "parent_span_id": "span123",
  "operation_name": "database.query",
  "start_time": "2025-12-05T14:30:00.000Z",
  "duration_ms": 45,
  "status": "OK",
  "attributes": {
    "db.system": "postgresql",
    "db.statement": "SELECT * FROM users WHERE id = ?",
    "db.rows_affected": 1
  }
}
```

### OpenTelemetry (OTel)

**The standard for observability instrumentation**

```python
# Python example
from opentelemetry import trace
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter

# Setup
provider = TracerProvider()
processor = BatchSpanProcessor(OTLPSpanExporter(endpoint="http://collector:4317"))
provider.add_span_processor(processor)
trace.set_tracer_provider(provider)

# Usage
tracer = trace.get_tracer(__name__)

with tracer.start_as_current_span("process_order") as span:
    span.set_attribute("order.id", order_id)
    span.set_attribute("order.total", total)
    # ... process order
```

### Tracing Tools
| Tool | Type | Best For |
|------|------|----------|
| **Jaeger** | Open source | Self-hosted, Kubernetes |
| **Zipkin** | Open source | Simpler setup |
| **Datadog APM** | SaaS | Full platform integration |
| **AWS X-Ray** | AWS native | AWS services |
| **Honeycomb** | SaaS | High-cardinality analysis |

## Alerting

### Alert Design Principles

**Alert on symptoms, not causes**:
```
# Bad: Alert on cause
ALERT: Database CPU > 80%

# Good: Alert on symptom
ALERT: API latency p99 > 500ms
```

**Include runbook links**:
```yaml
alert: HighErrorRate
annotations:
  summary: "Error rate above 5%"
  runbook_url: "https://runbooks.example.com/high-error-rate"
```

### Alert Severity Levels
| Severity | Response | Examples |
|----------|----------|----------|
| **Critical** | Wake someone up | Service down, data loss |
| **Warning** | Investigate soon | Degraded performance, disk filling |
| **Info** | Review during business hours | Unusual pattern, threshold approaching |

### Avoiding Alert Fatigue
- Set realistic thresholds (not too sensitive)
- Group related alerts
- Require sustained condition before alerting
- Have clear ownership for each alert
- Regularly review and tune alerts

## Dashboard Design

### Dashboard Types

**Executive/Overview**:
- Business KPIs
- System health summary
- Trend indicators
- No technical details

**Service Health**:
- RED metrics (Rate, Errors, Duration)
- Resource utilization
- Dependency status
- Recent deployments

**Debugging**:
- Detailed metrics
- Log panels
- Trace links
- Error breakdowns

### Dashboard Best Practices
```
Layout:
- Most important metrics at top-left
- Group related panels
- Use consistent time ranges
- Include "last updated" indicator

Visualization:
- Use appropriate chart types
- Show thresholds/SLOs on graphs
- Avoid 3D charts
- Use color consistently (red=bad, green=good)

Interactivity:
- Add drill-down links
- Include trace/log links
- Allow time range adjustment
- Support filtering by dimension
```

## SLOs and SLIs

### Definitions
- **SLI** (Service Level Indicator): Measurement of service behavior
- **SLO** (Service Level Objective): Target for SLI
- **SLA** (Service Level Agreement): Contract with consequences

### Common SLIs
| SLI | Calculation | Good SLO |
|-----|-------------|----------|
| **Availability** | Successful requests / Total requests | 99.9% |
| **Latency** | % requests < threshold | 95% < 200ms |
| **Error Rate** | Errors / Total requests | < 0.1% |
| **Throughput** | Requests processed per second | > 1000 RPS |

### Error Budget
```
If SLO = 99.9% availability:
Error budget = 100% - 99.9% = 0.1%

Per month (30 days):
Allowed downtime = 30 * 24 * 60 * 0.001 = 43.2 minutes
```

## Implementation Checklist

### Logging Setup
- [ ] Structured JSON logging configured
- [ ] Appropriate log levels defined
- [ ] Correlation IDs propagated
- [ ] Sensitive data excluded
- [ ] Log aggregation configured
- [ ] Log retention policy set

### Metrics Setup
- [ ] Key business metrics defined
- [ ] RED metrics for services
- [ ] USE metrics for resources
- [ ] Prometheus/metrics endpoint exposed
- [ ] Grafana dashboards created
- [ ] Cardinality limits enforced

### Tracing Setup
- [ ] OpenTelemetry instrumented
- [ ] Context propagation working
- [ ] Key operations traced
- [ ] Sampling strategy defined
- [ ] Trace collector deployed
- [ ] Trace visualization configured

### Alerting Setup
- [ ] Critical alerts defined
- [ ] Runbooks linked
- [ ] On-call rotation set
- [ ] Escalation policy defined
- [ ] Alert testing completed
- [ ] Alert review cadence scheduled
