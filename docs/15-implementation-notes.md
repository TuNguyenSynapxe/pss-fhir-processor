# 15 — Implementation Notes & Features
PSS FHIR Processor — Advanced Features Implementation Guide  
Version 1.0 — December 2025

---

## Overview

This document describes advanced features and implementation details for the PSS FHIR Processor that extend beyond the core validation and extraction engines.

---

## 1. Dynamic Metadata Management

### Overview
Users can view and update validation metadata (RuleSets and CodesMaster) through the UI without code changes or redeployment.

### Features

**Backend:**
- `PUT /api/fhir/rules` - Update RuleSets
- `PUT /api/fhir/codes-master` - Update CodesMaster
- In-memory storage with default seed values
- Per-request metadata override support

**Frontend:**
- Global React Context for metadata state management
- Modal-based JSON editor with tabs
- Real-time JSON syntax validation
- Metadata editor available in Playground and ValidationRules pages

### Usage Workflow

1. **View Current Metadata**
   - Navigate to Validation Rules page
   - See all RuleSets organized by scope
   - View CodesMaster questions and code systems

2. **Edit Metadata**
   - Click "Edit Metadata" button
   - Edit JSON in modal tabs (RuleSets / CodesMaster)
   - Save changes (validates JSON syntax)
   - Updates apply globally

3. **Process with Updated Metadata**
   - Changes automatically used in all subsequent requests
   - Test immediately in Playground

### Technical Details

**State Management:**
- Backend: Singleton service maintains metadata in memory
- Frontend: React Context provides global state
- Sync: Frontend updates trigger backend updates

**Persistence:**
- Current: In-memory only (resets on restart)
- Default: Always available from seed classes
- Future: Add database or file-based persistence

---

## 2. GUID-URI Validation

### Overview
Special type validation for URN UUID format commonly used in FHIR bundle references.

### Pattern
```
^urn:uuid:[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$
```

### Usage
```json
{
  "RuleType": "Type",
  "Path": "entry.fullUrl",
  "ExpectedType": "guid-uri",
  "ErrorCode": "INVALID_FULLURL",
  "Message": "fullUrl must be valid URN UUID"
}
```

### Valid Examples
- `urn:uuid:12345678-1234-1234-1234-123456789abc`
- `urn:uuid:ABCDEF12-3456-7890-ABCD-EF1234567890`
- Case-insensitive matching

### Invalid Examples
- `uuid:12345678-1234-1234-1234-123456789abc` (missing urn: prefix)
- `urn:oid:12345678-1234-1234-1234-123456789abc` (wrong prefix)
- Incomplete or invalid GUIDs

---

## 3. Reference Validation

### Overview
Validates FHIR references between resources in a bundle, ensuring referential integrity.

### Features
- Reference format validation
- Target resource type checking
- Bundle reference resolution (urn:uuid:)
- External reference support (optional)
- Circular reference detection

### Configuration
```json
{
  "RuleType": "Reference",
  "Path": "subject",
  "TargetResourceType": "Patient",
  "AllowBundleReferences": true,
  "AllowExternalReferences": false,
  "ErrorCode": "INVALID_REFERENCE",
  "Message": "Invalid patient reference"
}
```

### Validation Logic
1. Check reference field exists
2. Validate reference format
3. For `urn:uuid:` references:
   - Resolve in bundle
   - Verify target resource exists
   - Check resource type matches
4. For external references:
   - Only allow if `AllowExternalReferences = true`

### Error Codes
- `MISSING_REFERENCE`: Reference field missing
- `INVALID_REFERENCE_FORMAT`: Malformed reference
- `REFERENCE_NOT_FOUND`: Referenced resource not in bundle
- `INVALID_RESOURCE_TYPE`: Wrong target resource type

---

## 4. FullUrlIdMatch Validation

### Overview
Validates that a resource's `id` matches the GUID portion of its `entry.fullUrl`.

### Purpose
Ensures consistency between bundle entry identifiers and resource identifiers, preventing mismatches that could cause resolution issues.

