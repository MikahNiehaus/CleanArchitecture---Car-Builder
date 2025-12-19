# Application Security Best Practices

TRIGGER: security, vulnerability, OWASP, injection, XSS, authentication, authorization, secrets, CVE

## Overview

This guide covers secure coding practices, vulnerability prevention, and security review methodology. Fixing vulnerabilities early (during development) costs up to 100x less than fixing them in production.

## OWASP Top 10 2025

The OWASP Top 10 represents the most critical web application security risks:

### A01:2025 - Broken Access Control
**Risk**: Users accessing data or functions beyond their permissions

**Prevention**:
- Deny by default—access requires explicit permission
- Server-side enforcement (never trust client-side checks)
- Use RBAC (Role-Based Access Control) consistently
- Log access control failures
- Rate limit API endpoints

**Code Example**:
```python
# VULNERABLE - client could modify request
def get_user_data(user_id):
    return db.query(f"SELECT * FROM users WHERE id = {user_id}")

# SECURE - server-side ownership check
def get_user_data(user_id, current_user):
    if user_id != current_user.id and not current_user.is_admin:
        raise PermissionDenied()
    return db.query("SELECT * FROM users WHERE id = ?", user_id)
```

### A02:2025 - Security Misconfiguration
**Risk**: Insecure default configs, unnecessary features, verbose errors

**Prevention**:
- Remove unnecessary features, frameworks, components
- Secure defaults for all environments
- Consistent hardening across dev/staging/prod
- Disable detailed error messages in production
- Review cloud permissions regularly

**Checklist**:
- [ ] Default accounts/passwords changed
- [ ] Unnecessary ports/services disabled
- [ ] Error messages don't reveal internal details
- [ ] Security headers configured (CSP, HSTS, X-Frame-Options)
- [ ] Directory listing disabled

### A03:2025 - Injection
**Risk**: Untrusted data sent to interpreter (SQL, OS, LDAP, XPath)

**Prevention**:
- Use parameterized queries/prepared statements
- Use ORMs carefully (still validate input)
- Escape special characters for the specific interpreter
- Use LIMIT clauses to prevent mass disclosure

**Code Examples**:
```javascript
// VULNERABLE - SQL Injection
const query = `SELECT * FROM users WHERE email = '${email}'`;

// SECURE - Parameterized query
const query = 'SELECT * FROM users WHERE email = ?';
db.query(query, [email]);
```

```python
# VULNERABLE - Command Injection
os.system(f"convert {filename} output.png")

# SECURE - Use subprocess with list args
subprocess.run(["convert", filename, "output.png"])
```

### A04:2025 - Insecure Design
**Risk**: Missing or ineffective security controls in design

**Prevention**:
- Threat modeling during design phase
- Use secure design patterns
- Define security requirements upfront
- Implement defense in depth
- Limit resource consumption

**Secure Design Patterns**:
- Input validation at trust boundaries
- Output encoding based on context
- Separation of concerns
- Principle of least privilege
- Fail-safe defaults

### A05:2025 - Security Logging & Monitoring Failures
**Risk**: Breaches go undetected; attackers remain persistent

**Prevention**:
- Log login, access control, and validation failures
- Log format suitable for log management tools
- Protect logs from tampering
- Establish alerting thresholds
- Incident response plan

**What to Log**:
- Authentication events (success and failure)
- Authorization failures
- Input validation failures
- Security-relevant application errors
- Changes to security configurations

**What NOT to Log**:
- Passwords (even hashed)
- Session tokens
- Credit card numbers
- Personal health information

### A06:2025 - Vulnerable and Outdated Components
**Risk**: Known vulnerabilities in dependencies

**Prevention**:
- Inventory all components and versions
- Remove unused dependencies
- Continuously monitor for vulnerabilities (CVE databases)
- Prefer maintained packages from official sources
- Automate dependency scanning in CI/CD

**Tools**:
- npm audit / yarn audit
- pip-audit
- OWASP Dependency-Check
- Snyk
- Dependabot

### A07:2025 - Identification and Authentication Failures
**Risk**: Credential stuffing, weak passwords, session hijacking

**Prevention**:
- Implement MFA where possible
- Secure password requirements (length > complexity)
- Hash passwords with Argon2, bcrypt, or scrypt
- Secure session management (rotate IDs, secure cookies)
- Rate limit authentication endpoints

**Password Storage**:
```python
# WRONG
password_hash = hashlib.md5(password).hexdigest()

# BETTER
password_hash = bcrypt.hashpw(password.encode(), bcrypt.gensalt())

# BEST (if available)
password_hash = argon2.hash(password)
```

### A08:2025 - Software and Data Integrity Failures
**Risk**: Code/infrastructure without integrity verification; insecure CI/CD

**Prevention**:
- Verify digital signatures on updates
- Use integrity checks (SRI for CDN resources)
- Secure CI/CD pipelines
- Require code review before deployment
- Sign commits and releases

### A09:2025 - Server-Side Request Forgery (SSRF)
**Risk**: Application fetches remote resource from user-supplied URL

**Prevention**:
- Validate and sanitize all user-supplied URLs
- Allow only specific URL schemes (https)
- Block requests to internal/private IPs
- Use allowlists for permitted destinations
- Don't return raw responses to users

