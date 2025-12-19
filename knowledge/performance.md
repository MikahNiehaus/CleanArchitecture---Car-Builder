# Performance Optimization Knowledge Base

TRIGGER: performance, profiling, benchmark, optimization, bottleneck, latency, throughput, slow, memory leak, load test

## Overview

Performance optimization follows a systematic process: measure, identify, optimize, verify. This knowledge base provides techniques for finding and fixing performance problems across languages and platforms.

## Core Principles

1. **Measure before optimizing**: Never guess at performance problems
2. **Profile in production-like conditions**: Use realistic data and load
3. **Focus on hot paths**: 80% of time is in 20% of code
4. **Optimize the right metric**: Latency vs throughput vs resource usage
5. **Verify improvements**: Measure after every change
6. **Consider trade-offs**: Speed vs memory vs complexity vs maintainability

## Performance Metrics

### Latency Metrics
| Metric | Meaning | Target |
|--------|---------|--------|
| **p50 (median)** | Half of requests faster | User-facing: < 100ms |
| **p90** | 90% of requests faster | User-facing: < 200ms |
| **p99** | 99% of requests faster | User-facing: < 500ms |
| **p99.9** | 99.9% of requests faster | Critical paths: < 1s |
| **Average** | Mean response time | Misleading - don't optimize for this |

### Throughput Metrics
| Metric | Meaning | Considerations |
|--------|---------|----------------|
| **RPS** | Requests per second | Depends on request complexity |
| **TPS** | Transactions per second | Database-focused |
| **QPS** | Queries per second | Search/database systems |
| **Bandwidth** | Data transferred per second | Network-bound systems |

### Resource Metrics
| Metric | Warning Threshold | Critical Threshold |
|--------|-------------------|-------------------|
| **CPU** | > 70% sustained | > 90% |
| **Memory** | > 80% | > 95% |
| **Disk I/O** | > 80% utilization | > 95% |
| **Network** | > 70% bandwidth | > 90% |
| **DB Connections** | > 80% pool | > 95% |

## Profiling Tools by Language

### Python
```
CPU Profiling:
- cProfile (built-in): python -m cProfile -s cumtime script.py
- py-spy: py-spy record -o profile.svg -- python script.py
- line_profiler: @profile decorator for line-by-line

Memory Profiling:
- memory_profiler: @profile decorator
- tracemalloc: Built-in allocation tracking
- objgraph: Object reference visualization

Async Profiling:
- yappi: Supports asyncio/threading
```

### JavaScript/Node.js
```
CPU Profiling:
- Node --inspect + Chrome DevTools
- clinic.js: clinic doctor/flame/bubbleprof
- 0x: Flame graph generation

Memory Profiling:
- --inspect + Chrome DevTools heap snapshots
- memwatch-next: Leak detection
- heapdump: On-demand heap snapshots

Production:
- Node --prof for V8 profiling
- process.memoryUsage() for monitoring
```

### Java
```
CPU Profiling:
- async-profiler: Low-overhead sampling
- JFR (Java Flight Recorder): Built into JDK
- VisualVM: GUI profiler

Memory Profiling:
- jmap: Heap dumps
- MAT (Memory Analyzer Tool): Heap analysis
- JFR: Allocation tracking

GC Analysis:
- -Xlog:gc*: GC logging
- GCViewer: Log visualization
```

### Go
```
CPU Profiling:
- pprof: import _ "net/http/pprof"
- go test -bench -cpuprofile
- go tool pprof (flame graphs)

Memory Profiling:
- pprof heap profiles
- go test -bench -memprofile
- runtime.ReadMemStats()

Tracing:
- go tool trace
- runtime/trace package
```

### Rust
```
CPU Profiling:
- perf + flamegraph
- cargo-flamegraph
- samply

Memory Profiling:
- heaptrack
- valgrind --tool=massif
- DHAT (Valgrind)

Benchmarking:
- criterion.rs
- cargo bench
```

## Bottleneck Types

### CPU-Bound
**Symptoms**:
- High CPU usage (> 80%)
- Latency scales with request complexity
- More CPU cores help

**Common Causes**:
- Inefficient algorithms (O(n²) when O(n log n) possible)
- Excessive string concatenation
- Regex in hot paths
- JSON/XML parsing in loops
- Cryptographic operations

**Solutions**:
- Improve algorithm complexity
- Cache computed results
- Use native/compiled code for hot paths
- Parallelize across cores

### Memory-Bound
**Symptoms**:
- High memory usage
- GC pauses affecting latency
- OOM errors under load

**Common Causes**:
- Memory leaks (unclosed resources, growing caches)
- Large object graphs
- Excessive allocations in loops
- Unbounded caches/queues

**Solutions**:
- Object pooling
- Streaming instead of loading all in memory
- Bounded caches with eviction
- Fix leaks (weak references, proper cleanup)

### I/O-Bound
**Symptoms**:
- High disk/network utilization
- CPU relatively idle
- Latency dominated by external calls

**Common Causes**:
- Synchronous I/O blocking threads
- Many small I/O operations
- Missing connection pooling
- Large file reads/writes

**Solutions**:
- Async I/O
- Batching requests
- Connection pooling
- Caching responses
- Compression

### Database-Bound
**Symptoms**:
- Slow queries in profiler
- High database CPU/I/O
- Lock contention

**Common Causes**:
- Missing indexes
- N+1 query patterns
- Full table scans
- Lock contention
- Too many round trips

**Solutions**:
- Add appropriate indexes
- Eager loading / JOIN optimization
- Query caching
- Read replicas
- Connection pooling
- Batch operations

## Caching Strategies

