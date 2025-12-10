# 01 — Project Overview
## PSS FHIR Processor — Validation & Extraction Framework
Version: 1.0
Last Updated: 2025-12
Author: System Architecture (based on requirements by Tu Nguyen)

---

# 1. Introduction

The **PSS FHIR Processor** project provides a reusable, metadata-driven C# library (DLL) and a modern web application for validating and extracting data from **put-screening** FHIR Bundle submissions for *Project Silver Screen (PSS)*.

It ensures that all screening submissions follow:

- The latest **PSS Integration Specification (v1.6.3)**
- The latest **Requirement Specification (v1.6)**
- FHIR R4 conventions
- Internal rules, fixed values, and mandatory fields
- Codes Master certifications for question code, display, and answer validity

This project is designed to operate inside:

- CRM plugins (Dynamics 365)
- Modern web applications
- Batch/Scheduled processors
- Integration middleware

The DLL is **entirely CRM-agnostic** and does not contain any Dataverse dependency.

---

# 2. Problem Statement

PSS Screening providers submit the **Screening Bundle** in FHIR JSON format.
However:

- Many incoming bundles contain incomplete or invalid screening data.
- Screening Observations sometimes miss mandatory questions.
- Answers may not match the question’s allowed values (e.g., PureTone).
- Question codes or displays may be mismatched.
- Screens may fail because HS/OS/VS Observations are missing.
- Fixed values (e.g., Encounter.status) may be incorrect.

There is currently **no consistent validation or extraction** utility to verify and flatten submissions before CRM ingestion.

This project solves that gap.

---

# 3. Goals & Deliverables

## 3.1 DLL (Core Processing Engine)
A single reusable assembly:

```
MOH.HealthierSG.PSS.FhirProcessor.dll
```

With responsibilities:

### Validation
- Validate JSON → FHIR structure
- Validate **mandatory resource types**
- Validate **mandatory questions**
- Validate **all 3 screening types (HS/OS/VS)** exist
- Validate **Codes Master**:
  - Question codes
  - Display texts
  - Allowed single-value answers
  - Allowed multi-value answers (PureTone)
- Validate conditional dependencies (if/then rules)
- Validate fixed values and fixed codings

### Flattening
Convert FHIR Bundle → **FlattenResult**

- `EventData`
- `ParticipantData`
- `ScreeningSet` for each:
  - HearingRaw
  - OralRaw
  - VisionRaw
- Multi-value parsing (pipe-separated values)
- Provide **lookup helpers**:
  - `GetHearing(questionCode)`
  - `GetOral(questionCode)`
  - `GetVision(questionCode)`

### Metadata-driven (no hardcoding)
- Validation RuleSets in JSON
- Codes Master JSON
- No hard-coded logic in C#

### No environmental dependencies
- No file system
- No database
- Host passes metadata as JSON strings

---

## 3.2 Modern Web Application (.NET 8.0 + React)

A modern full-stack application featuring:

### Backend (.NET 8.0 Web API)
- RESTful API endpoints
- Swagger/OpenAPI documentation
- Singleton service pattern
- CORS-enabled for frontend

### Frontend (React + Vite + Ant Design + Tailwind CSS)
- **Playground** - Interactive JSON editor with validation/extraction
- **Validation Rules** - Display all configured rules and code systems
- **Test Cases** - Run predefined test scenarios
- **API Documentation** - Interactive API explorer

### Features
- Real-time validation
- Modern responsive UI
- Component-based architecture
- Hot module replacement (HMR)

### All metadata in-memory
- RuleSetSeed
- CodesMasterSeed
- TestCaseSeed

---

## 3.3 Full Documentation Set (Files 01–13)

- Architecture
- Models
- Validation engine
- Extraction engine
- Modern web application
- Test suite
- Sample bundles

---

# 4. Non-Goals

- No CRM model classes
- No CRM save logic
- No authentication (for local development)
- No database
- No production hosting requirements

---

# 5. Technology Stack

| Layer | Technology |
|------|------------|
| Core DLL | C#, .NET 6.0 |
| Backend API | .NET 8.0 Web API |
| Frontend | React 19 + Vite + Ant Design 5 + Tailwind CSS |
| Tests | MSTest / NUnit / xUnit |
| JSON | Newtonsoft.Json |
| Build Tools | dotnet, npm, Vite |
| Metadata | In-memory JSON |

---

# 6. Key Architectural Principles

## 6.1 Strict Validation for Screening Types
System **always expects all three**:
- HS
- OS
- VS

Missing any → error `MISSING_SCREENING_TYPE`.

## 6.2 Metadata-Driven Validation
All logic comes from:
- RuleSets JSON
- CodesMaster JSON

## 6.3 No Environment Dependencies
Everything injected through:
```
LoadRuleSets()
LoadCodesMaster()
SetValidationOptions()
```

## 6.4 CRM-Agnostic
FlattenResult is purely generic.

---

# 7. Users

- CRM Plugin Developers
- Modern web developers (React/REST API)
- QA engineers
- Architects

---

# 8. Benefits

- Standardization
- Reusable architecture
- Predictable flattened output
- Supports automation and Copilot

---

# 9. Risks & Mitigations

| Risk | Mitigation |
|------|------------|
| Malformed JSON | Strict validation |
| Missing screening types | Always-enforced rule |
| Wrong display text | CodesMaster validation |

---

# 10. Workflow

```
FHIR JSON → FhirProcessor → Validation → Extraction → ProcessResult
```

---

File 01 End.
