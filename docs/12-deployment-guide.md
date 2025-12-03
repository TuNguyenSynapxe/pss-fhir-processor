
# 12 — Deployment Guide
PSS FHIR Processor — Deployment Guide  
Version 1.0 — 2025

---

# 1. Introduction

This guide describes how to deploy:
- PSS FHIR Processor DLL  
- WebApp Playground  
- Public API (optional)

Environments:
- Local Development
- QA / SIT
- UAT
- Production (if required)

---

# 2. Deployment Artifacts

Artifacts produced:
- `Pss.FhirProcessor.dll`
- `RuleSets.json`
- `CodesMaster.json`
- WebApp Playground folder
- Public API binaries

No database required.

---

# 3. Prerequisites

## 3.1 Platform
- Windows Server / IIS
- .NET Framework 4.7.2
- ASP.NET MVC (installed via Web Features)
- Newtonsoft.Json (DLL included)

## 3.2 Configuration Files
```
/Metadata/RuleSets/*.json
/Metadata/CodesMaster.json
```

These files are required at startup.

---

# 4. Deploying the DLL (CRM Plugin Scenario)

1. Copy DLL into CRM Plugin project `/bin`  
2. Register plugin assemblies normally  
3. Ensure:
   ```
   Pss.FhirProcessor.dll
   Newtonsoft.Json.dll
   ```
   are uploaded to CRM Plugin store

4. CRM plugin loads metadata from CRM or secure configuration

---

# 5. Deploying WebApp Playground (IIS)

Steps:

1. Build project:
```
Pss.FhirProcessor.WebApp → Release
```

2. Copy output to IIS site folder

3. Configure app pool:
- .NET CLR Version: v4.0
- Pipeline mode: Integrated

4. Configure Web.config:
```xml
<appSettings>
  <add key="LogLevel" value="info"/>
</appSettings>
```

5. Test via browser:
```
http://localhost/Playground
```

---

# 12 — Deployment Guide (Part 2/2)

# 6. Deploying Public API

Same as WebApp Playground.

### URLs:
```
POST /api/fhir/validate
POST /api/fhir/process
GET /api/fhir/codes-master
GET /api/fhir/rules
```

---

# 7. Environment Configuration

Recommended structure:

```
/config
   /dev
      RuleSets.json
      CodesMaster.json
   /qa
   /uat
   /prod
```

Load at application startup:
```csharp
validator.LoadRuleSets(envPath + "/RuleSets.json");
```

---

# 8. Logs

Logs stored in-memory and rendered to UI.  
Optional: write to IIS logs.

---

# 9. Deployment Checklist

## DLL
- [ ] Latest DLL copied  
- [ ] Newtonsoft.Json included  
- [ ] RuleSets JSON available  
- [ ] Codes Master JSON available  

## WebApp
- [ ] Builds successfully  
- [ ] App Pool configured  
- [ ] Metadata loads on startup  
- [ ] Playground validates FHIR samples  

## API
- [ ] Endpoints respond 200  
- [ ] Validation returns errors correctly  
- [ ] Extraction returns FlattenResult  

---

# 10. Summary

Deployment requires:
- DLL + metadata  
- Optional WebApp  
- Optional Public API  
- No database  
- Minimal server configuration  

---

# END OF FILE 12

