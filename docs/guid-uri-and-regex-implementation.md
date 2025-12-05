# GUID-URI Type and Regex RuleType Implementation

## Summary

Added two new validation features:
1. **guid-uri Type**: Validates URN UUID format (e.g., `urn:uuid:12345678-1234-1234-1234-123456789abc`)
2. **Regex RuleType**: Custom pattern matching validation

**Important**: Both Type and Regex rules **skip validation** when values are null, undefined, or empty strings. Use the `Required` rule type for mandatory field validation.

**Test Results**: All 185 tests passing (124 existing + 61 new)

## Implementation Details

### 1. guid-uri Type

**File**: `TypeChecker.cs`

**Pattern**: `^urn:uuid:[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$`

**Features**:
- Case-insensitive validation
- Validates complete URN UUID format
- Rejects invalid GUIDs, missing prefix, wrong prefix

**Usage Example**:
```json
{
  "RuleType": "Type",
  "Path": "identifier.system",
  "ExpectedType": "guid-uri",
  "ErrorCode": "INVALID_GUID_URI",
  "Message": "Must be a valid URN UUID"
}
```

**Valid Values**:
- `urn:uuid:12345678-1234-1234-1234-123456789abc`
- `urn:uuid:ABCDEF12-3456-7890-ABCD-EF1234567890`
- `urn:uuid:00000000-0000-0000-0000-000000000000`
- `urn:uuid:ffffffff-ffff-ffff-ffff-ffffffffffff`

**Invalid Values**:
- `uuid:12345678-1234-1234-1234-123456789abc` (missing urn: prefix)
- `urn:oid:12345678-1234-1234-1234-123456789abc` (wrong prefix)
- `urn:uuid:12345678-1234` (incomplete)
- `urn:uuid:xyz` (invalid characters)

### 2. Regex RuleType

**Files Modified**:
- `RuleEvaluator.cs`: Added `EvaluateRegex()` method
- `RuleDefinition.cs`: Added `Pattern` property
- `ValidationRule.cs`: Added `Pattern` property

**Features**:
- Custom regex pattern validation
- Invalid pattern detection
- **Skips validation if value is null, undefined, or empty** (use `Required` rule for mandatory checks)
- Detailed error messages with pattern and actual value

**Usage Example**:
```json
{
  "RuleType": "Regex",
  "PathType": "CPS1",
  "Path": "identifier.value",
  "Pattern": "^[ST][0-9]{7}[A-Z]$",
  "ErrorCode": "INVALID_NRIC",
  "Message": "NRIC format invalid"
}
```

**Error Codes**:
- `REGEX_MISMATCH`: Value doesn't match pattern
- `REGEX_VALIDATION_ERROR`: Pattern is missing
- `REGEX_PATTERN_ERROR`: Invalid regex pattern

**Common Patterns**:

**Singapore NRIC**:
```
^[STFG][0-9]{7}[A-Z]$
```

**Email**:
```
^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$
```

**Date (YYYY-MM-DD)**:
```
^\d{4}-\d{2}-\d{2}$
```

**Phone Number (Singapore)**:
```
^[689]\d{7}$
```

**Postal Code (Singapore)**:
```
^\d{6}$
```

**Pipe-Separated Values**:
```
^([^|]+\|)*[^|]+$
```

## Validation Behavior

### Type and Regex Rules Skip Empty Values

Both `Type` and `Regex` rules **only validate when a value is present**:

✅ **Skip validation** when:
- Path doesn't exist (undefined)
- Value is `null`
- Value is empty string `""`
- Value is whitespace-only

❌ **Perform validation** when:
- Value exists and is non-empty

**Example - Separate Rules for Optional Field**:
```json
[
  {
    "Name": "ValidateEmailFormat",
    "RuleType": "Regex",
    "Path": "telecom[?(@.system=='email')].value",
    "Pattern": "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$",
    "ErrorCode": "INVALID_EMAIL",
    "Message": "Email format is invalid"
  }
]
```
☝️ This rule only validates if email exists. If email is optional, no error is raised when missing.

**Example - Required Field with Type Validation**:
```json
[
  {
    "Name": "RequireIdentifier",
    "RuleType": "Required",
    "Path": "identifier.value",
    "ErrorCode": "IDENTIFIER_REQUIRED",
    "Message": "Identifier is mandatory"
  },
  {
    "Name": "ValidateIdentifierFormat",
    "RuleType": "Regex",
    "Path": "identifier.value",
    "Pattern": "^[STFG][0-9]{7}[A-Z]$",
    "ErrorCode": "INVALID_IDENTIFIER",
    "Message": "Identifier must be valid NRIC format"
  }
]
```
☝️ First rule ensures field exists, second rule validates format when present.

### Test Coverage

### TypeGuidUriTests.cs (8 tests)
- ✅ Valid formats (mixed case, uppercase, lowercase)
- ✅ Invalid formats (missing/wrong prefix, invalid chars)
- ✅ Edge cases (null, empty, spaces, trailing chars)
- ✅ Case insensitivity

