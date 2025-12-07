# JSON Editor Modal - Quick Reference

## Opening the Editor
- Click **"Edit JSON Input"** button in Processing Results panel
- Or click **"Edit JSON Input"** in empty state

## Modal Controls

### Header Buttons (Right Side)

| Button | Icon | Function | Shortcut (Future) |
|--------|------|----------|-------------------|
| Format JSON | 🎨 | Validate and pretty-print JSON with 2-space indentation | Ctrl/Cmd+F |
| Validate JSON | ✓ | Check JSON syntax and show errors with line/column | Ctrl/Cmd+V |
| Reset | ↻ | Restore editor to initial state when modal opened | Ctrl/Cmd+R |
| Close | × | Close modal and discard changes | Esc |

### Footer Buttons

| Button | Function |
|--------|----------|
| Cancel | Close modal without applying changes |
| Apply Changes | Validate, format, update parent, and close modal |

## Editor Features

### Monaco Editor Capabilities
- **Syntax Highlighting:** JSON tokens colorized
- **Line Numbers:** Easy navigation
- **Minimap:** Overview of document structure
- **Auto-complete:** Bracket matching
- **Word Wrap:** Long lines wrapped automatically
- **Monospace Font:** Fira Code, Consolas, or Monaco

### Validation Feedback
- **Inline Error Alert:** Shows above editor when validation fails
- **Error Details:**
  - Error message (e.g., "Unexpected token }")
  - Line number
  - Column number
- **Cursor Navigation:** Automatically moves to error position
- **Closable Alert:** Click × to dismiss

## Workflows

### ✏️ Edit and Apply
```
1. Click "Edit JSON Input"
2. Make changes in Monaco editor
3. Click "Apply Changes"
4. JSON validated and formatted
5. Processing pipeline runs
6. Results appear in left panel
7. Tree view updates in right panel
```

### 🎨 Format Unstructured JSON
```
1. Paste minified/unformatted JSON
2. Click "Format JSON"
3. Editor updates with pretty-printed JSON
4. Continue editing or apply
```

### ✓ Validate Before Applying
```
1. Edit JSON content
2. Click "Validate JSON"
3. See ✓ success or ⚠ error
4. Fix errors if any
5. Click "Apply Changes"
```

### ↻ Reset to Start Over
```
1. Made unwanted changes?
2. Click "Reset"
3. Editor restores to initial state
4. Start editing again
```

## Tree View Controls

### Expand/Collapse Buttons

| Button | Icon | Function |
|--------|------|----------|
| Expand All | ⇱ | Expand all tree nodes for full visibility |
| Collapse All | ⇲ | Collapse all tree nodes for clean view |

**Location:** Top-right of Tree View header

### Usage Scenarios

**Expand All:** Use when you need to see the complete JSON structure
```
Example: Finding all Observation resources in bundle
1. Click "Expand All"
2. Scroll through tree
3. Locate target nodes
```

**Collapse All:** Use when tree is cluttered
```
Example: After deep navigation, return to overview
1. Click "Collapse All"
2. Tree shows only root nodes
3. Manually expand sections of interest
```

## Error Messages

### Common Validation Errors

| Error | Meaning | Fix |
|-------|---------|-----|
| `Unexpected token }` | Extra closing bracket | Remove or add opening bracket |
| `Unexpected end of JSON input` | Missing closing bracket | Add closing bracket |
| `Expected ',' or '}'` | Missing comma between properties | Add comma |
| `Unexpected token :` | Missing property name or quote | Add quotes around property name |

### Error Display Example
```
⚠ Validation Error
Line 23, Column 15: Unexpected token }
```
**Action:** Editor cursor moves to Line 23, Column 15 automatically

## Tips & Tricks

### 💡 Best Practices

1. **Save Incremental Progress:**
   - Use "Validate JSON" frequently while editing
   - Catch errors early

2. **Format Before Reviewing:**
   - Click "Format JSON" to make structure readable
   - Easier to spot errors

3. **Reset When Confused:**
   - Lost in edits? Click "Reset"
   - Start from known-good state

4. **Use Tree View for Navigation:**
   - Process JSON first to see structure in tree
   - Reference tree while editing in modal

