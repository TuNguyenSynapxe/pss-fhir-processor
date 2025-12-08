# Error Helper Panel System

## Overview

The Error Helper Panel system provides comprehensive, contextual help messages for FHIR validation errors. It transforms minimal backend error objects into detailed, actionable guidance with code examples.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Backend Error Object                      │
│  { code, message, fieldPath, scope, rule, resourcePointer } │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
         ┌────────────────────────┐
         │   Helper Generator     │
         │  generateHelper()      │
         └────────┬───────────────┘
                  │
        ┌─────────┼─────────┐
        │         │         │
        ▼         ▼         ▼
   ┌────────┐ ┌──────┐ ┌─────────┐
   │ Path   │ │Helper│ │Snippet  │
   │Parser  │ │Tmpl  │ │Builder  │
   └────────┘ └──────┘ └─────────┘
        │         │         │
        └─────────┼─────────┘
                  │
                  ▼
         ┌────────────────────────┐
         │   Helper Message       │
         │  { whatThisMeans,      │
         │    location, expected, │
         │    howToFix, snippet,  │
         │    pathBreakdown }     │
         └────────┬───────────────┘
                  │
                  ▼
         ┌────────────────────────┐
         │  ErrorHelperPanel      │
         │  (React Component)     │
         └────────────────────────┘
```

## Core Components

### 1. Path Parser (`utils/pathParser.ts`)

Parses and analyzes FHIR path expressions:

```typescript
// Parse path into structured segments
const segments = parseFieldPath('Patient.extension[url:https://...].valueCodeableConcept.coding[0].system');

// Analyze which segments exist in JSON
const statuses = analyzePath(jsonTree, segments);

