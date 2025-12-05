
# Folder Structure — PSS FHIR Processor

```
PSS-FhirProcessor/
│
├── src/
│   ├── Pss.FhirProcessor/
│   │   ├── Models/
│   │   ├── Validation/
│   │   ├── Extraction/
│   │   ├── Metadata/
│   │   ├── Utilities/
│   │   └── Pss.FhirProcessor.csproj
│   │
│   ├── Pss.FhirProcessor.NetCore/
│   │   ├── Backend/
│   │   │   ├── Controllers/
│   │   │   ├── Services/
│   │   │   ├── Seed/
│   │   │   ├── Program.cs
│   │   │   └── Pss.FhirProcessor.Api.csproj
│   │   │
│   │   └── Frontend/
│   │       ├── src/
│   │       │   ├── components/
│   │       │   ├── services/
│   │       │   ├── App.jsx
│   │       │   └── main.jsx
│   │       ├── public/
│   │       ├── index.html
│   │       ├── package.json
│   │       └── vite.config.js
│   │
│   └── Pss.FhirProcessor.Tests/
│       ├── Validation/
│       ├── Extraction/
│       ├── EndToEnd/
│       └── TestData/
│
├── docs/
│   ├── 01-overview.md
│   ├── TODO.md
│   ├── folder-structure.md
│   └── *.md (architecture, specs, etc.)
│
├── README.md
└── PSS-FhirProcessor.sln
```

# END OF FILE