### Validation Rules
- **Skip** if either `id` or `fullUrl` is missing
- **Skip** if `fullUrl` is not in `urn:uuid:<GUID>` format
- **Pass** if GUID in `fullUrl` matches resource `id` (case-insensitive)
- **Fail** if GUIDs don't match

### Usage
```json
{
  "RuleType": "FullUrlIdMatch",
  "ErrorCode": "ID_FULLURL_MISMATCH",
  "Message": "Resource.id must match GUID portion of entry.fullUrl"
}
```

### Valid Example
```json
{
  "fullUrl": "urn:uuid:a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91",
  "resource": {
    "resourceType": "Patient",
    "id": "a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91"
  }
}
```

### Invalid Example
```json
{
  "fullUrl": "urn:uuid:a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91",
  "resource": {
    "resourceType": "Patient",
    "id": "different-guid-here"  // ❌ Mismatch
  }
}
```

### Implementation Details
- Operates at bundle entry level
- GUID extraction from `urn:uuid:<GUID>` format
- Case-insensitive GUID comparison
- No Path property required (resource-level validation)

---

## 5. Regex Pattern Validation

### Overview
Custom regular expression validation for flexible format checking.

### Features
- Custom pattern support
- Invalid pattern detection
- Unicode support
- Automatic null/empty skip

### Common Use Cases

**Singapore NRIC:**
```json
{
  "RuleType": "Regex",
  "Path": "identifier.value",
  "Pattern": "^[STFG][0-9]{7}[A-Z]$",
  "ErrorCode": "INVALID_NRIC",
  "Message": "Invalid NRIC format"
}
```

**Email Validation:**
```json
{
  "RuleType": "Regex",
  "Path": "telecom[system:email].value",
  "Pattern": "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$",
  "ErrorCode": "INVALID_EMAIL",
  "Message": "Invalid email format"
}
```

**Singapore Phone Number:**
```json
{
  "RuleType": "Regex",
  "Path": "telecom[system:phone].value",
  "Pattern": "^[689]\\d{7}$",
  "ErrorCode": "INVALID_PHONE",
  "Message": "Invalid Singapore phone number"
}
```

### Error Handling
- `REGEX_MISMATCH`: Value doesn't match pattern
- `REGEX_VALIDATION_ERROR`: Pattern property missing
- `REGEX_PATTERN_ERROR`: Invalid regex syntax

---

## 6. Conditional Validation (Advanced)

### Overview
Supports complex if-then business rules with two modes: JSONPath and Component.

### JSONPath Mode
Uses JSONPath expressions for flexible path navigation.

```json
{
  "RuleType": "Conditional",
  "If": "$.extension[?(@.url=='has-referral')].valueBoolean",
  "Then": "extension[url:referral-id].valueString",
  "ErrorCode": "MISSING_REFERRAL_ID",
  "Message": "Referral ID required when has referral"
}
```

### Component Mode with WhenValue
Checks specific answer values in observation components.

```json
{
  "RuleType": "Conditional",
  "If": "component[code.coding.code:SQ-L2H9-00000001].valueString",
  "WhenValue": "Yes (Proceed to next question)",
  "Then": "component[code.coding.code:SQ-Q1P1-00000002].valueString",
  "ErrorCode": "CONDITIONAL_FAILED",
  "Message": "Follow-up question required"
}
```

### Use Cases
- Follow-up questions based on previous answers
- Required fields based on flags/checkboxes
- Multi-step validation workflows
- Complex business logic

---

## 7. Display Matching Modes

### Overview
Configurable display text validation for coded values.

### Modes

**Lenient (Default):**
- Case-insensitive comparison
- Whitespace normalization
- Punctuation tolerance

**Strict:**
- Exact character-by-character match
- Case-sensitive
- Whitespace-sensitive

**Normalized:**
- Lowercase comparison
- Multiple spaces → single space
- Trim leading/trailing

### Configuration
```csharp
new ValidationOptions 
{
    StrictDisplayValidation = false  // or true
}
```

