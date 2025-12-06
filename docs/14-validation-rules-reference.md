# 14 — Validation Rules Reference
PSS FHIR Processor — Complete Validation Rule Types Guide  
Version 1.0 — December 2025

---

## Overview

This document provides a comprehensive reference for all validation rule types supported by the PSS FHIR Processor. The validation engine is metadata-driven and supports multiple rule types for flexible, configurable validation.

---

## Rule Structure

All validation rules follow this base structure:

```json
{
  "RuleType": "<rule-type>",
  "PathType": "CPS1",
  "Path": "<path-to-field>",
  "ErrorCode": "<error-code>",
  "Message": "<error-message>",
  // Additional properties based on RuleType
}
```

---

## Supported Rule Types

### 1. Required

Validates that a field exists and is not null.

**Properties:**
- `Path`: Field path to validate

**Example:**
```json
{
  "RuleType": "Required",
  "Path": "Patient.birthDate",
  "ErrorCode": "MISSING_BIRTH_DATE",
  "Message": "Patient birth date is required"
}
```

**Validation:**
- ✅ Pass: Field exists and has a value
- ❌ Fail: Field is missing, null, or undefined

---

### 2. Type

Validates that a field value matches the expected data type.

**Properties:**
- `Path`: Field path to validate
- `ExpectedType`: One of the supported types

**Supported Types:**

| Type | Format | Example |
|------|--------|---------|
| `string` | Any text | `"hello"` |
| `integer` | 32-bit integer | `"123"` |
| `decimal` | Decimal number | `"123.45"` |
| `boolean` | true/false (case-insensitive) | `"true"` |
| `guid` | RFC4122 UUID | `"a1b2c3d4-e5f6-7890-abcd-ef1234567890"` |
| `guid-uri` | URN UUID format | `"urn:uuid:12345678-1234-1234-1234-123456789abc"` |
| `date` | YYYY-MM-DD | `"2024-01-01"` |
| `datetime` | ISO-8601 | `"2024-01-01T00:00:00Z"` |
| `pipestring[]` | Pipe-separated (no empty parts) | `"A|B|C"` |
| `array` | JSON array | `"[1, 2, 3]"` |
| `object` | JSON object | `"{\"key\": \"value\"}"` |

**Example:**
```json
{
  "RuleType": "Type",
  "Path": "Patient.birthDate",
  "ExpectedType": "date",
  "ErrorCode": "INVALID_BIRTH_DATE",
  "Message": "Birth date must be in YYYY-MM-DD format"
}
```

**Validation Behavior:**
- ⊘ Skip: Value is null, undefined, or empty (use `Required` for mandatory checks)
- ✅ Pass: Value matches expected type
- ❌ Fail: Value doesn't match expected type

---

### 3. Regex

Validates that a field value matches a custom regular expression pattern.

**Properties:**
- `Path`: Field path to validate
- `Pattern`: Regular expression pattern

**Example:**
```json
{
  "RuleType": "Regex",
  "Path": "identifier.value",
  "Pattern": "^[STFG][0-9]{7}[A-Z]$",
  "ErrorCode": "INVALID_NRIC",
  "Message": "NRIC format invalid"
}
```

**Common Patterns:**

**Singapore NRIC:**
```
^[STFG][0-9]{7}[A-Z]$
```

**Email:**
```
^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$
```

**Singapore Phone:**
```
^[689]\d{7}$
```

**Singapore Postal Code:**
```
^\d{6}$
```

**Validation Behavior:**
- ⊘ Skip: Value is null, undefined, or empty
- ✅ Pass: Value matches pattern
- ❌ Fail: Value doesn't match pattern or pattern is invalid

---

### 4. FixedValue

Validates that a field has a specific fixed value.

**Properties:**
- `Path`: Field path to validate
- `FixedValue`: Expected value

**Example:**
```json
{
  "RuleType": "FixedValue",
  "Path": "Patient.gender",
  "FixedValue": "male",
  "ErrorCode": "INVALID_GENDER",
  "Message": "Gender must be male"
}
```

---

### 5. FixedCoding

Validates that a coding field has specific system and code values.

**Properties:**
- `Path`: Path to coding field
- `FixedSystem`: Expected system URI
- `FixedCode`: Expected code value

**Example:**
```json
{
  "RuleType": "FixedCoding",
  "Path": "code.coding",
  "FixedSystem": "https://fhir.synapxe.sg/CodeSystem/screening-type",
  "FixedCode": "HS",
  "ErrorCode": "INVALID_SCREENING_CODE",
  "Message": "Must use HS screening code"
}
```

---

### 6. AllowedValues

Validates that a field value is one of the allowed values.

**Properties:**
- `Path`: Field path to validate
- `AllowedValues`: Array of permitted values

**Example:**
```json
{
  "RuleType": "AllowedValues",
  "Path": "Patient.gender",
  "AllowedValues": ["male", "female", "other", "unknown"],
  "ErrorCode": "INVALID_GENDER",
  "Message": "Gender must be one of the allowed values"
}
```

---

### 7. CodesMaster

Validates observation component questions against the Codes Master metadata.

**Properties:**
- `Path`: Path to component array

**Validations Performed:**
- Question code exists in Codes Master
- Question display matches (if strict mode enabled)
- Screening type is correct
- Answer values are in allowed answers list
- Multi-value answers formatted correctly (pipe-separated)

