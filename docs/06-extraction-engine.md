
# 06 — Extraction Engine Design 
Version 1.0 — 2025

---

# 1. Introduction

The Extraction Engine converts a validated `put-screening` FHIR Bundle into a normalized, CRM‑agnostic `FlattenResult` object.

If validation fails:
- Extraction does NOT run.
- `FlattenResult` = null.

Extraction always:
- Reads minimal FHIR structures
- Produces deterministic output
- Never throws for data errors (validation already handled)
- Supports HS / OS / VS screening extraction
- Produces helper lookup functions for CRM developers

---

# 2. Architectural Principles

## 2.1 Validation-First
Extraction runs **only when**:
```
ValidationResult.IsValid == true
```

## 2.2 No Business Logic
Extraction does not interpret:
- meanings of answers  
- workflow conditions  

It only maps data into structured models.

## 2.3 Deterministic Output
Given the same bundle → same FlattenResult.

## 2.4 No External Dependencies
No DB, no file I/O, no CRM calls.

---

# 3. Public API

```csharp
public class ExtractionEngine
{
    public FlattenResult Extract(Bundle bundle);
}
```

The engine assumes:
- Validation already completed
- Bundle contains HS, OS, VS
- FHIR structure is correctly formed

---

# 4. Extraction Workflow

```
Extract(bundle)
  → Extract Event data
  → Extract Participant data
  → Extract Location data
  → Extract Organizations
  → Extract HS/OS/VS Observation sets
  → Build ObservationItem list
  → Return FlattenResult
```

---

# 5. Event Extraction

Event data is compiled from:
- Encounter
- Location
- HealthcareService
- Provider Organization
- Cluster Organization

Populated fields:
- Start, End
- VenueName
- PostalCode
- GRC / Constituency (from extension)
- ProviderName
- ClusterName

Model:
```csharp
public class EventData { ... }
```

---

# 6. Participant Extraction

From Patient resource:
- NRIC
- Name
- Gender
- BirthDate
- Address (optional flattening)

Model:
```csharp
public class ParticipantData { ... }
```

---

# 7. Screening Extraction (HS/OS/VS)

Each Observation is converted into:

```csharp
public class ScreeningSet
{
    public string ScreeningType; // HS / OS / VS
    public List<ObservationItem> Items;
}
```

ObservationItem built from:

- component.code.coding[0].code → Question.Code
- component.code.coding[0].display → Question.Display
- component.valueString → Values (split if multi-value)

Model:
```csharp
public class ObservationItem
{
    public CodeDisplayValue Question;
    public List<string> Values;
}
```

---

# 8. Multi-Value Answer Handling

If `valueString` contains `"|"`:
1. Split by `"|"`  
2. Trim entries  
3. Add to `Values[]`

Example:
```
"500Hz – R|1000Hz – NR"
```

→
```
["500Hz – R", "1000Hz – NR"]
```

---

# 9. Lookup Helpers

Helpers inside FlattenResult:

```csharp
GetHearing(code)
GetOral(code)
GetVision(code)
```

Returns the matching `ObservationItem`.

This allows CRM developers to write:

```csharp
var aid = flatten.GetHearing("SQ-L2H9-00000001");
```

---

# 10. Error Handling

Extraction never throws exceptions for:
- Missing fields  
- Empty components  
- Unexpected formats  

These cases are impossible because:
- ValidationEngine enforces correctness
- Extraction assumes data is valid

---

# 11. Full FlattenResult Structure

```csharp
public class FlattenResult
{
    public EventData Event;
    public ParticipantData Participant;

    public ScreeningSet HearingRaw;
    public ScreeningSet OralRaw;
    public ScreeningSet VisionRaw;

    public ObservationItem GetHearing(string code);
    public ObservationItem GetOral(string code);
    public ObservationItem GetVision(string code);
}
```

---

# 12. Summary Checklist

Extraction Engine:
- Reads validated FHIR Bundle  
- Produces clean FlattenResult  
- Handles HS/OS/VS screening  
- Handles multi-value answers  
- Provides lookup helpers  
- No business logic  
- No external dependencies  

---

# END OF FILE 06
