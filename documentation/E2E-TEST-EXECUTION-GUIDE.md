# Car Builder E2E Test Execution Guide

## Prerequisites

Before running end-to-end tests, ensure all services are running:

### 1. Start SQL Server

```bash
# Start Docker Desktop first
# Then run:
docker-compose up -d

# Verify it's running:
docker ps | findstr carbuilder-sqlserver
```

Expected output:
```
carbuilder-sqlserver   Up X minutes   0.0.0.0:1433->1433/tcp
```

### 2. Start Backend API

```bash
cd src/CarBuilder.API
dotnet run
```

Expected output:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5001
```

Verify API is accessible:
- Navigate to: https://localhost:7001/swagger
- Should see Swagger UI with API documentation

### 3. Start Frontend

```bash
cd frontend
npm run dev
```

Expected output:
```
  VITE v7.x.x  ready in xxx ms

  ➜  Local:   http://localhost:5173/
  ➜  Network: use --host to expose
```

Verify frontend is accessible:
- Navigate to: http://localhost:5173
- Should redirect to /login

### 4. Configure Playwright MCP

If not already configured, add to Claude Code configuration:

**Windows**: `%APPDATA%\Claude\claude_desktop_config.json`
**Mac**: `~/Library/Application Support/Claude/claude_desktop_config.json`

```json
{
  "mcpServers": {
    "playwright": {
      "command": "npx",
      "args": ["-y", "@playwright/mcp@latest"]
    }
  }
}
```

Then restart Claude Code.

---

## Test Execution Plan

### Phase 1: Authentication Tests

#### Test 1.1: User Registration - Valid Data

**Objective**: Verify user can register with valid credentials

**Steps**:
1. Navigate to http://localhost:5173/register
2. Fill first name: "John"
3. Fill last name: "Doe"
4. Fill email: `test-${timestamp}@carbuilder.com`
5. Fill password: "Test123!@#"
6. Click "Create account" button
7. Wait for redirect to home page

**Expected Results**:
- ✅ Redirected to http://localhost:5173/
- ✅ User name "John Doe" visible in navigation
- ✅ JWT token stored in localStorage
- ✅ User object stored in localStorage

**Playwright MCP Commands**:
```
Navigate to http://localhost:5173/register
Fill the textbox labeled "First name" with "John"
Fill the textbox labeled "Last name" with "Doe"
Fill the textbox labeled "Email address" with "test-1234567890@carbuilder.com"
Fill the textbox labeled "Password" with "Test123!@#"
Click the button labeled "Create account"
Wait for URL to change to http://localhost:5173/
Take a screenshot named "registration-success"
```

#### Test 1.2: User Registration - Weak Password

**Objective**: Verify validation prevents weak passwords

**Steps**:
1. Navigate to http://localhost:5173/register
2. Fill first name: "John"
3. Fill last name: "Doe"
4. Fill email: "test@example.com"
5. Fill password: "weak" (does not meet requirements)
6. Click "Create account" button

**Expected Results**:
- ❌ Registration fails
- ✅ Error message displayed explaining password requirements
- ✅ Stays on registration page

#### Test 1.3: User Login - Valid Credentials

**Objective**: Verify user can login with correct credentials

**Prerequisites**: User already registered (from Test 1.1)

**Steps**:
1. Navigate to http://localhost:5173/login
2. Fill email: registered user's email
3. Fill password: registered user's password
4. Click "Sign in" button
5. Wait for redirect

**Expected Results**:
- ✅ Redirected to http://localhost:5173/
- ✅ JWT token stored in localStorage
- ✅ Navigation shows user's name

**Playwright MCP Commands**:
```
Navigate to http://localhost:5173/login
Fill the textbox labeled "Email address" with "test@example.com"
Fill the textbox labeled "Password" with "Test123!@#"
Click the button labeled "Sign in"
Wait for URL to become http://localhost:5173/
Take a screenshot named "login-success"
```

#### Test 1.4: User Login - Invalid Credentials

**Objective**: Verify login fails with incorrect password

**Steps**:
1. Navigate to http://localhost:5173/login
2. Fill email: "test@example.com"
3. Fill password: "WrongPassword123"
4. Click "Sign in" button

**Expected Results**:
- ❌ Login fails
- ✅ Error message: "Invalid email or password"
- ✅ Stays on login page
- ✅ No token in localStorage

#### Test 1.5: Protected Route Access - Unauthenticated

**Objective**: Verify unauthenticated users cannot access protected pages

**Steps**:
1. Clear localStorage (ensure no auth token)
2. Navigate directly to http://localhost:5173/cars/create

**Expected Results**:
- ✅ Redirected to http://localhost:5173/login
- ✅ Cannot access create car page

**Playwright MCP Commands**:
```
Navigate to http://localhost:5173/
Execute JavaScript: localStorage.clear()
Navigate to http://localhost:5173/cars/create
Wait for URL to become http://localhost:5173/login
Take a screenshot named "protected-route-redirect"
```

#### Test 1.6: Logout

**Objective**: Verify logout clears session

**Prerequisites**: User logged in

**Steps**:
1. User already logged in
2. Click "Logout" button in navigation
3. Verify redirect to login page

**Expected Results**:
- ✅ Redirected to http://localhost:5173/login
- ✅ localStorage cleared (no token)
- ✅ localStorage cleared (no user)

---

### Phase 2: Car CRUD Tests

#### Test 2.1: View Empty Cars List

**Objective**: Verify empty state displays correctly

**Prerequisites**: Logged in, no cars in database

**Steps**:
1. Navigate to http://localhost:5173/
2. Wait for page to load

**Expected Results**:
- ✅ Page displays "Cars" heading
- ✅ "Add New Car" button visible
- ✅ Empty state message: "No cars found. Add your first car!"

#### Test 2.2: Create Car - Valid Data

**Objective**: Verify user can create a car with valid data

**Prerequisites**: Logged in

**Steps**:
1. Navigate to http://localhost:5173/
2. Click "Add New Car" button
3. Wait for redirect to /cars/create
4. Fill make: "Toyota"
5. Fill model: "Camry"
6. Fill year: "2024"
7. Fill price: "25000"
8. Fill description: "Reliable sedan"
9. Click "Create Car" button
10. Wait for redirect to home page

**Expected Results**:
- ✅ Redirected to http://localhost:5173/
- ✅ Car card visible with "Toyota Camry"
- ✅ Price displayed as "$25,000"
- ✅ Year displayed as "2024"

**Playwright MCP Commands**:
```
Navigate to http://localhost:5173/
Click the link labeled "Add New Car"
Wait for URL to become http://localhost:5173/cars/create
Fill the textbox labeled "Make" with "Toyota"
Fill the textbox labeled "Model" with "Camry"
Fill the textbox labeled "Year" with "2024"
Fill the textbox labeled "Price" with "25000"
Fill the textarea labeled "Description" with "Reliable sedan"
Click the button labeled "Create Car"
Wait for URL to become http://localhost:5173/
Take a screenshot named "car-created"
```

#### Test 2.3: Create Car - Validation Errors

**Objective**: Verify validation prevents invalid car data

**Prerequisites**: Logged in

**Test 2.3a: Negative Price**

**Steps**:
1. Navigate to http://localhost:5173/cars/create
2. Fill make: "Honda"
3. Fill model: "Accord"
4. Fill year: "2024"
5. Fill price: "-1000" (negative)
6. Click "Create Car" button

**Expected Results**:
- ❌ Car not created
- ✅ Validation error displayed
- ✅ Stays on create page

**Test 2.3b: Invalid Year**

**Steps**:
1. Fill year: "1800" (before minimum)
2. Attempt to submit

**Expected Results**:
- ❌ Car not created
- ✅ HTML5 validation or backend validation error

**Test 2.3c: Empty Required Fields**

**Steps**:
1. Leave make and model empty
2. Click "Create Car" button

**Expected Results**:
- ❌ Form not submitted
- ✅ Browser validation messages appear
- ✅ Required fields highlighted

#### Test 2.4: Edit Car

**Objective**: Verify user can edit existing car

**Prerequisites**: Logged in, at least one car exists

**Steps**:
1. Navigate to http://localhost:5173/
2. Find car card for "Toyota Camry"
3. Click "Edit" link on the card
4. Wait for redirect to edit page
5. Verify form is pre-filled with current values
6. Change model: "Camry Hybrid"
7. Change price: "28000"
8. Click "Update Car" button
9. Wait for redirect to home page

**Expected Results**:
- ✅ Redirected to http://localhost:5173/
- ✅ Car card shows updated "Toyota Camry Hybrid"
- ✅ Price updated to "$28,000"

**Playwright MCP Commands**:
```
Navigate to http://localhost:5173/
Find the element containing text "Toyota Camry"
Click the link labeled "Edit" within that element
Wait for URL to match pattern /cars/edit/
Fill the textbox labeled "Model" with "Camry Hybrid"
Fill the textbox labeled "Price" with "28000"
Click the button labeled "Update Car"
Wait for URL to become http://localhost:5173/
Take a screenshot named "car-updated"
```

#### Test 2.5: Delete Car

**Objective**: Verify user can delete a car

**Prerequisites**: Logged in, at least one car exists

**Steps**:
1. Navigate to http://localhost:5173/
2. Find car card for "Toyota Camry Hybrid"
3. Click "Delete" button on the card
4. Confirm deletion in dialog prompt
5. Wait for car to disappear from list

**Expected Results**:
- ✅ Confirmation dialog appears
- ✅ After confirming, car removed from list
- ✅ If it was the only car, empty state appears

**Playwright MCP Commands**:
```
Navigate to http://localhost:5173/
Find the element containing text "Toyota Camry"
Click the button labeled "Delete" within that element
Accept the confirmation dialog
Wait 1 second
Take a screenshot named "car-deleted"
```

#### Test 2.6: Cancel Delete

**Objective**: Verify canceling delete keeps the car

**Prerequisites**: Logged in, at least one car exists

**Steps**:
1. Navigate to http://localhost:5173/
2. Click "Delete" on a car card
3. Cancel the confirmation dialog
4. Verify car still visible

**Expected Results**:
- ✅ Dialog dismissed
- ✅ Car still in list
- ✅ No deletion occurred

---

### Phase 3: Navigation & Routing Tests

#### Test 3.1: Navigation Between Pages

**Objective**: Verify all navigation links work correctly

**Steps**:
1. Start at home page (/)
2. Click "Add New Car" → verify at /cars/create
3. Click "Back to Cars" → verify at /
4. Click "Edit" on a car → verify at /cars/edit/:id
5. Click "Back to Cars" → verify at /
6. Click "Car Builder" logo → verify at /

**Expected Results**:
- ✅ All navigation links work
- ✅ URLs match expected patterns
- ✅ Pages load correctly

#### Test 3.2: Browser Back/Forward

**Objective**: Verify browser navigation buttons work

**Steps**:
1. Navigate: / → /cars/create → /
2. Click browser back button → should be at /cars/create
3. Click browser back button → should be at /
4. Click browser forward button → should be at /cars/create

**Expected Results**:
- ✅ Back/forward navigation works
- ✅ Page state preserved correctly

---

### Phase 4: API Integration Tests

#### Test 4.1: Verify JWT Token in Headers

**Objective**: Verify authenticated requests include Bearer token

**Prerequisites**: Logged in

**Steps**:
1. Open browser DevTools Network tab
2. Navigate to http://localhost:5173/
3. Observe network request to /api/v1/cars
4. Check request headers

**Expected Results**:
- ✅ Request header includes: `Authorization: Bearer <token>`
- ✅ Token matches localStorage token

#### Test 4.2: Handle 401 Unauthorized

**Objective**: Verify expired token handling

**Steps**:
1. Login successfully
2. Manually set expired/invalid token in localStorage
3. Try to access protected resource (e.g., create car)
4. Observe behavior

**Expected Results**:
- ✅ API returns 401
- ✅ Axios interceptor catches error
- ✅ User redirected to /login
- ✅ localStorage cleared

#### Test 4.3: Handle Network Errors

**Objective**: Verify graceful handling of network failures

**Steps**:
1. Stop the backend API
2. Try to login
3. Observe error handling

**Expected Results**:
- ✅ User-friendly error message displayed
- ✅ No unhandled exceptions
- ✅ App remains functional after error

---

### Phase 5: UI/UX Tests

#### Test 5.1: Loading States

**Objective**: Verify loading indicators display

**Steps**:
1. Navigate to cars list with slow network (throttle in DevTools)
2. Observe "Loading cars..." message
3. Wait for data to load

**Expected Results**:
- ✅ Loading message appears briefly
- ✅ Loading message disappears when data loads
- ✅ Smooth transition to content

#### Test 5.2: Error Messages

**Objective**: Verify error messages are user-friendly

**Test Cases**:
- Login error: "Invalid email or password"
- Registration error: Clear validation messages
- API error: "Failed to create car. Please try again."

**Expected Results**:
- ✅ All errors have friendly messages
- ✅ Errors are clearly visible (red background)
- ✅ Errors provide actionable guidance

#### Test 5.3: Form Validation Feedback

**Objective**: Verify real-time validation feedback

**Steps**:
1. Navigate to create car form
2. Enter invalid year (e.g., 999)
3. Tab out of field
4. Observe validation

**Expected Results**:
- ✅ HTML5 validation triggers
- ✅ Invalid fields highlighted
- ✅ Helpful validation messages

---

## Test Execution Checklist

Before each test session, verify:

- [ ] SQL Server running (Docker)
- [ ] Backend API running (localhost:7001)
- [ ] Frontend running (localhost:5173)
- [ ] Browser DevTools open (for debugging)
- [ ] Screenshot folder exists

During testing:

- [ ] Take screenshots at each major step
- [ ] Note any unexpected behavior
- [ ] Check browser console for errors
- [ ] Monitor network tab for API calls
- [ ] Verify database state when needed

After testing:

- [ ] Document any bugs found
- [ ] Save all screenshots
- [ ] Clean up test data (optional)
- [ ] Generate test report

---

## Example Playwright MCP Test Session

Here's a complete test session you can run with Claude Code once Playwright MCP is configured:

```
Test the complete car creation flow:

