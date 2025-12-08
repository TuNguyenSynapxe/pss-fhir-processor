# Path Analysis System Upgrade

## Overview
The path analysis system has been comprehensively upgraded to provide detailed, context-aware error diagnostics with 5 distinct segment status types.

## What Changed

### 1. New 5-State Segment Status System

**Before**: Simple binary "exists" vs "missing"
```typescript
interface PathStatus {
  exists: boolean;
  segment: string;
}
```

**After**: Detailed status with 5 types
```typescript
type SegmentStatusKind =
  | 'EXISTS'                 // ✅ Segment found and usable
  | 'MISSING_PROPERTY'       // ❌ Property doesn't exist in object
  | 'MISSING_ARRAY'          // ❌ Array itself doesn't exist
  | 'FILTER_NO_MATCH'        // ⚠️ Array exists but no element matches filter
  | 'INDEX_OUT_OF_RANGE';    // ⚠️ Array exists but index too high

interface SegmentStatus {
  segment: PathSegment;
  status: SegmentStatusKind;
  node?: any;  // Actual value if EXISTS
}
```

### 2. Enhanced PathSegment Structure

**Before**:
```typescript
{
  type: 'property' | 'array' | 'filteredArray',
  name: string,
  index?: number,
  filter?: { property: string; value: string }
}
```

**After**:
```typescript
{
  raw: string,           // "extension[url:https://...]"
  kind: SegmentKind,     // 'property' | 'arrayIndex' | 'filteredArray'
  property: string,      // "extension"
  index?: number,        // 0
  filterKey?: string,    // "url"
  filterValue?: string   // "https://..."
}
```

### 3. Smart URL-Preserving Parser

**Problem**: URLs in filters were being broken at dots:
```
extension[url:https://fhir.synapxe.sg/StructureDefinition/...]
  ↓ OLD: Split on all dots
extension[url:https://fhir  <-- BROKEN
synapxe  <-- BROKEN
sg/...
```

**Solution**: Bracket-aware splitting preserves URLs:
```typescript
// Smart split algorithm
let bracketDepth = 0;
if (char === '.') {
  if (bracketDepth === 0) {
    // Only split on dots OUTSIDE brackets
    parts.push(currentPart);
  }
}
```

Result:
```
extension[url:https://fhir.synapxe.sg/StructureDefinition/...] ✅ PRESERVED
```

### 4. Scenario-Based Fix Instructions

The system now generates specific instructions for each failure type:

#### FILTER_NO_MATCH
```
❌ Array `extension` exists but no element has url="https://wrong.url"
✅ Fix: Add extension with correct url, or update existing extension's url
```

#### INDEX_OUT_OF_RANGE
```
❌ Array `coding` exists with 2 elements, but index [5] is out of range
✅ Fix: Either add more elements to reach index 5, or use existing indices [0-1]
```

#### Leaf-Missing (Property Only)
```
❌ Parent exists, but property `system` is missing
✅ Fix: Add "system": "your-value" to the object
```

#### Parent-Missing (Structural)
```
❌ Multiple parent levels missing starting at `identifier`
✅ Fix: Create nested structure with 3 levels (see example)
```

## Files Updated

### Core Utilities (4 files)

1. **pathParser.ts** (~460 lines)
   - Added `SegmentStatusKind` enum
   - Added `SegmentStatus` interface
   - Updated `PathSegment` interface
   - NEW: `resolvePathSegments()` - comprehensive path resolver
   - UPDATED: `parseFieldPath()` - smart bracket-aware split
   - UPDATED: `analyzePath()` - now wraps resolvePathSegments for backward compatibility
   - UPDATED: `formatSegment()` - simplified to return segment.raw

2. **helperTemplates.ts** (~277 lines)
   - UPDATED: `generateFixInstructions()` signature
     * Before: `(errorCode, missingSegmentIndex, segments[], ...)`
     * After: `(errorCode, segmentStatuses: SegmentStatus[], ...)`
   - Added `scenario` field: 'value-mismatch' | 'filter-no-match' | 'index-out-of-range' | 'parent-missing' | 'leaf-missing'
   - NEW: FILTER_NO_MATCH logic with specific guidance
   - NEW: INDEX_OUT_OF_RANGE logic with range suggestions
   - IMPROVED: Structural missing distinguishes leaf vs parent

3. **helperGenerator.ts** (~333 lines)
   - UPDATED: `HelperMessage.pathBreakdown` type
     * Before: `PathStatus[]`
     * After: `SegmentStatus[]`
   - UPDATED: `generateHelper()` - uses resolvePathSegments
   - NEW: `navigateToResource()` - extracts resource from Bundle
   - UPDATED: `formatSegmentForBreadcrumb()` - simplified
   - UPDATED: `navigateToParent()` - uses new property names

