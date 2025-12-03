
# 13 — Appendix & Sample Bundles (Part 1/2)
PSS FHIR Processor — Reference Appendix  
Version 1.0 — 2025

---

# 1. Introduction

This appendix includes:
- Sample FHIR bundles (valid/invalid)
- Sample FlattenResult JSON
- Sample Validation Errors
- Codes Master excerpt
- RuleSet excerpt

These samples help developers, QA, and vendors understand expected input/output.

---

# 2. Sample: Valid put-screening Bundle (Trimmed)

```json
{
  "resourceType": "Bundle",
  "type": "collection",
  "entry": [
    {
      "resource": {
        "resourceType": "Encounter",
        "status": "completed",
        "actualPeriod": {
          "start": "2025-01-10T09:00:00+08:00",
          "end": "2025-01-10T09:20:00+08:00"
        }
      }
    },
    {
      "resource": {
        "resourceType": "Patient",
        "identifier": [
          { "system": "https://fhir.synapxe.sg/identifier/nric", "value": "S1234567A" }
        ],
        "name": [{ "text": "John Tan" }],
        "gender": "male",
        "birthDate": "1950-01-01"
      }
    }
  ]
}
```

(Trimmed for readability.)

---

# 3. Sample: Invalid Bundle (Missing HS/OS/VS)

```json
{
  "resourceType": "Bundle",
  "type": "collection",
  "entry": [
    { "resource": { "resourceType": "Encounter" } }
  ]
}
```

Validation output:

```json
{
  "isValid": false,
  "errors": [
    { "code": "MISSING_SCREENING_TYPE", "scope": "Bundle" }
  ]
}
```

---

# 4. Sample: Invalid Display

```json
"coding": [
  { "code": "SQ-L2H9-00000001", "display": "Wrong Display Text" }
]
```

Error:

```json
{
  "code": "QUESTION_DISPLAY_MISMATCH",
  "fieldPath": "Observation.component[0].code.coding[0].display"
}
```

---

# 5. Codes Master Excerpt

```json
{
  "questionCode": "SQ-L2H9-00000001",
  "questionDisplay": "Currently wearing hearing aid(s)?",
  "screeningType": "HS",
  "allowedAnswers": ["Yes", "No"]
}
```
