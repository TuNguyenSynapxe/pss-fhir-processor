# GUID-URI Type and Regex RuleType Implementation

## Summary

Added two new validation features:
1. **guid-uri Type**: Validates URN UUID format (e.g., `urn:uuid:12345678-1234-1234-1234-123456789abc`)
2. **Regex RuleType**: Custom pattern matching validation

**Test Results**: All 186 tests passing (124 existing + 62 new)

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
- Null value handling
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

## Test Coverage

### TypeGuidUriTests.cs (8 tests)
- ✅ Valid formats (mixed case, uppercase, lowercase)
- ✅ Invalid formats (missing/wrong prefix, invalid chars)
- ✅ Edge cases (null, empty, spaces, trailing chars)
- ✅ Case insensitivity

### RegexValidationTests.cs (54 tests)
- ✅ NRIC validation (valid/invalid formats)
- ✅ Email validation
- ✅ Null/empty value handling
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

## Technical Notes

### Implementation Details

**TypeChecker.cs**:
```csharp
case "guid-uri":
    var guidPattern = @"^urn:uuid:[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$";
    return Regex.IsMatch(value, guidPattern);
```

**RuleEvaluator.cs**:
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
    
    // 3. Handle null values
    if (strValue == null)
    {
        result.AddError(rule.ErrorCode ?? "REGEX_MISMATCH", rule.Path,
            $"{rule.Message} | Path '{rule.Path}' not found or value is null", scope);
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
