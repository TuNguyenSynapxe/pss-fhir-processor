# Validation Helper UX Testing Guide

## Overview
This guide helps you test the enhanced Validation Helper system with human-friendly features for non-FHIR users.

## Prerequisites
- Backend running on port 5063
- Frontend running on port 5173
- Test FHIR Bundles ready

## Test Scenarios

### 1. Required Field Error (NRIC Missing)

**Test Bundle**:
```json
{
  "resourceType": "Bundle",
  "type": "collection",
  "entry": [
    {
      "fullUrl": "urn:uuid:550e8400-e29b-41d4-a716-446655440000",
      "resource": {
        "resourceType": "Patient",
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "identifier": [
          {
            "system": "http://synapxe.sg/fhir/identifier/nric",
            "value": ""
          }
        ]
      }
    }
  ]
}
```

**Expected Result**:
- ❌ Error code: `PSS_VAL_002` (or similar)
- 🏷️ Tag: "Required" (red)
- 🏠 Resource: "Patient" (blue)

**What This Means**:
> "This Patient record is missing the NRIC value. The system requires this information to uniquely identify the patient."

**Breadcrumb**:
> Patient → identifier (NRIC) → value

**Resource Pointer**:
- Entry #0
- Patient
- urn:uuid:550e8400-e29b-41d4-a716-446655440000

