# Completion Verification Methodology

TRIGGER: completion, verify, done, criteria, persistent mode, finish, complete task

## Overview

This knowledge base defines how to verify task completion, especially for PERSISTENT mode tasks that must continue until explicit criteria are met.

## Verification Types

### 1. Count-Based Verification
For "all X" type goals where completion means zero items remaining.

**Pattern**: Count before → Process → Count after → Complete when count = 0

**Examples**:
```bash
# Convert all .js files to .ts
# Before: Count .js files
find . -name "*.js" -not -path "./node_modules/*" | wc -l  # e.g., 45

# After each iteration: Verify progress
find . -name "*.js" -not -path "./node_modules/*" | wc -l  # Should decrease
find . -name "*.ts" -not -path "./node_modules/*" | wc -l  # Should increase

# Complete when: .js count = 0
```

```bash
# Fix all lint errors
# Before: Count errors
npm run lint 2>&1 | grep -c "error"  # e.g., 23

# Complete when: error count = 0
npm run lint && echo "PASS" || echo "FAIL"
```

### 2. Threshold-Based Verification
For "until X%" type goals where completion means meeting a metric threshold.

**Pattern**: Measure baseline → Process → Re-measure → Complete when metric >= threshold

**Examples**:
```bash
# Test coverage >= 90%
# Measure current coverage
npm run coverage -- --json 2>/dev/null | jq '.total.lines.pct'

# Complete when: coverage >= 90
```

```bash
# Performance improvement >= 2x
# Baseline measurement
time npm run benchmark 2>&1 | grep "total"

# Complete when: new_time <= baseline_time / 2
```

### 3. State-Based Verification
For "make X true" type goals where completion means a command succeeds.

**Pattern**: Define success command → Process → Run command → Complete when exit code = 0

**Examples**:
```bash
# No TypeScript errors
npx tsc --noEmit
# Exit code 0 = complete, non-zero = continue

# All tests pass
npm test
# Exit code 0 = complete

# Build succeeds
npm run build
# Exit code 0 = complete
```

### 4. Composite Verification
For complex goals requiring multiple criteria.

**Pattern**: Define all criteria → All must pass → Complete when ALL met

**Example**:
```markdown
| # | Criterion | Command | Threshold | Status |
|---|-----------|---------|-----------|--------|
| 1 | All files converted | find . -name "*.js" \| wc -l | = 0 | pending |
| 2 | TypeScript compiles | npx tsc --noEmit | exit 0 | pending |
| 3 | Tests pass | npm test | exit 0 | pending |
| 4 | No new lint errors | npm run lint | exit 0 | pending |

Complete when: ALL criteria status = "met"
```

---

## Verification Protocol

### Before Starting Task (PERSISTENT Mode)

1. **Parse completion criteria from user request**:
   - Extract explicit criteria: "all files", "90% coverage", "no errors"
   - Identify implicit criteria: code compiles, tests pass

2. **Define verification command for each criterion**:
   - Must be deterministic (same input → same output)
   - Must return measurable result (count, percentage, exit code)
   - Must be runnable without side effects

3. **Run baseline verification**:
   - Establish starting point (e.g., "45 files to convert")
   - Record in context.md Completion Criteria table

4. **Store in context.md**:
   ```markdown
   ### Completion Criteria
   | # | Criterion | Verification Command | Threshold | Status |
   |---|-----------|---------------------|-----------|--------|
   | 1 | All .js -> .ts | find . -name "*.js" \| wc -l | = 0 | pending (baseline: 45) |
   ```

### After Each Iteration

1. **Run all verification commands**
2. **Compare against thresholds**
3. **Update status in context.md**:
   - `pending` → not yet checked
   - `met` → criterion satisfied
   - `failed` → criterion checked, not satisfied
   - `error` → verification command failed

4. **Decision logic**:
   ```
   If ALL criteria status = "met":
       → Mark task COMPLETE
   Else if ANY criterion status = "error":
       → Mark task BLOCKED (verification failed)
   Else:
       → Continue processing (if not BLOCKED)
   ```

### On Resume (After Compaction)

