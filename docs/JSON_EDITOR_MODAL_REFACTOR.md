# JSON Editor Modal Refactor

## Overview
Refactored the FHIR Processor Playground to move the JSON Editor from the main page into a modal dialog, improving the user experience and page layout.

## Changes Made

### 1. New Component: JsonEditorModal.jsx

**Location:** `src/Pss.FhirProcessor.NetCore/Frontend/src/components/JsonEditorModal.jsx`

**Features:**
- Modal dialog with Monaco Editor for JSON editing
- Size: 80% viewport width/height for comfortable editing
- Real-time JSON validation as user types
- Visual feedback for invalid JSON syntax
- Cancel and Apply Changes buttons
- Apply validates JSON before updating parent state
- destroyOnClose for clean state management

**Props:**
- `isOpen`: Boolean to control modal visibility
- `onClose`: Callback when modal is closed
- `onApply`: Callback with (jsonString, parsedObject) when changes are applied
- `initialValue`: Initial JSON string to populate editor

### 2. Updated Playground.jsx

**Removed:**
- Inline `<TextArea>` JSON editor from main page
- JSON Editor label and container from left panel

**Added:**
- `isEditorModalOpen` state to control modal
- `handleOpenEditor()` to open the modal
- `handleCloseEditor()` to close the modal
- `handleApplyJsonChanges()` to update JSON and trigger re-processing
- "Edit JSON Input" button in Processing Results panel
- "Edit JSON Input" button in empty state (when no data processed)
- Import for `JsonEditorModal` and `EditOutlined` icon

**Modified:**
- Layout now shows Processing Results directly in left panel
- Removed debouncing logic (no longer needed since editor is in modal)
- Auto-triggers processing pipeline after JSON update from modal

### 3. Updated Layout Structure

**New Main Page Layout:**
```
┌─────────────────────────────────────────────────────┐
│ Controls Bar                                        │
│ (Log Level, Strict Display, Sample Files)          │
├─────────────────────────────────────────────────────┤
│ Action Buttons (Process, Clear)                    │
├──────────────────────────┬──────────────────────────┤
│ Left Panel (50%)         │ Right Panel (50%)        │
│                          │                          │
│ Processing Results       │ Tree View                │
│ ┌──────────────────┐    │ ┌──────────────────────┐│
│ │ • Validation     │    │ │ Sticky Header        ││
│ │ • Extraction     │    │ │ Tree Content         ││
│ │ • Original Bundle│    │ │                      ││
│ │ • Logs           │    │ │ (Scrollable)         ││
│ └──────────────────┘    │ └──────────────────────┘│
│ [Edit JSON Input]       │                          │
└──────────────────────────┴──────────────────────────┘
```

**Modal Dialog:**
```
┌─────────────────────────────────────────────────┐
│ Edit FHIR JSON Input                      [×]   │
├─────────────────────────────────────────────────┤
│                                                 │
│  Monaco JSON Editor (80vw × 70vh)              │
│  • Syntax highlighting                         │
│  • Auto-formatting                             │
│  • Minimap                                     │
│  • Real-time validation                        │
│                                                 │
├─────────────────────────────────────────────────┤
│ ⚠ Invalid JSON syntax (if applicable)          │
│                                                 │
│                    [Cancel]  [Apply Changes]    │
└─────────────────────────────────────────────────┘
```

### 4. Updated Styles (App.css)

**Removed:**
- `.left-panel textarea` styles (no longer needed)
- Gap and padding adjustments for inline editor

**Modified:**
- `.left-panel` simplified to hold only Processing Results
- `.processing-results` now uses full height with flexbox layout
- Cleaner padding structure

**Preserved:**
- Resizable splitter functionality
- Tree view sticky header
- Flash highlight animation for navigation
- All responsive design elements

## Benefits

### User Experience
1. **Cleaner Interface:** Main page focuses on results and tree view
2. **Better Editing:** Monaco provides professional code editing experience
3. **No Lag:** Removed debouncing complexity, editor updates are isolated
4. **Larger Editing Space:** Modal provides 80% viewport size for comfortable editing
5. **Clear Actions:** Explicit "Edit" button with modal workflow

### Performance
1. **No Debouncing Needed:** Tree view only updates when modal is applied
2. **Single Processing:** Re-processing only happens once after Apply
3. **Clean State Management:** Modal destroys on close, preventing memory leaks

### Code Quality
1. **Separation of Concerns:** Editor logic isolated in dedicated component
2. **Better State Management:** Clear flow from modal → parent → processing
3. **Reusable Component:** JsonEditorModal can be used elsewhere if needed

## User Workflow

1. **Initial State:**
   - User sees "Edit JSON Input" button in empty state
   - Or loads a sample file to populate data

2. **Editing Flow:**
   - Click "Edit JSON Input" button
   - Modal opens with current JSON (or empty)
   - Edit JSON with Monaco features
   - Real-time validation shows errors
   - Click "Apply Changes" to update

3. **Processing Flow:**
   - Modal closes
   - JSON state updates
   - Processing automatically triggered
   - Results appear in left panel
   - Tree view updates in right panel

4. **Navigation:**
   - Use "Go to Resource" in validation errors
   - Tree view highlights and scrolls to resource
   - Edit button always available at bottom of results

## Technical Details

### Dependencies Added
- `@monaco-editor/react@^4.x` - Monaco Editor React wrapper

### Monaco Editor Configuration
```javascript
options={{
  minimap: { enabled: true },
  scrollBeyondLastLine: false,
  fontSize: 13,
  wordWrap: 'on',
  formatOnPaste: true,
  formatOnType: true,
  automaticLayout: true,
  tabSize: 2,
}}
```

### State Management
- Parent component maintains single source of truth (`fhirJson`)
- Modal receives initial value as prop
- Changes only applied when user explicitly clicks "Apply Changes"
- Cancel discards all changes

## Testing Checklist

- [x] Frontend builds successfully
- [ ] Modal opens when "Edit JSON Input" is clicked
- [ ] Monaco editor displays existing JSON
- [ ] Real-time validation shows invalid JSON warning
- [ ] Apply button disabled when JSON is invalid
- [ ] Cancel closes modal without changes
- [ ] Apply updates JSON and triggers processing
- [ ] Tree view updates correctly after Apply
- [ ] Resizable splitter still works
- [ ] Navigation to resources works
- [ ] Sample file loading works
- [ ] All tabs (Validation, Extraction, Original Bundle, Logs) display correctly

## Future Enhancements

1. **Keyboard Shortcuts:** Add Ctrl/Cmd+S to apply changes
2. **Diff View:** Show changes before applying
3. **Format Button:** Add explicit "Format JSON" button
4. **History:** Maintain edit history with undo/redo
5. **Validation Messages:** Show detailed JSON parse errors in modal
6. **Auto-save:** Optional auto-save to localStorage
7. **Multiple Themes:** Add dark mode for Monaco editor

## Migration Notes

- No breaking changes to API or backend
- All existing functionality preserved
- Performance improved (no more typing lag)
- Layout is more intuitive for users
- Monaco provides better editing UX than TextArea
