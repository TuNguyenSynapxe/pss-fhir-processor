# Type Validation Rule - Quick Reference

## Rule Format
```json
{
  "RuleType": "Type",
  "PathType": "CPS1",
  "Path": "<CPS1 path>",
  "ExpectedType": "<type>",
  "ErrorCode": "<code>",
  "Message": "<message>"
}
```

## Supported Types

| Type | Description | Example |
|------|-------------|---------|
| `string` | Any text | `"hello"` |
| `integer` | Whole number (-2147483648 to 2147483647) | `"123"` |
| `decimal` | Decimal number | `"123.45"` |
| `boolean` | true or false | `"true"` |
| `guid` | UUID format | `"a1b2c3d4-e5f6-7890-abcd-ef1234567890"` |
| `date` | YYYY-MM-DD | `"2024-01-01"` |
| `datetime` | ISO-8601 | `"2024-01-01T00:00:00Z"` |
| `pipestring[]` | Pipe-separated (no empty parts) | `"A|B|C"` |
| `array` | JSON array | `"[1, 2, 3]"` |
| `object` | JSON object | `"{\"key\": \"value\"}"` |

## Quick Examples

### Date Validation
```json
{
  "RuleType": "Type",
  "Path": "Patient.birthDate",
  "ExpectedType": "date",
  "ErrorCode": "INVALID_BIRTH_DATE",
  "Message": "Birth date must be YYYY-MM-DD"
}
```

### Integer Validation
```json
{
  "RuleType": "Type",
  "Path": "Patient.age",
  "ExpectedType": "integer",
  "ErrorCode": "INVALID_AGE",
  "Message": "Age must be a whole number"
}
```

### Boolean Validation
```json
{
  "RuleType": "Type",
  "Path": "Patient.active",
  "ExpectedType": "boolean",
  "ErrorCode": "INVALID_ACTIVE",
  "Message": "Active must be true or false"
}
```

### Pipe-Separated Multi-Select
```json
{
  "RuleType": "Type",
  "Path": "component[code.coding.code:pure-tone].valueString",
  "ExpectedType": "pipestring[]",
  "ErrorCode": "INVALID_MULTI_SELECT",
  "Message": "Must be pipe-separated list"
}
```

## Common Patterns

### Required + Type
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

## Validation Rules

✅ **Valid:** Value matches expected type
❌ **Invalid:** Value doesn't match expected type
❌ **Invalid:** Path not found or null
❌ **Invalid:** ExpectedType missing
✅ **Valid (fallback):** ExpectedType is unknown/empty

## Case Sensitivity

- ExpectedType is **case-insensitive**
- `"date"`, `"Date"`, `"DATE"` are all valid

## Error Format
```
<Message> | Expected type: '<ExpectedType>' | Actual value: '<ActualValue>'
```

Example:
```
Birth date must be YYYY-MM-DD | Expected type: 'date' | Actual value: '15-05-1990'
```

## Type-Specific Rules

### boolean
- Only accepts: `"true"`, `"false"` (case-insensitive)
- Does NOT accept: `"1"`, `"0"`, `"yes"`, `"no"`

### date
- Format: `YYYY-MM-DD` (strict)
- Must have leading zeros: `2024-01-01` ✅, `2024-1-1` ❌

### pipestring[]
- All parts must be non-empty
- `"A|B|C"` ✅
- `"A||C"` ❌ (empty middle part)
- `"|B|C"` ❌ (empty first part)

### array / object
- Simple bracket/brace check
- Does NOT validate full JSON structure
- `"[...]"` format for arrays
- `"{...}"` format for objects

## See Also
- Full documentation: `docs/type-validation-examples.md`
- Implementation details: `docs/type-validation-implementation-summary.md`
- Source code: `src/Pss.FhirProcessor/Core/Validation/TypeChecker.cs`
