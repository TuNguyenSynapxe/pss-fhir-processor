
# PSS FHIR Processor — Full Solution Overview

## 1. Introduction
The PSS FHIR Processor is a metadata-driven engine that validates and extracts 
Healthier SG (HSG) screening submissions (HS/OS/VS) in FHIR Bundle format.
It includes:
- Standalone DLL (Validation + Extraction Engines)
- Modern .NET 8.0 Web API + React Frontend
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
- **Pss.FhirProcessor** — Core DLL (.NET 6.0)
- **Pss.FhirProcessor.NetCore** — Modern Web Stack
  - Backend: .NET 8.0 Web API
  - Frontend: React + Vite + Ant Design + Tailwind CSS
- **Unit Tests**
- **Metadata** — JSON rule definitions

## 4. Documentation
Located under `/docs/`:
- **01-10**: Core documentation (Overview, Architecture, FHIR Spec, etc.)
- **11**: Unit Test Plan
- **12**: Deployment Guide
- **13**: Appendix (Sample Bundles)
- **14**: Validation Rules Reference (Complete guide to all 10 rule types)
- **15**: Implementation Notes (Advanced features)
- **LOGGING_GUIDE.md**: Comprehensive logging documentation
- **metadata-user-guide.md**: Metadata configuration guide
- **folder-structure.md**: Project structure overview

## 5. Getting Started

### Backend
```bash
cd src/Pss.FhirProcessor.NetCore/Backend
dotnet run
```
API runs on `http://localhost:5000`

### Frontend
```bash
cd src/Pss.FhirProcessor.NetCore/Frontend
npm install
npm run dev
```
Frontend runs on `http://localhost:5174`

## 6. API Endpoints
```
POST /api/fhir/validate
POST /api/fhir/process
GET  /api/fhir/codes-master
GET  /api/fhir/rules
```

## 7. Running Tests
**213/213 tests passing (100%)**
- 107 unit tests (TypeChecker, validation rules, extraction logic)
- 53 integration tests (complete bundle scenarios)
- 15 reference validation tests
- 11 FullUrlIdMatch tests
- 27 extraction tests

```bash
dotnet test --framework net6.0
```

## 8. Folder Structure
See folder-structure.md.

# END OF README
