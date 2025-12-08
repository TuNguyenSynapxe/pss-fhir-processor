# Playground V2 - Error Helper Panel Integration

## ✅ Integration Complete

The Error Helper Panel system has been successfully integrated into Playground V2.

---

## 🔄 Changes Made

### 1. **PlaygroundLayout.tsx**
- Added `jsonTree` state to store parsed FHIR JSON
- Parses JSON when processing starts and stores in state
- Passes `jsonTree` to `RightPanel`

```tsx
// New state
const [jsonTree, setJsonTree] = useState<any>(null);

// Parse JSON during processing
const handleProcess = async () => {
  // ... existing code
  try {
    parsedJson = JSON.parse(fhirJson);
    setJsonTree(parsedJson);
  } catch (e) {
    console.warn('Failed to parse JSON for error helper:', e);
  }
  // ... rest of processing
};
```

### 2. **RightPanel.tsx**
- Added `jsonTree?: any` prop
- Passes `jsonTree` to `ValidationTab`

```tsx
interface RightPanelProps {
  // ... existing props
  jsonTree?: any;
}

// Pass to ValidationTab
<ValidationTab 
  validation={result?.validation}
  onGoToResource={onGoToResource}
  jsonTree={jsonTree}
/>
```

### 3. **ValidationTab.tsx** (Complete Rewrite)
- ✅ Now uses `ErrorHelperPanel` from the new error helper system
- ✅ Groups errors by category (Missing, Value Mismatches, Format, Reference, Other)
- ✅ Collapsible groups with error counts
- ✅ Expand All / Collapse All buttons
- ✅ Export Errors to JSON functionality
- ✅ Each error shows in expandable panel with full helper details

**Error Groups:**
1. **Missing Required Fields** - MANDATORY_MISSING, Required rules
2. **Value Mismatches** - INVALID_CODE, FIXED_VALUE_MISMATCH, CodesMaster
3. **Format Errors** - REGEX_*, TYPE_*, Type rules
4. **Reference Errors** - REF_*, Reference rules
5. **Other Errors** - Any remaining errors

---

## 🎨 New Features in Validation Tab

### Error Organization
```
📊 Found 12 validation error(s)

[Expand All] [Collapse All] [Export Errors]

▼ Missing Required Fields (5)
  ├─ [MANDATORY_MISSING] Patient.name is required
  │   └─ [Expandable] Full error helper with:
  │       • What This Means
  │       • Location in Record (breadcrumb)
  │       • Expected Values
  │       • How to Fix (step-by-step)
  │       • Path Breakdown (visual tree)
  │       • Example JSON Snippet
  └─ ...

▼ Value Mismatches (4)
  └─ ...

▼ Format Errors (2)
  └─ ...

▼ Reference Errors (1)
  └─ ...
```

### Error Helper Panel Features
Each error now displays the full `ErrorHelperPanel` with:

1. **What This Means** 📖
   - Human-readable explanation
   - Common causes
   - Documentation links

2. **Location in Record** 📍
   - Entry index tag
   - Resource type tag
   - Full breadcrumb path
   - "Jump to Location" button

3. **Expected** ✓
   - Shows expected value/format
   - Tables for allowed codes
   - Reference types
   - Data type information

4. **How to Fix This** 🔧
   - Step-by-step instructions
   - Different guidance for missing leaf vs missing parent
   - Discriminator handling for filtered arrays

5. **Path Breakdown** 🌲
   - Visual tree showing where structure breaks
   - ✓ for existing segments
   - ✗ for missing segments
   - Shows actual values when available

6. **Example JSON Snippet** 📝
   - Minimal valid JSON with proper context
   - Copy-to-clipboard button
   - Only shows necessary parent structure

---

## 🎯 User Experience Improvements

### Before (Old ValidationErrorCard)
```
❌ [MANDATORY_MISSING] Required
Patient.extension[url:https://...].valueCodeableConcept.coding[0].system

[Show Help] → Shows basic explanation
```

### After (New ErrorHelperPanel)
```
📁 Missing Required Fields (1)
  ▼ [MANDATORY_MISSING] A required field is missing...
    
    📖 What This Means:
    The field Patient.extension[...].coding[0].system is required
    but is missing from the FHIR resource.
    
    📍 Location:
    Entry #0 → Patient → extension[url:...] → valueCodeableConcept 
    → coding[0] → system
    [Jump to Location]
    
    ✓ Expected:
    Must be CodeSystem URL string
    
    🔧 How to Fix:
    1. Locate the coding[0] object
    2. Add the "system" field
    3. Set it to the CodeSystem URL (e.g., "https://...")
    
    🌲 Path Breakdown:
    ✓ Patient
    ✓ extension[url:https://...]
    ✓ valueCodeableConcept
    ✓ coding
    ✓ coding[0]
    ✗ system ← MISSING
    
    📝 Example:
    {
      "system": "https://fhir.synapxe.sg/CodeSystem/ethnicity"
    }
    [Copy]
```

