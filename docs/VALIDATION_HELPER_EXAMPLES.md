# Validation Helper - Real-World Examples

## Example 1: Missing NRIC (Required Field)

### Input Bundle
```json
{
  "resourceType": "Bundle",
  "type": "collection",
  "entry": [
    {
      "fullUrl": "urn:uuid:pat-001",
      "resource": {
        "resourceType": "Patient",
        "id": "pat-001",
        "identifier": [
          {
            "system": "http://synapxe.sg/fhir/identifier/nric",
            "value": ""
          }
        ],
        "name": [
          {
            "text": "Tan Ah Kow"
          }
        ]
      }
    }
  ]
}
```

### Validation Error (Backend Response)
```json
{
  "code": "PSS_VAL_002",
  "message": "Required field 'identifier[0].value' is missing",
  "severity": "error",
  "fieldPath": "entry[0].resource.identifier[0].value",
  "scope": "entry[0].resource",
  "ruleType": "Required",
  "rule": {
    "path": "identifier[0].value",
    "identifierSystem": "http://synapxe.sg/fhir/identifier/nric",
    "description": "NRIC number is required for patient identification"
  },
  "context": {
    "resourceType": "Patient",
    "identifierType": "NRIC"
  },
  "resourcePointer": {
    "entryIndex": 0,
    "fullUrl": "urn:uuid:pat-001",
    "resourceType": "Patient",
    "resourceId": "pat-001"
  }
}
```

### UI Rendering (Frontend)

**Collapsed View**:
```
┌─────────────────────────────────────────────────────────────┐
│ ⚠️ [PSS_VAL_002] Required 🏠 Patient        [Show Help]    │
│ Missing required field: NRIC                                 │
└─────────────────────────────────────────────────────────────┘
```

**Expanded View**:
```
┌─────────────────────────────────────────────────────────────────┐
│ ⚠️ [PSS_VAL_002] Required 🏠 Patient        [Hide Details]     │
│ Missing required field: NRIC                                     │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ 💡 What this means:                                         │ │
│ │ This Patient record is missing the NRIC value. The system  │ │
│ │ requires this information to uniquely identify the patient. │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                  │
│ 📍 Location in record:                                          │
│    Patient → identifier (NRIC) → value                          │
│                                                                  │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ Resource in Bundle:                      [Go to Resource →]  │ │
│ │    Entry #0   Patient                                        │ │
│ │    urn:uuid:pat-001                                          │ │
│ │    ID: pat-001                                               │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                  │
│ ℹ️ Technical Details:                                           │
│    Required field 'identifier[0].value' is missing              │
│                                                                  │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ 🔧 How to fix this:                                         │ │
│ │ 1. Open the Patient resource (entry #0).                    │ │
│ │ 2. Add the NRIC value in the format: S1234567A              │ │
│ │ 3. Ensure the NRIC is valid and matches official records.   │ │
│ │ 4. Save the changes and re-validate the Bundle.             │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                  │
│ 📝 Scope: entry[0].resource | Resource: Patient                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Example 2: Invalid NRIC Format (Regex)

### Input Bundle
```json
{
  "resourceType": "Bundle",
  "type": "collection",
  "entry": [
    {
      "fullUrl": "urn:uuid:pat-002",
      "resource": {
        "resourceType": "Patient",
        "id": "pat-002",
        "identifier": [
          {
            "system": "http://synapxe.sg/fhir/identifier/nric",
            "value": "ABC123XYZ"
          }
        ]
      }
    }
  ]
}
```

### Validation Error
```json
{
  "code": "PSS_VAL_003",
  "message": "Value 'ABC123XYZ' does not match required pattern '^[STFG]\\d{7}[A-Z]$'",
  "severity": "error",
  "fieldPath": "entry[0].resource.identifier[0].value",
  "scope": "entry[0].resource",
  "ruleType": "Regex",
  "rule": {
    "path": "identifier[0].value",
    "pattern": "^[STFG]\\d{7}[A-Z]$",
    "description": "NRIC must start with S/T/F/G, followed by 7 digits and a checksum letter"
  },
  "context": {
    "resourceType": "Patient",
    "identifierType": "NRIC",
    "actualValue": "ABC123XYZ"
  },
  "resourcePointer": {
    "entryIndex": 0,
    "fullUrl": "urn:uuid:pat-002",
    "resourceType": "Patient",
    "resourceId": "pat-002"
  }
}
```

### UI Rendering

**What This Means**:
> "The NRIC format is invalid. It should follow the pattern: Must start with S/T/F/G followed by 7 digits and a checksum letter (e.g., S1234567A)."

**Breadcrumb**:
> Patient → identifier (NRIC) → value

**Expected**: ✅ `^[STFG]\d{7}[A-Z]$`

**Actual**: ❌ `ABC123XYZ`

**How to Fix**:
1. Open the Patient resource (entry #0).
2. Correct the NRIC value to match the pattern: S1234567A
3. Valid examples: S1234567A, T9876543B, F5678901C
4. Ensure the checksum letter is correct.
5. Save and re-validate.

---

## Example 3: Invalid Question Answer (CodesMaster)

### Input Bundle
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
            "code": "HS_SMOKING_STATUS",
            "display": "Smoking Status"
          }]
        },
        "valueCodeableConcept": {
          "coding": [{
            "code": "occasionally",
            "display": "Smokes Occasionally"
          }]
        },
        "subject": {
          "reference": "urn:uuid:pat-003"
        }
      }
    },
    {
      "fullUrl": "urn:uuid:pat-003",
      "resource": {
        "resourceType": "Patient",
        "id": "pat-003"
      }
    }
  ]
}
```

