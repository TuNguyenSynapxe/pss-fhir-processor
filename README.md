
# PSS FHIR Processor — Full Solution Overview

## 1. Introduction
The PSS FHIR Processor is a metadata-driven engine that validates and extracts 
Healthier SG (HSG) screening submissions (HS/OS/VS) in FHIR Bundle format.
It includes:
- Standalone DLL (Validation + Extraction Engines)
- ASP.NET MVC Playground WebApp
- Public API (POC)
- Unit Test Suite
- Metadata RuleSets + Codes Master
- Comprehensive documentation

## 2. Key Features
- Metadata-driven validation
- Deterministic extraction
- Detailed error reporting
- Vendor testing Playground
- REST API for automation
- No CRM or DB dependencies

## 3. Components
- **Pss.FhirProcessor** — Core DLL
- **WebApp** — Interactive Playground
- **Public API**
- **Unit Tests**
- **Metadata** — JSON rule definitions

## 4. Documentation
Located under `/docs`.

## 5. WebApp Playground
Access:
```
/Playground
```

## 6. Public API Endpoints
```
POST /api/fhir/validate
POST /api/fhir/process
GET  /api/fhir/codes-master
GET  /api/fhir/rules
```

## 7. Running Tests
```
dotnet test
```

## 8. Folder Structure
See folder-structure.md.

# END OF README