**Example:**
```json
{
  "RuleType": "CodesMaster",
  "Path": "component",
  "ErrorCode": "CODES_MASTER_VIOLATION",
  "Message": "Question validation failed"
}
```

---

### 8. Conditional

Validates conditional business rules (if-then logic).

**Properties:**
- `If`: Condition path (JSONPath syntax)
- `Then`: Required path if condition is met
- `WhenValue`: Optional specific value to match

**Example (JSONPath mode):**
```json
{
  "RuleType": "Conditional",
  "If": "$.extension[?(@.url=='has-referral')].valueBoolean",
  "Then": "extension[url:referral-id].valueString",
  "ErrorCode": "MISSING_REFERRAL_ID",
  "Message": "Referral ID required when referral flag is true"
}
```

**Example (Component mode with WhenValue):**
```json
{
  "RuleType": "Conditional",
  "If": "component[code.coding.code:SQ-001].valueString",
  "WhenValue": "Yes",
  "Then": "component[code.coding.code:SQ-002].valueString",
  "ErrorCode": "CONDITIONAL_FAILED",
  "Message": "Follow-up question required when answer is Yes"
}
```

---

### 9. Reference

Validates FHIR references between resources.

**Properties:**
- `Path`: Path to reference field
- `TargetResourceType`: Expected resource type
- `AllowBundleReferences`: Allow `urn:uuid:` references (default: true)
- `AllowExternalReferences`: Allow external URLs (default: false)

**Example:**
```json
{
  "RuleType": "Reference",
  "Path": "subject",
  "TargetResourceType": "Patient",
  "AllowBundleReferences": true,
  "ErrorCode": "INVALID_PATIENT_REFERENCE",
  "Message": "Subject must reference a Patient resource"
}
```

**Validations:**
- Reference format is valid
- Target resource type matches expected type
- Referenced resource exists in bundle (for bundle references)

---

### 10. FullUrlIdMatch

Validates that a resource's `id` matches the GUID portion of its `entry.fullUrl`.

**Properties:**
- No additional properties needed (operates on resource level)

**Example:**
```json
{
  "RuleType": "FullUrlIdMatch",
  "ErrorCode": "ID_FULLURL_MISMATCH",
  "Message": "Resource.id must match GUID portion of entry.fullUrl (urn:uuid:<GUID>)"
}
```

**Validation Behavior:**
- ⊘ Skip: Either `id` or `fullUrl` is missing (let Required/Type rules handle)
- ⊘ Skip: `fullUrl` is not in `urn:uuid:` format
- ✅ Pass: GUID in `fullUrl` matches resource `id` (case-insensitive)
- ❌ Fail: GUIDs don't match

**Example valid pair:**
```json
{
  "fullUrl": "urn:uuid:a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91",
  "resource": {
    "resourceType": "Patient",
    "id": "a3f9c2d1-2b40-4ab9-8133-5b1f2d4e9f91"
  }
}
```

---

## Combining Rules

Multiple rules can be applied to the same field for comprehensive validation:

**Example: Required + Type + Regex**
```json
[
  {
    "RuleType": "Required",
    "Path": "identifier.value",
    "ErrorCode": "MISSING_NRIC",
    "Message": "NRIC is required"
  },
  {
    "RuleType": "Type",
    "Path": "identifier.value",
    "ExpectedType": "string",
    "ErrorCode": "INVALID_NRIC_TYPE",
    "Message": "NRIC must be a string"
  },
  {
    "RuleType": "Regex",
    "Path": "identifier.value",
    "Pattern": "^[STFG][0-9]{7}[A-Z]$",
    "ErrorCode": "INVALID_NRIC_FORMAT",
    "Message": "NRIC format must be valid Singapore NRIC"
  }
]
```

---

## Error Message Format

When validation fails, errors include:

```json
{
  "Code": "ERROR_CODE",
  "FieldPath": "path.to.field",
  "Message": "Custom message | Additional context",
  "Scope": "ResourceScope"
}
```

**Type validation error example:**
```
Birth date must be in YYYY-MM-DD format. | Expected type: 'date' | Actual value: '15-05-1990'
```

**Regex validation error example:**
```
NRIC format invalid | Pattern: '^[STFG][0-9]{7}[A-Z]$' | Actual value: 'S123456'
```

---

## Validation Options

Validation behavior can be customized using `ValidationOptions`:

```csharp
new ValidationOptions 
{
    StrictDisplayValidation = false,  // Strict or normalized display matching
    // ... other options
}
```

---

## Path Resolution

All rules use CPS1 (Custom Path Syntax 1) for path resolution:

**Features:**
- Dot notation: `Patient.name.family`
- Array indexing: `identifier[0].value`
- Filtering: `identifier[system:nric].value`
- Component matching: `component[code.coding.code:SQ-001].valueString`

---

## Best Practices

1. **Use Required + Type/Regex**: Combine for mandatory fields with format requirements
2. **Skip validation when optional**: Type and Regex rules automatically skip null/empty values
3. **Specific error codes**: Use descriptive error codes for easier debugging
4. **Clear messages**: Provide actionable error messages for vendors
5. **Test thoroughly**: Create test cases for each rule combination

---

## See Also

- [05 — Validation Engine Design](./05-validation-engine.md)
- [03 — FHIR Specification](./03-fhir-spec.md)
- [11 — Unit Test Plan](./11-unit-test-plan.md)

---

**Last Updated:** December 2025  
**Status:** Complete and Tested
