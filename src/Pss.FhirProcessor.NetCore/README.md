# PSS FHIR Processor - .NET 8.0 + React

Modern web application for PSS FHIR Bundle validation and extraction with .NET 8.0 backend and React frontend.

## Architecture

- **Backend**: .NET 8.0 Web API with Swagger
- **Frontend**: React 19 + Vite + Tailwind CSS + Ant Design
- **Core Library**: Pss.FhirProcessor (multi-targets net472 + net6.0)

## Project Structure

```
Pss.FhirProcessor.NetCore/
├── Backend/                 # .NET 8.0 Web API
│   ├── Controllers/         # API endpoints
│   ├── Services/            # Business logic
│   ├── Models/              # Request/Response models
│   └── Seed/                # Sample data
└── Frontend/                # React application
    ├── src/
    │   ├── components/      # React components
    │   ├── services/        # API client
    │   └── App.jsx          # Main app
    └── package.json
```

## Features

### Playground
- Interactive FHIR JSON processor
- Configurable log levels (debug, info, warn, error)
- Strict/non-strict display validation
- Real-time validation results
- Data extraction preview
- Detailed processing logs

### Test Runner
- Pre-configured test cases
- Run individual or all tests
- Real-time test results
- Pass/fail statistics

### API Documentation
- Complete endpoint reference
- Request/response schemas
- Configuration options

## Setup & Run

### Backend

```bash
cd Backend
dotnet restore
dotnet run
```

Backend runs on: **http://localhost:5000**

Swagger UI: **http://localhost:5000/swagger**

### Frontend

```bash
cd Frontend
npm install
npm run dev
```

Frontend runs on: **http://localhost:5173**

## API Endpoints

### POST /api/fhir/validate
Validate FHIR Bundle without extraction

### POST /api/fhir/extract
Validate and extract data from FHIR Bundle

### POST /api/fhir/process
Complete processing (validate + extract)

### GET /api/fhir/codes-master
Get Codes Master metadata

### GET /api/fhir/rules
Get validation rules

### GET /api/fhir/test-cases
Get all test cases

### GET /api/fhir/test-cases/{name}
Get specific test case

## Request Format

```json
{
  "fhirJson": "{ ... FHIR Bundle JSON ... }",
  "logLevel": "info",
  "strictDisplayMatch": true
}
```

## Response Format

```json
{
  "success": true,
  "validation": {
    "isValid": true,
    "errors": []
  },
  "flatten": { ... extracted data ... },
  "logs": [ ... processing logs ... ]
}
```

## Technologies

### Backend
- .NET 8.0
- ASP.NET Core Web API
- Swashbuckle (Swagger/OpenAPI)
- Newtonsoft.Json

### Frontend
- React 19
- Vite
- Ant Design 5
- Tailwind CSS 3
- Axios

## Development

### Backend Development
```bash
dotnet watch run
```

### Frontend Development
```bash
npm run dev
```

### Build for Production

Backend:
```bash
dotnet publish -c Release
```

Frontend:
```bash
npm run build
```

## CORS Configuration

Backend is configured to accept requests from:
- http://localhost:5173 (Vite dev server)
- http://localhost:3000 (Alternative React port)

## License

© 2025 MOH HealthierSG - Synapxe