5. **Expand Strategically:**
   - Use "Collapse All" for overview
   - Manually expand sections you're editing
   - Use "Expand All" for deep inspection

### ⚡ Keyboard Tips (Editor)

Monaco Editor supports standard shortcuts:
- **Ctrl/Cmd+F:** Find
- **Ctrl/Cmd+H:** Replace
- **Ctrl/Cmd+Z:** Undo
- **Ctrl/Cmd+Shift+Z:** Redo
- **Ctrl/Cmd+/:** Toggle comment
- **Alt+Click:** Multi-cursor
- **Ctrl/Cmd+D:** Select next occurrence

### 🚫 Common Mistakes

**Mistake:** Clicking "Apply Changes" without validation
**Problem:** Modal closes with error message, must reopen
**Solution:** Always "Validate JSON" before "Apply Changes"

**Mistake:** Losing work by clicking "Cancel"
**Problem:** All edits discarded
**Solution:** Use "Apply Changes" to save, or "Reset" to revert safely

**Mistake:** Editing without formatting first
**Problem:** Hard to read structure, easy to make mistakes
**Solution:** Click "Format JSON" immediately after opening modal

## Size & Layout

| Property | Value | Reason |
|----------|-------|--------|
| Width | 90vw | Maximum horizontal space |
| Height | 90vh | Maximum vertical space |
| Top Margin | 20px | Small gap from viewport top |
| Editor Font | 14px | Readable monospace size |

## Integration with Playground

### Data Flow
```
Sample File → Edit JSON → Validate → Apply → Process → Results
     ↓           ↓           ↓          ↓        ↓         ↓
  Load      Open Modal   Check     Format   Validate   Display
                                  & Update             + Tree
```

### State Management
- **Initial Value:** Passed from Playground to Modal
- **Editor State:** Local to modal (isolated)
- **Apply Callback:** Returns formatted JSON to Playground
- **Tree Rebuild:** Triggered by Playground after Apply

## Performance Notes

- **No Lag:** Editor in modal, tree updates only on Apply
- **Memoization:** Tree data cached, rebuilds only when needed
- **Monaco Optimization:** Handles large files efficiently (tested up to 10MB)

## Accessibility

- **Keyboard Navigation:** Full keyboard support in editor
- **Screen Readers:** Ant Design components ARIA-compliant
- **Focus Management:** Modal traps focus, Esc to close
- **High Contrast:** Monaco supports theme customization

## Browser Compatibility

| Browser | Version | Support |
|---------|---------|---------|
| Chrome | 90+ | ✓ Full |
| Firefox | 88+ | ✓ Full |
| Safari | 14+ | ✓ Full |
| Edge | 90+ | ✓ Full |

## Need Help?

### Documentation
- Full implementation guide: `JSON_EDITOR_CODE_EDITOR_FEATURES.md`
- Original refactor doc: `JSON_EDITOR_MODAL_REFACTOR.md`

### Common Questions

**Q: Can I undo changes after clicking Apply?**  
A: No. Always validate before applying. Use "Reset" if unsure.

**Q: What happens to invalid JSON on Apply?**  
A: Error is shown, modal stays open, cursor moves to error.

**Q: Does Format JSON modify my data?**  
A: No. It only changes whitespace/indentation. Values unchanged.

**Q: Can I paste JSON from clipboard?**  
A: Yes. Ctrl/Cmd+V pastes. Use "Format JSON" to clean up.

**Q: Why is Apply Changes button always enabled?**  
A: Real-time validation removed to improve performance. Click "Validate JSON" to check first.

---

## Quick Action Reference

| Task | Steps |
|------|-------|
| Load sample | Select sample → Load Sample → Edit JSON Input |
| Format pasted JSON | Paste → Format JSON → Apply Changes |
| Fix validation error | Validate JSON → Fix at line:col → Apply Changes |
| Discard changes | Reset → Cancel |
| View full structure | Apply Changes → Tree View → Expand All |
| Focus on section | Tree View → Collapse All → Expand specific nodes |

---

**Version:** 1.0  
**Last Updated:** December 7, 2025  
**Component:** JsonEditorModal.jsx, Playground.jsx (Tree View)
