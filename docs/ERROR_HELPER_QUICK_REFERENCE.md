# Error Helper Panel - Quick Reference

## Component Updated ✅

The `ErrorHelperPanel` component now displays **5 distinct path segment statuses** with visual indicators:

## Visual Status Indicators

| Status | Icon | Color | Tag | Meaning |
|--------|------|-------|-----|---------|
| `EXISTS` | ✅ CheckCircle | Green | (value info) | Segment found and accessible |
| `FILTER_NO_MATCH` | ⚠️ CloseCircle | Orange | "filter mismatch" | Array exists but no element matches filter |
| `INDEX_OUT_OF_RANGE` | ⚠️ CloseCircle | Orange | "index out of range" | Array exists but index too high |
| `MISSING_ARRAY` | ❌ CloseCircle | Red | "array missing" | Array property doesn't exist |
| `MISSING_PROPERTY` | ❌ CloseCircle | Red | "property missing" | Object property doesn't exist |

## Example Displays

### 1. Filter Mismatch (Orange Warning)
```
Path: extension[url:https://fhir.synapxe.sg/screening-type].valueCodeableConcept

Display:
✅ extension [array: 2 items]
⚠️ extension[url:https://fhir.synapxe.sg/screening-type] [filter mismatch]

Meaning: 
- The extension array exists
- But none of the 2 elements have url="https://fhir.synapxe.sg/screening-type"
- Fix: Add extension with correct URL or update existing
```

### 2. Index Out of Range (Orange Warning)
```
Path: coding[5].system

Display:
✅ coding [array: 2 items]
⚠️ coding[5] [index out of range]

Meaning:
- The coding array exists with 2 elements [0-1]
- But you're trying to access index [5]
- Fix: Add more elements or use existing indices
```

### 3. Property Missing (Red Error)
```
Path: identifier.system

Display:
✅ identifier [object]
❌ system [property missing]

Meaning:
- The identifier object exists
- But it doesn't have a "system" property
- Fix: Add "system" property to identifier
```

### 4. Parent Missing (Red Error)
```
Path: identifier.extension[0].valueString

Display:
❌ identifier [property missing]
❌ extension [array missing]
❌ extension[0] [index out of range]
❌ valueString [property missing]

Meaning:
- The entire path is missing starting from "identifier"
- Multiple levels need to be created
- Fix: Create nested structure (see complete example)
```

## Component Props

```typescript
interface ErrorHelperPanelProps {
  error: ValidationError;
  json: any;  // The JSON tree (can be Bundle or resource)
  onJumpToLocation?: (entryIndex?: number, path?: string) => void;
}
```

## Usage in Playground

```typescript
import { ErrorHelperPanel } from '../components/ErrorHelperPanel';

// In your component
<ErrorHelperPanel
  error={validationError}
  json={jsonTree}  // Pass the parsed JSON
  onJumpToLocation={(entryIndex, path) => {
    // Handle navigation to error location
    console.log(`Jump to entry ${entryIndex}, path ${path}`);
  }}
/>
```

## Path Breakdown Section

The "Path Breakdown" section shows step-by-step navigation:

```tsx
{helper.pathBreakdown.map((segStatus, idx) => (
  <div style={{ paddingLeft: idx * 20 }}>
    {/* Icon based on status */}
    {/* Segment display */}
    {/* Status tag */}
    {/* Value info if EXISTS */}
  </div>
))}
```

### Indentation
- Each segment is indented 20px more than the previous
- Creates visual hierarchy showing nesting

### Value Display (for EXISTS)
- Objects: `[object]`
- Arrays: `[array: N items]`
- Primitives: `= "value"`

## How Fix Instructions Change

### Before (Generic)
```
Fix Instructions:
- Parent structure at identifier is missing
- Create the nested structure with 3 missing segments
- See example below
```

### After (Specific)
```
Fix Instructions (FILTER_NO_MATCH):
- The array 'extension' exists with 1 element
- No element has url='https://fhir.synapxe.sg/screening-type'
- Either add a new extension with the correct url
- Or update the existing extension's url to match

Fix Instructions (INDEX_OUT_OF_RANGE):
- The array 'coding' exists with 2 elements [0-1]
- Index [5] is out of range
- Either add 4 more elements to reach index 5
- Or use an existing index [0-1]

Fix Instructions (Leaf Missing):
- The parent 'identifier' exists
- But property 'system' is missing
- Add this field: "system": "expected-value"

Fix Instructions (Parent Missing):
- Multiple parent levels missing starting at 'identifier'
- Need to create 3 levels of nested structure
- See complete example below
```

