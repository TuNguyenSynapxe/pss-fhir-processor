
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
Located under `/docs`.

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
**82/82 tests passing (100%)**
- 29 unit tests (validation rules, extraction logic)
- 53 end-to-end tests (complete bundle scenarios)

```bash
dotnet test --framework net6.0
```

## 8. Folder Structure
See folder-structure.md.

# END OF README
