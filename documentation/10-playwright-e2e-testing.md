# End-to-End Testing with Playwright MCP

## Table of Contents
1. [Overview](#overview)
2. [Playwright MCP Introduction](#playwright-mcp-introduction)
3. [Setup and Configuration](#setup-and-configuration)
4. [E2E Testing Best Practices (2025)](#e2e-testing-best-practices-2025)
5. [React + TypeScript Testing Patterns](#react--typescript-testing-patterns)
6. [JWT Authentication Testing](#jwt-authentication-testing)
7. [Page Object Model Implementation](#page-object-model-implementation)
8. [Car Builder Test Strategy](#car-builder-test-strategy)
9. [Running Tests](#running-tests)
10. [Debugging and Troubleshooting](#debugging-and-troubleshooting)

---

## Overview

End-to-end (E2E) testing validates the entire application flow from the user's perspective, ensuring all integrated components work together correctly. This guide covers modern E2E testing using Playwright with the Model Context Protocol (MCP) for AI-powered browser automation.

### Why E2E Testing?

- **User Perspective**: Tests real user workflows, not isolated components
- **Integration Validation**: Ensures frontend, backend, and database work together
- **Regression Prevention**: Catches breaking changes across the entire stack
- **Confidence**: Provides high confidence before deploying to production
- **Documentation**: Serves as executable specifications of user flows

### Testing Pyramid for Car Builder

```
        /\
       /E2E\          <- Playwright (Critical user flows)
      /------\
     /Integration\    <- API tests, Component tests
    /------------\
   /  Unit Tests  \   <- Jest/Vitest (Business logic)
  /----------------\
```

---

## Playwright MCP Introduction

### What is Playwright MCP?

**Playwright MCP** is a Model Context Protocol server that enables AI assistants (like Claude) to control browsers using Playwright. It provides structured browser automation through accessibility trees rather than screenshots.

### Key Features

1. **Accessibility Tree Based**
   - Uses structured text representation of web pages
   - No need for visual models or screenshots
   - Fast and deterministic

2. **AI-Powered Automation**
   - Natural language test descriptions
   - Intelligent element detection
   - Self-healing locators

3. **Interactive Mode**
   - Headed browser by default (visible window)
   - Real-time debugging
   - Step-by-step execution

4. **Cross-Browser Support**
   - Chromium (default)
   - Firefox
   - WebKit (Safari)

### How It Works

```
┌─────────────┐      ┌──────────────┐      ┌─────────────┐
│   Claude    │ MCP  │  Playwright  │      │   Browser   │
│   Code      ├─────>│  MCP Server  ├─────>│  (Chrome)   │
│             │      │              │      │             │
└─────────────┘      └──────────────┘      └─────────────┘
     │                      │                      │
     │  Test Instructions   │  Browser Commands    │
     │─────────────────────>│─────────────────────>│
     │                      │                      │
     │  Test Results        │  Accessibility Tree  │
     │<─────────────────────│<─────────────────────│
```

### Advantages Over Traditional Testing

| Traditional Playwright | Playwright MCP |
|------------------------|----------------|
| Manual script writing  | AI-assisted test generation |
| Fixed locator strategies | Adaptive element detection |
| Screenshot-based debugging | Accessibility tree inspection |
| Requires coding expertise | Natural language descriptions |

---

## Setup and Configuration

### Prerequisites

- Node.js 18+ and npm
- Claude Code (Claude Desktop)
- Running Car Builder application (API + Frontend)
- Docker Desktop (for SQL Server)

### 1. Configure Playwright MCP in Claude Code

**Location**: Claude Code configuration file
- Windows: `%APPDATA%\Claude\claude_desktop_config.json`
- Mac: `~/Library/Application Support/Claude/claude_desktop_config.json`

**Configuration for Headed Mode** (Recommended for interactive testing):

```json
{
  "mcpServers": {
    "playwright": {
      "command": "npx",
      "args": [
        "-y",
        "@playwright/mcp@latest"
      ]
    }
  }
}
```

**Configuration for Headless Mode** (CI/CD):

```json
{
  "mcpServers": {
    "playwright": {
      "command": "npx",
      "args": [
        "-y",
        "@playwright/mcp@latest",
        "--headless"
      ]
    }
  }
}
```

**Configuration with Browser Extension** (Reuse logged-in sessions):

```json
{
  "mcpServers": {
    "playwright": {
      "command": "npx",
      "args": [
        "-y",
        "@playwright/mcp@latest",
        "--extension"
      ]
    }
  }
}
```

**Note**: For the `--extension` flag, you need to install the "Playwright MCP Bridge" extension in Chrome/Edge.

### 2. Restart Claude Code

After updating the configuration, restart Claude Code to load the Playwright MCP server.

### 3. Verify MCP Connection

Once configured, Claude Code will have access to MCP tools:
- `mcp__playwright_navigate` - Navigate to URLs
- `mcp__playwright_screenshot` - Capture screenshots
- `mcp__playwright_click` - Click elements
- `mcp__playwright_fill` - Fill form fields
- `mcp__playwright_select` - Select dropdown options
- And more...

---

## E2E Testing Best Practices (2025)

### 1. Test Isolation

**Principle**: Each test should be completely isolated and run independently.

```typescript
// ✅ GOOD: Isolated test with own data
test('create car', async () => {
  const uniqueCar = {
    make: 'Toyota',
    model: `Camry-${Date.now()}`, // Unique identifier
    year: 2024,
    price: 25000
  };

  await createCar(uniqueCar);
  await expect(page.getByText(uniqueCar.model)).toBeVisible();
});

// ❌ BAD: Tests depend on each other
test('create car', async () => {
  await createCar(testCar); // Creates global testCar
});

test('edit car', async () => {
  await editCar(testCar); // Depends on previous test
});
```

**Benefits:**
- Tests can run in any order
- Parallel execution possible
- Easier debugging (no cascading failures)
- More reliable CI/CD pipelines

### 2. Use Resilient Locators

**Locator Priority** (from most to least resilient):

1. **Role-based** (mirrors user/assistive technology)
2. **Label text** (visible to users)
3. **Placeholder text** (form fields)
4. **Test IDs** (stable, semantic)
5. **CSS selectors** (fragile, avoid)

```typescript
// ✅ BEST: Role-based locators
await page.getByRole('button', { name: 'Sign in' }).click();
await page.getByRole('textbox', { name: 'Email' }).fill('user@example.com');

// ✅ GOOD: Label text
await page.getByLabel('Password').fill('password123');

// ✅ GOOD: Placeholder
await page.getByPlaceholder('Enter your email').fill('user@example.com');

// ✅ ACCEPTABLE: Test IDs
await page.getByTestId('submit-button').click();

// ❌ AVOID: CSS selectors
await page.locator('.btn-primary').click(); // Breaks when CSS changes
await page.locator('#login-form > div:nth-child(2) > button').click(); // Very fragile
```

### 3. Leverage Auto-Wait Capabilities

Playwright automatically waits for elements to be actionable before performing actions. **No need for arbitrary sleeps!**

```typescript
// ✅ GOOD: Playwright auto-waits
await page.getByRole('button', { name: 'Submit' }).click();
// Automatically waits for button to be:
// - Attached to DOM
// - Visible
// - Stable (not animating)
// - Enabled
// - Receiving events

// ❌ BAD: Arbitrary timeouts
await page.waitForTimeout(3000); // Flaky and slow
await page.getByRole('button').click();
```

**Explicit Waits** (when needed):

```typescript
// Wait for navigation
await page.waitForURL('**/dashboard');

// Wait for API response
await page.waitForResponse(resp =>
  resp.url().includes('/api/cars') && resp.status() === 200
);

// Wait for element state
await page.getByRole('button', { name: 'Loading...' }).waitFor({ state: 'hidden' });
```

### 4. Authentication State Management

**Problem**: Logging in for every test is slow and creates unnecessary load.

**Solution**: Authenticate once, reuse state across tests.

**Global Setup** (`global-setup.ts`):

```typescript
import { chromium, FullConfig } from '@playwright/test';

async function globalSetup(config: FullConfig) {
  const browser = await chromium.launch();
  const page = await browser.newPage();

  // Navigate to login
  await page.goto('http://localhost:5173/login');

  // Fill credentials
  await page.getByLabel('Email').fill('test@example.com');
  await page.getByLabel('Password').fill('Test123!@#');

  // Submit and wait for redirect
  await page.getByRole('button', { name: 'Sign in' }).click();
  await page.waitForURL('**/');

  // Save authenticated state
  await page.context().storageState({ path: 'auth.json' });

  await browser.close();
}

export default globalSetup;
```

**Reuse in Tests**:

```typescript
import { test } from '@playwright/test';

test.use({ storageState: 'auth.json' });

test('create car while authenticated', async ({ page }) => {
  await page.goto('http://localhost:5173');
  // Already logged in!
  await page.getByRole('link', { name: 'Add New Car' }).click();
  // ...
});
```

**Benefits:**
- 10-100x faster test execution
- Reduced server load
- Still maintains test isolation (each test gets fresh page context)

### 5. Page Object Model (POM)

**Principle**: Encapsulate page interactions in reusable classes.

```typescript
// pages/LoginPage.ts
export class LoginPage {
  constructor(private page: Page) {}

  async navigate() {
    await this.page.goto('http://localhost:5173/login');
  }

  async login(email: string, password: string) {
    await this.page.getByLabel('Email').fill(email);
    await this.page.getByLabel('Password').fill(password);
    await this.page.getByRole('button', { name: 'Sign in' }).click();
    await this.page.waitForURL('**/');
  }

  async getErrorMessage() {
    return await this.page.getByRole('alert').textContent();
  }
}

// Test using POM
test('login with valid credentials', async ({ page }) => {
  const loginPage = new LoginPage(page);

  await loginPage.navigate();
  await loginPage.login('user@example.com', 'Password123!');

  await expect(page).toHaveURL(/\//);
});
```

**Benefits:**
- **DRY**: Don't repeat selectors across tests
- **Maintainability**: Change selectors in one place
- **Readability**: Tests read like user stories
- **Reusability**: Share page objects across test suites

### 6. Test Data Management

```typescript
// helpers/test-data.ts
export const testUsers = {
  admin: {
    email: 'admin@carbuilder.com',
    password: 'Admin123!@#',
    firstName: 'Admin',
    lastName: 'User'
  },
  regularUser: {
    email: 'user@carbuilder.com',
    password: 'User123!@#',
    firstName: 'Regular',
    lastName: 'User'
  }
};

export const generateUniqueCar = () => ({
  make: 'Toyota',
  model: `Camry-${Date.now()}`,
  year: 2024,
  price: Math.floor(Math.random() * 50000) + 20000,
  description: 'Test car for E2E testing'
});
```

### 7. Error Handling and Debugging

```typescript
test('create car with screenshot on failure', async ({ page }) => {
  try {
    await createCar(page, testCarData);
  } catch (error) {
    // Capture screenshot on failure
    await page.screenshot({
      path: `screenshots/create-car-failure-${Date.now()}.png`,
      fullPage: true
    });
    throw error;
  }
});
```

**Playwright Config for Auto-Screenshots**:

```typescript
// playwright.config.ts
export default defineConfig({
  use: {
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    trace: 'on-first-retry'
  }
});
```

### 8. Parallel Execution

```typescript
// playwright.config.ts
export default defineConfig({
  workers: process.env.CI ? 2 : 4, // Adjust based on environment
  fullyParallel: true // Run files in parallel
});

// Parallelize within a file
test.describe.configure({ mode: 'parallel' });
```

**Caution**: Ensure tests are truly isolated before enabling parallel execution.

---

## React + TypeScript Testing Patterns

### 1. Testing React Router Navigation

```typescript
test('navigate between pages', async ({ page }) => {
  await page.goto('http://localhost:5173/');

  // Click navigation link
  await page.getByRole('link', { name: 'Add New Car' }).click();

  // Verify URL changed
  await expect(page).toHaveURL(/\/cars\/create/);

  // Verify page loaded
  await expect(page.getByRole('heading', { name: 'Add New Car' })).toBeVisible();

  // Test back button
  await page.goBack();
  await expect(page).toHaveURL(/\/$/);
});
```

### 2. Testing React Query Data Fetching

```typescript
test('display loading state then data', async ({ page }) => {
  await page.goto('http://localhost:5173/');

  // Might see loading state (if slow network)
  const loadingText = page.getByText('Loading cars...');
  if (await loadingText.isVisible()) {
    await loadingText.waitFor({ state: 'hidden', timeout: 5000 });
  }

  // Data should be loaded
  await expect(page.getByRole('heading', { name: 'Cars' })).toBeVisible();
});
```

### 3. Testing Context API (AuthContext)

```typescript
test('logout clears user context', async ({ page }) => {
  // Login first
  await loginPage.login('user@example.com', 'Password123!');

  // Verify logged in (user name visible)
  await expect(page.getByText(/User/)).toBeVisible();

  // Logout
  await page.getByRole('button', { name: 'Logout' }).click();

  // Verify redirected to login
  await expect(page).toHaveURL(/\/login/);

  // Verify localStorage cleared
  const token = await page.evaluate(() => localStorage.getItem('token'));
  expect(token).toBeNull();
});
```

### 4. Testing Form Validation

```typescript
test('display validation errors', async ({ page }) => {
  await page.goto('http://localhost:5173/cars/create');

  // Submit empty form
  await page.getByRole('button', { name: 'Create Car' }).click();

  // Verify HTML5 validation
  const makeInput = page.getByLabel('Make');
  const validationMessage = await makeInput.evaluate(
    (el: HTMLInputElement) => el.validationMessage
  );
  expect(validationMessage).toBeTruthy();
});
```

### 5. Testing TypeScript Types in Runtime

```typescript
test('API returns correct data shape', async ({ page }) => {
  // Intercept API call
  const responsePromise = page.waitForResponse(
    resp => resp.url().includes('/api/v1/cars') && resp.status() === 200
  );

  await page.goto('http://localhost:5173/');
  const response = await responsePromise;

  // Validate response structure
  const cars = await response.json();
  expect(Array.isArray(cars)).toBeTruthy();

  if (cars.length > 0) {
    const car = cars[0];
    expect(car).toHaveProperty('id');
    expect(car).toHaveProperty('make');
    expect(car).toHaveProperty('model');
    expect(car).toHaveProperty('year');
    expect(car).toHaveProperty('price');
  }
});
```

---

## JWT Authentication Testing

### 1. Testing Token Storage

```typescript
test('login stores JWT token', async ({ page }) => {
  await page.goto('http://localhost:5173/login');

  // Intercept login API call
  const loginPromise = page.waitForResponse(
    resp => resp.url().includes('/api/v1/auth/login')
  );

  await page.getByLabel('Email').fill('user@example.com');
  await page.getByLabel('Password').fill('Password123!');
  await page.getByRole('button', { name: 'Sign in' }).click();

  const loginResponse = await loginPromise;
  const authData = await loginResponse.json();

  // Verify token in response
  expect(authData.token).toBeTruthy();

  // Verify token stored in localStorage
  const storedToken = await page.evaluate(() => localStorage.getItem('token'));
  expect(storedToken).toBe(authData.token);
});
```

### 2. Testing Token in Request Headers

```typescript
test('authenticated requests include Bearer token', async ({ page }) => {
  // Use pre-authenticated state
  await page.goto('http://localhost:5173/', {
    storageState: 'auth.json'
  });

  // Intercept API request
  const requestPromise = page.waitForRequest(
    req => req.url().includes('/api/v1/cars')
  );

  await page.getByRole('link', { name: 'Add New Car' }).click();

  const request = await requestPromise;
  const authHeader = request.headers()['authorization'];

  expect(authHeader).toMatch(/^Bearer .+/);
});
```

### 3. Testing Token Expiration

```typescript
test('expired token redirects to login', async ({ page }) => {
  // Set expired token in localStorage
  await page.goto('http://localhost:5173/');
  await page.evaluate(() => {
    localStorage.setItem('token', 'expired.jwt.token');
    localStorage.setItem('user', JSON.stringify({ email: 'test@test.com' }));
  });

  // Try to access protected page
  await page.goto('http://localhost:5173/');

  // Should redirect to login (due to 401 from API)
  await page.waitForURL('**/login', { timeout: 5000 });

  // Token should be cleared
  const token = await page.evaluate(() => localStorage.getItem('token'));
  expect(token).toBeNull();
});
```

### 4. Testing Protected Routes

```typescript
test('unauthenticated user redirected to login', async ({ page }) => {
  // Clear any existing auth
  await page.goto('http://localhost:5173/');
  await page.evaluate(() => {
    localStorage.clear();
  });

  // Try to access protected route
  await page.goto('http://localhost:5173/cars/create');

  // Should be redirected to login
  await expect(page).toHaveURL(/\/login/);
});
```

---

## Page Object Model Implementation

### Complete Example for Car Builder

#### 1. Base Page Object

```typescript
// pages/BasePage.ts
import { Page } from '@playwright/test';

export class BasePage {
  constructor(protected page: Page) {}

  async navigate(path: string) {
    await this.page.goto(`http://localhost:5173${path}`);
  }

  async waitForNetworkIdle() {
    await this.page.waitForLoadState('networkidle');
  }

  async takeScreenshot(name: string) {
    await this.page.screenshot({
      path: `screenshots/${name}-${Date.now()}.png`,
      fullPage: true
    });
  }
}
```

#### 2. Login Page

```typescript
// pages/LoginPage.ts
import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

export class LoginPage extends BasePage {
  // Locators
  private readonly emailInput: Locator;
  private readonly passwordInput: Locator;
  private readonly submitButton: Locator;
  private readonly errorMessage: Locator;
  private readonly registerLink: Locator;

  constructor(page: Page) {
    super(page);
    this.emailInput = page.getByLabel('Email address');
    this.passwordInput = page.getByLabel('Password');
    this.submitButton = page.getByRole('button', { name: 'Sign in' });
    this.errorMessage = page.getByRole('alert');
    this.registerLink = page.getByRole('link', { name: 'create a new account' });
  }

  async navigate() {
    await super.navigate('/login');
  }

  async fillEmail(email: string) {
    await this.emailInput.fill(email);
  }

  async fillPassword(password: string) {
    await this.passwordInput.fill(password);
  }

  async clickSubmit() {
    await this.submitButton.click();
  }

  async login(email: string, password: string) {
    await this.fillEmail(email);
    await this.fillPassword(password);
    await this.clickSubmit();
    await this.page.waitForURL('**/');
  }

  async getErrorMessage(): Promise<string | null> {
    if (await this.errorMessage.isVisible()) {
      return await this.errorMessage.textContent();
    }
    return null;
  }

  async goToRegister() {
    await this.registerLink.click();
    await this.page.waitForURL('**/register');
  }

  async isLoginSuccessful(): Promise<boolean> {
    await this.page.waitForURL('**/');
    return this.page.url().endsWith('/');
  }
}
```

#### 3. Register Page

```typescript
// pages/RegisterPage.ts
import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

export class RegisterPage extends BasePage {
  private readonly firstNameInput: Locator;
  private readonly lastNameInput: Locator;
  private readonly emailInput: Locator;
  private readonly passwordInput: Locator;
  private readonly submitButton: Locator;
  private readonly errorMessage: Locator;
  private readonly loginLink: Locator;

  constructor(page: Page) {
    super(page);
    this.firstNameInput = page.getByLabel('First name');
    this.lastNameInput = page.getByLabel('Last name');
    this.emailInput = page.getByLabel('Email address');
    this.passwordInput = page.getByLabel(/^Password/);
    this.submitButton = page.getByRole('button', { name: 'Create account' });
    this.errorMessage = page.getByRole('alert');
    this.loginLink = page.getByRole('link', { name: 'sign in to your account' });
  }

  async navigate() {
    await super.navigate('/register');
  }

  async register(
    firstName: string,
    lastName: string,
    email: string,
    password: string
  ) {
    await this.firstNameInput.fill(firstName);
    await this.lastNameInput.fill(lastName);
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
    await this.submitButton.click();
    await this.page.waitForURL('**/');
  }

  async getErrorMessage(): Promise<string | null> {
    if (await this.errorMessage.isVisible()) {
      return await this.errorMessage.textContent();
    }
    return null;
  }
}
```

#### 4. Cars List Page

```typescript
// pages/CarsPage.ts
import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

export class CarsPage extends BasePage {
  private readonly pageTitle: Locator;
  private readonly addNewCarButton: Locator;
  private readonly loadingIndicator: Locator;
  private readonly emptyStateMessage: Locator;

  constructor(page: Page) {
    super(page);
    this.pageTitle = page.getByRole('heading', { name: 'Cars' });
    this.addNewCarButton = page.getByRole('link', { name: 'Add New Car' });
    this.loadingIndicator = page.getByText('Loading cars...');
    this.emptyStateMessage = page.getByText('No cars found');
  }

  async navigate() {
    await super.navigate('/');
  }

  async waitForCarsToLoad() {
    // Wait for loading indicator to disappear
    if (await this.loadingIndicator.isVisible({ timeout: 1000 }).catch(() => false)) {
      await this.loadingIndicator.waitFor({ state: 'hidden' });
    }
  }

  async clickAddNewCar() {
    await this.addNewCarButton.click();
    await this.page.waitForURL('**/cars/create');
  }

  async getCarCard(make: string, model: string): Promise<Locator> {
    return this.page.locator('.bg-white.overflow-hidden.shadow-md', {
      hasText: `${make} ${model}`
    });
  }

  async clickEditCar(make: string, model: string) {
    const carCard = await this.getCarCard(make, model);
    await carCard.getByRole('link', { name: 'Edit' }).click();
    await this.page.waitForURL('**/cars/edit/**');
  }

  async clickDeleteCar(make: string, model: string) {
    const carCard = await this.getCarCard(make, model);

    // Setup dialog handler before clicking delete
    this.page.once('dialog', dialog => dialog.accept());

    await carCard.getByRole('button', { name: 'Delete' }).click();

    // Wait for car to be removed from list
    await this.page.waitForTimeout(500); // Brief wait for deletion
  }

  async isCarVisible(make: string, model: string): Promise<boolean> {
    const carCard = await this.getCarCard(make, model);
    return await carCard.isVisible().catch(() => false);
  }

  async getCarCount(): Promise<number> {
    const cards = await this.page.locator('.bg-white.overflow-hidden.shadow-md').count();
    return cards;
  }

  async isEmptyState(): Promise<boolean> {
    return await this.emptyStateMessage.isVisible();
  }
}
```

#### 5. Create Car Page

```typescript
// pages/CreateCarPage.ts
import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

export class CreateCarPage extends BasePage {
  private readonly pageTitle: Locator;
  private readonly makeInput: Locator;
  private readonly modelInput: Locator;
  private readonly yearInput: Locator;
  private readonly priceInput: Locator;
  private readonly descriptionTextarea: Locator;
  private readonly submitButton: Locator;
  private readonly cancelButton: Locator;
  private readonly errorMessage: Locator;
  private readonly backLink: Locator;

  constructor(page: Page) {
    super(page);
    this.pageTitle = page.getByRole('heading', { name: 'Add New Car' });
    this.makeInput = page.getByLabel('Make', { exact: true });
    this.modelInput = page.getByLabel('Model', { exact: true });
    this.yearInput = page.getByLabel('Year', { exact: true });
    this.priceInput = page.getByLabel('Price', { exact: true });
    this.descriptionTextarea = page.getByLabel('Description');
    this.submitButton = page.getByRole('button', { name: 'Create Car' });
    this.cancelButton = page.getByRole('link', { name: 'Cancel' });
    this.backLink = page.getByRole('link', { name: /Back to Cars/ });
    this.errorMessage = page.locator('.bg-red-50');
  }

  async navigate() {
    await super.navigate('/cars/create');
  }

  async fillCarDetails(car: {
    make: string;
    model: string;
    year: number;
    price: number;
    description?: string;
  }) {
    await this.makeInput.fill(car.make);
    await this.modelInput.fill(car.model);
    await this.yearInput.fill(car.year.toString());
    await this.priceInput.fill(car.price.toString());

    if (car.description) {
      await this.descriptionTextarea.fill(car.description);
    }
  }

  async clickSubmit() {
    await this.submitButton.click();
  }

  async createCar(car: {
    make: string;
    model: string;
    year: number;
    price: number;
    description?: string;
  }) {
    await this.fillCarDetails(car);
    await this.clickSubmit();
    await this.page.waitForURL('**/');
  }

  async clickCancel() {
    await this.cancelButton.click();
    await this.page.waitForURL('**/');
  }

  async getErrorMessage(): Promise<string | null> {
    if (await this.errorMessage.isVisible()) {
      return await this.errorMessage.textContent();
    }
    return null;
  }

  async isFormValid(): Promise<boolean> {
    const makeValid = await this.makeInput.evaluate(
      (el: HTMLInputElement) => el.validity.valid
    );
    const modelValid = await this.modelInput.evaluate(
      (el: HTMLInputElement) => el.validity.valid
    );
    return makeValid && modelValid;
  }
}
```

#### 6. Edit Car Page

```typescript
// pages/EditCarPage.ts
import { Page, Locator } from '@playwright/test';
import { BasePage } from './BasePage';

export class EditCarPage extends BasePage {
  private readonly pageTitle: Locator;
  private readonly makeInput: Locator;
  private readonly modelInput: Locator;
  private readonly yearInput: Locator;
  private readonly priceInput: Locator;
  private readonly descriptionTextarea: Locator;
  private readonly submitButton: Locator;
  private readonly cancelButton: Locator;
  private readonly errorMessage: Locator;
  private readonly loadingIndicator: Locator;

  constructor(page: Page) {
    super(page);
    this.pageTitle = page.getByRole('heading', { name: 'Edit Car' });
    this.makeInput = page.getByLabel('Make', { exact: true });
    this.modelInput = page.getByLabel('Model', { exact: true });
    this.yearInput = page.getByLabel('Year', { exact: true });
    this.priceInput = page.getByLabel('Price', { exact: true });
    this.descriptionTextarea = page.getByLabel('Description');
    this.submitButton = page.getByRole('button', { name: 'Update Car' });
    this.cancelButton = page.getByRole('link', { name: 'Cancel' });
    this.errorMessage = page.locator('.bg-red-50');
    this.loadingIndicator = page.getByText('Loading car details...');
  }

  async navigate(carId: string) {
    await super.navigate(`/cars/edit/${carId}`);
  }

  async waitForCarToLoad() {
    if (await this.loadingIndicator.isVisible({ timeout: 1000 }).catch(() => false)) {
      await this.loadingIndicator.waitFor({ state: 'hidden' });
    }
  }

  async updateCar(updates: {
    make?: string;
    model?: string;
    year?: number;
    price?: number;
    description?: string;
  }) {
    if (updates.make !== undefined) {
      await this.makeInput.fill(updates.make);
    }
    if (updates.model !== undefined) {
      await this.modelInput.fill(updates.model);
    }
    if (updates.year !== undefined) {
      await this.yearInput.fill(updates.year.toString());
    }
    if (updates.price !== undefined) {
      await this.priceInput.fill(updates.price.toString());
    }
    if (updates.description !== undefined) {
      await this.descriptionTextarea.fill(updates.description);
    }

    await this.submitButton.click();
    await this.page.waitForURL('**/');
  }

  async getCurrentValues() {
    return {
      make: await this.makeInput.inputValue(),
      model: await this.modelInput.inputValue(),
      year: parseInt(await this.yearInput.inputValue()),
      price: parseFloat(await this.priceInput.inputValue()),
      description: await this.descriptionTextarea.inputValue()
    };
  }
}
```

---

## Car Builder Test Strategy

### Test Coverage Matrix

| Feature | Unit Tests | Integration Tests | E2E Tests |
|---------|-----------|-------------------|-----------|
| Car entity validation | ✅ | | |
| API endpoints | ✅ | ✅ | |
| Authentication flow | | ✅ | ✅ |
| Car CRUD operations | | ✅ | ✅ |
| UI components | ✅ | | |
| User workflows | | | ✅ |

### E2E Test Scenarios

#### 1. Authentication Suite

**Scenarios:**
- ✅ Register new user with valid data
- ✅ Register with invalid email format
- ✅ Register with weak password
- ✅ Register with duplicate email
- ✅ Login with valid credentials
- ✅ Login with invalid credentials
- ✅ Logout clears session
- ✅ Protected routes redirect when unauthenticated
- ✅ Token persists across page reloads

#### 2. Car Management Suite

**Scenarios:**
- ✅ View empty cars list
- ✅ Create car with valid data
- ✅ Create car with validation errors (negative price, invalid year)
- ✅ View created car in list
- ✅ Edit car successfully
- ✅ Delete car with confirmation
- ✅ Cancel delete operation
- ✅ Navigate between car pages

#### 3. API Integration Suite

**Scenarios:**
- ✅ Verify JWT token in request headers
- ✅ Handle 401 unauthorized (expired token)
- ✅ Handle 404 not found
- ✅ Handle server validation errors (400)
- ✅ Handle network errors

#### 4. UI/UX Suite

**Scenarios:**
- ✅ Loading states display correctly
- ✅ Error messages are user-friendly
- ✅ Form validation feedback
- ✅ Responsive layout on mobile/tablet/desktop
- ✅ Accessibility (keyboard navigation, screen reader)

### Test Data Strategy

```typescript
// helpers/test-data.ts

export const validUser = {
  email: `test-${Date.now()}@carbuilder.com`,
  password: 'Test123!@#',
  firstName: 'Test',
  lastName: 'User'
};

export const validCar = () => ({
  make: 'Toyota',
  model: `Camry-${Date.now()}`,
  year: 2024,
  price: 25000,
  description: 'A reliable sedan'
});

export const invalidCars = {
  negativePric: {
    make: 'Honda',
    model: 'Accord',
    year: 2024,
    price: -100,
    description: 'Invalid price'
  },
  invalidYear: {
    make: 'Ford',
    model: 'Focus',
    year: 1800,
    price: 15000,
    description: 'Invalid year'
  },
  emptyMake: {
    make: '',
    model: 'Civic',
    year: 2023,
    price: 20000,
    description: 'Missing make'
  }
};
```

### Test Organization

```
tests/
├── e2e/
│   ├── auth/
│   │   ├── login.spec.ts
│   │   ├── register.spec.ts
│   │   └── logout.spec.ts
│   ├── cars/
│   │   ├── list.spec.ts
│   │   ├── create.spec.ts
│   │   ├── edit.spec.ts
│   │   └── delete.spec.ts
│   ├── navigation/
│   │   └── routing.spec.ts
│   └── api/
│       └── integration.spec.ts
├── pages/
│   ├── BasePage.ts
│   ├── LoginPage.ts
│   ├── RegisterPage.ts
│   ├── CarsPage.ts
│   ├── CreateCarPage.ts
│   └── EditCarPage.ts
├── helpers/
│   ├── test-data.ts
│   └── auth-helper.ts
├── fixtures/
│   └── cars.json
├── screenshots/
├── global-setup.ts
└── playwright.config.ts
```

---

## Running Tests

### Prerequisites

Before running E2E tests, ensure:

1. ✅ SQL Server is running (Docker)
2. ✅ Backend API is running (localhost:7001)
3. ✅ Frontend is running (localhost:5173)
4. ✅ Playwright MCP is configured in Claude Code

### Start the Application Stack

```bash
# Terminal 1: Start SQL Server
docker-compose up -d

# Terminal 2: Start Backend API
cd src/CarBuilder.API
dotnet run

# Terminal 3: Start Frontend
cd frontend
npm run dev
```

### Running Tests with Playwright MCP

With Playwright MCP configured, you can run tests interactively through Claude Code:

**Example Test Request:**
```
"Test the login flow:
1. Navigate to http://localhost:5173/login
2. Fill in email: test@example.com
3. Fill in password: Test123!@#
4. Click Sign in button
5. Verify redirected to home page
6. Take a screenshot"
```

Claude Code will use the MCP tools to execute this test in a visible browser window.

### Running Traditional Playwright Tests

If you set up a traditional Playwright project:

```bash
# Run all tests
npx playwright test

# Run specific test file
npx playwright test tests/e2e/auth/login.spec.ts

# Run in headed mode (visible browser)
npx playwright test --headed

# Run in debug mode
npx playwright test --debug

# Run with UI mode (interactive)
npx playwright test --ui

# Generate HTML report
npx playwright show-report
```

### CI/CD Integration

```yaml
# .github/workflows/e2e-tests.yml
name: E2E Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          ACCEPT_EULA: Y
          SA_PASSWORD: YourStrong@Passw0rd
        ports:
          - 1433:1433

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'

      - name: Setup Node
        uses: actions/setup-node@v4
        with:
          node-version: '18'

      - name: Install dependencies
        run: |
          cd src/CarBuilder.API && dotnet restore
          cd ../../frontend && npm ci

      - name: Start backend
        run: |
          cd src/CarBuilder.API
          dotnet run &
          sleep 10

      - name: Start frontend
        run: |
          cd frontend
          npm run dev &
          sleep 5

      - name: Install Playwright
        run: npx playwright install --with-deps

      - name: Run E2E tests
        run: npx playwright test

      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: playwright-report
          path: playwright-report/
```

---

## Debugging and Troubleshooting

### 1. Headed Mode Debugging

Run tests with visible browser to see what's happening:

```bash
# Playwright MCP runs in headed mode by default
# Or with traditional Playwright:
npx playwright test --headed
```

### 2. Debug Mode

Step through tests with Playwright Inspector:

```bash
npx playwright test --debug
```

### 3. Screenshots on Failure

Automatically capture screenshots when tests fail:

```typescript
// playwright.config.ts
export default defineConfig({
  use: {
    screenshot: 'only-on-failure'
  }
});
```

### 4. Video Recording

Record video of test execution:

```typescript
// playwright.config.ts
export default defineConfig({
  use: {
    video: 'on-first-retry' // or 'on', 'retain-on-failure'
  }
});
```

### 5. Trace Viewer

Generate detailed traces for debugging:

```bash
# Enable tracing in config
use: { trace: 'on-first-retry' }

# View trace
npx playwright show-trace trace.zip
```

### 6. Console Logs

Capture browser console logs:

```typescript
page.on('console', msg => console.log('BROWSER:', msg.text()));
page.on('pageerror', error => console.log('PAGE ERROR:', error));
```

### 7. Network Activity

Monitor network requests:

```typescript
page.on('request', request =>
  console.log('>>', request.method(), request.url())
);

page.on('response', response =>
  console.log('<<', response.status(), response.url())
);
```

### Common Issues and Solutions

#### Issue: "Element not found"
**Solution:**
- Verify element exists with correct selector
- Check if element is in different frame/iframe
- Ensure page has loaded (wait for network idle)

#### Issue: "Timeout exceeded"
**Solution:**
- Increase timeout for slow operations
- Check if element is hidden or disabled
- Verify API is responding

#### Issue: "Test works locally but fails in CI"
**Solution:**
- Ensure same environment (database, API)
- Use `await page.waitForLoadState('networkidle')`
- Disable animations in CI

#### Issue: "Flaky tests"
**Solution:**
- Use Playwright's auto-waiting
- Avoid arbitrary `waitForTimeout`
- Ensure test isolation (no shared state)

---

## Summary

This guide provides a comprehensive approach to E2E testing with Playwright MCP for the Car Builder application. Key takeaways:

1. **Use Playwright MCP** for AI-powered, interactive browser automation
2. **Follow best practices**: Test isolation, resilient locators, auto-wait
3. **Implement Page Object Model** for maintainability and reusability
4. **Test critical user flows** end-to-end with real backend integration
5. **Debug effectively** with headed mode, screenshots, and traces

By following these practices, you'll build a robust E2E test suite that provides confidence in your application's functionality and prevents regressions.

---

## Resources

- **Playwright Documentation**: https://playwright.dev/
- **Playwright MCP**: https://github.com/microsoft/playwright-mcp
- **Testing Library**: https://testing-library.com/
- **Car Builder Documentation**: See other docs in this folder

---

**Last Updated**: October 2025
**Version**: 1.0
