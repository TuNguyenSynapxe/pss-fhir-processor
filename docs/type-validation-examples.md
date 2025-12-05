# Type Validation Rule - Usage Examples

## Overview
The Type validation rule validates that field values match expected data types when resolved from CPS1 paths.

## Rule Definition Structure

```json
{
  "RuleType": "Type",
  "PathType": "CPS1",
  "Path": "<CPS1 path to field>",
  "ExpectedType": "<type name>",
  "ErrorCode": "<error code>",
  "Message": "<error message>"
}
```

## Supported Types

| ExpectedType | Description | Example Valid Values |
|--------------|-------------|---------------------|
| `string` | Any string value | `"test"`, `"hello world"`, `""` |
| `integer` | 32-bit integer | `"123"`, `"-456"`, `"0"` |
| `decimal` | Decimal number | `"123.45"`, `"-0.001"`, `"999.99"` |
| `boolean` | true or false (case-insensitive) | `"true"`, `"false"`, `"True"`, `"FALSE"` |
| `guid` | RFC4122 GUID format | `"a1b2c3d4-e5f6-7890-abcd-ef1234567890"` |
| `date` | YYYY-MM-DD format | `"2024-01-01"`, `"1990-12-31"` |
| `datetime` | ISO-8601 datetime | `"2024-01-01T00:00:00Z"`, `"2024-01-01T12:30:00+08:00"` |
| `pipestring[]` | Pipe-separated list with no empty parts | `"A|B|C"`, `"pure tone 250Hz|pure tone 500Hz"` |
| `array` | JSON array (simple check) | `"[]"`, `"[1, 2, 3]"` |
| `object` | JSON object (simple check) | `"{}"`, `"{\"key\": \"value\"}"` |

## Examples

### 1. Date Validation (Patient Birth Date)

```json
{
  "RuleType": "Type",
  "PathType": "CPS1",
  "Path": "Patient.birthDate",
  "ExpectedType": "date",
  "ErrorCode": "INVALID_BIRTH_DATE",
  "Message": "birthDate must be in YYYY-MM-DD format."
}
```

**Valid:** `"1990-05-15"`
**Invalid:** `"15-05-1990"`, `"1990/05/15"`, `"1990-5-15"`

### 2. Integer Validation (Patient Age)

```json
{
  "RuleType": "Type",
  "PathType": "CPS1",
  "Path": "extension[url:patient-age].valueInteger",
  "ExpectedType": "integer",
  "ErrorCode": "INVALID_AGE",
  "Message": "Patient age must be a whole number."
}
```

**Valid:** `"30"`, `"0"`, `"120"`
**Invalid:** `"30.5"`, `"abc"`, `""`

### 3. Boolean Validation

```json
{
  "RuleType": "Type",
  "PathType": "CPS1",
  "Path": "Patient.active",
  "ExpectedType": "boolean",
  "ErrorCode": "INVALID_ACTIVE_FLAG",
  "Message": "active field must be true or false."
}
```

**Valid:** `"true"`, `"false"`, `"True"`, `"FALSE"`
**Invalid:** `"1"`, `"0"`, `"yes"`, `"no"`

### 4. GUID Validation

```json
{
  "RuleType": "Type",
  "PathType": "CPS1",
  "Path": "identifier[system:uuid].value",
  "ExpectedType": "guid",
  "ErrorCode": "INVALID_UUID",
  "Message": "UUID must be in RFC4122 format."
}
```

**Valid:** `"a1b2c3d4-e5f6-7890-abcd-ef1234567890"`, `"00000000-0000-0000-0000-000000000000"`
**Invalid:** `"not-a-guid"`, `"12345678"`

### 5. Decimal Validation

```json
{
  "RuleType": "Type",
  "PathType": "CPS1",
  "Path": "Observation.valueQuantity.value",
  "ExpectedType": "decimal",
  "ErrorCode": "INVALID_MEASUREMENT",
  "Message": "Measurement value must be a valid decimal number."
}
```

**Valid:** `"123.45"`, `"0.001"`, `"999"`, `"-5.5"`
**Invalid:** `"abc"`, `"12.34.56"`

### 6. DateTime Validation

```json
{
  "RuleType": "Type",
  "PathType": "CPS1",
  "Path": "Observation.effectiveDateTime",
  "ExpectedType": "datetime",
  "ErrorCode": "INVALID_DATETIME",
  "Message": "effectiveDateTime must be ISO-8601 format."
}
```

**Valid:** `"2024-01-01T00:00:00Z"`, `"2024-12-31T23:59:59+08:00"`, `"2024-01-01T12:30:00.000Z"`
**Invalid:** `"not-a-datetime"`, `"2024-01-01 12:00:00"` (wrong format)

### 7. Pipe-Separated Array (Multi-Select Answers)