## Color Coding Strategy

### Green (✅) - Success Path
- Everything is fine up to this point
- These segments exist and are accessible
- Continue analyzing deeper

### Orange (⚠️) - Partial Success
- The container exists but content doesn't match
- **FILTER_NO_MATCH**: Right array, wrong discriminator
- **INDEX_OUT_OF_RANGE**: Right array, wrong index
- These are "fixable" without restructuring

### Red (❌) - Complete Failure
- The segment doesn't exist at all
- **MISSING_PROPERTY**: Object property not found
- **MISSING_ARRAY**: Array property not found
- Requires adding new structure

## Implementation Notes

### URL Preservation
The parser preserves URLs in filters:
```
✅ CORRECT: extension[url:https://fhir.synapxe.sg/screening-type]
❌ OLD BUG: extension[url:https://fhir | synapxe | sg/screening-type]
```

### Bracket-Aware Splitting
```typescript
// Smart split algorithm
let bracketDepth = 0;
for (char of path) {
  if (char === '[') bracketDepth++;
  else if (char === ']') bracketDepth--;
  else if (char === '.' && bracketDepth === 0) {
    // Only split here
  }
}
```

### Status Detection Logic
```typescript
// In PathBreakdownView component
const isSuccess = segStatus.status === 'EXISTS';
const isFilterNoMatch = segStatus.status === 'FILTER_NO_MATCH';
const isIndexOutOfRange = segStatus.status === 'INDEX_OUT_OF_RANGE';
const isMissingArray = segStatus.status === 'MISSING_ARRAY';
const isMissingProperty = segStatus.status === 'MISSING_PROPERTY';
```

## Testing Checklist

- [ ] Test with filter mismatch error
  - Load bundle with wrong extension URL
  - Verify orange warning icon
  - Verify "filter mismatch" tag
  - Check fix instructions mention array exists

- [ ] Test with index out of range
  - Load bundle with coding[5] but only 2 elements
  - Verify orange warning icon
  - Verify "index out of range" tag
  - Check fix instructions show available range

- [ ] Test with leaf missing
  - Load bundle with identifier but no system
  - Verify red error icon
  - Verify "property missing" tag
  - Check fix instructions show parent exists

- [ ] Test with parent missing
  - Load bundle completely missing identifier
  - Verify all red error icons
  - Check fix instructions show structural creation needed

- [ ] Test URL preservation
  - Use extension with complex URL containing dots/colons
  - Verify breadcrumb shows full URL on one line
  - Verify no splitting on dots inside brackets

## Troubleshooting

### Issue: Wrong status displayed
**Check**: Is the JSON tree passed correctly to ErrorHelperPanel?
```typescript
// Make sure json is the actual parsed object
<ErrorHelperPanel json={JSON.parse(jsonString)} />
```

### Issue: Breadcrumb still breaking URLs
**Check**: Are you using the latest pathParser.ts?
```typescript
// Should see this in parseFieldPath:
if (char === '.' && bracketDepth === 0) {
  // Only split outside brackets
}
```

### Issue: No status tags showing
**Check**: Is SegmentStatus imported?
```typescript
import { SegmentStatus } from '../utils/pathParser';
```

### Issue: Colors not showing
**Check**: Ant Design styles loaded?
```typescript
import 'antd/dist/reset.css';
```

## Performance Notes

- Path resolution is efficient (single pass)
- Early termination on first non-existing segment
- No unnecessary deep cloning
- Minimal re-renders (useMemo on helper generation)

## Related Files

- `src/utils/pathParser.ts` - Core path analysis logic
- `src/utils/helperGenerator.ts` - Helper message orchestration
- `src/utils/helperTemplates.ts` - Fix instruction templates
- `src/utils/snippetBuilder.ts` - JSON snippet generation
- `src/components/ErrorHelperPanel.tsx` - UI component
- `docs/PATH_ANALYSIS_UPGRADE.md` - Detailed upgrade guide
