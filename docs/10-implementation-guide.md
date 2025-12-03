
# 10 — Implementation Guide
PSS FHIR Processor — Implementation Guide  
Version 1.0 — 2025

---

# 1. Introduction

This guide explains how developers use the PSS FHIR Processor:
- Validation Engine
- Extraction Engine
- Playground WebApp
- Public API

The DLL is standalone:
- No CRM dependency
- No DB
- No file I/O
- Metadata-driven

---

# 2. Folder Structure (Recommended)

```
/src
   /Pss.FhirProcessor
   /Pss.FhirProcessor.Tests
   /Pss.FhirProcessor.WebApp
/docs
   *.md
```

---

# 3. Referencing the DLL

```csharp
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
```

```csharp
var validator = new ValidationEngine();
var extractor = new ExtractionEngine();
```

Load metadata:

```csharp
validator.LoadRuleSets(rules);
validator.LoadCodesMaster(codesMaster);
validator.SetOptions(new ValidationOptions());
```

---

# 4. Processing Flow

```
JSON → Deserialize → Validate → Extract → FlattenResult
```

---

# 5. Validating JSON

```csharp
var bundle = JsonConvert.DeserializeObject<Bundle>(json);
var result = validator.Validate(bundle);

if (!result.IsValid)
    return result.Errors;
```

---

# 6. Extracting FlattenResult

```csharp
var flat = extractor.Extract(bundle);
```

Access fields:

```csharp
flat.Event.Start;
flat.Participant.Nric;
flat.HearingRaw.Items;
```

Lookup:

```csharp
var q = flat.GetHearing("SQ-L2H9-00000001");
```

---

# 7. Log Handling

```csharp
foreach(var log in result.Logs)
    Console.WriteLine(log);
```

---

# 8. Using Options

```csharp
new ValidationOptions {
    StrictDisplayMatch = true,
    NormalizeDisplayMatch = false
};
```

Log Levels: info, debug, verbose.

---

# 9. CRM Plugin Scenario (High Level)

```csharp
var v = validator.Validate(bundle);
if (!v.IsValid) throw new Exception("Invalid");

var flat = extractor.Extract(bundle);
SaveToCrm(flat);
```

---

# 10. Best Practices

### Do NOT bypass validator
Extraction requires valid data.

### Always log validation errors
Useful for vendor troubleshooting.

### Save original payload
Recommended for audit.

---

# 11. Extending Metadata

RuleSets & Codes Master can be extended without DLL changes.

---

# 12. New Screening Types (Future)

Steps:
1. Add RuleSet  
2. Add CodesMaster  
3. Extend ExtractionEngine  
4. Add FlattenResult lookup  

---

# 13. Deployment Checklist

- RuleSets JSON loaded  
- Codes Master loaded  
- Newtonsoft.Json referenced  
- DLL placed in application bin  
- Test with Playground WebApp  

---

# 14. Summary

Developers can:
- Load metadata  
- Validate bundles  
- Extract flattened data  
- Integrate into CRM  
- Extend using metadata  

---

# END OF FILE 10


