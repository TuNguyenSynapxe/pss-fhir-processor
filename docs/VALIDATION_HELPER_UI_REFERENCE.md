# Validation Helper UI Reference

## UI Layout (Expanded View)

```
┌─────────────────────────────────────────────────────────────────┐
│ [PSS_VAL_002] ⚠️ Required   🏠 Patient         [Hide Details]   │
│ Missing required field: NRIC                                     │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ 💡 What this means:                               (Blue box) │ │
│ │ This Patient record is missing the NRIC value. The system   │ │
│ │ requires this information to uniquely identify the patient.  │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                  │
│ 📍 Location in record:                                          │
│    Patient → identifier (NRIC) → value                          │
│                                                                  │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ Resource in Bundle:                      [Go to Resource →]  │ │
│ │    Entry #2   Patient                                        │ │
│ │    urn:uuid:550e8400-e29b-41d4-a716-446655440000            │ │
│ │    ID: 550e8400-e29b-41d4-a716-446655440000                 │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                  │
│ ℹ️ Technical Details:                                           │
│    Field 'identifier[0].value' is required but missing.         │
│                                                                  │
│ 📝 Scope: entry[2].resource   | Resource: Patient               │
│                                                                  │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ 🔧 How to fix this:                           (Green box)    │ │
│ │ 1. Open the Patient resource (entry #2).                    │ │
│ │ 2. Add the NRIC value in the format: S1234567A              │ │
│ │ 3. Ensure the NRIC is valid and matches official records.   │ │
│ │ 4. Save the changes and re-validate the Bundle.             │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## Section Breakdown

### 1. Header
- **Error Code**: `[PSS_VAL_002]` in red
- **Rule Type Tag**: Red badge "Required"
- **Resource Type Tag**: Blue badge with home icon "Patient"
- **Toggle Button**: "Show Help" / "Hide Details"
- **Title**: Brief error summary

### 2. What This Means (Blue Box)
- **Icon**: 💡 BulbOutlined
- **Background**: Light blue (`bg-blue-50`)
- **Border**: Left blue border (`border-l-4 border-blue-500`)
- **Content**: Plain-English explanation for non-technical users
- **Style**: Friendly, conversational tone

**Examples by Rule Type**:
- **Required**: "This [ResourceType] record is missing the [FieldName]. The system requires this information to [purpose]."
- **Regex**: "The [FieldName] format is invalid. It should follow the pattern: [pattern example]."
- **CodesMaster**: "The answer provided for '[QuestionName]' is not in the approved list of answers."
- **Reference**: "The reference to '[FieldName]' points to the wrong type of resource."

### 3. Location Breadcrumb
- **Icon**: 📍 EnvironmentOutlined
- **Component**: Ant Design Breadcrumb
- **Separator**: Right arrow (→)
- **Style**: Monospace font for technical accuracy
- **Content**: Hierarchical path through data structure

**Examples**:
- `Patient → identifier (NRIC) → value`
- `Observation → code → coding[0] → code`
- `entry → fullUrl vs resource.id`
- `QuestionnaireResponse → item[3] → answer[0] → valueCoding → code`

### 4. Resource Pointer Card (Gray Box)
- **Background**: Light gray (`bg-gray-50`)
- **Border**: Gray border (`border-gray-300`)
- **Left Side**: Resource information
  - Entry number: Purple tag `Entry #2`
  - Resource type: Blue tag `Patient`
  - Full URL: Small gray monospace text
  - Resource ID: Small gray text with code formatting
- **Right Side**: Navigation button
  - **Button**: Primary blue button "Go to Resource →"
  - **Action**: Dispatches `navigateToEntry` event with entry index

### 5. Technical Details
- **Icon**: ℹ️ InfoCircleOutlined
- **Content**: Original FHIR-level error message
- **Audience**: For developers/technical users
- **Style**: Gray text, smaller font

### 6. Scope Information
- **Location**: Bottom of expanded view
- **Border**: Top border separator
- **Content**: 
  - Scope (CPS1 path)
  - Screening type (if applicable)
  - Resource type
- **Style**: Small gray text (metadata)

### 7. How to Fix (Green Box)
- **Icon**: 🔧 ToolOutlined
- **Background**: Light green (`bg-green-50`)
- **Border**: Left green border (`border-l-4 border-green-500`)
- **Content**: Numbered list of actionable steps
- **Style**: Step-by-step, includes entry numbers and examples

**Step Format**:
1. "Open the [ResourceType] resource (entry #[X])."
2. "Add/Update [FieldName] with [specific guidance]."
3. "Ensure [validation requirement]."
4. "Save and re-validate."

## Collapsed View

```
┌─────────────────────────────────────────────────────────────────┐
│ ⚠️ [PSS_VAL_002] ⚠️ Required   🏠 Patient      [Show Help]      │
│ Missing required field: NRIC                                     │
└─────────────────────────────────────────────────────────────────┘
```

- Compact single-line view
- Shows error icon, code, rule type, resource type, and title
- Click "Show Help" to expand

## Special Cases

### CodesMaster (Question Answers)

