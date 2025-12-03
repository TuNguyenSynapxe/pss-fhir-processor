
# 11 — Unit Test Plan
PSS FHIR Processor — Comprehensive Unit & Integration Test Strategy  
Version 2.0 — 2025

---

# 1. Introduction
This Unit Test Plan defines a complete testing strategy for:
- Validation Engine  
- Extraction Engine  
- Metadata  
- PathResolver  
- End-to-end workflow  

Two layers of tests are required:
1. **Function-Level Unit Tests** — isolated, deterministic  
2. **End-to-End Pipeline Tests** — simulate real vendor submissions  

---

# 2. Test Strategy Overview

## 2.1 Unit Tests (Function-Level)
Validate individual engine components:
- Rule evaluators  
- Path resolver  
- Metadata loaders  
- Extractors  
- PureTone processors  
- Screening type validation  

These tests guarantee correct behavior of each logical component.

## 2.2 End-to-End Tests
Validate the full pipeline:

```
JSON Bundle 
→ Deserialize 
→ ValidationEngine 
→ ExtractionEngine 
→ FlattenResult
```

These tests simulate real production submissions.

---

# 3. Test Project Structure (Mandatory)
```
/Pss.FhirProcessor.Tests
    /Validation
    /Extraction
    /EndToEnd
    /TestData
```

All tests must use this folder structure.

### Validation Tests Folder (Expected Files)
- RequiredRuleTests.cs  
- FixedValueRuleTests.cs  
- FixedCodingRuleTests.cs  
- AllowedValuesRuleTests.cs  
- CodesMasterRuleTests.cs  
- ConditionalRuleTests.cs  
- PureToneValidationTests.cs  
- DisplayMatchingTests.cs  
- ScreeningTypeValidationTests.cs  
- PathResolverTests.cs  
- RuleSetsLoaderTests.cs  

### Extraction Tests Folder
- EventExtractionTests.cs  
- ParticipantExtractionTests.cs  
- ScreeningExtractionTests.cs  
- MultiValueSplitTests.cs  
- LookupHelperTests.cs  

### EndToEnd Folder
- ValidBundleTests.cs  
- MissingScreeningTests.cs  
- InvalidDisplayTests.cs  
- InvalidAnswerTests.cs  
- InvalidPureToneTests.cs  
- ConditionalLogicFailureTests.cs  
- CombinedErrorTests.cs  
- ExtractionOutputTests.cs  

---

# 4. Test Data Structure
All JSON bundles stored in:

```
/Pss.FhirProcessor.Tests/TestData
```

Expected:
- **5 valid bundles**
- **15 invalid bundles**  
- Multi-type coverage:
  - Missing HS/OS/VS  
  - Wrong display  
  - Unknown question code  
  - Invalid answer  
  - Invalid PureTone  
  - Conditional rule failures  
  - Multi-error bundles  

---

# 5. Function-Level Validation Tests (Detailed Requirements)

## 5.1 Required Rule Tests
Cases:
- Missing required field  
- Empty string field  
- Valid field present  
Error expected: `MANDATORY_MISSING`

## 5.2 FixedValue Rule Tests
Validate fixed literal values.  
Error: `FIXED_VALUE_MISMATCH`

## 5.3 FixedCoding Rule Tests
Validate:
- coding.system
- coding.code  
Error: `FIXED_CODING_MISMATCH`

## 5.4 AllowedValues Rule Tests
Ensure answer is within allowed values.  
Error: `INVALID_ANSWER_VALUE`

## 5.5 CodesMaster Rule Tests
Validate:
- questionCode exists  
- questionDisplay matches  
- answer matches allowed values  
- multi-value for PureTone  
- screeningType is correct  

Errors:
- `UNKNOWN_QUESTION_CODE`
- `QUESTION_DISPLAY_MISMATCH`
- `INVALID_ANSWER_VALUE`
- `INVALID_MULTI_VALUE`
- `INVALID_SCREENING_TYPE_FOR_QUESTION`

## 5.6 Conditional Rule Tests
Test "if X then Y" scenarios.  
Error: `CONDITIONAL_FAILED`

## 5.7 PureTone Validation Tests
Validate multi-value splitting + allowed values.  
Error: `INVALID_MULTI_VALUE`

## 5.8 Display Matching Tests
Test strict vs normalized matching.  
Error: `QUESTION_DISPLAY_MISMATCH`

## 5.9 Screening Type Validation Tests
Missing HS/OS/VS → `MISSING_SCREENING_TYPE`  
Wrong question type → `INVALID_SCREENING_TYPE_FOR_QUESTION`

## 5.10 Path Resolver Tests
Test:
- Dot path resolution  
- Array index  
- Wildcard  
- Missing path  
- Complex nested extraction  

---

# 6. Extraction Engine Unit Tests

## 6.1 EventExtraction Tests
Test mapping of:
- Start / End  
- Venue  
- Postal code  
- GRC / Constituency  
- Provider / Cluster  

## 6.2 ParticipantExtraction Tests
Test mapping of:
- NRIC  
- Name  
- Gender  
- BirthDate  

## 6.3 ScreeningExtraction Tests
Test:
- Each Observation → ObservationItem  
- Question mapping  
- Answer mapping  

## 6.4 MultiValueSplit Tests
Test splitting `"A|B|C"` into array.

## 6.5 LookupHelper Tests
Test:
- GetHearing()  
- GetOral()  
- GetVision()  

---

# 7. End-to-End Full Pipeline Tests

## 7.1 Valid Bundle Tests
Expect:
- `IsValid = true`
- FlattenResult contains:
  - Event
  - Participant
  - HS/OS/VS screening sets

## 7.2 Missing Screening Tests
Missing HS / OS / VS → return:
- `MISSING_SCREENING_TYPE`
- FlattenResult = null

## 7.3 Invalid Display Tests
Return:
- `QUESTION_DISPLAY_MISMATCH`

## 7.4 Invalid Answer Tests
Return:
- `INVALID_ANSWER_VALUE`

## 7.5 Invalid PureTone Tests
Return:
- `INVALID_MULTI_VALUE`

## 7.6 Conditional Logic Tests
Return:
- `CONDITIONAL_FAILED`

## 7.7 Combined Error Tests
Multiple validation failures must all appear in the same ValidationResult.

## 7.8 Extraction Output Tests
Validate output of:
- EventData  
- ParticipantData  
- ScreeningSet  
- Multi-value formatting  

---

# 8. Coverage Requirements

- ValidationEngine: **≥ 95%**
- ExtractionEngine: **≥ 80%**
- All rule evaluators: **100%**
- PureTone validator: **100%**
- PathResolver: **100%**
- Conditional rules: **100%**

---

# 9. Summary

This test plan guarantees:
- Deterministic rule-based validation  
- Safe extraction  
- Vendor-proof processing  
- High maintainability  
- Full correctness across metadata-driven logic  

---

# END OF FILE 11