### Validation Error
```json
{
  "code": "PSS_VAL_008",
  "message": "Answer 'Smokes Occasionally' is not in the allowed list for question 'Smoking Status'",
  "severity": "error",
  "fieldPath": "entry[0].resource.valueCodeableConcept.coding[0].code",
  "scope": "entry[0].resource",
  "ruleType": "CodesMaster",
  "rule": {
    "questionCode": "HS_SMOKING_STATUS",
    "screeningType": "HS"
  },
  "context": {
    "resourceType": "Observation",
    "screeningType": "HS",
    "questionCode": "HS_SMOKING_STATUS",
    "questionDisplay": "Smoking Status",
    "actualAnswer": "Smokes Occasionally",
    "allowedAnswers": [
      "Current Smoker",
      "Never Smoked",
      "Former Smoker",
      "Passive Smoker"
    ]
  },
  "resourcePointer": {
    "entryIndex": 0,
    "fullUrl": "urn:uuid:obs-smoking",
    "resourceType": "Observation",
    "resourceId": "obs-smoking"
  }
}
```

### UI Rendering

**What This Means**:
> "The answer provided for 'Smoking Status' is not in the approved list of answers. Only specific pre-defined answers are accepted for this question to ensure data consistency."

**Breadcrumb**:
> Observation → valueCodeableConcept → coding[0] → code

**Question Details**:
- **Question**: Smoking Status
- **Code**: HS_SMOKING_STATUS
- **Screening Type**: Health Screening (HS)

**Actual Answer**: ❌ `Smokes Occasionally`

**Allowed Answers**:
- ✅ Current Smoker
- ✅ Never Smoked
- ✅ Former Smoker
- ✅ Passive Smoker