### Use Cases
- **Lenient**: Vendor flexibility, minor text variations
- **Strict**: Regulatory compliance, exact terminology
- **Normalized**: Balance between strict and lenient

---

## 8. Multi-Value Support (PipeSeparated)

### Overview
Special handling for pipe-separated multi-select answers.

### Format
```
value1|value2|value3
```

### Validation Rules
- All parts must be non-empty
- Each part validated against allowed answers
- Maintains order
- No duplicate checking (allows if needed)

### Example
```json
{
  "component": [{
    "code": {
      "coding": [{
        "code": "SQ-F7B7-00000007",
        "display": "Pure Tone 25dBHL (Left)"
      }]
    },
    "valueString": "500Hz – R|1000Hz – R|2000Hz – R|4000Hz – NR"
  }]
}
```

### CodesMaster Configuration
```json
{
  "QuestionCode": "SQ-F7B7-00000007",
  "QuestionDisplay": "Pure Tone 25dBHL (Left)",
  "IsMultiValue": true,
  "AllowedAnswers": [
    "500Hz – R",
    "500Hz – NR",
    "1000Hz – R",
    "1000Hz – NR",
    "2000Hz – R",
    "2000Hz – NR",
    "4000Hz – R",
    "4000Hz – NR"
  ]
}
```

---

## 9. Logging System

### Log Levels
- **Info**: High-level flow (bundle received, validation complete)
- **Debug**: Rule-level details (which rules executed)
- **Verbose**: Step-by-step validation (path resolution, value comparison)

### Configuration
```csharp
new LoggingOptions 
{
    Enabled = true,
    Level = "Verbose"  // or "Info", "Debug"
}
```

### Log Output Format
```
[INFO] Starting validation
[DEBUG] Evaluating rule: Required @ Patient.birthDate
[VERBOSE]   ✓ Path resolved: "1990-05-15"
[VERBOSE]   ✓ Value exists - validation passed
```

### Use Cases
- Development debugging
- Production troubleshooting
- Vendor support
- Audit trails

---

## 10. Test Coverage

### Current Status
**213/213 tests passing (100%)**

**Breakdown:**
- Unit tests: 107 (TypeChecker, PathResolver, etc.)
- Integration tests: 53 (Full validation workflows)
- Reference validation: 15
- FullUrlIdMatch validation: 11
- Extraction tests: 27

### Test Categories
1. **Rule Type Tests**: Each rule type thoroughly tested
2. **Edge Cases**: Null, empty, malformed inputs
3. **Integration**: Complete bundle scenarios
4. **Error Handling**: Expected failures validated
5. **Performance**: Large bundle handling

---

## Best Practices

### 1. Metadata Design
- Group related rules by scope
- Use descriptive error codes
- Provide actionable error messages
- Test rule combinations

### 2. Error Handling
- Combine Required + Type for mandatory fields
- Use specific error codes per validation type
- Include context in error messages
- Don't duplicate validation logic

### 3. Performance
- Minimize complex JSONPath expressions
- Use simple paths when possible
- Cache compiled regex patterns (future enhancement)
- Batch validation when appropriate

### 4. Testing
- Create test cases for each rule
- Test edge cases (null, empty, malformed)
- Validate error messages
- Test rule combinations

---

## Future Enhancements

### Planned
1. Metadata persistence (database/file)
2. Metadata versioning/history
3. Import/export metadata
4. Pattern library for common validations
5. Compiled regex caching

### Considered
1. Custom JavaScript validation rules
2. Multi-language error messages
3. Validation profiles (dev/staging/prod)
4. Real-time validation in UI editor
5. LLM-assisted rule generation

---

## See Also

- [14 — Validation Rules Reference](./14-validation-rules-reference.md)
- [05 — Validation Engine Design](./05-validation-engine.md)
- [08 — WebApp Playground](./08-webapp-playground.md)

---

**Last Updated:** December 2025  
**Status:** Production Ready
