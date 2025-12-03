
# 09 — Public API Design
PSS FHIR Processor — External API Specification  
Version 1.0 — 2025

---

# 1. Introduction

The Public API exposes the PSS FHIR Processor functionalities to:
- Vendors
- Internal QA automation
- Integration gateways

This API is part of the **Playground WebApp**, but can be deployed standalone if required.

The API does **NOT**:
- Persist data
- Require authentication (for POC)
- Call CRM
- Modify any system state

---

# 2. Base URL (POC)

```
/api/fhir
```

All APIs return:
```json
{
  "success": true/false,
  "validation": { ... },
  "flatten": { ... },
  "logs": [ ... ]
}
```

---

# 3. API Endpoints Overview

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/fhir/validate | Validate FHIR JSON |
| POST | /api/fhir/extract | Validate + extract |
| POST | /api/fhir/process | Shortcut = validate + extract |
| GET | /api/fhir/codes-master | Return Codes Master JSON |
| GET | /api/fhir/rules | Return RuleSets JSON |

---

# 4. Endpoint: Validate Only

```
POST /api/fhir/validate
```

### Request Body
Raw FHIR JSON bundle.

### Response Example
```json
{
  "success": false,
  "validation": {
    "isValid": false,
    "errors": [
      {
        "code": "MISSING_SCREENING_TYPE",
        "fieldPath": "Bundle",
        "message": "Missing screening type: HS",
        "scope": "Bundle"
      }
    ]
  },
  "flatten": null,
  "logs": [ "Validation started...", "Missing HS" ]
}
```

---

# 5. Endpoint: Extract (After Validation)

```
POST /api/fhir/extract
```

Same request body as `/validate`.

### Behavior
- Runs ValidationEngine
- If valid → ExtractionEngine
- If invalid → flatten = null

---

# 6. Endpoint: Process (Validate + Extract)

```
POST /api/fhir/process
```

Equivalent to calling:
1. `/validate`
2. `/extract`

### Response Example
```json
{
  "success": true,
  "validation": { "isValid": true },
  "flatten": {
    "event": {
      "venueName": "ABC CC",
      "postalCode": "123456"
    },
    "participant": {
      "nric": "S1234567A",
      "name": "John"
    },
    "hearingRaw": { "screeningType": "HS" },
    "oralRaw": { "screeningType": "OS" },
    "visionRaw": { "screeningType": "VS" }
  },
  "logs": [
    "Validation completed: OK",
    "Extraction completed"
  ]
}
```

---

# 7. Endpoint: Get Codes Master

```
GET /api/fhir/codes-master
```

Returns:
```json
{
  "questions": [
    {
      "questionCode": "SQ-L2H9-00000001",
      "questionDisplay": "Currently wearing hearing aid(s)?",
      "screeningType": "HS",
      "allowedAnswers": ["Yes","No"]
    }
  ]
}
```

---

# 8. Endpoint: Get RuleSets

```
GET /api/fhir/rules
```

Returns all rules per scope:
- Event
- Participant
- HS
- OS
- VS

---

# 9. Error Handling

API never throws HTTP 500 for data-validation issues.

HTTP codes:
- 200 → request processed normally  
- 400 → malformed JSON in request  
- 500 → internal system error  

Validation errors appear inside:
```
response.validation.errors[]
```

---

# 10. Summary

The Public API:
- Wraps ValidationEngine + ExtractionEngine
- Supports validate-only, extract-only, full-process
- Returns detailed errors + logs
- Fully stateless
- Can be used for QA automation & vendor onboarding

---

# END OF FILE 09
