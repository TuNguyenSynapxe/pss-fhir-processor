# 08 — Modern Web Application
PSS FHIR Processor — Web Application Architecture  
Version 2.0 — 2025

---

# 1. Introduction

This document describes the PSS FHIR Processor modern web application, built using:
- **Backend**: .NET 8.0 Web API
- **Frontend**: React 19 + Vite + Ant Design 5 + Tailwind CSS
- **Architecture**: RESTful API + SPA
- **Data**: In-memory seeding (no database)

The web application serves multiple purposes:

1. Interactive Developer Console  
2. Validation Rules Explorer
3. Test Case Runner
4. API Documentation

---

# 2. Architecture Overview

## 2.1 Backend Components (.NET 8.0)

```
Backend/
├── Controllers/
│   └── FhirController.cs        # RESTful API endpoints
├── Services/
│   ├── IFhirProcessorService.cs
│   └── FhirProcessorService.cs  # Business logic
├── Seed/
│   ├── RuleSetSeed.cs          # Validation rules
│   ├── CodesMasterSeed.cs      # Questions & code systems
│   └── TestCaseSeed.cs         # Test scenarios
├── Program.cs                   # API configuration
└── Pss.FhirProcessor.Api.csproj
```

## 2.2 Frontend Components (React)

```
Frontend/
├── src/
│   ├── components/
│   │   ├── Playground.jsx         # Interactive JSON processor
│   │   ├── ValidationRules.jsx    # Rules & code systems display
│   │   ├── TestRunner.jsx         # Test case execution
│   │   └── ApiDocs.jsx           # API documentation
│   ├── services/
│   │   └── api.js                # Axios HTTP client
│   ├── App.jsx                   # Main application
│   └── main.jsx                  # Entry point
├── index.html
├── vite.config.js
└── package.json
```

---

# 3. Application Flow

```
User Input (JSON) → React Component → Axios → .NET API → FhirProcessor DLL → Response → React UI
```

---

# 4. Pages

## 4.1 Playground
**Route**: `/`  
**Purpose**: Interactive FHIR Bundle validation and extraction

**Features**:
- Monaco code editor with JSON syntax highlighting
- Three processing modes (tabs):
  - **Validation**: Check Bundle validity
  - **Extraction**: Flatten to CRM-ready format
  - **Logs**: View processing details
- Real-time validation
- Error highlighting
- Copy result functionality

## 4.2 Validation Rules
**Route**: `/validation-rules`  
**Purpose**: Display all configured validation rules and code systems

**Features**:
- Tabbed interface for rule scopes (Event, Participant, HS, OS, VS)
- Code Systems tab showing all 7 code systems
- Question repository grouped by screening type
- Color-coded rule types
- Searchable tables

## 4.3 Test Cases
**Route**: `/test-cases`  
**Purpose**: Run predefined test scenarios

**Features**:
- List of 40+ test cases
- Run individual or all tests
- Real-time execution status
- Success/failure indicators
- Detailed results modal

## 4.4 API Documentation
**Route**: `/api-docs`  
**Purpose**: Interactive API reference

**Features**:
- Endpoint listing with descriptions
- Request/response examples
- Try-it-out functionality
- Code samples

---

# 5. API Endpoints

### POST /api/fhir/validate
Validate FHIR Bundle only

### POST /api/fhir/extract  
Extract and flatten Bundle (validation included)

### POST /api/fhir/process
Complete validation + extraction + logs

### GET /api/fhir/codes-master
Retrieve all questions and code systems

### GET /api/fhir/rules
Retrieve all validation rules

### GET /api/fhir/test-cases
List all available test cases

### GET /api/fhir/test-cases/{name}
Get specific test case

---

# 6. Technology Stack

## Backend
- .NET 8.0 Web API
- ASP.NET Core Minimal API
- Swagger/OpenAPI
- Newtonsoft.Json
- CORS-enabled

## Frontend  
- React 19.2.0
- Vite 7.2.6 (build tool)
- Ant Design 5.12.0 (UI components)
- Tailwind CSS 3.4.0 (styling)
- Axios (HTTP client)

---

# 7. Development Workflow

## Starting Backend
```bash
cd Backend
dotnet run
```
Runs on: `http://localhost:5000`

## Starting Frontend
```bash
cd Frontend
npm install
npm run dev
```
Runs on: `http://localhost:5174`

---

# 8. Data Models

## Request Models
```csharp
public class ValidateRequest
{
    public string FhirJson { get; set; }
    public string LogLevel { get; set; } = "info";
    public bool StrictDisplayMatch { get; set; } = true;
}
```

## Response Models
```csharp
public class ProcessResult
{
    public ValidationResult Validation { get; set; }
    public FlattenResult Flatten { get; set; }
    public List<string> Logs { get; set; }
}
```

---

# 9. Key Features

✅ **Modern UI/UX** - Responsive design with Ant Design components  
✅ **Real-time Processing** - Instant feedback on validation  
✅ **No Database** - All metadata in-memory for portability  
✅ **Hot Module Replacement** - Fast development with Vite  
✅ **API-First** - RESTful architecture for easy integration  
✅ **Component-Based** - Reusable React components  
✅ **Type Safety** - PropTypes validation  

---

# 10. Configuration

## Backend (Program.cs)
- CORS policy for frontend ports
- Swagger UI enabled
- Singleton service registration
- JSON serialization settings

## Frontend (vite.config.js)
- Proxy configuration for API calls
- Build optimization
- Hot reload settings

---

File 08 End.
{
    public string InputJson { get; set; }
    public ValidationResult Validation { get; set; }
    public FlattenResult Flatten { get; set; }
    public List<string> Logs { get; set; }
}
```

---

# 10. No Database

All data is:
- In-memory
- Reset per restart

---

# 11. Summary

The Playground:
- Tests full FHIR bundles
- Displays validation + extraction
- Has seeded test suite
- Uses ASP.NET Framework & Razor

---

# END OF FILE 08
