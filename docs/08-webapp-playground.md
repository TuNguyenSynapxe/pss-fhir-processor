# 08 — WebApp Playground
PSS FHIR Processor — WebApp Playground Design  
Version 1.0 — 2025

---

# 1. Introduction

This document describes the PSS FHIR Processor Playground WebApp, built using:
- ASP.NET Framework 4.7.2
- Razor Views
- In-memory seeding
- Controllers + Views architecture

The Playground WebApp serves two purposes:

1. Developer Testing Console  
2. Automated Test Showcase

---

# 2. Architecture Overview

## 2.1 Components

- Controllers
  - PlaygroundController
  - TestController
- Service Layer
- Views
- Static Seeds

---

# 3. WebApp Flow

```
Input JSON → DLL Validation → Extraction → Display Result
```

---

# 4. Playground Page

Sections:
- Input Panel
- Result Tabs
- Options Panel

---

# 5. Tests Page

URL: /Tests  
Sections:  
- Test Table  
- Modal Dialog  
- Run/Run All  

---

# 6. TestCase Model

```csharp
public class PlaygroundTestCase
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string InputJson { get; set; }
    public bool ExpectedIsValid { get; set; }
}
```

---

# 7. Pre-Seeding

Includes:
- success cases
- failure cases
- invalid codes, missing HS/OS/VS, PureTone errors

---

# 8. Controllers

PlaygroundController
- GET Index
- POST Process

TestController
- GET Index
- POST Run
- POST RunAll

---

# 9. View Models

```csharp
public class PlaygroundResultModel
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
