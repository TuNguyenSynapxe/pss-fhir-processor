# 03 — FHIR Bundle Specification
## PSS put-screening — Bundled Screening Submission  
Version: 1.0  
Last Updated: 2025-12  

---

# 1. Purpose

This document defines the **expected FHIR R4 structure** of the `put-screening` Bundle for Project Silver Screen (PSS).  
It describes:

- Required resources  
- Screening Observations (HS/OS/VS)  
- Component question/answer structures  
- Codes Master alignment requirements  
- Conditional rules  
- PureTone multi-value structures  
- Validation constraints for the DLL  

---

# 2. Bundle Structure

Each submission is a FHIR Bundle:

```json
{
  "resourceType": "Bundle",
  "type": "collection",
  "entry": [ ... ]
}
```

---

# 3. Required Resources (Exactly One Each)

The bundle **must contain** the following resources once:

| Resource | Purpose |
|---------|---------|
| Encounter | Event metadata |
| Patient | Participant demographics |
| Location | Venue, postal code, GRC, constituency |
| HealthcareService | Mobile team |
| Organization (Provider) | Screening provider |
| Organization (Cluster) | Cluster organization |
| Observation (HS) | Hearing screening |
| Observation (OS) | Oral screening |
| Observation (VS) | Vision screening |

Missing HS/OS/VS → `MISSING_SCREENING_TYPE`.

---

# 4. Encounter Resource

Required fields:

- `status = "completed"`  
- `actualPeriod.start`  
- `actualPeriod.end`  
- `location[0].location.reference`  
- `serviceType[*].reference`  

Example:

```json
{
  "resourceType": "Encounter",
  "status": "completed",
  "location": [
    { "location": { "reference": "urn:uuid:loc-1" } }
  ],
  "actualPeriod": {
    "start": "2025-01-01T09:00:00+08:00",
    "end": "2025-01-01T09:20:00+08:00"
  }
}
```

---

# 5. Patient Resource

Must contain:

- `identifier[system=nric]`
- `name.text`
- `gender`
- `birthDate`

Example:

```json
{
  "resourceType": "Patient",
  "identifier": [
    { "system": "https://fhir.synapxe.sg/identifier/nric", "value": "S1234567A" }
  ],
  "name": [{ "text": "John Tan" }],
  "gender": "male",
  "birthDate": "1950-01-01"
}
```

---

# 6. Location Resource

Required for:

- Venue name
- Postal code
- GRC
- Constituency

Example:

```json
{
  "resourceType": "Location",
  "name": "ABC CC",
  "address": { "postalCode": "123456" },
  "extension": [
    {
      "url": "https://fhir.synapxe.sg/StructureDefinition/grc",
      "valueString": "Ang Mo Kio GRC"
    },
    {
      "url": "https://fhir.synapxe.sg/StructureDefinition/constituency",
      "valueString": "Ang Mo Kio"
    }
  ]
}
```

---

# 7. HealthcareService & Organizations

Each must exist once.  
Provider organization and cluster organization distinguished by `Organization.type.coding.code`.

---

# 8. Screening Observations (HS / OS / VS)

Each screening type is distinguished by:

```
Observation.code.coding[0].code
```

| Code | Type |
|------|------|
| HS | Hearing |
| OS | Oral |
| VS | Vision |

Every Observation contains:

```
component[]
```

Each component is a question+answer pair.

---

# 9. Observation Component Structure

Each `component[]` entry:

| Field | Meaning |
|-------|--------|
| `component.code.coding[0].code` | Question code |
| `component.code.coding[0].display` | Question label |
| `valueString` | Answer value |

ValidationEngine checks:

- Question code exists in Codes Master  
- Display matches  
- Value is allowed  

---

# 10. PureTone Multi-Value Questions

`valueString` may contain multiple entries:

```
"500Hz – R|1000Hz – NR|2000Hz – R"
```

DLL handles:

- Splitting  
- Trimming  
- Validation per allowed answers  
- Populating `Values[]`

---

# 11. Mandatory Questions

Defined in RuleSets:

- HS mandatory questions
- OS mandatory questions
- VS mandatory questions

Missing one → `MANDATORY_MISSING`.

---

# 12. Fixed Value & Coding Validation

Examples:

| Resource | Path | Expected | Error Code |
|---------|------|----------|------------|
| Encounter | status | completed | FIXED_VALUE_MISMATCH |
| Observation.code.coding.system | screening-type system | fixed URL | FIXED_CODING_MISMATCH |

---

# 13. Conditional Logic

Example:

- If `Proceed with hearing screening? = No`  
  → PureTone questions **must NOT** be included.

- If `Proceed with hearing screening? = Yes`  
  → PureTone questions **must** be included.

---

# 14. Full Valid Sample

Provided in:  
`docs/13-appendix-sample-bundles.md`

---

# 15. Summary

The put-screening bundle must contain:

- All required resources  
- All three screening Observations  
- Valid question codes  
- Valid displays  
- Allowed answers  
- Correct PureTone structure  

ValidationEngine enforces structural, semantic, and business-rule correctness.