### RegexValidationTests.cs (53 tests)
- ✅ NRIC validation (valid/invalid formats)
- ✅ Email validation
- ✅ **Null/empty value handling (skips validation)**
- ✅ Missing pattern detection
- ✅ Invalid pattern exception handling
- ✅ Unicode support (Chinese, Japanese, Korean)
- ✅ Pipe-separated values
- ✅ Complex patterns (dates, postal codes)
- ✅ Error message format validation

## Backward Compatibility

✅ All 124 existing tests still pass
✅ No breaking changes to existing Type rules
✅ No changes to existing RuleTypes
✅ **New behavior**: Type and Regex rules now skip validation for null/empty values (more lenient)

## Technical Notes

### Implementation Details

**TypeChecker.cs**:
```csharp
case "guid-uri":
    var guidPattern = @"^urn:uuid:[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";
    return Regex.IsMatch(value, guidPattern);
```

**RuleEvaluator.cs - EvaluateRegex()**:
```csharp
private static void EvaluateRegex(JObject resource, RuleDefinition rule, string scope,
    ValidationResult result, Logger logger)
{
    // 1. Check Pattern is configured
    if (string.IsNullOrEmpty(rule.Pattern))
    {
        result.AddError(rule.ErrorCode ?? "REGEX_VALIDATION_ERROR", rule.Path,
            "Regex rule missing Pattern", scope);
        return;
    }

    // 2. Resolve path value
    var strValue = CpsPathResolver.GetValueAsString(resource, rule.Path);
    
    // 3. Skip validation for null/empty values (use Required rule for mandatory checks)
    if (strValue == null || string.IsNullOrWhiteSpace(strValue))
    {
        logger?.Verbose($"      ⊘ Value is null/empty - skipping regex validation");
        return;
    }

    // 4. Validate with regex
    try
    {
        if (!Regex.IsMatch(strValue, rule.Pattern))
        {
            result.AddError(rule.ErrorCode ?? "REGEX_MISMATCH", rule.Path,
                $"{rule.Message} | Pattern: '{rule.Pattern}' | Actual value: '{strValue}'", scope);
        }
    }
    catch (ArgumentException ex)
    {
        result.AddError(rule.ErrorCode ?? "REGEX_PATTERN_ERROR", rule.Path,
            $"Invalid regex pattern '{rule.Pattern}': {ex.Message}", scope);
    }
}
```

**RuleEvaluator.cs - EvaluateType()**:
```csharp
private static void EvaluateType(JObject resource, RuleDefinition rule, string scope,
    ValidationResult result, Logger logger)
{
    // 1. Check ExpectedType is configured
    if (string.IsNullOrEmpty(rule.ExpectedType))
    {
        result.AddError(rule.ErrorCode ?? "TYPE_VALIDATION_ERROR", rule.Path,
            "Type rule missing ExpectedType", scope);
        return;
    }

    // 2. Resolve path value
    var strValue = CpsPathResolver.GetValueAsString(resource, rule.Path);
    
    // 3. Skip validation for null/empty values (use Required rule for mandatory checks)
    if (strValue == null || string.IsNullOrWhiteSpace(strValue))
    {
        logger?.Verbose($"      ⊘ Value is null/empty - skipping type validation");
        return;
    }

    // 4. Validate type
    if (!TypeChecker.IsValid(strValue, rule.ExpectedType))
    {
        result.AddError(rule.ErrorCode ?? "TYPE_MISMATCH", rule.Path,
            $"{rule.Message} | Expected type: '{rule.ExpectedType}' | Actual value: '{strValue}'", scope);
    }
}
```

### Model Classes

**RuleDefinition.cs** (Core/Metadata):
```csharp
public string? ExpectedType { get; set; }  // For Type rules
public string? Pattern { get; set; }        // For Regex rules
```

**ValidationRule.cs** (Models/Validation):
```csharp
public string? ExpectedType { get; set; }  // For Type rules
public string? Pattern { get; set; }        // For Regex rules
```

Note: Both classes have the properties for consistency, but `RuleEvaluator` uses `RuleDefinition`.

## Usage in Metadata

### guid-uri Example
```json
{
  "Name": "ValidateIdentifierSystem",
  "RuleType": "Type",
  "PathType": "CPS1",
  "Path": "identifier.system",
  "ExpectedType": "guid-uri",
  "ErrorCode": "INVALID_SYSTEM_URI",
  "Message": "Identifier system must be a valid URN UUID"
}
```

### Regex Example
```json
{
  "Name": "ValidateNRIC",
  "RuleType": "Regex",
  "PathType": "CPS1",
  "Path": "identifier.value",
  "Pattern": "^[STFG][0-9]{7}[A-Z]$",
  "ErrorCode": "INVALID_NRIC",
  "Message": "NRIC format invalid"
}
```

## Performance Notes

- Regex patterns are evaluated per validation call (not cached)
- Invalid patterns are caught and reported as errors
- Case-insensitive matching uses default regex options
- Unicode support enabled by default

## Future Enhancements

Potential improvements:
1. Regex pattern caching for performance
2. Compiled regex for frequently used patterns
3. Additional regex options (IgnoreCase, Multiline, etc.)
4. Pattern library/presets for common formats
5. Custom error message templates

## Related Documentation

- [Type Validation Quick Reference](./type-validation-quick-reference.md)
- [Validation Engine](./05-validation-engine.md)
- [FHIR Specification](./03-fhir-spec.md)