**How to Fix**:
1. Open the Patient resource (entry #0).
2. Add the NRIC value in the format: S1234567A
3. Ensure the NRIC is valid and matches official records.
4. Save the changes and re-validate the Bundle.

**Test Steps**:
1. ✅ Paste bundle into Playground
2. ✅ Click "Validate"
3. ✅ See collapsed error
4. ✅ Click "Show Help"
5. ✅ Verify all sections appear
6. ✅ Check plain-English explanation
7. ✅ Verify entry number is #0
8. ✅ Click "Go to Resource →" button
9. ⏳ (Future) JSON should scroll to entry[0]

---

### 2. Regex Pattern Error (Invalid NRIC Format)

**Test Bundle**:
```json
{
  "resourceType": "Bundle",
  "type": "collection",
  "entry": [
    {
      "fullUrl": "urn:uuid:patient-123",
      "resource": {
        "resourceType": "Patient",
        "id": "patient-123",
        "identifier": [
          {
            "system": "http://synapxe.sg/fhir/identifier/nric",
            "value": "INVALID"
          }
        ]
      }
    }
  ]
}
```

**Expected Result**:
- ❌ Error code: `PSS_VAL_003`
- 🏷️ Tag: "Regex"
- 🏠 Resource: "Patient"

**What This Means**:
> "The NRIC format is invalid. It should follow the pattern: Must start with S/T/F/G followed by 7 digits and a checksum letter."

**Breadcrumb**:
> Patient → identifier (NRIC) → value

**How to Fix**:
1. Open the Patient resource (entry #0).
2. Correct the NRIC value to match the pattern: S1234567A
3. Valid examples: S1234567A, T9876543B, F5678901C
4. Ensure the checksum letter is correct.

**Test Steps**:
1. ✅ Paste bundle
2. ✅ Click "Validate"
3. ✅ Click "Show Help"
4. ✅ Verify pattern explanation
5. ✅ Check example formats shown
6. ✅ Verify breadcrumb shows "identifier (NRIC)"

---

### 3. CodesMaster Error (Invalid Question Answer)

**Test Bundle**:
```json
{
  "resourceType": "Bundle",
  "type": "collection",
  "entry": [
    {
      "fullUrl": "urn:uuid:obs-smoking",
      "resource": {
        "resourceType": "Observation",
        "id": "obs-smoking",
        "code": {
          "coding": [{
            "system": "http://synapxe.sg/fhir/CodeSystem/pss-questions",
            "code": "SMOKING_STATUS"
          }]
        },
        "valueCodeableConcept": {
          "coding": [{
            "code": "Sometimes",
            "display": "Sometimes Smoker"
          }]
        }
      }
    }
  ]
}
```

**Expected Result**:
- ❌ Error code: `PSS_VAL_008`
- 🏷️ Tag: "CodesMaster"
- 🏠 Resource: "Observation"

**What This Means**:
> "The answer provided for 'Smoking Status' is not in the approved list of answers. Only specific pre-defined answers are accepted for this question."

**Question Details**:
- **Question**: Smoking Status
- **Code**: SMOKING_STATUS

**Allowed Answers**:
- ✅ Current Smoker
- ✅ Never Smoked
- ✅ Former Smoker

**Actual Answer**: ❌ "Sometimes Smoker"

**How to Fix**:
1. Open the Observation resource (entry #0).
2. Review the question: "Smoking Status"
3. Choose one of the allowed answers: "Current Smoker", "Never Smoked", "Former Smoker"
4. Update both the code and display text to match exactly.

**Test Steps**:
1. ✅ Paste bundle
2. ✅ Click "Validate"
3. ✅ Click "Show Help"
4. ✅ Verify question name displayed
5. ✅ Check allowed answers list
6. ✅ Verify actual answer shown in red box
7. ✅ Verify entry number shown

---

### 4. Reference Error (Wrong Target Type)

**Test Bundle**:
```json
{
  "resourceType": "Bundle",
  "type": "collection",
  "entry": [
    {
      "fullUrl": "urn:uuid:obs-123",
      "resource": {
        "resourceType": "Observation",
        "id": "obs-123",
        "subject": {
          "reference": "urn:uuid:org-456"
        }
      }
    },
    {
      "fullUrl": "urn:uuid:org-456",
      "resource": {
        "resourceType": "Organization",
        "id": "org-456"
      }
    }
  ]
}
```

**Expected Result**:
- ❌ Error code: `PSS_VAL_006`
- 🏷️ Tag: "Reference"
- 🏠 Resource: "Observation"

**What This Means**:
> "The reference to 'subject' points to the wrong type of resource. References must link to approved resource types to ensure data integrity."

**Allowed Types**:
- Patient
- Group

**How to Fix**:
1. Open the Observation resource (entry #0).
2. Update the subject reference to point to a Patient or Group resource.
3. Verify the referenced resource exists in the Bundle.
4. Use the format: "urn:uuid:<resource-id>"

**Test Steps**:
1. ✅ Paste bundle
2. ✅ Click "Validate"
3. ✅ Click "Show Help"
4. ✅ Verify allowed types shown
5. ✅ Check breadcrumb shows "subject"
6. ✅ Verify entry #0 shown

---

### 5. FullUrl/ID Mismatch

**Test Bundle**:
```json
{
  "resourceType": "Bundle",
  "type": "collection",
  "entry": [
    {
      "fullUrl": "urn:uuid:abc-123",
      "resource": {
        "resourceType": "Patient",
        "id": "xyz-789"
      }
    }
  ]
}
```

**Expected Result**:
- ❌ Error code: `PSS_VAL_001`
- 🏷️ Tag: "FullUrlIdMatch"
- 🏠 Resource: "Patient"

**What This Means**:
> "The resource ID does not match the ID in the fullUrl. These must be identical to ensure the Bundle is correctly structured."

**Breadcrumb**:
> entry → fullUrl vs resource.id

**Expected**:
> entry.fullUrl = "urn:uuid:<GUID>" and resource.id = "<GUID>"

**How to Fix**:
1. Open the Bundle entry (entry #0).
2. Extract the GUID from fullUrl: "abc-123"
3. Set resource.id to match: "abc-123"
4. Ensure they are identical.

**Test Steps**:
1. ✅ Paste bundle
2. ✅ Click "Validate"
3. ✅ Click "Show Help"
4. ✅ Verify explanation of mismatch
5. ✅ Check breadcrumb shows "fullUrl vs resource.id"
6. ✅ Verify fix steps show GUID extraction

---

### 6. CodeSystem Error (Invalid Code)

**Test Bundle**:
```json
{
  "resourceType": "Bundle",
  "type": "collection",
  "entry": [
    {
      "fullUrl": "urn:uuid:obs-status",
      "resource": {
        "resourceType": "Observation",
        "id": "obs-status",
        "status": "unknown"
      }
    }
  ]
}
```

**Expected Result**:
- ❌ Error code: `PSS_VAL_007`
- 🏷️ Tag: "CodeSystem"
- 🏠 Resource: "Observation"

**What This Means**:
> "The code 'unknown' is not valid for status. Only codes from the official FHIR ObservationStatus code system are allowed."

**Expected**: http://hl7.org/fhir/observation-status

**Allowed Codes**:
| Code | Display |
|------|---------|
| registered | Registered |
| preliminary | Preliminary |
| final | Final |
| amended | Amended |

**How to Fix**:
1. Open the Observation resource (entry #0).
2. Choose one of the allowed codes from the table above.
3. Update the status field.
4. Ensure you use the exact code value.

**Test Steps**:
1. ✅ Paste bundle
2. ✅ Click "Validate"
3. ✅ Click "Show Help"
4. ✅ Verify code table displayed
5. ✅ Check code system URL shown
6. ✅ Verify scrollable table for many codes

---

## UI Component Tests

### Collapsed State
- [ ] Error shows in red Alert box
- [ ] Error code visible `[PSS_VAL_XXX]`
- [ ] Rule type tag displayed (red)
- [ ] Resource type tag displayed (blue with home icon)
- [ ] Title shows brief summary
- [ ] "Show Help" button visible

### Expanded State
- [ ] "What This Means" section appears (blue box with bulb icon)
- [ ] Plain-English explanation readable
- [ ] Breadcrumb navigation visible with arrows
- [ ] Resource Pointer card shows:
  - [ ] Entry number (purple tag)
  - [ ] Resource type (blue tag)
  - [ ] Full URL (gray monospace)
  - [ ] Resource ID (if present)
  - [ ] "Go to Resource →" button
- [ ] Technical details section present
- [ ] "How to Fix" section appears (green box with tool icon)
- [ ] Numbered steps formatted correctly
- [ ] Scope information at bottom

### Special Sections
- [ ] **CodesMaster**: Question name, allowed answers list
- [ ] **CodesMaster**: Multi-value format note (if applicable)
- [ ] **CodeSystem**: Allowed codes table with scrolling
- [ ] **Reference**: Allowed types as tags
- [ ] **Expected/Actual**: Color-coded boxes (green/red)

### Interactive Elements
- [ ] Toggle button changes text (Show Help ↔ Hide Details)
- [ ] Expanding preserves error list position
- [ ] "Go to Resource →" button clickable
- [ ] Event dispatched on button click
- [ ] Navigation button disabled if no entry index

### Responsive Design
- [ ] Mobile view: elements stack vertically
- [ ] Desktop view: resource pointer card side-by-side
- [ ] Long URLs wrap properly
- [ ] Long code lists scroll
- [ ] Breadcrumb wraps on small screens

---

## Integration Tests

### 1. Multiple Errors
**Test**: Bundle with 3+ validation errors

**Expected**:
- [ ] All errors show in list
- [ ] Each can be expanded independently
- [ ] Entry numbers different for each
- [ ] Breadcrumbs unique per error
- [ ] "What this means" contextual to each error

### 2. Same Entry, Multiple Errors
**Test**: One Patient with missing NRIC AND invalid birthDate

**Expected**:
- [ ] Both errors show Entry #0
- [ ] Different breadcrumbs (identifier vs birthDate)
- [ ] Different "What this means" explanations
- [ ] "Go to Resource →" points to same entry

### 3. Navigation Event
**Test**: Click "Go to Resource →" button

**Expected**:
- [ ] Custom event `navigateToEntry` dispatched
- [ ] Event detail contains `entryIndex`
- [ ] Entry index matches resource pointer
- [ ] Console shows event (until Playground integration)

### 4. Mixed Rule Types
**Test**: Bundle with Required, Regex, CodesMaster, Reference errors

**Expected**:
- [ ] Each shows appropriate template
- [ ] CodesMaster shows allowed answers
- [ ] Reference shows allowed types
- [ ] Required shows "missing" language
- [ ] Regex shows pattern example

---

## Performance Tests

### Large Bundle
**Test**: Bundle with 50+ entries, 20+ validation errors

**Expected**:
- [ ] Page loads without freezing
- [ ] Errors render within 2 seconds
- [ ] Expanding errors is smooth
- [ ] Scrolling is responsive
- [ ] No memory leaks after multiple validations

### Large Code List
**Test**: CodeSystem with 100+ allowed codes

**Expected**:
- [ ] Table renders correctly
- [ ] Scrollbar appears
- [ ] Scrolling is smooth
- [ ] Search/filter works (if implemented)

---

## Accessibility Tests

### Keyboard Navigation
- [ ] Tab through errors
- [ ] Enter/Space toggles expansion
- [ ] Tab reaches "Go to Resource →" button
- [ ] Enter activates navigation button
- [ ] Focus visible on all interactive elements

### Screen Reader
- [ ] Error code announced
- [ ] Rule type announced
- [ ] "What this means" section announced
- [ ] How to fix steps announced in order
- [ ] Button purpose clear ("Go to Resource")

### Color Contrast
- [ ] Blue box text readable (WCAG AA)
- [ ] Green box text readable
- [ ] Red error text readable
- [ ] Gray metadata text readable
- [ ] Tags have sufficient contrast

---

## Edge Cases

### 1. No Resource Pointer
**Test**: Error without entry index (bundle-level error)

**Expected**:
- [ ] Resource pointer card hidden
- [ ] No "Go to Resource →" button
- [ ] Other sections still appear
- [ ] No errors in console

### 2. Empty Allowed Answers
**Test**: CodesMaster error with empty allowed answers list

**Expected**:
- [ ] "Allowed Answers" section hidden
- [ ] "What this means" still shows
- [ ] "How to fix" still shows
- [ ] No broken UI

### 3. Very Long Field Path
**Test**: Deeply nested path like `QuestionnaireResponse.item[0].item[3].answer[0].valueCoding.code`

**Expected**:
- [ ] Breadcrumb wraps or scrolls
- [ ] Still readable
- [ ] Smart abbreviation (if implemented)

### 4. Special Characters in Values
**Test**: Answer with quotes, pipes, newlines

**Expected**:
- [ ] Values properly escaped
- [ ] No broken formatting
- [ ] Multi-value detection works

---

## User Acceptance Tests

### Non-FHIR User (Clinician)
**Scenario**: Healthcare professional with no FHIR knowledge

**Tasks**:
1. [ ] User can understand what went wrong (plain English)
2. [ ] User can find where the error is (breadcrumb)
3. [ ] User can navigate to the problem (button click)
4. [ ] User knows how to fix it (step-by-step)
5. [ ] User feels confident making the correction

**Questions**:
- Is "What this means" clear enough?
- Are the steps actionable without technical knowledge?
- Is the breadcrumb navigation intuitive?
- Does the entry number help locate the resource?

### FHIR Developer
**Scenario**: Developer integrating PSS validation

**Tasks**:
1. [ ] Can quickly identify error type (tag)
2. [ ] Can see technical details (FHIR path)
3. [ ] Can verify against specification
4. [ ] Can programmatically handle errors
5. [ ] Can understand metadata structure

**Questions**:
- Is technical information still accessible?
- Are rule types clearly indicated?
- Is the scope information useful?
- Can errors be mapped back to code?

---

## Regression Tests

### Original Features Still Work
- [ ] Collapse/expand functionality
- [ ] Error code display
- [ ] Rule type detection
- [ ] Scope information
- [ ] Question details (CodesMaster)
- [ ] Allowed codes table (CodeSystem)
- [ ] Allowed types list (Reference)
- [ ] Example values
- [ ] Multi-value format note

### Metadata Loading
- [ ] Rules loaded from validation-metadata.json
- [ ] Codes loaded from codes-master.json
- [ ] Context enrichment works
- [ ] All 8 rule types supported

### No Breaking Changes
- [ ] Existing test bundles still work
- [ ] API responses unchanged
- [ ] Backend enrichment still runs
- [ ] Frontend helper still generates

---

## Browser Compatibility

### Chrome (Latest)
- [ ] All features work
- [ ] Styling correct
- [ ] Events fire properly

### Firefox (Latest)
- [ ] All features work
- [ ] Styling correct
- [ ] Events fire properly

### Safari (Latest)
- [ ] All features work
- [ ] Styling correct
- [ ] Events fire properly

### Edge (Latest)
- [ ] All features work
- [ ] Styling correct
- [ ] Events fire properly

---

## Reporting Issues

When reporting bugs, include:

1. **Error Type**: Rule type (Required, Regex, etc.)
2. **Test Bundle**: Minimal JSON that reproduces issue
3. **Expected Result**: What should happen
4. **Actual Result**: What actually happened
5. **Screenshots**: UI before/after expanding
6. **Browser**: Version and OS
7. **Console Errors**: JavaScript errors if any

---

## Next Steps After Testing

1. ✅ Fix any bugs found
2. ✅ Improve unclear explanations
3. ✅ Adjust styling based on feedback
4. ⏳ Implement JSON viewer navigation
5. ⏳ Add entry highlighting on navigation
6. ⏳ Consider adding search/filter for errors
7. ⏳ Gather user feedback from clinicians

---

## Success Criteria

**UX Goals Met**:
- [ ] Non-FHIR users can understand errors without training
- [ ] Users can navigate to problem location in < 10 seconds
- [ ] Users can fix errors with provided steps
- [ ] No technical FHIR knowledge required
- [ ] Reduces support tickets by 50%+

**Technical Goals Met**:
- [ ] All 8 rule types supported
- [ ] Metadata-driven (zero hardcoding)
- [ ] No breaking changes
- [ ] Performance acceptable (< 2s render)
- [ ] Accessible (WCAG AA)
- [ ] Mobile-friendly

---

## Test Data Repository

Create these test bundles in `test-data/validation-helper/`:

1. `required-nric-missing.json`
2. `regex-nric-invalid.json`
3. `codesmaster-smoking-invalid.json`
4. `reference-wrong-type.json`
5. `fullurl-mismatch.json`
6. `codesystem-invalid-status.json`
7. `multiple-errors-one-patient.json`
8. `large-bundle-many-errors.json`

---

## Automated Test Script

```bash
#!/bin/bash
# test-validation-helper.sh

echo "🧪 Testing Validation Helper UX Enhancements"
echo "=============================================="

# Start backend
cd Backend
dotnet run &
BACKEND_PID=$!
sleep 5

# Start frontend
cd ../Frontend
npm run dev &
FRONTEND_PID=$!
sleep 5

# Run tests
echo "📋 Running test scenarios..."

for file in ../../test-data/validation-helper/*.json; do
  echo "Testing: $file"
  curl -X POST http://localhost:5063/api/fhir/validate \
    -H "Content-Type: application/json" \
    -d @$file \
    | jq '.errors[] | {code, ruleType, resourcePointer}'
done

# Cleanup
kill $BACKEND_PID $FRONTEND_PID
echo "✅ Tests complete"
```

---

## Manual Testing Checklist

Print this checklist and test each item:

```
□ Test Scenario 1: Required Field
□ Test Scenario 2: Regex Pattern
□ Test Scenario 3: CodesMaster
□ Test Scenario 4: Reference
□ Test Scenario 5: FullUrl/ID Mismatch
□ Test Scenario 6: CodeSystem
□ UI: Collapsed State
□ UI: Expanded State
□ UI: What This Means (blue box)
□ UI: Breadcrumb Navigation
□ UI: Resource Pointer Card
□ UI: How to Fix (green box)
□ UI: Toggle Button
□ UI: Go to Resource Button
□ Special: CodesMaster Answers List
□ Special: CodeSystem Codes Table
□ Special: Multi-value Format Note
□ Integration: Multiple Errors
□ Integration: Same Entry Errors
□ Integration: Navigation Event
□ Edge Case: No Resource Pointer
□ Edge Case: Empty Allowed Answers
□ Edge Case: Long Field Path
□ Accessibility: Keyboard Navigation
□ Accessibility: Screen Reader
□ Accessibility: Color Contrast
□ Performance: Large Bundle
□ Performance: Large Code List
□ Browser: Chrome
□ Browser: Firefox
□ Browser: Safari
```

---

## Notes

- All test bundles should be minimal but realistic
- Focus on user experience, not just technical correctness
- Get feedback from actual target users (clinicians)
- Document any unexpected behavior
- Iterate on explanations based on user comprehension
