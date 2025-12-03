
# 07 — CRM Mapping Guidelines
PSS FHIR Processor — CRM Integration Guidance  
Version 1.0 — 2025

---

# 1. Introduction

This document provides guidance for CRM developers who will map the `FlattenResult` output into Microsoft Dynamics 365 (Dataverse) entities.  
The Extraction Engine produces a CRM-agnostic data model, and CRM developers decide:

- Which entity stores which field  
- Whether options are text or lookup  
- How to normalize multi-value answers  
- How screening questions map to CRM schema  

This file is **guidance only**. No CRM logic exists inside the DLL.

---

# 2. Principles

## 2.1 CRM-Agnostic Processor
The FHIR Processor DLL:
- Does NOT depend on Dataverse
- Does NOT reference CRM assemblies
- Does NOT output CRM entity models

CRM developers derive logic externally.

## 2.2 FlattenResult as the Source of Truth
All mappings must use values in:

```csharp
FlattenResult.Event
FlattenResult.Participant
FlattenResult.HearingRaw
FlattenResult.OralRaw
FlattenResult.VisionRaw
```

---

# 3. Recommended CRM Entity Structure

A possible CRM design includes:

### Core Entities
- **PSS Screening Case** (parent record)
- **PSS Participant Profile**
- **PSS Event Metadata**

### Screening Detail Entities
- **PSS Hearing Screening**
- **PSS Oral Screening**
- **PSS Vision Screening**

### Observations (if needed)
- **PSS Screening Observation** (1:N under each screening)

---

# 4. Mapping Strategy

## 4.1 Event Metadata
Map:
- Start → datetime  
- End → datetime  
- VenueName → text  
- PostalCode → text  
- GRC → lookup or text  
- Constituency → lookup or text  
- ProviderName → lookup or text  
- ClusterName → lookup or text  

## 4.2 Participant
Map:
- NRIC → text  
- Name → text  
- Gender → optionset  
- BirthDate → date  

Address mapping optional.

---

# 5. Screening Sets (HS / OS / VS)

Each screening set:
```csharp
ScreeningSet.Items : List<ObservationItem>
```

Each observation:
```csharp
ObservationItem.Question.Code
ObservationItem.Question.Display
ObservationItem.Values[]
```

CRM developers decide:
- Store raw Value(s)
- Normalize into fields
- Use lookup entities for answers

---

# 6. Lookup Patterns

### Option A — Store as Text (simple)
Fields:
- QuestionCode (text)
- QuestionDisplay (text)
- Answer (text)

### Option B — Store Answer as Lookup
Create:
- PSS Screening Question (master list)
- PSS Screening Answer (master list)

This allows:
- versioning  
- reporting consistency  
- analytics on structured options  

---

# 7. Multi-Value Answers (PureTone)

PureTone results return:
```
["500Hz – R", "1000Hz – NR"]
```

CRM options:
- Store as multiline text  
- Store as separate child records (recommended for analytics)  
- Store as multi-select option set (if limited values)  

---

# 8. Recommended Mapping for CRM Developers

### 8.1 Use Lookup Helpers
```csharp
var pt = flatten.GetHearing("SQ-L2H9-00000001");
```

### 8.2 Avoid Hardcoding Question Codes
Use a configuration table for mapping.

### 8.3 Store Original Payload
Save the original FHIR Bundle in a text field for audit.

### 8.4 Use Plugin Tracing for Debugging
Log extracted fields at debug level.

---

# 9. Future Automation Possibility

Later phases may generate:
- CRM entities
- Field mappings
- Option sets
- Plugin stubs

based on the `Codes Master`.

---

# END OF FILE 07

