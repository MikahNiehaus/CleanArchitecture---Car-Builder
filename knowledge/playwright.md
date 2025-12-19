# Browser Testing with Playwright MCP

TRIGGER: browser, playwright, interactive, e2e, end-to-end, click, navigate, test app, manual test

---

## CRITICAL: DEFAULT BEHAVIOR

**ALWAYS use interactive mode (MCP tools) for ANY browser/Playwright request.**

Unless the user explicitly says "write a test script" or "generate Playwright code", ALWAYS use the MCP tools directly. This is non-negotiable.

| User Says | Action |
|-----------|--------|
| "Open google" | Use `mcp__playwright__browser_navigate` |
| "Test my app" | Use MCP tools interactively |
| "Click the button" | Use `mcp__playwright__browser_click` |
| "Check the login flow" | Use MCP tools to navigate and interact |
| "Write a Playwright test" | ONLY THEN write code |
| "Generate test automation" | ONLY THEN write code |

**When in doubt: USE MCP TOOLS, NOT CODE.**

---

## KNOWN FAILURES & LESSONS LEARNED

### Failure Log (Session 2025-12-12)

These are actual mistakes made that MUST NOT be repeated:

#### Failure 1: Wrong Package Name
```
WRONG: claude mcp add playwright -- npx @anthropic-ai/mcp-playwright@latest
ERROR: npm error code E404 - package does not exist
```

**The package `@anthropic-ai/mcp-playwright` does not exist.** This was a hallucinated package name.

#### Failure 2: Correct Package
```
CORRECT: claude mcp add playwright -- npx @playwright/mcp@latest
```

