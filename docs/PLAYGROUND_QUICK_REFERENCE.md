# Playground UI Refactor - Quick Reference

## 📁 New File Structure

```
src/
├── components/
│   ├── PlaygroundLayout.tsx ⭐ Main container
│   ├── ValidationErrorCard.tsx ⭐ Enhanced errors
│   ├── TreeView.tsx ⭐ JSON tree
│   ├── JsonEditorModal.tsx ⭐ Modal editor
│   └── Splitter.tsx ⭐ Draggable divider
├── hooks/
│   ├── useLocalStorageWidth.ts 🪝 Width persistence
│   ├── useScrollToTreeNode.ts 🪝 Navigation
│   ├── useJSONFormatter.ts 🪝 JSON utils
│   └── useSplitter.ts 🪝 Drag logic
└── types/
    └── fhir.ts 📝 TypeScript definitions
```

## 🎯 Component Usage

### Import the Main Layout

```typescript
import { PlaygroundLayout } from './components/PlaygroundLayout';

// In your App component
<PlaygroundLayout />
```

### Use Custom Hooks

```typescript
import { useLocalStorageWidth, useJSONFormatter } from './hooks';

// Width persistence
const [width, setWidth] = useLocalStorageWidth({
  key: 'my-panel-width',
  defaultValue: 50,
  min: 30,
  max: 70
});

// JSON formatting
const { formatJSON, validateJSON } = useJSONFormatter();
const result = formatJSON(jsonString, 2);
```

## 🎨 Key Features

### 1. Resizable Panels
- Drag splitter to adjust width
- 30-70% range constraint
- Persists in localStorage
- Smooth visual feedback

### 2. Enhanced Error Cards
- Plain English explanations
- Breadcrumb navigation
- "Go to Resource" button
- Step-by-step fix instructions

### 3. JSON Editor Modal
- Monaco editor (IDE-like)
- Format, Validate, Reset buttons
- 90vw × 90vh size
- Auto-process on Apply

### 4. Smart Tree View
- Expand/Collapse all
- Color-coded values
- Virtualized (500+ nodes)
- Highlight on navigation

## 🔧 Configuration

### Customize Panel Width
```typescript
const DEFAULT_WIDTH = 50; // Change default split
const STORAGE_KEY = 'custom-key'; // Change localStorage key
```

### Customize Tree Height
```typescript
<Tree
  virtual
  height={1000} // Adjust height for virtualization
/>
```

### Customize Monaco Theme
```typescript
<Editor
  theme="vs-dark" // or 'vs-light', 'hc-black'
/>
```

## 🐛 Debugging

### Enable Console Logging
```typescript
// In PlaygroundLayout.tsx
const DEBUG = true;

useEffect(() => {
  if (DEBUG) console.log('State changed:', state);
}, [state]);
```

### Check TypeScript Errors
```bash
npm run build
# Look for type errors in output
```

### Verify Hook Behavior
```typescript
useEffect(() => {
  console.log('Width changed:', leftWidth);
}, [leftWidth]);
```

## 📊 Performance Tips

1. **Memoize Expensive Calculations**
```typescript
const treeData = useMemo(() => {
  return jsonToTreeData(fhirJson);
}, [fhirJson]);
```

2. **Use Callbacks for Handlers**
```typescript
const handleClick = useCallback(() => {
  // Handler logic
}, [dependencies]);
```

3. **Virtualize Large Lists**
```typescript
<Tree virtual height={800} />
```

## 🎨 Styling Guide

### Colors (Ant Design Palette)
- Primary: `#1890ff`
- Success: `#52c41a`
- Warning: `#faad14`
- Error: `#ff4d4f`
- Text: `#262626`, `#595959`, `#8c8c8c`

### Spacing
- Small: `8px`
- Medium: `16px`
- Large: `24px`

### Border Radius
- Small: `4px`
- Medium: `6px`
- Large: `8px`

### Shadows
- Subtle: `0 2px 8px rgba(0, 0, 0, 0.08)`
- Medium: `0 4px 12px rgba(0, 0, 0, 0.12)`

## 🚀 Deployment Checklist

- [ ] Run `npm run build` - verify no errors
- [ ] Test in Chrome, Firefox, Safari
- [ ] Verify responsive design (mobile/tablet)
- [ ] Test with large JSON files (>1MB)
- [ ] Verify localStorage works
- [ ] Test all error card scenarios
- [ ] Check "Go to Resource" navigation
- [ ] Test JSON editor modal (format, validate, reset)
- [ ] Verify splitter persists width
- [ ] Test expand/collapse all in tree view

## 📱 Responsive Breakpoints

| Screen | Width | Layout |
|--------|-------|--------|
| Mobile | < 768px | Vertical stack, no splitter |
| Tablet | 768-1199px | Stacked controls |
| Desktop | 1200px+ | Full 2-panel layout |

## ⌨️ Keyboard Shortcuts

**Monaco Editor:**
- `Ctrl/Cmd + F` - Find
- `Ctrl/Cmd + H` - Replace
- `Ctrl/Cmd + Z` - Undo
- `Alt + Click` - Multi-cursor

**Planned (Future):**
- `Ctrl/Cmd + S` - Apply changes
- `Ctrl/Cmd + /` - Toggle panel
- `Ctrl/Cmd + B` - Format JSON

## 🔗 Dependencies

```bash
# Install all dependencies
npm install

# Key packages
npm install @monaco-editor/react antd @ant-design/icons
```

## 📚 Documentation Links

- [Full Refactor Guide](./PLAYGROUND_UI_REFACTOR_TYPESCRIPT.md)
- [JSON Editor Features](./JSON_EDITOR_CODE_EDITOR_FEATURES.md)
- [Validation Helper Guide](./VALIDATION_HELPER_IMPLEMENTATION.md)

## 🆘 Quick Fixes

### Issue: Build fails
```bash
# Clear cache and rebuild
rm -rf node_modules dist
npm install
npm run build
```

### Issue: Type errors
```bash
# Check TypeScript config
cat tsconfig.json

# Install type definitions
npm install @types/react @types/react-dom
```

### Issue: Monaco not loading
```bash
# Verify package installed
npm list @monaco-editor/react

# Reinstall if needed
npm install @monaco-editor/react@latest
```

### Issue: Splitter not working
```typescript
// Verify hook is called
const { startDragging } = useSplitter({ onWidthChange });
console.log('Splitter initialized');
```

## 📈 Metrics to Monitor

- Build time: < 5 seconds
- Bundle size: < 400 KB (gzipped)
- Initial load: < 500ms
- Process time: < 2s
- Tree render: < 200ms

## ✅ Quality Checklist

**Code Quality:**
- [ ] All components TypeScript
- [ ] Props interfaces defined
- [ ] Hooks properly typed
- [ ] No `any` types (unless necessary)
- [ ] ESLint passes

**UX Quality:**
- [ ] Loading states shown
- [ ] Error messages user-friendly
- [ ] Smooth animations
- [ ] Responsive design
- [ ] Accessible (ARIA labels)

**Performance:**
- [ ] Tree virtualized (>500 nodes)
- [ ] Memoization used
- [ ] No unnecessary re-renders
- [ ] Bundle size optimized

---

**Version:** 1.0  
**Created:** December 7, 2025  
**Status:** ✅ Ready to Use