### Cache Levels
```
L1: Application memory (fastest, limited size)
L2: Local cache (Redis, Memcached on same machine)
L3: Distributed cache (shared Redis cluster)
L4: CDN (for static/semi-static content)
```

### Cache Patterns

**Cache-Aside (Lazy Loading)**:
```
1. Check cache
2. If miss, load from source
3. Store in cache
4. Return data
```
Good for: Read-heavy, tolerates stale data

**Write-Through**:
```
1. Write to cache AND source together
2. Read always from cache
```
Good for: Consistency required, write volume manageable

**Write-Behind**:
```
1. Write to cache immediately
2. Async write to source
```
Good for: Write-heavy, can tolerate eventual consistency

### Cache Invalidation
```
Time-based (TTL): Simple but may serve stale data
Event-based: Complex but always fresh
Version-based: Add version to cache key
```

### Cache Sizing
```
Hit Rate Target: > 90% for most caches
Memory Budget: Don't exceed available RAM
Eviction Policy: LRU for general use, LFU for skewed access patterns
```

## Load Testing

### Test Types
| Type | Purpose | Duration |
|------|---------|----------|
| **Smoke** | Verify system works | Minutes |
| **Load** | Normal expected traffic | 30-60 min |
| **Stress** | Find breaking point | Until failure |
| **Soak** | Find memory leaks | Hours/days |
| **Spike** | Handle sudden traffic | Burst patterns |

### Tools
```
HTTP Load Testing:
- k6 (modern, scriptable)
- Apache JMeter (GUI, enterprise)
- wrk (simple, fast)
- Locust (Python-based)
- Artillery (Node.js)

Database Load:
- pgbench (PostgreSQL)
- mysqlslap (MySQL)
- sysbench (general)
```

### Load Test Design
```
1. Define scenarios (user journeys, not just endpoints)
2. Set realistic think times between requests
3. Use production-like data volumes
4. Ramp up gradually (don't spike immediately)
5. Monitor all components (app, DB, cache, network)
6. Run long enough to see patterns (30+ minutes)
```

## Database Optimization

### Index Strategy
```sql
-- Create indexes for:
-- 1. WHERE clause columns
-- 2. JOIN columns
-- 3. ORDER BY columns
-- 4. Columns with high selectivity

-- Avoid indexes on:
-- 1. Small tables
-- 2. Columns updated frequently
-- 3. Low cardinality columns (boolean, status)
```

### Query Optimization Checklist
- [ ] EXPLAIN/EXPLAIN ANALYZE the query
- [ ] Check for full table scans
- [ ] Verify indexes are being used
- [ ] Look for implicit type conversions
- [ ] Check for N+1 patterns
- [ ] Consider query caching
- [ ] Batch multiple small queries

### Connection Pooling
```
Pool Size Formula:
connections = (core_count * 2) + spindle_count

Example: 8 cores, SSD:
connections = (8 * 2) + 1 = 17

Start conservative, increase based on monitoring
```

## Memory Optimization

### Allocation Reduction
```python
# Bad: Creates new string each iteration
result = ""
for item in items:
    result += str(item)

# Good: Single allocation
result = "".join(str(item) for item in items)
```

### Object Pooling
```python
# Pool reusable objects instead of allocating/deallocating
class ObjectPool:
    def __init__(self, factory, max_size=100):
        self._pool = []
        self._factory = factory
        self._max_size = max_size

    def acquire(self):
        if self._pool:
            return self._pool.pop()
        return self._factory()

    def release(self, obj):
        if len(self._pool) < self._max_size:
            self._pool.append(obj)
```

### Memory Leak Detection
```
Signs of memory leaks:
1. Memory grows over time under constant load
2. GC runs more frequently
3. Eventually OOM or severe slowdown

Common causes:
1. Event listeners not removed
2. Caches without bounds
3. Closures capturing large objects
4. Circular references (in ref-counted languages)
5. Global/static collections growing
```

## Concurrency Optimization

### Thread Pool Sizing
```
CPU-bound work:
threads = number_of_cores

I/O-bound work:
threads = number_of_cores * (1 + wait_time/compute_time)

Example: 8 cores, 100ms wait, 10ms compute:
threads = 8 * (1 + 100/10) = 88 threads
```

### Lock Optimization
```
Strategies:
1. Reduce lock scope (hold briefly)
2. Use read-write locks for read-heavy workloads
3. Lock striping (multiple locks for different data segments)
4. Lock-free data structures where possible
5. Avoid nested locks (deadlock risk)
```

### Async Optimization
```
DO:
- Use async for I/O operations
- Batch concurrent requests
- Use connection pooling

DON'T:
- Use async for CPU-bound work
- Block in async code
- Create unbounded concurrency
```

## Quick Reference: Optimization Checklist

### Before Optimizing
- [ ] Defined performance requirements (SLOs)
- [ ] Established baseline metrics
- [ ] Identified bottleneck with profiling data
- [ ] Quantified expected improvement

### During Optimization
- [ ] Changed one thing at a time
- [ ] Measured after each change
- [ ] Verified no regressions in other areas
- [ ] Documented what was changed and why

### After Optimizing
- [ ] Confirmed improvement meets requirements
- [ ] Added performance tests to prevent regression
- [ ] Updated documentation
- [ ] Shared learnings with team

## Anti-Patterns to Avoid

### Premature Optimization
- Optimizing without profiling data
- Micro-optimizing rarely-executed code
- Sacrificing readability for marginal gains

### Over-Caching
- Caching everything "just in case"
- Not considering cache invalidation
- Unbounded cache growth

### Incorrect Parallelization
- Parallelizing CPU-bound work beyond core count
- Creating thread per request
- Ignoring thread pool saturation

### Ignoring Tail Latency
- Only measuring averages
- Not monitoring p99/p99.9
- Missing timeout configurations