1. Navigate to http://localhost:5173/login
2. Login with:
   - Email: existing_user@example.com
   - Password: Test123!@#
3. Wait for redirect to home page
4. Take screenshot named "01-logged-in"
5. Click the "Add New Car" link
6. Wait for create car page to load
7. Take screenshot named "02-create-car-form"
8. Fill in the form:
   - Make: Ford
   - Model: Mustang
   - Year: 2024
   - Price: 35000
   - Description: Classic American muscle car
9. Take screenshot named "03-form-filled"
10. Click "Create Car" button
11. Wait for redirect to home page
12. Take screenshot named "04-car-created"
13. Verify "Ford Mustang" appears in the list
14. Take a final screenshot named "05-verification"
```

This will run the entire flow in a visible browser window and generate screenshots at each step for verification.

---

## Troubleshooting

### Issue: Frontend not loading

**Check**:
```bash
cd frontend
npm run dev
```

If port 5173 is in use, Vite will use 5174. Update test URLs accordingly.

### Issue: API not responding

**Check**:
```bash
cd src/CarBuilder.API
dotnet run
```

Verify Swagger is accessible at https://localhost:7001/swagger

### Issue: Database connection error

**Check**:
```bash
docker ps
```

If SQL Server isn't running:
```bash
docker-compose up -d
```

### Issue: Playwright MCP not working

**Verify configuration**:
1. Check Claude Code config file has correct JSON
2. Restart Claude Code after config changes
3. Try running: `npx @playwright/mcp@latest` manually to test

---

## Success Criteria

All tests pass when:

- ✅ Users can register and login
- ✅ JWT authentication works end-to-end
- ✅ All CRUD operations work (Create, Read, Update, Delete)
- ✅ Validation prevents invalid data
- ✅ Navigation works correctly
- ✅ Error handling is graceful
- ✅ UI provides good user experience

---

**Document Version**: 1.0
**Last Updated**: October 2025
**Test Environment**: Development (localhost)
