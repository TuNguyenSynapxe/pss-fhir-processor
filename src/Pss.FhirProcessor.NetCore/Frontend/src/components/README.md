# FHIR Playground V2 - Refactored Architecture

## Overview
This is the refactored FHIR Playground with a clean two-panel layout, improved performance, and better UX.

## Architecture

### Component Structure
```
playground-v2/
├── PlaygroundLayout.tsx       # Main container with state management
├── TreeViewPanel.tsx          # Left panel - virtualized tree view
├── RightPanel.tsx             # Right panel - controls + results tabs
├── Splitter.tsx               # Draggable panel divider
├── JsonEditorModal.tsx        # Modal editor with Monaco
├── ValidationTab.tsx          # Validation results tab
├── ValidationErrorCard.tsx    # Individual error card with help
├── ExtractionTab.tsx          # Extraction results tab
├── BundleTab.tsx              # Original bundle tab
├── LogsTab.tsx                # Logs tab
├── types.ts                   # TypeScript type definitions
└── index.ts                   # Module exports
```

### Key Features

#### 1. Two-Panel Layout
- **Left Panel (40% default)**: Tree View only
  - Sticky header with expand/collapse buttons
  - Edit button to open JSON modal
  - Virtualized for >500 nodes
  - Auto-scroll to entry on "Go to Resource"
  
- **Right Panel (60% default)**: Processing controls + results
  - Process, Clear, Load Sample buttons
  - Log level and Strict Display toggle
  - Tabs: Validation | Extraction | Bundle | Logs
  
- **Draggable Splitter**: Resize panels (20-70% range)
- **Persistent Width**: Saved to localStorage

#### 2. JSON Editing
- **Modal Only**: No inline editor
- Monaco Editor with features:
  - Format JSON
  - Validate JSON
  - Reset to initial
  - Apply Changes
- Updates tree + results after closing

#### 3. Validation Error Cards
- Summary with error code and rule type
- "Show Help" toggle for details
- Human-readable explanations
- Breadcrumb path
- "Go to Resource" button → scrolls tree view
- Expected vs Actual values
- Step-by-step fix instructions

#### 4. Performance Optimizations
- No inline JSON rendering
- Tree virtualization for large bundles
- React memoization
- Updates only after processing

## Usage

### To Use This New Playground

#### Option 1: Replace Existing (Recommended)
```jsx
// In App.jsx, replace the import:
import Playground from './components/playground-v2';
```

#### Option 2: A/B Testing
```jsx
// In App.jsx, add a toggle:
import PlaygroundV1 from './components/Playground';
import PlaygroundV2 from './components/playground-v2';

const [useV2, setUseV2] = useState(true);
// ... render based on useV2 flag
```

### Props & Configuration

#### PlaygroundLayout
No props required - self-contained with metadata context.

```tsx
<PlaygroundLayout />
```

#### Storage Keys
- `pss_playground_left_width`: Panel width (percentage)

### API Integration
Uses existing `fhirApi.process()` from `services/api.js`:
- Sends FHIR JSON + validation metadata
- Receives validation, extraction, logs, bundle

## Migration from V1

### Breaking Changes
- None - fully backward compatible
- Uses same API endpoints
- Uses same metadata context
- Uses same utility functions

### New Features
- Two-panel layout (was single column)
- Modal-only JSON editing (was inline)
- Virtualized tree (no limit before)
- Splitter with persistence
- Enhanced error cards

### Removed Features
- Inline JSON editor (moved to modal)
- Single-column layout

## Development

### Adding New Tab
1. Create `NewTab.tsx` in `playground-v2/`
2. Import in `RightPanel.tsx`
3. Add to `tabItems` array

```tsx
{
  key: 'newtab',
  label: 'New Tab',
  children: <NewTab data={result?.newData} />
}
```

### Customizing Tree View
Edit `TreeViewPanel.tsx`:
- `VIRTUALIZATION_THRESHOLD`: Change from 500
- `defaultExpandedKeys`: Modify expansion logic
- Node rendering: Edit `buildNode()` function

### Customizing Error Cards
Edit `ValidationErrorCard.tsx`:
- Modify `generateHelper()` in `utils/validationHelper.js`
- Add new sections
- Change styling

## TypeScript Support
- Full TypeScript for all new components
- Type definitions in `types.ts`
- Vite environment types in `vite-env.d.ts`

## Testing

### Manual Testing Checklist
- [ ] Load sample FHIR bundle
- [ ] Process validation
- [ ] Click "Show Help" on error
- [ ] Click "Go to Resource" button
- [ ] Tree scrolls to correct entry
- [ ] Drag splitter left/right
- [ ] Width persists after refresh
- [ ] Open JSON editor modal
- [ ] Format/Validate JSON
- [ ] Apply changes
- [ ] Tree updates
- [ ] Check all tabs (Validation, Extraction, Bundle, Logs)
- [ ] Expand/Collapse all tree nodes
- [ ] Test with >500 node bundle (virtualization)

### Performance Testing
```javascript
// Generate large bundle for testing
const largeBundle = {
  resourceType: "Bundle",
  entry: Array.from({ length: 1000 }, (_, i) => ({
    fullUrl: `urn:uuid:test-${i}`,
    resource: {
      resourceType: "Patient",
      id: `test-${i}`,
      // ... more fields
    }
  }))
};
```

## Troubleshooting

### Tree Not Scrolling to Entry
- Check `scrollTargetRef` in `PlaygroundLayout`
- Verify `entryIndex` in resourcePointer
- Check console for navigation events

### Splitter Not Working
- Check mouse event handlers in `Splitter.tsx`
- Verify width constraints (MIN_WIDTH, MAX_WIDTH)
- Check localStorage for corrupted values

### Editor Modal Not Updating Tree
- Verify `onApply` callback chain
- Check `setFhirJson` in PlaygroundLayout
- Ensure `useMemo` dependencies for `treeData`

### TypeScript Errors
- Run `npm install --save-dev @types/react @types/node`
- Check `vite-env.d.ts` for import.meta.glob
- Verify `types.ts` exports

## Future Enhancements
- [ ] Add search/filter in tree view
- [ ] Syntax highlighting in tree nodes
- [ ] Collapsible error card sections
- [ ] Export validation report
- [ ] Diff view for before/after edits
- [ ] Keyboard shortcuts
- [ ] Dark mode support
- [ ] Responsive mobile layout

## Version History
- **v2.0** (2025-12-07): Initial refactored release
  - Two-panel layout with splitter
  - Modal JSON editor
  - Virtualized tree view
  - Enhanced error cards
  - TypeScript support