```
┌─────────────────────────────────────────────────────────────────┐
│ ❓ Question:                                                     │
│    Smoking Status                                                │
│    Code: SMOKING_STATUS                                          │
│                                                                  │
│ ✅ Expected:                                                     │
│    One of the allowed answers                                    │
│                                                                  │
│ ❌ Actual:                                                       │
│    "Sometimes Smoker"                                            │
│                                                                  │
│ ✅ Allowed Answers:                                              │
│    • Current Smoker                                              │
│    • Never Smoked                                                │
│    • Former Smoker                                               │
│                                                                  │
│ 📝 Multi-value format:                                           │
│    Separate multiple values with pipe (|)                        │
│    Example: "500Hz – R|1000Hz – R"                               │
└─────────────────────────────────────────────────────────────────┘
```

### CodeSystem Validation

```
┌─────────────────────────────────────────────────────────────────┐
│ ✅ Allowed Codes:                                                │
│ ┌────────────┬─────────────────────────────────────────────────┐│
│ │ Code       │ Display                                         ││
│ ├────────────┼─────────────────────────────────────────────────┤│
│ │ 55561003   │ Active                                          ││
│ │ 73425007   │ Inactive                                        ││
│ │ 421139008  │ On hold                                         ││
│ └────────────┴─────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────┘
```

### Reference Validation

```
┌─────────────────────────────────────────────────────────────────┐
│ Allowed Reference Types:                                         │
│    Patient   Practitioner   Organization                         │
└─────────────────────────────────────────────────────────────────┘
```

## Color Scheme

| Section | Background | Border | Text | Icon |
|---------|-----------|---------|------|------|
| What This Means | `bg-blue-50` | `border-blue-500` | `text-gray-800` | Blue 💡 |
| How to Fix | `bg-green-50` | `border-green-500` | `text-gray-800` | Green 🔧 |
| Resource Pointer | `bg-gray-50` | `border-gray-300` | `text-gray-800` | - |
| Error Header | - | - | `text-red-600` | Red ⚠️ |
| Success/Expected | `bg-green-50` | `border-green-200` | - | Green ✅ |
| Actual/Error | `bg-red-50` | `border-red-200` | - | Red ❌ |

## Icon Reference

| Icon | Ant Design Component | Usage |
|------|---------------------|--------|
| 💡 | `BulbOutlined` | "What this means" section |
| 🔧 | `ToolOutlined` | "How to fix" section |
| 📍 | `EnvironmentOutlined` | Location/breadcrumb |
| ℹ️ | `InfoCircleOutlined` | Technical details |
| ✅ | `CheckCircleOutlined` | Expected values |
| ❌ | `CloseCircleOutlined` | Actual (wrong) values |
| ❓ | `QuestionCircleOutlined` | Question details |
| 🏠 | `HomeOutlined` | Resource type |
| → | `RightOutlined` | Breadcrumb separator |

## Typography

- **Headings**: `<Text strong>` component
- **Body text**: `<Paragraph>` component  
- **Code/paths**: Monospace font with `font-mono` class
- **Small metadata**: `text-xs` and `text-gray-500`
- **Tags**: Ant Design `<Tag>` with color coding

## Responsive Behavior

- **Mobile**: Stack elements vertically
- **Desktop**: Side-by-side layout for resource pointer
- **Long lists**: Max height with scroll for allowed codes
- **Overflow**: Text wrapping for long URLs/paths

## Accessibility

- **Color contrast**: Meets WCAG AA standards
- **Icons**: Paired with text labels
- **Interactive elements**: Clear focus states
- **Semantic HTML**: Proper heading hierarchy
- **Screen readers**: Descriptive ARIA labels

## Interactive Elements

### Navigation Button
- **Text**: "Go to Resource →"
- **Type**: Primary button (blue)
- **Size**: Small
- **Action**: Dispatches custom event with entry index
- **Future**: Will scroll JSON viewer to entry and highlight

### Expand/Collapse Toggle
- **Text**: "Show Help" / "Hide Details"
- **Style**: Blue underlined link
- **Position**: Top-right of error card
- **State**: Controlled by `isExpanded` state

## Integration with Playground

The ValidationHelper component is designed to integrate with the Playground's JSON viewer:

```javascript
// In Playground.jsx
useEffect(() => {
  const handleNavigate = (event) => {
    const { entryIndex } = event.detail;
    
    // Scroll JSON viewer to bundle.entry[entryIndex]
    const jsonViewer = document.querySelector('.json-viewer');
    const entryElement = jsonViewer.querySelector(`[data-entry-index="${entryIndex}"]`);
    
    if (entryElement) {
      entryElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
      entryElement.classList.add('highlight-entry');
      
      setTimeout(() => {
        entryElement.classList.remove('highlight-entry');
      }, 3000);
    }
  };
  
  window.addEventListener('navigateToEntry', handleNavigate);
  return () => window.removeEventListener('navigateToEntry', handleNavigate);
}, []);
```

## User Flow

1. **See Error**: User sees collapsed error with title
2. **Expand**: Clicks "Show Help"
3. **Understand**: Reads "What this means" in plain English
4. **Locate**: Follows breadcrumb path
5. **Navigate**: Clicks "Go to Resource →" button
6. **View**: JSON viewer scrolls to exact entry
7. **Fix**: Follows "How to fix" steps
8. **Validate**: Re-runs validation

## Design Principles

1. **Progressive Disclosure**: Start collapsed, expand for details
2. **Plain Language First**: Non-technical explanation at top
3. **Visual Hierarchy**: Color-coded sections guide attention
4. **Actionable Guidance**: Every error has clear fix steps
5. **Context Awareness**: Shows entry numbers and resource types
6. **Metadata-Driven**: All content generated from rules, no hardcoding