---

## 🚀 Testing

### Test the Integration

1. **Start the development server** (if not running):
   ```bash
   cd src/Pss.FhirProcessor.NetCore/Frontend
   npm run dev
   ```

2. **Navigate to Playground V2**:
   - Go to http://localhost:5173 (or your configured port)
   - Find the Playground V2 interface

3. **Load a FHIR Bundle**:
   - Use the "Load Sample" button to load a test bundle
   - Or paste your own FHIR JSON

4. **Process the Bundle**:
   - Click "Process" button
   - Backend will validate and return errors

5. **View Enhanced Error Helpers**:
   - Switch to "Validation" tab
   - See errors grouped by type
   - Expand any error to see full helper panel
   - Click "Jump to Location" to navigate to the error in tree view
   - Copy JSON snippets to fix errors

### Test Scenarios

#### ✅ Test 1: Missing Required Field
- Remove `Patient.name` from a bundle
- Process
- See error in "Missing Required Fields" group
- Expand to see path breakdown showing exactly where `name` is missing
- Copy example snippet

#### ✅ Test 2: Invalid Code Value
- Set a coding.code to an invalid value
- Process
- See error in "Value Mismatches" group
- View table of allowed codes
- See how to fix instructions

#### ✅ Test 3: Format Error (NRIC)
- Set invalid NRIC format
- Process
- See error in "Format Errors" group
- View NRIC format requirements

#### ✅ Test 4: Reference Error
- Remove a referenced resource
- Process
- See error in "Reference Errors" group
- See guidance on what resource is expected

---

## 🔧 Customization Options

### Adjust Error Grouping

Edit `ValidationTab.tsx` to change grouping logic:

```tsx
const errorGroups = {
  missing: errors.filter((e: any) => 
    e.code?.includes('MISSING') || e.ruleType === 'Required'
  ),
  // Add more custom groups...
};
```

### Add More Error Templates

Add new error code templates in `src/utils/helperTemplates.ts`:

```tsx
export const errorTemplates: Record<string, ErrorTemplate> = {
  YOUR_NEW_ERROR_CODE: {
    whatThisMeans: "Explanation of what this error means...",
    commonCauses: [
      "Cause 1",
      "Cause 2"
    ],
    documentationLink: "https://..."
  },
  // ... existing templates
};
```

### Customize Snippet Generation

Edit `src/utils/snippetBuilder.ts` to change how JSON snippets are generated:

```tsx
function generateSmartValue(
  fieldName: string,
  expectedValue: any,
  expectedType?: string
): any {
  // Your custom logic...
}
```

---

## 📊 Performance Considerations

### Current Implementation
- ✅ JSON parsed once during processing
- ✅ Error grouping happens on render (negligible cost)
- ✅ Helper generation is lazy (only when error expanded)
- ✅ Path analysis is O(n) where n = path depth

### For Large Error Lists (50+ errors)
Consider adding virtualization:

```tsx
import { List } from 'react-virtualized';

// Render only visible error panels
<List
  height={600}
  rowCount={errors.length}
  rowHeight={100}
  rowRenderer={({ index, key, style }) => (
    <div key={key} style={style}>
      <ErrorHelperPanel error={errors[index]} {...props} />
    </div>
  )}
/>
```

---

## 🐛 Troubleshooting

### Issue: "Cannot read property 'entry' of null"
**Cause**: `jsonTree` is null when errors are displayed  
**Fix**: Ensure JSON is parsed successfully before processing

```tsx
if (!jsonTree) {
  console.warn('JSON tree not available for error helper');
  return <OldValidationErrorCard error={error} />;
}
```

### Issue: Helper shows "Path not found"
**Cause**: Field path doesn't exist in JSON tree  
**Fix**: Check if backend is sending correct `fieldPath` format

### Issue: Collapse not working properly
**Cause**: Ant Design Collapse activeKey state management  
**Fix**: Ensure keys are strings and properly tracked in state

---

## 📚 Related Documentation

- **Error Helper System**: `/ERROR_HELPER_README.md`
- **Implementation Summary**: `/IMPLEMENTATION_SUMMARY.md`
- **Path Parser Tests**: `/src/__tests__/pathParser.test.ts`
- **Helper Generator Tests**: `/src/__tests__/helperGenerator.test.ts`

---

## 🎉 Summary

The Playground V2 now features:

✅ **Organized error display** with grouping and badges  
✅ **Comprehensive error helpers** with 6 detailed sections  
✅ **Interactive navigation** - jump to error location in tree  
✅ **Copy-paste JSON snippets** for quick fixes  
✅ **Visual path breakdown** showing exactly where errors occur  
✅ **Export functionality** for error reporting  
✅ **Expand/collapse all** for better UX  

Users can now understand and fix validation errors much more easily! 🚀