```json
{
  "RuleType": "Type",
  "PathType": "CPS1",
  "Path": "component[code.coding.code:pure-tone-test].valueString",
  "ExpectedType": "pipestring[]",
  "ErrorCode": "INVALID_MULTI_SELECT",
  "Message": "Pure tone test results must be pipe-separated with no empty values."
}
```

**Valid:** `"pure tone 250Hz|pure tone 500Hz|pure tone 1kHz"`, `"A|B|C"`, `"single"`
**Invalid:** `"|B|C"` (empty first), `"A||C"` (empty middle), `"A|B|"` (empty last)

### 8. Array Validation

```json
{
  "RuleType": "Type",
  "PathType": "CPS1",
  "Path": "extension[url:array-data].valueString",
  "ExpectedType": "array",
  "ErrorCode": "INVALID_ARRAY",
  "Message": "Value must be a JSON array."
}
```

**Valid:** `"[]"`, `"[1, 2, 3]"`, `"[\"a\", \"b\"]"`
**Invalid:** `"not an array"`, `"{\"key\": \"value\"}"`, `"[1, 2, 3"` (malformed)

### 9. Object Validation

```json
{
  "RuleType": "Type",
  "PathType": "CPS1",
  "Path": "extension[url:json-data].valueString",
  "ExpectedType": "object",
  "ErrorCode": "INVALID_OBJECT",
  "Message": "Value must be a JSON object."
}
```

**Valid:** `"{}"`, `"{\"key\": \"value\"}"`, `"{\"nested\": {\"key\": \"value\"}}"`
**Invalid:** `"not an object"`, `"[1, 2, 3]"`, `"{\"key\": \"value\""` (malformed)

## Complete Example in Validation Metadata

```json
{
  "Version": "7.0",
  "PathSyntax": "CPS1",
  "RuleSets": [
    {
      "Scope": "Patient",
      "Rules": [
        {
          "RuleType": "Required",
          "PathType": "CPS1",
          "Path": "Patient.birthDate",
          "ErrorCode": "MISSING_BIRTH_DATE",
          "Message": "Patient birth date is required."
        },
        {
          "RuleType": "Type",
          "PathType": "CPS1",
          "Path": "Patient.birthDate",
          "ExpectedType": "date",
          "ErrorCode": "INVALID_BIRTH_DATE",
          "Message": "Birth date must be in YYYY-MM-DD format."
        },
        {
          "RuleType": "Type",
          "PathType": "CPS1",
          "Path": "Patient.active",
          "ExpectedType": "boolean",
          "ErrorCode": "INVALID_ACTIVE",
          "Message": "active must be true or false."
        }
      ]
    },
    {
      "Scope": "Observation",
      "Rules": [
        {
          "RuleType": "Type",
          "PathType": "CPS1",
          "Path": "Observation.effectiveDateTime",
          "ExpectedType": "datetime",
          "ErrorCode": "INVALID_DATETIME",
          "Message": "effectiveDateTime must be ISO-8601 format."
        },
        {
          "RuleType": "Type",
          "PathType": "CPS1",
          "Path": "component[code.coding.code:pure-tone].valueString",
          "ExpectedType": "pipestring[]",
          "ErrorCode": "INVALID_PURE_TONE",
          "Message": "Pure tone results must be pipe-separated list."
        }
      ]
    }
  ]
}
```

## Error Messages

When a Type validation fails, the error message includes:
- The custom message from the rule
- The expected type
- The actual value that failed validation

Example error output:
```json
{
  "Code": "INVALID_BIRTH_DATE",
  "FieldPath": "Patient.birthDate",
  "Message": "Birth date must be in YYYY-MM-DD format. | Expected type: 'date' | Actual value: '15-05-1990'",
  "Scope": "Patient"
}
```

## Integration with Other Rules

Type validation can be combined with other rule types:

1. **Required + Type**: Ensure field exists AND has correct type
2. **Type + CodesMaster**: Validate type first, then validate against allowed values
3. **FixedValue + Type**: Less common, but can validate type consistency

Example combining Required and Type:
```json
[
  {
    "RuleType": "Required",
    "Path": "Patient.birthDate",
    "ErrorCode": "MISSING_BIRTH_DATE",
    "Message": "Birth date is required"
  },
  {
    "RuleType": "Type",
    "Path": "Patient.birthDate",
    "ExpectedType": "date",
    "ErrorCode": "INVALID_BIRTH_DATE",
    "Message": "Birth date must be YYYY-MM-DD"
  }
]
```

## Notes

- Type checking is **case-insensitive** for the `ExpectedType` field
- For `boolean` type, only `"true"` and `"false"` (case-insensitive) are accepted
- For `pipestring[]`, all parts must be non-empty after splitting by `|`
- For `array` and `object` types, only simple bracket/brace checks are performed (not full JSON validation)
- If `ExpectedType` is missing, null, or empty, the rule will fail with a validation error
- If the path is not found or returns null, the rule will fail