### A10:2025 - Mishandling of Exceptional Conditions (New)
**Risk**: Poor error handling leads to security issues

**Prevention**:
- Handle all exceptions explicitly
- Don't expose stack traces to users
- Log exceptions with context (but not sensitive data)
- Fail securely (deny access on error)
- Test error handling paths

---

## Input Validation

### Validation Strategy
1. **Validate on server side** - Client validation is UX only
2. **Use allowlists** - Define what IS allowed, not what isn't
3. **Validate type, length, format, range**
4. **Encode output** - Context-appropriate encoding

### Input Types and Validation

| Input Type | Validation | Example |
|------------|------------|---------|
| Email | Regex + length limit | `^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$` |
| URL | Parse and validate components | Allow only https, specific hosts |
| Integer | Type check + range | `0 < value < MAX_INT` |
| String | Length + charset | `^[a-zA-Z0-9_-]{1,50}$` |
| File upload | Type, size, content validation | Check magic bytes, not just extension |

### Output Encoding

| Context | Encoding Method |
|---------|-----------------|
| HTML body | HTML entity encoding |
| HTML attribute | HTML attribute encoding |
| JavaScript | JavaScript encoding |
| CSS | CSS encoding |
| URL | URL encoding |

---

## Authentication Best Practices

### Password Requirements
- Minimum 12 characters (length over complexity)
- Check against breach databases (Have I Been Pwned)
- Allow special characters and spaces
- Don't truncate passwords
- Use secure hashing (Argon2id, bcrypt, scrypt)

### Session Management
- Generate cryptographically random session IDs
- Rotate session ID on authentication
- Set secure cookie attributes: `Secure`, `HttpOnly`, `SameSite`
- Implement session timeout (idle and absolute)
- Invalidate sessions on logout

### Multi-Factor Authentication
- Implement MFA for sensitive operations
- Support TOTP authenticator apps
- Consider WebAuthn/FIDO2 for phishing resistance
- Provide backup codes securely

---

## Secrets Management

### Never Do
- Hardcode secrets in source code
- Commit secrets to version control
- Log secrets (even partially)
- Pass secrets in URLs
- Store secrets in client-side code

### Best Practices
- Use environment variables or secret managers
- Rotate secrets regularly
- Different secrets per environment
- Audit secret access
- Use short-lived credentials where possible

### Tools
- HashiCorp Vault
- AWS Secrets Manager
- Azure Key Vault
- 1Password/Bitwarden for development

### Git Protection
```bash
# .gitignore
.env
*.pem
*_secret*
credentials.json

# Use git-secrets or pre-commit hooks to prevent accidental commits
```

---

## Secure Code Review Checklist

### Critical (Block PR)
- [ ] No hardcoded secrets or credentials
- [ ] No SQL injection vulnerabilities
- [ ] No command injection vulnerabilities
- [ ] Authentication required for sensitive endpoints
- [ ] Authorization checked at access points

### High Priority
- [ ] All user input validated
- [ ] Output properly encoded for context
- [ ] Error messages don't leak sensitive info
- [ ] Session management is secure
- [ ] Dependencies have no critical CVEs

### Standard Review
- [ ] HTTPS enforced for sensitive data
- [ ] CORS properly configured
- [ ] Security headers present
- [ ] Rate limiting on authentication
- [ ] Logging covers security events

---

## CI/CD Security Integration

### Pipeline Security Gates
1. **SAST** - Static analysis before merge
2. **Dependency scanning** - Check for vulnerable packages
3. **Secret scanning** - Detect leaked credentials
4. **Container scanning** - Check base images
5. **DAST** - Dynamic testing in staging

### Tools by Category
| Category | Tools |
|----------|-------|
| SAST | SonarQube, Semgrep, CodeQL |
| Dependencies | Snyk, Dependabot, npm audit |
| Secrets | git-secrets, trufflehog, gitleaks |
| Containers | Trivy, Clair, Anchore |
| DAST | OWASP ZAP, Burp Suite |

---

## Security Headers

### Recommended Headers
```
Content-Security-Policy: default-src 'self'; script-src 'self'
Strict-Transport-Security: max-age=31536000; includeSubDomains
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
Referrer-Policy: strict-origin-when-cross-origin
Permissions-Policy: geolocation=(), camera=(), microphone=()
```

---

## Incident Response

### If You Find a Vulnerability
1. **Assess severity** - Is it actively exploitable?
2. **Contain** - Can you mitigate without full fix?
3. **Document** - Record findings and evidence
4. **Fix** - Develop and test remediation
5. **Verify** - Confirm fix works
6. **Learn** - How did this happen? Prevent recurrence

### Severity Classification
| Severity | Criteria | Response Time |
|----------|----------|---------------|
| Critical | RCE, data breach possible | Immediate |
| High | Auth bypass, injection | 24-48 hours |
| Medium | XSS, information disclosure | 1 week |
| Low | Minor misconfiguration | Next sprint |

---

## References

- [OWASP Top 10 2025](https://owasp.org/Top10/)
- [OWASP Cheat Sheet Series](https://cheatsheetseries.owasp.org/)
- [CWE Top 25](https://cwe.mitre.org/top25/)
- [ASVS (Application Security Verification Standard)](https://owasp.org/www-project-application-security-verification-standard/)
