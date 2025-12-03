# 04 — Data Model Design
PSS FHIR Processor — Models for Validation, Extraction & Flattening  
Version 1.0 — 2025

---

# 1. Purpose
This document defines the C# models used in the PSS FHIR Processor, including:
- FHIR input models  
- Flattened output models  
- Validation models  
- Codes Master models  
- Common helper models  

All models are CRM-agnostic.

---

# 2. Namespaces & Structure

Root namespace:
```
MOH.HealthierSG.Plugins.PSS.FhirProcessor
```

Subfolders:
```
Models/Fhir
Models/Flattened
Models/Validation
Models/Codes
Models/Common
```

---

# 3. FHIR Model Layer (Minimal Input Models)

## Bundle
```csharp
public class Bundle
{
    public string ResourceType { get; set; }
    public string Type { get; set; }
    public List<BundleEntry> Entry { get; set; }
}
```

## BundleEntry
```csharp
public class BundleEntry
{
    public Resource Resource { get; set; }
}
```

## Base Resource
```csharp
public class Resource
{
    public string ResourceType { get; set; }
    public string Id { get; set; }
}
```

---

# 4. FHIR Resources

## Patient
```csharp
public class Patient : Resource
{
    public List<Identifier> Identifier { get; set; }
    public List<HumanName> Name { get; set; }
    public string Gender { get; set; }
    public string BirthDate { get; set; }
    public List<Address> Address { get; set; }
}
```

Supporting types:
```csharp
public class Identifier { public string System { get; set; } public string Value { get; set; } }
public class HumanName { public string Text { get; set; } }
public class Address { public List<string> Line { get; set; } public string PostalCode { get; set; } }
```

---

## Encounter
```csharp
public class Encounter : Resource
{
    public string Status { get; set; }
    public Period ActualPeriod { get; set; }
    public List<EncounterLocation> Location { get; set; }
    public List<ServiceTypeReference> ServiceType { get; set; }
}
```

Supporting:
```csharp
public class Period { public string Start { get; set; } public string End { get; set; } }
public class EncounterLocation { public Reference Location { get; set; } }
public class ServiceTypeReference { public Reference Reference { get; set; } }
```

---

## Location
```csharp
public class Location : Resource
{
    public string Name { get; set; }
    public Address Address { get; set; }
    public List<Extension> Extension { get; set; }
}
```

Supporting:
```csharp
public class Extension { public string Url { get; set; } public string ValueString { get; set; } }
```

---

## HealthcareService
```csharp
public class HealthcareService : Resource
{
    public string Name { get; set; }
    public List<Reference> ProvidedBy { get; set; }
}
```

---

## Organization
```csharp
public class Organization : Resource
{
    public string Name { get; set; }
    public List<OrganizationType> Type { get; set; }
}
```

Supporting:
```csharp
public class OrganizationType { public List<Coding> Coding { get; set; } }
```

---

## Observation
```csharp
public class Observation : Resource
{
    public CodeableConcept Code { get; set; }
    public List<ObservationComponent> Component { get; set; }
}
```

Supporting:
```csharp
public class ObservationComponent
{
    public CodeableConcept Code { get; set; }
    public string ValueString { get; set; }
}
```

---

## CodeableConcept & Coding
```csharp
public class CodeableConcept { public List<Coding> Coding { get; set; } }
public class Coding
{
    public string System { get; set; }
    public string Code { get; set; }
    public string Display { get; set; }
}
```

---

## Reference
```csharp
public class Reference
{
    public string ReferenceValue { get; set; }
}
```

---

# 5. Flattened Output Models

## FlattenResult
```csharp
public class FlattenResult
{
    public EventData Event { get; set; }
    public ParticipantData Participant { get; set; }

    public ScreeningSet HearingRaw { get; set; }
    public ScreeningSet OralRaw { get; set; }
    public ScreeningSet VisionRaw { get; set; }

    public ObservationItem GetHearing(string code) =>
        HearingRaw?.Items?.FirstOrDefault(i => i.Question.Code == code);

    public ObservationItem GetOral(string code) =>
        OralRaw?.Items?.FirstOrDefault(i => i.Question.Code == code);

    public ObservationItem GetVision(string code) =>
        VisionRaw?.Items?.FirstOrDefault(i => i.Question.Code == code);
}
```

---

## EventData
```csharp
public class EventData
{
    public string Start { get; set; }
    public string End { get; set; }
    public string VenueName { get; set; }
    public string PostalCode { get; set; }
    public string Grc { get; set; }
    public string Constituency { get; set; }
    public string ProviderName { get; set; }
    public string ClusterName { get; set; }
}
```

---

## ParticipantData
```csharp
public class ParticipantData
{
    public string Nric { get; set; }
    public string Name { get; set; }
    public string Gender { get; set; }
    public string BirthDate { get; set; }
    public string Address { get; set; }
}
```

---

## ScreeningSet
```csharp
public class ScreeningSet
{
    public string ScreeningType { get; set; } // HS / OS / VS
    public List<ObservationItem> Items { get; set; }
}
```

---

## ObservationItem
```csharp
public class ObservationItem
{
    public CodeDisplayValue Question { get; set; }
    public List<string> Values { get; set; }
}
```

---

# 6. Common Models

## CodeDisplayValue
```csharp
public class CodeDisplayValue
{
    public string Code { get; set; }
    public string Display { get; set; }
}
```

---

# 7. Validation Models

## ValidationRule
```csharp
public class ValidationRule
{
    public string Path { get; set; }
    public string RuleType { get; set; }
    public string FixedValue { get; set; }
    public string FixedSystem { get; set; }
    public string FixedCode { get; set; }
    public List<string> AllowedValues { get; set; }
    public string If { get; set; }
    public string Then { get; set; }
}
```

## RuleSet
```csharp
public class RuleSet
{
    public string Scope { get; set; } // Event, Participant, HS, OS, VS
    public List<ValidationRule> Rules { get; set; }
}
```

## ValidationError
```csharp
public class ValidationError
{
    public string Code { get; set; }
    public string FieldPath { get; set; }
    public string Message { get; set; }
    public string Scope { get; set; }
}
```

## ValidationOptions
```csharp
public class ValidationOptions
{
    public bool StrictDisplayMatch { get; set; } = true;
    public bool NormalizeDisplayMatch { get; set; } = false;
}
```

## LoggingOptions
```csharp
public class LoggingOptions
{
    public string LogLevel { get; set; }
}
```

## ValidationResult
```csharp
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; }
}
```

## ProcessResult
```csharp
public class ProcessResult
{
    public ValidationResult Validation { get; set; }
    public FlattenResult Flatten { get; set; }
    public List<string> Logs { get; set; }
}
```

---

# 8. Codes Master Models

## ClinicalCodeMetadata
```csharp
public class ClinicalCodeMetadata
{
    public string QuestionCode { get; set; }
    public string QuestionDisplay { get; set; }
    public string ScreeningType { get; set; }
    public List<string> AllowedAnswers { get; set; }
}
```

## CodesMasterMetadata
```csharp
public class CodesMasterMetadata
{
    public List<ClinicalCodeMetadata> Questions { get; set; }
}
```

---

# 9. Summary
These models enable:
- FHIR deserialization  
- Metadata-driven validation  
- Extraction into a normalized structure  
- CRM-agnostic downstream mapping  

---

File 04 End.