// Find first missing segment
const missingIndex = findFirstMissingSegment(statuses);
```

**Supported Path Types:**
- Simple properties: `Patient.name`
- Array indices: `coding[0]`
- Filtered arrays: `extension[url:https://...]`
- Wildcards: `component[*]`

### 2. Helper Templates (`utils/helperTemplates.ts`)

Provides pre-written explanations for each error code:

```typescript
const template = getErrorTemplate('MANDATORY_MISSING');
// Returns: { whatThisMeans, commonCauses, documentationLink }
```

**Supported Error Codes:**
- `MANDATORY_MISSING` - Required field missing
- `FIXED_VALUE_MISMATCH` - Wrong fixed value
- `INVALID_CODE` - Code not in CodeSystem
- `TYPE_MISMATCH` - Wrong data type
- `REGEX_INVALID_NRIC` - Invalid NRIC format
- `REGEX_INVALID_POSTAL` - Invalid postal code
- `ID_FULLURL_MISMATCH` - ID doesn't match fullUrl
- `REF_*` - Reference errors
- `INVALID_ANSWER_VALUE` - CodesMaster validation

### 3. Snippet Builder (`utils/snippetBuilder.ts`)

Generates minimal valid JSON examples:

```typescript
const snippet = buildJsonSnippet(segments, missingIndex, {
  expectedValue: 'https://fhir.synapxe.sg/CodeSystem/ethnicity',
  expectedType: 'string',
});

// Returns:
// {
//   "valueCodeableConcept": {
//     "coding": [{
//       "system": "https://fhir.synapxe.sg/CodeSystem/ethnicity"
//     }]
//   }
// }
```

### 4. Helper Generator (`utils/helperGenerator.ts`)

Main orchestrator that combines all components:

```typescript
const helper = generateHelper(error, jsonTree);

// Returns:
// {
//   whatThisMeans: ErrorCodeTemplate,
//   location: LocationInfo,
//   expected: ExpectedInfo,
//   howToFix: FixInstructions,
//   exampleSnippet: string,
//   pathBreakdown: PathStatus[],
//   completeExample?: string
// }
```

### 5. ErrorHelperPanel Component (`components/ErrorHelperPanel.tsx`)

React component that renders the helper message:

```tsx
<ErrorHelperPanel
  error={validationError}
  json={fhirBundle}
  onJumpToLocation={(entryIndex, path) => {
    // Navigate JSON viewer to specific location
  }}
/>
```

## Usage Examples

### Example 1: Missing System in Coding

**Backend Error:**
```json
{
  "code": "MANDATORY_MISSING",
  "message": "Ethnicity coding system is mandatory.",
  "fieldPath": "Patient.extension[url:https://fhir.synapxe.sg/StructureDefinition/ext-ethnicity].valueCodeableConcept.coding[0].system",
  "scope": "Patient",
  "resourcePointer": {
    "entryIndex": 0,
    "resourceType": "Patient"
  }
}
```

**Generated Helper:**
- ✅ **What This Means:** Required field missing explanation
- ✅ **Location:** Entry #0 → Patient → extension[url:...] → valueCodeableConcept → coding[0] → system
- ✅ **Expected:** Must be string with CodeSystem URL
- ✅ **How to Fix:** 3 step-by-step instructions
- ✅ **Example Snippet:**
  ```json
  {
    "system": "https://fhir.synapxe.sg/CodeSystem/ethnicity"
  }
  ```
- ✅ **Path Breakdown:**
  - ✓ Patient
  - ✓ extension[url:...]
  - ✓ valueCodeableConcept
  - ✓ coding
  - ✓ coding[0]
  - ✗ **system** ← missing

### Example 2: Invalid Code Value

**Backend Error:**
```json
{
  "code": "INVALID_CODE",
  "message": "Ethnicity code must be a valid value from the ethnicity CodeSystem.",
  "fieldPath": "Patient.extension[url:...].valueCodeableConcept.coding[0].code",
  "scope": "Patient",
  "rule": {
    "allowedValues": ["CN", "MY", "IN", "XX"]
  }
}
```

**Generated Helper:**
- ✅ **Expected:** Table showing all 4 allowed codes
- ✅ **How to Fix:** "Change the value to one of the allowed values"
- ✅ **Example Snippet:**
  ```json
  {
    "code": "CN"
  }
  ```

### Example 3: Missing Parent Structure

**Backend Error:**
```json
{
  "code": "MANDATORY_MISSING",
  "fieldPath": "Patient.communication.language.coding.system"
}
```

**Path Analysis:**
- ✓ Patient
- ✗ **communication** ← missing (parent)
- ✗ language ← missing (child)
- ✗ coding ← missing (child)
- ✗ system ← missing (child)

**Generated Helper:**
- ✅ **How to Fix:** "You need to create 4 missing level(s)"
- ✅ **Example Snippet:**
  ```json
  {
    "communication": {
      "language": {
        "coding": {
          "system": "urn:ietf:bcp:47"
        }
      }
    }
  }
  ```

## Integration Guide

### Step 1: Import Components

```tsx
import ErrorHelperPanel from '@/components/ErrorHelperPanel';
import { ValidationError } from '@/utils/helperGenerator';
```

### Step 2: Pass Validation Results

```tsx
function MyValidationTab() {
  const [validationResult, setValidationResult] = useState<{
    isValid: boolean;
    errors: ValidationError[];
  } | null>(null);

  const [jsonInput, setJsonInput] = useState<any>(null);

  return (
    <div>
      {validationResult?.errors.map((error, idx) => (
        <ErrorHelperPanel
          key={idx}
          error={error}
          json={jsonInput}
          onJumpToLocation={(entryIndex, path) => {
            // Your navigation logic
          }}
        />
      ))}
    </div>
  );
}
```

### Step 3: Handle Jump to Location (Optional)

```tsx
const handleJumpToLocation = (entryIndex?: number, path?: string) => {
  // Navigate Monaco editor or JSON tree viewer
  if (entryIndex !== undefined) {
    // Scroll to Bundle.entry[entryIndex]
  }
  if (path) {
    // Highlight specific field in JSON viewer
  }
};
```

## Testing

### Run Unit Tests

```bash
npm test -- pathParser.test.ts
npm test -- helperGenerator.test.ts
```

### Test Coverage

- ✅ Path parsing (simple, array, filtered, nested)
- ✅ Path analysis (existing, missing leaf, missing parent)
- ✅ Helper generation for all error codes
- ✅ Snippet building with various options
- ✅ Integration tests with real FHIR examples

## Extension Points

### Adding New Error Codes

1. Add template in `helperTemplates.ts`:
   ```typescript
   export const ERROR_CODE_TEMPLATES: Record<string, ErrorCodeTemplate> = {
     MY_NEW_ERROR: {
       whatThisMeans: 'Clear explanation...',
       commonCauses: ['Cause 1', 'Cause 2'],
     },
   };
   ```

2. Add special handling in `helperGenerator.ts` if needed:
   ```typescript
   if (error.code === 'MY_NEW_ERROR') {
     // Custom logic
   }
   ```

### Custom Snippet Builders

Add custom logic in `snippetBuilder.ts`:

```typescript
export function buildCompleteExample(errorCode: string, ...): any {
  if (errorCode === 'MY_CUSTOM_STRUCTURE') {
    return {
      // Your custom example
    };
  }
  return null;
}
```

## Performance Considerations

- Path parsing is O(n) where n = number of segments
- Path analysis is O(n*m) where m = depth of JSON tree
- Memoize `generateHelper()` results in React components
- Use `useMemo` for expensive computations

## Future Enhancements

- [ ] Add documentation links for each error code
- [ ] Generate fix suggestions based on similar valid records
- [ ] Interactive path navigator with clickable segments
- [ ] Auto-fix button that applies corrections
- [ ] Batch error fixing (apply all suggested fixes)
- [ ] Error statistics and patterns analysis
- [ ] Integration with FHIR validation server for real-time checks

## Troubleshooting

### Helper Panel Not Showing

- Check that `error` object has required fields: `code`, `fieldPath`
- Verify `json` prop contains the actual FHIR resource (not Bundle)
- Check browser console for TypeScript errors

### Path Breakdown Shows All Missing

- Ensure you're passing the correct resource (e.g., `entry[0].resource` not entire Bundle)
- Check if `resourcePointer.entryIndex` is set correctly

### Example Snippets Are Too Generic

- Provide `rule.expectedValue` or `rule.allowedValues` in error object
- Add custom builders in `snippetBuilder.ts` for your specific structures

## License

Part of PSS FHIR Processor - Internal Synapxe Tool