The official package is `@playwright/mcp` maintained by **Microsoft** at [github.com/microsoft/playwright-mcp](https://github.com/microsoft/playwright-mcp).

#### Failure 3: Not Knowing Restart Required
MCP servers are loaded at Claude Code startup. After adding an MCP server, **you MUST restart Claude Code** for the tools to become available. Simply running `claude mcp add` is not enough.

#### Failure 4: Not Checking MCP Health
Before attempting to use Playwright tools, ALWAYS verify the server is connected:
```bash
claude mcp list
# Look for: playwright: npx @playwright/mcp@latest - ✓ Connected
# NOT: ✗ Failed to connect
```

### Package Name Reference

| Status | Package Name | Notes |
|--------|--------------|-------|
| **CORRECT** | `@playwright/mcp` | Official Microsoft package |
| WRONG | `@anthropic-ai/mcp-playwright` | Does not exist |
| WRONG | `playwright-mcp` | Different unofficial package |
| WRONG | `@executeautomation/playwright-mcp-server` | Third-party, not official |
| WRONG | `mcp-playwright` | Different package |

---

## Overview

Interactive browser testing using Playwright MCP tools. This enables real-time browser control for testing applications - navigating pages, clicking elements, filling forms, and observing results in a visible browser window.

**Key Distinction**: This is for INTERACTIVE testing (you control the browser in real-time), NOT for writing automated test scripts (that's `test-agent` with `knowledge/testing.md`).

---

## MCP Setup

### Step 1: Install Playwright MCP

**Standard Installation:**
```bash
claude mcp add playwright -- npx @playwright/mcp@latest
```

**With Headless Mode** (no visible browser window):
```bash
claude mcp add playwright -- npx @playwright/mcp@latest --headless
```

**With Specific Browser:**
```bash
claude mcp add playwright -- npx @playwright/mcp@latest --browser chromium
claude mcp add playwright -- npx @playwright/mcp@latest --browser firefox
claude mcp add playwright -- npx @playwright/mcp@latest --browser webkit
```

**With Device Emulation:**
```bash
claude mcp add playwright -- npx @playwright/mcp@latest --device "iPhone 15"
```

**With Persistent User Data** (keeps cookies/logins between sessions):
```bash
claude mcp add playwright -- npx @playwright/mcp@latest --user-data-dir ./browser-data
```

**With Custom Viewport:**
```bash
claude mcp add playwright -- npx @playwright/mcp@latest --viewport-size 1920x1080
```

### Step 2: Restart Claude Code

**CRITICAL: MCP servers load at startup. You MUST restart Claude Code after installation.**

### Step 3: Verify Installation

```bash
claude mcp list
```

Expected output:
```
playwright: npx @playwright/mcp@latest - ✓ Connected
```

If you see `✗ Failed to connect`:
1. Check the package name is exactly `@playwright/mcp@latest`
2. Ensure Node.js 18+ is installed
3. Try: `npx @playwright/mcp@latest --help` to test manually
4. Run `npx playwright install` to install browser binaries

### Step 4: Confirm Tools Available

Run `/mcp` in Claude Code. You should see these tools:
- `mcp__playwright__browser_navigate`
- `mcp__playwright__browser_click`
- `mcp__playwright__browser_type`
- `mcp__playwright__browser_snapshot`
- `mcp__playwright__browser_take_screenshot`
- `mcp__playwright__browser_close`
- And more...

---

## All CLI Options Reference

| Option | Description | Example |
|--------|-------------|---------|
| `--browser <type>` | Browser: chromium, firefox, webkit, msedge | `--browser firefox` |
| `--headless` | Run without visible window | `--headless` |
| `--viewport-size <WxH>` | Window dimensions | `--viewport-size 1280x720` |
| `--device <name>` | Emulate device | `--device "iPhone 15"` |
| `--user-data-dir <path>` | Persistent browser profile | `--user-data-dir ./data` |
| `--storage-state <file>` | Load cookies/storage from JSON | `--storage-state auth.json` |
| `--isolated` | Fresh profile each session | `--isolated` |
| `--user-agent <string>` | Custom user agent | `--user-agent "MyBot/1.0"` |
| `--timeout-action <ms>` | Action timeout (default 5000) | `--timeout-action 10000` |
| `--timeout-navigation <ms>` | Nav timeout (default 60000) | `--timeout-navigation 30000` |
| `--proxy-server <url>` | Use proxy | `--proxy-server http://proxy:3128` |
| `--save-video <WxH>` | Record session video | `--save-video 800x600` |
| `--save-trace` | Save Playwright trace | `--save-trace` |
| `--output-dir <path>` | Output directory | `--output-dir ./recordings` |
| `--allowed-origins <list>` | Allow specific origins (;-separated) | `--allowed-origins "localhost;127.0.0.1"` |
| `--blocked-origins <list>` | Block specific origins | `--blocked-origins "ads.example.com"` |

---

## JSON Configuration (Alternative)

Instead of CLI args, use a config file:

```json
{
  "mcpServers": {
    "playwright": {
      "command": "npx",
      "args": ["@playwright/mcp@latest"]
    }
  }
}
```

With options:
```json
{
  "mcpServers": {
    "playwright": {
      "command": "npx",
      "args": [
        "@playwright/mcp@latest",
        "--headless",
        "--browser", "chromium",
        "--viewport-size", "1920x1080"
      ]
    }
  }
}
```

---

## CRITICAL RULES

### Rule 1: ALWAYS USE MCP TOOLS (DEFAULT BEHAVIOR)

| DO | DON'T |
|----|-------|
| Use `mcp__playwright__browser_navigate` | Write Playwright scripts |
| Use `mcp__playwright__browser_click` | Use Bash to run `npx playwright` |
| Use `mcp__playwright__browser_type` | Create `.spec.ts` files |
| Use `mcp__playwright__browser_snapshot` | Generate automation code |

**This is the DEFAULT. Only write code if user explicitly asks for it.**

### Rule 2: URL ACCESS POLICY

| URL Type | Action | Examples |
|----------|--------|----------|
| **Localhost** | AUTO-ALLOW | `localhost:*`, `127.0.0.1:*`, `*.localhost`, `[::1]` |
| **OAuth/Auth providers** | AUTO-ALLOW | See list below |
| **Other external URLs** | ASK USER | `google.com`, production sites |
| **User explicitly allows** | ALLOW | Any URL user says is okay |

**OAuth/Auth Auto-Allow List**:
- Microsoft: `*.b2clogin.com`, `login.microsoftonline.com`, `login.live.com`
- Google: `accounts.google.com`
- Auth0: `*.auth0.com`
- Okta: `*.okta.com`, `*.oktapreview.com`
- GitHub: `github.com/login/oauth`
- AWS Cognito: `*.auth.*.amazoncognito.com`
- Firebase: `*.firebaseapp.com`
- Keycloak: Common patterns for self-hosted

---

## Essential Tools

| Tool | Purpose | When to Use |
|------|---------|-------------|
| `browser_navigate` | Go to URL | Start of any test flow |
| `browser_snapshot` | Get page state as accessibility tree | Before clicking, to see elements |
| `browser_click` | Click element | Buttons, links, form controls |
| `browser_type` | Enter text | Form inputs, search boxes |
| `browser_take_screenshot` | Visual capture | Document findings, debug issues |
| `browser_close` | End session | When done testing |
| `browser_fill_form` | Fill multiple fields at once | Complex forms |
| `browser_select_option` | Select dropdown option | Select/combobox elements |
| `browser_hover` | Hover over element | Tooltips, dropdown menus |
| `browser_press_key` | Press keyboard key | Enter, Escape, Tab, etc. |
| `browser_wait_for` | Wait for text/element | Dynamic content loading |

---

## Troubleshooting

### Problem: Wrong package name used

**Symptoms**: `npm error code E404`, package not found

**Solution**: Use the correct package name:
```bash
# Remove wrong one
claude mcp remove playwright

# Add correct one
claude mcp add playwright -- npx @playwright/mcp@latest
```

### Problem: MCP tools not appearing after install

**Symptoms**: Tools like `mcp__playwright__browser_navigate` don't show up

**Solutions**:
1. **Restart Claude Code** - MCP servers load at startup
2. Check server health: `claude mcp list`
3. Look for `✓ Connected` not `✗ Failed to connect`

### Problem: Server fails to connect

**Symptoms**: `claude mcp list` shows `✗ Failed to connect`

**Solutions**:
1. Verify Node.js 18+ is installed: `node --version`
2. Test package directly: `npx @playwright/mcp@latest --help`
3. Install browsers: `npx playwright install`
4. Check for typos in package name

### Problem: Browser doesn't open

**Solutions**:
1. Ensure Playwright browsers are installed: `npx playwright install`
2. Remove `--headless` flag if you want visible browser
3. Check for port conflicts
4. Try specifying browser: `--browser chromium`

### Problem: Claude writes code instead of using tools

**Symptoms**: Claude generates `.spec.ts` files or Playwright scripts

**Solutions**:
1. This knowledge base says ALWAYS use MCP tools by default
2. If Claude writes code, interrupt and say "use MCP tools"
3. The only exception is if user explicitly asks for code

### Problem: Can't find elements to click

**Solutions**:
1. Always take a `browser_snapshot` first
2. The snapshot shows all interactive elements with `[ref=...]` IDs
3. Use those ref IDs in your click/type commands

---

## Browser Profile Locations

Playwright stores persistent profiles at:

| OS | Location |
|----|----------|
| Windows | `%USERPROFILE%\AppData\Local\ms-playwright\mcp-{channel}-profile` |
| macOS | `~/Library/Caches/ms-playwright/mcp-{channel}-profile` |
| Linux | `~/.cache/ms-playwright/mcp-{channel}-profile` |

---

## Session Lifecycle

### Starting a Session
1. Verify MCP is installed and connected
2. Use `browser_navigate` to open URL
3. Use `browser_snapshot` to see page state

### During Session
- Snapshots show elements with `[ref=eXX]` identifiers
- Use those refs for clicking: `ref="e42"`
- Take screenshots to document findings

### Ending a Session
- Use `browser_close` to close browser cleanly
- Report summary of what was tested

---

## Integration with Other Agents

| Scenario | Hand Off To |
|----------|-------------|
| Found a bug during testing | `debug-agent` for root cause analysis |
| Flow works, need automated tests | `test-agent` for regression tests |
| Security concern discovered | `security-agent` for assessment |
| UI looks wrong | `ui-agent` for implementation fix |
| Performance issue observed | `performance-agent` for profiling |

---

## Quick Reference Card

```
INSTALL:   claude mcp add playwright -- npx @playwright/mcp@latest
VERIFY:    claude mcp list
RESTART:   Required after install!

PACKAGE:   @playwright/mcp (Microsoft official)
NOT:       @anthropic-ai/mcp-playwright (doesn't exist)

DEFAULT:   ALWAYS use MCP tools interactively
EXCEPTION: Only write code if user explicitly asks

TOOLS:     browser_navigate, browser_snapshot, browser_click,
           browser_type, browser_take_screenshot, browser_close
```