**How to Fix**:
1. Open the Observation resource (entry #0).
2. Review the question: "Smoking Status"
3. Choose one of the allowed answers: "Current Smoker", "Never Smoked", "Former Smoker", "Passive Smoker"
4. Update both the code and display text to match exactly.
5. For questions with multiple values, use pipe separator: "Value1|Value2"
6. Save and re-validate.

---

## Example 4: Wrong Reference Type (Reference)

### Input Bundle
```json
{
  "resourceType": "Bundle",
  "type": "collection",
  "entry": [
    {
      "fullUrl": "urn:uuid:obs-weight",
      "resource": {
        "resourceType": "Observation",
        "id": "obs-weight",
        "code": {
          "coding": [{
            "system": "http://loinc.org",
            "code": "29463-7",
            "display": "Body Weight"
          }]
        },
        "subject": {
          "reference": "urn:uuid:org-clinic"
        },
        "valueQuantity": {
          "value": 70,
          "unit": "kg"
        }
      }
    },
    {
      "fullUrl": "urn:uuid:org-clinic",
      "resource": {
        "resourceType": "Organization",
        "id": "org-clinic",
        "name": "Singapore General Hospital"
      }
    }
  ]
}
```

### Validation Error
```json
{
  "code": "PSS_VAL_006",
  "message": "Reference 'urn:uuid:org-clinic' points to Organization, but only [Patient, Group] are allowed",
  "severity": "error",
  "fieldPath": "entry[0].resource.subject.reference",
  "scope": "entry[0].resource",
  "ruleType": "Reference",
  "rule": {
    "path": "subject",
    "targetTypes": ["Patient", "Group"],
    "description": "Observation subject must be a Patient or Group"
  },
  "context": {
    "resourceType": "Observation",
    "referencedType": "Organization",
    "referencedId": "org-clinic"
  },
  "resourcePointer": {
    "entryIndex": 0,
    "fullUrl": "urn:uuid:obs-weight",
    "resourceType": "Observation",
    "resourceId": "obs-weight"
  }
}
```

### UI Rendering

**What This Means**:
> "The reference to 'subject' points to the wrong type of resource (Organization). References must link to approved resource types (Patient, Group) to ensure data integrity."

**Breadcrumb**:
> Observation → subject → reference

**Allowed Reference Types**:
- Patient
- Group

**How to Fix**:
1. Open the Observation resource (entry #0).
2. Update the subject reference to point to a Patient or Group resource.
3. Verify the referenced resource exists in the Bundle.
4. Use the format: "urn:uuid:<resource-id>"
5. Ensure the resource type matches one of: Patient, Group
6. Save and re-validate.

---

## Example 5: Multi-Value Answer (PureTone)

### Input Bundle
```json
{
  "resourceType": "Bundle",
  "type": "collection",
  "entry": [
    {
      "fullUrl": "urn:uuid:obs-puretone",
      "resource": {
        "resourceType": "Observation",
        "id": "obs-puretone",
        "code": {
          "coding": [{
            "system": "http://synapxe.sg/fhir/CodeSystem/pss-questions",
            "code": "HS_PURETONE",
            "display": "PureTone Results"
          }]
        },
        "valueCodeableConcept": {
          "text": "500Hz Right, 1000Hz Right"
        }
      }
    }
  ]
}
```

### Validation Error
```json
{
  "code": "PSS_VAL_008",
  "message": "Multi-value answer must use pipe separator: '500Hz – R|1000Hz – R'",
  "severity": "error",
  "fieldPath": "entry[0].resource.valueCodeableConcept.text",
  "scope": "entry[0].resource",
  "ruleType": "CodesMaster",
  "rule": {
    "questionCode": "HS_PURETONE",
    "isMultiValue": true
  },
  "context": {
    "resourceType": "Observation",
    "questionCode": "HS_PURETONE",
    "questionDisplay": "PureTone Results",
    "actualAnswer": "500Hz Right, 1000Hz Right",
    "allowedAnswers": [
      "500Hz – R",
      "1000Hz – R",
      "2000Hz – R",
      "4000Hz – R",
      "500Hz – L",
      "1000Hz – L",
      "2000Hz – L",
      "4000Hz – L"
    ],
    "isMultiValue": true
  },
  "resourcePointer": {
    "entryIndex": 0,
    "fullUrl": "urn:uuid:obs-puretone",
    "resourceType": "Observation",
    "resourceId": "obs-puretone"
  }
}
```

### UI Rendering

**What This Means**:
> "The answer provided for 'PureTone Results' is not in the approved list of answers. This question allows multiple values separated by pipe (|)."

**Question Details**:
- **Question**: PureTone Results
- **Code**: HS_PURETONE
- **Format**: Multi-value (use pipe separator)

**Allowed Answers** (select multiple):
- ✅ 500Hz – R
- ✅ 1000Hz – R
- ✅ 2000Hz – R
- ✅ 4000Hz – R
- ✅ 500Hz – L
- ✅ 1000Hz – L
- ✅ 2000Hz – L
- ✅ 4000Hz – L

**Multi-value Format**:
> 📝 For questions that allow multiple answers, separate values with a pipe (|).  
> Example: `500Hz – R|1000Hz – R|2000Hz – R`

**How to Fix**:
1. Open the Observation resource (entry #0).
2. Review the question: "PureTone Results"
3. Choose one or more allowed answers from the list.
4. Combine multiple answers using pipe separator: "500Hz – R|1000Hz – R"
5. Ensure exact spelling and spacing (including "–" dash character).
6. Save and re-validate.

---

## Example 6: FullUrl/ID Mismatch

### Input Bundle
```json
{
  "resourceType": "Bundle",
  "type": "collection",
  "entry": [
    {
      "fullUrl": "urn:uuid:abc-123-def-456",
      "resource": {
        "resourceType": "Patient",
        "id": "xyz-789-ghi-000",
        "identifier": [
          {
            "system": "http://synapxe.sg/fhir/identifier/nric",
            "value": "S1234567A"
          }
        ]
      }
    }
  ]
}
```

### Validation Error
```json
{
  "code": "PSS_VAL_001",
  "message": "Resource.id 'xyz-789-ghi-000' does not match fullUrl GUID 'abc-123-def-456'",
  "severity": "error",
  "fieldPath": "entry[0].resource.id",
  "scope": "entry[0]",
  "ruleType": "FullUrlIdMatch",
  "rule": {
    "description": "Bundle entry fullUrl and resource.id must match"
  },
  "context": {
    "fullUrl": "urn:uuid:abc-123-def-456",
    "resourceId": "xyz-789-ghi-000",
    "resourceType": "Patient"
  },
  "resourcePointer": {
    "entryIndex": 0,
    "fullUrl": "urn:uuid:abc-123-def-456",
    "resourceType": "Patient",
    "resourceId": "xyz-789-ghi-000"
  }
}
```

### UI Rendering

**What This Means**:
> "The resource ID does not match the ID in the fullUrl. These must be identical to ensure the Bundle is correctly structured and resources can be referenced."

**Breadcrumb**:
> entry → fullUrl vs resource.id

**Expected Format**:
> entry.fullUrl = "urn:uuid:`abc-123-def-456`"  
> resource.id = "`abc-123-def-456`"

**Actual Values**:
- **fullUrl**: urn:uuid:`abc-123-def-456`
- **resource.id**: `xyz-789-ghi-000` ❌

**How to Fix**:
1. Open the Bundle entry (entry #0).
2. Extract the GUID from fullUrl: `abc-123-def-456`
3. Set resource.id to match: `"id": "abc-123-def-456"`
4. Ensure they are identical (case-sensitive).
5. Save and re-validate the Bundle.

---

## Example 7: CodeSystem - Invalid Status Code

### Input Bundle
```json
{
  "resourceType": "Bundle",
  "type": "collection",
  "entry": [
    {
      "fullUrl": "urn:uuid:obs-bp",
      "resource": {
        "resourceType": "Observation",
        "id": "obs-bp",
        "status": "completed",
        "code": {
          "coding": [{
            "system": "http://loinc.org",
            "code": "85354-9",
            "display": "Blood Pressure"
          }]
        },
        "component": [
          {
            "code": {
              "coding": [{
                "system": "http://loinc.org",
                "code": "8480-6"
              }]
            },
            "valueQuantity": {
              "value": 120,
              "unit": "mmHg"
            }
          }
        ]
      }
    }
  ]
}
```

### Validation Error
```json
{
  "code": "PSS_VAL_007",
  "message": "Code 'completed' is not valid for Observation.status",
  "severity": "error",
  "fieldPath": "entry[0].resource.status",
  "scope": "entry[0].resource",
  "ruleType": "CodeSystem",
  "rule": {
    "path": "status",
    "system": "http://hl7.org/fhir/observation-status",
    "description": "Observation status must be from ObservationStatus code system"
  },
  "context": {
    "resourceType": "Observation",
    "actualCode": "completed",
    "codeSystemConcepts": [
      {"code": "registered", "display": "Registered"},
      {"code": "preliminary", "display": "Preliminary"},
      {"code": "final", "display": "Final"},
      {"code": "amended", "display": "Amended"},
      {"code": "corrected", "display": "Corrected"},
      {"code": "cancelled", "display": "Cancelled"},
      {"code": "entered-in-error", "display": "Entered in Error"},
      {"code": "unknown", "display": "Unknown"}
    ]
  },
  "resourcePointer": {
    "entryIndex": 0,
    "fullUrl": "urn:uuid:obs-bp",
    "resourceType": "Observation",
    "resourceId": "obs-bp"
  }
}
```

### UI Rendering

**What This Means**:
> "The code 'completed' is not valid for status. Only codes from the official FHIR ObservationStatus code system are allowed."

**Breadcrumb**:
> Observation → status

**Expected**: `http://hl7.org/fhir/observation-status`

**Allowed Codes**:
| Code | Display |
|------|---------|
| registered | Registered |
| preliminary | Preliminary |
| final | Final |
| amended | Amended |
| corrected | Corrected |
| cancelled | Cancelled |
| entered-in-error | Entered in Error |
| unknown | Unknown |

**How to Fix**:
1. Open the Observation resource (entry #0).
2. Review the allowed codes in the table above.
3. Choose the appropriate status code (likely "final" instead of "completed").
4. Update the status field: `"status": "final"`
5. Ensure you use the exact code value from the table.
6. Save and re-validate.

---

## Code Organization

### Backend Enrichment Flow
```
Bundle → ValidationEngine.ValidateWithMetadata()
      ↓
    ValidationError (basic)
      ↓
    ValidationErrorEnricher.EnrichError()
      ↓
    + Rule (from validation-metadata.json)
    + Context (from CodesMaster, resource analysis)
    + ResourcePointer (from scope parsing)
      ↓
    ValidationError (enriched)
      ↓
    API Response
```

### Frontend Rendering Flow
```
ValidationError (from API)
      ↓
    generateHelper(error)
      ↓
    Detect RuleType
      ↓
    Call appropriate template function:
      - renderRequired()
      - renderRegex()
      - renderCodesMaster()
      - etc.
      ↓
    Template calls helper functions:
      - generateWhatThisMeans()
      - generateBreadcrumb()
      - generateHowToFix()
      ↓
    Return structured helper object
      ↓
    ValidationHelper.jsx renders UI
      - Blue "What this means" box
      - Breadcrumb navigation
      - Resource pointer card
      - Green "How to fix" box
```

### Metadata Sources
```
validation-metadata.json
  ↓
  Rules by screeningType + resourceType
  ↓
  • RuleType (Required, Regex, etc.)
  • Path (CPS1 format)
  • Pattern, TargetTypes, System, etc.
  • Description

codes-master.json
  ↓
  Questions by screeningType
  ↓
  • QuestionCode
  • QuestionDisplay
  • AllowedAnswers[]
  • IsMultiValue

Bundle (at runtime)
  ↓
  Entry analysis
  ↓
  • EntryIndex
  • FullUrl
  • ResourceType
  • ResourceId
```

---

## Testing Each Example

### Quick Test Script
```bash
# Test Required (Example 1)
curl -X POST http://localhost:5063/api/fhir/validate \
  -H "Content-Type: application/json" \
  -d @examples/ex1-missing-nric.json

# Test Regex (Example 2)
curl -X POST http://localhost:5063/api/fhir/validate \
  -H "Content-Type: application/json" \
  -d @examples/ex2-invalid-nric.json

# Test CodesMaster (Example 3)
curl -X POST http://localhost:5063/api/fhir/validate \
  -H "Content-Type: application/json" \
  -d @examples/ex3-invalid-answer.json

# Test Reference (Example 4)
curl -X POST http://localhost:5063/api/fhir/validate \
  -H "Content-Type: application/json" \
  -d @examples/ex4-wrong-reference.json

# Test Multi-Value (Example 5)
curl -X POST http://localhost:5063/api/fhir/validate \
  -H "Content-Type: application/json" \
  -d @examples/ex5-multivalue.json

# Test FullUrl (Example 6)
curl -X POST http://localhost:5063/api/fhir/validate \
  -H "Content-Type: application/json" \
  -d @examples/ex6-fullurl-mismatch.json

# Test CodeSystem (Example 7)
curl -X POST http://localhost:5063/api/fhir/validate \
  -H "Content-Type: application/json" \
  -d @examples/ex7-invalid-status.json
```

### Expected Response Structure
```json
{
  "isValid": false,
  "errors": [
    {
      "code": "PSS_VAL_XXX",
      "message": "...",
      "severity": "error",
      "fieldPath": "...",
      "scope": "...",
      "ruleType": "...",
      "rule": { ... },
      "context": { ... },
      "resourcePointer": {
        "entryIndex": 0,
        "fullUrl": "...",
        "resourceType": "...",
        "resourceId": "..."
      }
    }
  ]
}
```

---

## Summary

These real-world examples demonstrate:

1. **Complete Flow**: From input bundle → backend validation → enrichment → API response → frontend rendering
2. **Rule Type Coverage**: All major rule types with actual data
3. **Plain-English**: Non-technical explanations for each error type
4. **Navigation**: ResourcePointer with entry index for all errors
5. **Actionable Steps**: Specific fix instructions with entry numbers
6. **Visual Breadcrumbs**: Clear path through data structure
7. **Metadata-Driven**: All content generated from rules and codes, no hardcoding

Each example can be used as:
- **Test data** for development
- **Documentation** for users
- **Training material** for clinicians
- **Reference** for future enhancements