1. **Read completion criteria from context.md**
2. **Run ALL verification commands** (don't trust stored status):
   - Filesystem may have changed
   - Need current state, not cached state
3. **Compare with stored progress**:
   - If actual < stored: Something reverted, investigate
   - If actual > stored: Progress made outside session
   - If actual = stored: Resume normally
4. **Continue from documented "Next Item"**

---

## Common Verification Commands

### File Operations
```bash
# Count files by extension
find . -name "*.ext" -not -path "./node_modules/*" | wc -l

# Count files matching pattern
find . -type f -name "pattern*" | wc -l

# Check file exists
test -f "path/to/file" && echo "exists" || echo "missing"

# Check directory exists
test -d "path/to/dir" && echo "exists" || echo "missing"
```

### Code Quality
```bash
# TypeScript compilation check
npx tsc --noEmit; echo "Exit: $?"

# ESLint error count
npm run lint 2>&1 | grep -c "error" || echo "0"

# Test suite
npm test; echo "Exit: $?"

# Test coverage percentage
npm run coverage -- --json | jq '.total.lines.pct'
```

### Build & Deploy
```bash
# Build succeeds
npm run build; echo "Exit: $?"

# Docker build succeeds
docker build -t test . ; echo "Exit: $?"

# No console.log statements (example pattern check)
grep -r "console.log" src/ --include="*.ts" | wc -l
```

### Git Operations
```bash
# No uncommitted changes
git status --porcelain | wc -l  # 0 = clean

# On specific branch
git branch --show-current  # Compare to expected

# All changes committed
git diff --cached --stat | wc -l  # 0 = nothing staged
```

---

## Implicit Criteria (Always Check)

These criteria should be verified for ANY task, even if not explicitly requested:

| Criterion | Why | Verification |
|-----------|-----|--------------|
| Code compiles | Broken code is useless | Build command exits 0 |
| Tests pass | No regressions introduced | Test command exits 0 |
| No new lint errors | Code quality maintained | Lint command exits 0 |
| No secrets exposed | Security requirement | grep for API keys, passwords |

### Adding Implicit Criteria
When defining completion criteria, ALWAYS add:
```markdown
### Implicit Criteria (auto-added)
| # | Criterion | Verification | Status |
|---|-----------|--------------|--------|
| I1 | Code compiles | [build command] | pending |
| I2 | Tests pass | [test command] | pending |
| I3 | No lint errors | [lint command] | pending |
```

---

## Checkpoint Protocol

For long-running PERSISTENT tasks, checkpoint to prevent token exhaustion.

### When to Checkpoint
- Every N items (default: 10)
- Before any operation that might consume >10K tokens
- When token usage approaches ~75% capacity
- After completing a logical phase

### What to Save at Checkpoint
1. **Progress state**:
   - Items completed / total
   - Last processed item
   - Next item to process

2. **Verification state**:
   - Current count for each criterion
   - Timestamp of last verification

3. **Context state**:
   - Quick Resume updated
   - Agent Contributions logged

### Checkpoint Format in context.md
```markdown
### Last Checkpoint - [Timestamp]
- **Progress**: 23/45 items
- **Last Item**: src/utils/auth.js
- **Next Item**: src/utils/crypto.js
- **Criteria Status**: [1] 22 remaining [2] Not checked [3] Not checked
- **Token Estimate**: ~50K used
```

---

## Anti-Premature-Completion Protocol

### Before Saying "Done" to User

STOP and verify:

1. **Check explicit criteria**:
   - Run ALL verification commands
   - ALL must return "met" status

2. **Check implicit criteria**:
   - Build passes?
   - Tests pass?
   - No new lint errors?

3. **Check task mode**:
   - PERSISTENT mode: ALL completion criteria met?
   - NORMAL mode: Current step complete?

4. **Check todo list**:
   - All TodoWrite items marked complete?

### If Verification Fails

DO NOT tell user "done". Instead:
1. Report what IS complete
2. Report what is NOT complete
3. If PERSISTENT mode: Continue automatically
4. If NORMAL mode: Ask user if they want to continue

### Completion Statement Format (PERSISTENT Mode)

Only after ALL criteria verified:
```markdown
## Task Complete

All completion criteria have been verified:
| # | Criterion | Final Value | Threshold | Status |
|---|-----------|-------------|-----------|--------|
| 1 | All .js converted | 0 files | = 0 | MET |
| 2 | TypeScript compiles | exit 0 | exit 0 | MET |
| 3 | Tests pass | exit 0 | exit 0 | MET |

Total items processed: 45
Time elapsed: [calculated from timestamps]
```

---

## Error Handling

### Verification Command Fails
```
Criterion: Test coverage >= 90%
Command: npm run coverage -- --json
Result: Command not found

Action:
1. Mark criterion as "error"
2. Log error in context.md
3. DO NOT mark task complete
4. Report to user: "Cannot verify criterion - command failed"
```

### Infinite Loop Detection
If same item processed 3+ times:
1. Mark as BLOCKED
2. Log pattern in context.md
3. Ask user for guidance

### Threshold Never Met
If 10+ iterations with no progress toward threshold:
1. Report current state
2. Ask user: "Progress stalled at X%. Continue or adjust threshold?"

---

## Integration with Execution Mode

### NORMAL Mode (Default)
- Verify only implicit criteria (build, test, lint)
- Single verification at end of step
- Report and stop

### PERSISTENT Mode
- Verify ALL criteria (explicit + implicit)
- Continuous verification after each iteration
- Auto-continue until all criteria met
- Checkpoint every N items
- Resume automatically after compaction
