# Discriminator Helper Guide

## Overview

The validation helper system now intelligently detects and provides specific guidance for three distinct error scenarios:

1. **Child Missing** - Simple case where only the final field is missing
2. **Parent Missing** - Structural issue where parent objects/arrays don't exist
3. **Discriminator Parent Missing** - Array exists but no element matches the discriminator filter

## Discriminator Detection

### What is a Discriminator?

In FHIR paths, discriminators are filters that identify specific array elements by a property value:

```
identifier[system:https://fhir.synapxe.sg/NamingSystem/nric].value
           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
           This is the discriminator - filters for specific system
```

### Detection Logic

The helper parses paths to detect patterns like:
- `identifier[system:VALUE]`
- `extension[url:VALUE]`
- `coding[code:VALUE]`

When a discriminator is found AND there are child segments after it, the error is classified as `discriminatorParentMissing`.

## Example Scenarios

### Scenario 1: Child Missing (Green Box)

**Error Path:** `Patient.name`

**Detection:**
- No discriminator in path
- Parent path exists
- Only final field missing

**Helper Output:**
```
✅ How to fix this:
1. Open the Patient resource (entry #0) in your bundle
2. Add the name field with an appropriate value
3. Ensure the field is not empty or null
4. Save your changes and validate again
```

### Scenario 2: Discriminator Parent Missing (Orange Box)

**Error Path:** `Patient.identifier[system:https://fhir.synapxe.sg/NamingSystem/nric].value`

**Detection:**
- Discriminator found: `identifier[system:...]`
- Child segment exists: `.value`
- Classified as discriminator issue

**Helper Output:**
```
⚠️ How to fix this (array discriminator issue):
1. Open the Patient resource (entry #0) in your bundle
2. ⚠️ STRUCTURAL FIX REQUIRED: The identifier array needs an entry with 
   system = "https://fhir.synapxe.sg/NamingSystem/nric"
3. Locate the "identifier" array in the resource
4. Check if any identifier element has system: "https://fhir.synapxe.sg/NamingSystem/nric"
5. If no matching element exists, add a new object to the identifier array with:
   • system: "https://fhir.synapxe.sg/NamingSystem/nric"
6. Once the matching identifier element exists, add the missing value field to it
7. Provide an appropriate value for value
8. Save your changes and validate again
```

**What This Means:**
```
This patient is missing the Value field within a specific identifier entry. 
The system could not find an identifier element with 
system="https://fhir.synapxe.sg/NamingSystem/nric", 
or found it but the value is missing.
```

### Scenario 3: Generic Parent Missing (Orange Box)

**Error Path:** `Patient.contact.name`

**Detection:**
- No discriminator
- Parent path analysis shows `contact` doesn't exist
- PathMismatchSegment = "contact"

**Helper Output:**
```
⚠️ How to fix this (requires structural changes):
1. Open the Patient resource (entry #0) in your bundle
2. ⚠️ WARNING: The parent path segment "contact" does not exist in the resource
3. Check for misspellings or incorrect path structure
4. Ensure the parent object/array structure matches the expected FHIR schema
5. After fixing the parent structure, add the name field
6. Save your changes and validate again
```

## Visual Indicators

### Color Coding

- **Green Background**: Simple fix - just add the missing field
- **Orange Background**: Complex fix - structural changes needed

### Text Styling

- **Bold Orange Text**: Steps starting with ⚠️ indicate critical structural issues
- **Regular Text**: Standard navigation/value-setting steps

## Implementation Details

### Frontend (validationHelper.js)

```javascript
// Parse discriminator from path segment
function parseDiscriminator(segment) {
  const match = segment.match(/^([^\[]+)\[([^:]+):(.+)\]$/);
  if (match) {
    return {
      arrayName: match[1],  // e.g., "identifier"
      field: match[2],       // e.g., "system"
      value: match[3]        // e.g., "https://..."
    };
  }
  return null;
}

// Detect scenario
function detectDiscriminatorScenario(fieldPath) {
  const segments = fieldPath.split('.');
  for (let i = 0; i < segments.length; i++) {
    const discriminator = parseDiscriminator(segments[i]);
    if (discriminator) {
      const hasChildSegments = i < segments.length - 1;
      if (hasChildSegments) {
        return {
          discriminator,
          childSegments: segments.slice(i + 1)
        };
      }
    }
  }
  return null;
}
```

### TypeScript Types

```typescript
interface Helper {
  // ... other fields
  fixScenario?: 'childMissing' | 'parentMissing' | 'discriminatorParentMissing';
  discriminator?: {
    field: string;
    value: string;
  };
}
```

## Testing

### Test Case 1: NRIC Value Missing

**Input:**
```json
{
  "resourceType": "Patient",
  "identifier": [
    {
      "system": "https://fhir.synapxe.sg/NamingSystem/fin",
      "value": "F1234567A"
    }
    // Missing NRIC identifier
  ]
}
```

**Error:** `Patient.identifier[system:https://fhir.synapxe.sg/NamingSystem/nric].value`

**Result:** Discriminator parent missing detected → Orange box with structural guidance

### Test Case 2: NRIC Value Present but Empty

**Input:**
```json
{
  "resourceType": "Patient",
  "identifier": [
    {
      "system": "https://fhir.synapxe.sg/NamingSystem/nric"
      // Missing 'value' field
    }
  ]
}
```

**Error:** `Patient.identifier[system:https://fhir.synapxe.sg/NamingSystem/nric].value`

**Result:** Discriminator parent exists, but child missing → Still shows discriminator guidance to ensure value is added to correct identifier

## Benefits

1. **Context-Aware Guidance**: Different instructions for different structural issues
2. **Visual Hierarchy**: Color coding helps developers quickly identify complexity
3. **Specific Instructions**: Tells developers exactly which array element needs the field
4. **Reduced Confusion**: Clarifies whether array itself exists vs. matching element exists

## Future Enhancements

- Backend path analysis to populate `PathAnalysis.ParentPathExists` automatically
- Support for nested discriminators: `extension[url:X].extension[url:Y].valueString`
- Interactive JSON navigator that highlights the exact path in the editor