4. **snippetBuilder.ts** (~291 lines)
   - Updated all references: `segment.name` → `segment.property`
   - Updated all references: `segment.type` → `segment.kind`
   - Updated all references: `segment.filter.property` → `segment.filterKey`
   - Updated all references: `segment.filter.value` → `segment.filterValue`

### UI Component (1 file)

5. **ErrorHelperPanel.tsx**
   - UPDATED: Import `SegmentStatus` instead of `PathStatus`
   - REFACTORED: `PathBreakdownView` component
     * Now renders 5 different status types with appropriate icons
     * Color coding: ✅ green (EXISTS), ⚠️ orange (FILTER_NO_MATCH, INDEX_OUT_OF_RANGE), ❌ red (MISSING)
     * Status tags: "filter mismatch", "index out of range", "array missing", "property missing"
     * Shows node values for EXISTS segments

## Visual Improvements

### Old Breadcrumb Display
```
✅ Patient
✅ identifier
❌ extension[url:https://fhir    <-- BROKEN
❌ synapxe                        <-- BROKEN
❌ sg/...                         <-- BROKEN
```

### New Breadcrumb Display
```
✅ Patient
✅ identifier
⚠️ extension[url:https://fhir.synapxe.sg/...] [filter mismatch]
```

### Status Icons & Tags

| Status | Icon | Color | Tag |
|--------|------|-------|-----|
| EXISTS | ✅ | Green | (value info) |
| FILTER_NO_MATCH | ⚠️ | Orange | "filter mismatch" |
| INDEX_OUT_OF_RANGE | ⚠️ | Orange | "index out of range" |
| MISSING_ARRAY | ❌ | Red | "array missing" |
| MISSING_PROPERTY | ❌ | Red | "property missing" |

## Backward Compatibility

Legacy functions are maintained as wrappers:

```typescript
// OLD API still works
export function analyzePath(node: any, segments: PathSegment[]): PathStatus[] {
  const { statuses } = resolvePathSegments(node, segments);
  
  // Convert SegmentStatus[] to PathStatus[] for legacy code
  return statuses.map((ss, idx) => ({
    exists: ss.status === 'EXISTS',
    segment: ss.segment.raw,
    nodeValue: ss.node,
    isMissingParent: /* compute from previous statuses */,
    depth: idx,
  }));
}
```

## Testing Scenarios

### 1. Filter Mismatch
```json
// FHIR path: extension[url:https://correct.url].valueString
// Actual JSON has: extension[url:https://wrong.url]

Status breakdown:
✅ extension [array: 1 items]
⚠️ extension[url:https://correct.url] [filter mismatch]

Fix instruction:
"The array 'extension' exists with 1 element, but no element has url='https://correct.url'.
Either add a new extension with the correct url, or update the existing extension's url."
```

### 2. Index Out of Range
```json
// FHIR path: coding[5].system
// Actual JSON has: coding[0], coding[1] (only 2 elements)

Status breakdown:
✅ coding [array: 2 items]
⚠️ coding[5] [index out of range]

Fix instruction:
"The array 'coding' exists with 2 elements [0-1], but index [5] is out of range.
Either add 4 more elements to reach index 5, or use an existing index [0-1]."
```

### 3. Leaf Missing
```json
// FHIR path: identifier.system
// Actual JSON has: { identifier: { /* system missing */ } }

Status breakdown:
✅ identifier [object]
❌ system [property missing]

Fix instruction:
"The parent 'identifier' exists, but the property 'system' is missing.
Add this field to the existing object with the expected value."
```

### 4. Parent Missing
```json
// FHIR path: identifier.extension[0].valueString
// Actual JSON has: { /* no identifier */ }

Status breakdown:
❌ identifier [property missing]
❌ extension [array missing]
❌ extension[0] [index out of range]
❌ valueString [property missing]

Fix instruction:
"Multiple parent levels are missing starting at 'identifier'.
You need to create the nested structure with 4 levels (see complete example)."
```

## Benefits

1. **More Accurate Diagnostics**: Distinguishes 5 different failure types instead of just "missing"
2. **Better User Guidance**: Scenario-specific fix instructions
3. **URL-Safe**: Preserves complex URLs in FHIR paths
4. **Visual Clarity**: Color-coded icons and status tags
5. **Maintainable**: Clear separation of concerns, well-documented
6. **Backward Compatible**: Legacy code continues to work

## Code Metrics

- **Lines Added/Changed**: ~500 lines
- **Files Updated**: 5 files
- **New Functions**: 2 (resolvePathSegments, navigateToResource)
- **Refactored Functions**: 6
- **TypeScript Errors**: 0 (all clean)
- **Build Status**: ✅ Successful

## Next Steps

1. Test with real validation errors in playground
2. Verify all 5 status types display correctly
3. Test with complex nested paths
4. Verify fix instructions are helpful
5. Consider adding more visual indicators (progress bars, tree views)
6. Update ERROR_HELPER_README.md with new capabilities
