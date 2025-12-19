# File Editing on Windows - Known Issues and Workarounds

TRIGGER: file edit, write file, unexpectedly modified, edit error, windows

## Known Bug: "File has been unexpectedly modified"

This is a critical Windows-specific bug in Claude Code (tracked in GitHub issues #7443, #7457, #10437, #12462, #12805).

### Symptoms
- Edit/Write tools fail with "File has been unexpectedly modified"
- Error occurs even immediately after reading the file
- The file was NOT actually modified by any external process
- More common with absolute paths

### Root Causes (Suspected)
1. Windows/MINGW file system timestamp resolution differences
2. Line ending conversion (CRLF vs LF) detected as modification
3. File hash/tracking cache not persisting correctly
4. VSCode background processes (formatters, linters, file watchers)

---

## Workarounds (In Order of Preference)

### 1. Use Relative Paths (Primary Workaround)

**ALWAYS use relative paths**, not absolute paths:

```
CORRECT: ./src/file.ts
CORRECT: agents/_shared-output.md
WRONG:   C:/prj/ClaudeMemory/agents/_shared-output.md
```

Add to agent prompts or CLAUDE.md:
```
When editing files, ALWAYS use relative paths (e.g., ./src/file.ts).
DO NOT use absolute paths (e.g., C:/Users/project/src/file.ts).
```

### 2. Retry Pattern

If first attempt fails:
1. Wait 1-2 seconds
2. Read file again
3. Attempt edit immediately after read
4. If fails again, try workaround #3

### 3. Use Bash Commands Instead of Edit Tool

When Edit tool fails repeatedly, fall back to Bash:

**For simple replacements:**
```bash
sed -i 's/old_string/new_string/g' file.txt
```

**For appending content:**
```bash
cat >> file.txt << 'EOF'
New content here
EOF
```

**For full file replacement (Python):**
```bash
python -c "
content = '''
Your file content here
'''
with open('file.txt', 'w', encoding='utf-8') as f:
    f.write(content)
"
```

### 4. Create New File with Different Name

If editing existing file fails:
1. Create new file with different name
2. Write full content to new file
3. Delete old file (using Bash: `rm old_file.md`)
4. Rename new file (using Bash: `mv new_file.md old_file.md`)

### 5. Restart Claude Code

If all else fails:
- Suggest user restart Claude Code
- This temporarily resolves the issue
- Note: This loses conversation context

---

## Agent Guidelines

### When Encountering This Error

1. **First attempt**: Try with relative path
2. **Second attempt**: Read file, immediately edit, don't wait
3. **Third attempt**: Use Bash workaround
4. **If all fail**:
   - Report to user
   - Suggest restart
   - Offer to write to new file instead

### Preventive Measures

1. **Always use relative paths** in file operations
2. **Don't batch multiple reads** before edits
3. **Edit immediately** after reading
4. **For critical files**: Consider the "new file + rename" pattern

---

## Rule Addition for CLAUDE.md

Add this rule to handle the issue:

```markdown
### RULE-011: Windows File Edit Resilience
- **ID**: RULE-011
- **TRIGGER**: When Edit/Write tool fails with "unexpectedly modified" error
- **CONDITION**: On Windows platform
- **ACTION**:
  1. Retry with relative path
  2. If fails, use Bash/sed workaround
  3. If fails, report to user
- **SEVERITY**: WARN

**Workaround Priority**:
1. Use relative paths (./path/file.ext)
2. Read immediately before edit
3. Fall back to Bash commands
4. Create new file + rename
```

---

## References

- [GitHub Issue #7443](https://github.com/anthropics/claude-code/issues/7443) - Original detailed report
- [GitHub Issue #12805](https://github.com/anthropics/claude-code/issues/12805) - Windows/MINGW specific
- [Medium Workaround Article](https://medium.com/@yunjeongiya/the-elusive-claude-file-has-been-unexpectedly-modified-bug-a-workaround-solution-831182038d1d)
