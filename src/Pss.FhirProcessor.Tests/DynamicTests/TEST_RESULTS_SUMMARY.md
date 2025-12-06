# Dynamic End-to-End Test Results Summary

**Date**: Error code alignment completed
**Total Tests**: 26 (1 happy + 25 mutations)
**Passed**: 12
**Failed**: 14

## ✅ Passing Tests (12)

### Happy Case
1. **HappyCase_AllValid** - Baseline valid bundle passes validation

### Type Validation (4 tests)
2. **InvalidPatientIdGuid** - Detects invalid GUID format with TYPE_MISMATCH
3. **MismatchedFullUrlAndId** - Detects fullUrl/id mismatch with ID_FULLURL_MISMATCH
4. **InvalidEncounterStartDateTime** - Detects invalid datetime with TYPE_MISMATCH
5. **InvalidPatientBirthDate** - Detects invalid date with TYPE_MISMATCH

### CodesMaster Validation (4 tests)
6. **InvalidQuestionCode_HS** - Detects invalid question code with INVALID_ANSWER_VALUE
7. **InvalidQuestionDisplay_OS** - Detects invalid question display with INVALID_ANSWER_VALUE
8. **InvalidAnswerValue_VS** - Detects invalid answer value with INVALID_ANSWER_VALUE
9. **InvalidPureToneValue_HS** - Detects invalid pure tone value with INVALID_ANSWER_VALUE

### Reference Validation (2 tests)
10. **BrokenSubjectReference_HS** - Detects broken subject reference with REF_OBS_SUBJECT_INVALID
11. **BrokenPerformerReference_VS** - Detects broken performer reference with REF_OBS_PERFORMER_INVALID

### Test Generation Validation
12. **VerifyTestCaseGeneration** - Confirms at least 20 test cases are generated

---

## ❌ Failing Tests (14)

### Missing Resource Validations (8 tests) - **METADATA GAP**
These tests expect `MISSING_*` error codes, but the validation engine's Bundle scope `Required` rules don't actually validate resource existence. When resources are removed, the actual error is broken references (REF_OBS_*) or validation just passes.

1. **MissingPatient** 
   - Expected: `MISSING_PATIENT`
   - Actual: `REF_OBS_SUBJECT_INVALID` (broken reference from Observation to Patient)
   - Root cause: Bundle Required rules don't check if Patient exists

2. **MissingEncounter**
   - Expected: `MISSING_ENCOUNTER`
   - Actual: `REF_OBS_ENCOUNTER_INVALID` (broken reference from Observation to Encounter)
   - Root cause: Bundle Required rules don't check if Encounter exists

3. **MissingLocation**
   - Expected: `MISSING_LOCATION`
   - Actual: Validation passes (no errors)
   - Root cause: No validation checks if Location resource exists

4. **MissingHealthcareService**
   - Expected: `MISSING_HEALTHCARESERVICE`
   - Actual: Validation passes (no errors)
   - Root cause: No validation checks if HealthcareService resource exists

5. **MissingProviderOrganization**
   - Expected: `MISSING_PROVIDER_ORG`
   - Actual: `REF_OBS_PERFORMER_INVALID` (3 times - one per screening Observation)
   - Root cause: Bundle Required rules don't check if Organization.Provider exists

6. **MissingClusterOrganization**
   - Expected: `MISSING_CLUSTER_ORG`
   - Actual: Validation passes (no errors)
   - Root cause: No validation checks if Organization.Cluster exists

7. **MissingHearingScreening**
   - Expected: `MISSING_SCREENING_TYPE`
   - Actual: Validation passes (no errors)
   - Root cause: No validation checks if HS Observation exists

8. **MissingOralScreening**
   - Expected: `MISSING_SCREENING_TYPE`
   - Actual: Validation passes (no errors)
   - Root cause: No validation checks if OS Observation exists

9. **MissingVisionScreening**
   - Expected: `MISSING_SCREENING_TYPE`
   - Actual: Validation passes (no errors)
   - Root cause: No validation checks if VS Observation exists

### FullUrl Format Validation (1 test) - **METADATA GAP**
10. **InvalidFullUrlFormat**
    - Expected: `TYPE_INVALID_FULLURL`
    - Actual: Validation passes (no errors)
    - Root cause: Bundle scope Type rule for `Bundle.entry[*].fullUrl` with type `guid-uri` doesn't actually validate format
    - Note: The metadata has this rule at line 74, but validation engine may not enforce it

### Screening Code Validation (1 test) - **METADATA GAP**
11. **InvalidHearingScreeningCode**
    - Expected: `FIXED_VALUE_MISMATCH`
    - Actual: Validation passes (no errors)
    - Root cause: No FixedValue or FixedCoding rule in metadata to enforce screening code = "HS"
    - Note: Metadata only validates question codes within components, not the root observation code

### Reference Validation (1 test) - **METADATA GAP**
12. **BrokenEncounterReference_OS**
    - Expected: `REF_OBS_ENCOUNTER_INVALID`
    - Actual: Validation passes (no errors)
    - Root cause: Encounter reference validation may only apply to specific scopes (HS, VS) but not OS
    - Note: Lines 856 (HS) and 1086 (VS) have encounter reference rules, but line 961 (OS scope) may not

### Code System Validations (2 tests) - **METADATA GAP**
13. **InvalidGRCCode**
    - Expected: `FIXED_VALUE_MISMATCH`
    - Actual: Validation passes (no errors)
    - Root cause: No AllowedValues or CodeSystem validation for GRC extension in metadata

14. **InvalidConstituencyCode**
    - Expected: `FIXED_VALUE_MISMATCH`
    - Actual: Validation passes (no errors)
    - Root cause: No AllowedValues or CodeSystem validation for Constituency extension in metadata

---

## Recommendations

### 1. Keep Passing Tests (12 tests)
These tests are working correctly and validating real rules in your metadata. Keep them as regression tests.

### 2. Remove or Adjust Failing Tests

#### Option A: Remove Tests for Unconfigured Validations (Recommended)
Remove the 14 failing mutation templates from `MutationTemplates.cs`:
- All 8 "Missing*" mutations (lines ~40-97)
- InvalidFullUrlFormat
- InvalidHearingScreeningCode
- BrokenEncounterReference_OS
- InvalidGRCCode
- InvalidConstituencyCode

**Rationale**: Test framework should only test validations that actually exist in metadata. These tests would require significant metadata changes to work.

#### Option B: Update Expected Error Codes for Indirect Detection
For the "Missing*" tests that trigger reference errors, update expected codes:
```csharp
// MissingPatient - now expects broken reference instead
ExpectedErrorCodes = new List<string> { ErrorCodes.REF_OBS_SUBJECT_INVALID }

// MissingEncounter - now expects broken reference instead
ExpectedErrorCodes = new List<string> { ErrorCodes.REF_OBS_ENCOUNTER_INVALID }

// MissingProviderOrganization - now expects broken reference instead
ExpectedErrorCodes = new List<string> { ErrorCodes.REF_OBS_PERFORMER_INVALID }
```

Still remove: MissingLocation, MissingHealthcareService, MissingClusterOrganization, Missing*Screening (these don't trigger any errors).

#### Option C: Extend Metadata (Future Work)
To make all 14 tests pass, you would need to add to validation-metadata.json:
1. Bundle-level custom validation logic for required resources (complex)
2. FixedValue rules for screening codes (Observation.code.coding[0].code = "HS"/"OS"/"VS")
3. AllowedValues or CodeSystem rules for GRC and Constituency extensions
4. Encounter reference validation for OS scope (like HS and VS have)
5. Enhanced fullUrl format validation in validation engine

---

## Summary Statistics

**Error Code Alignment**: ✅ **100% Complete**
- All CodesMaster validations use `INVALID_ANSWER_VALUE` ✅
- All type validations use `TYPE_MISMATCH` ✅
- All reference validations use specific `REF_OBS_*` codes ✅
- No obsolete error codes remain in codebase ✅

**Test Framework Status**: ✅ **Fully Operational**
- Test generation working correctly (26 cases from 25 templates + 1 happy)
- Test execution working correctly (runs validation, checks results)
- Test assertion logic working correctly (validates error codes)

**Validation Coverage**: ⚠️ **Metadata Gaps Identified**
- 12 validations fully configured and tested ✅
- 14 validations not configured in metadata ⚠️
- Framework correctly identified all metadata gaps ✅

**Next Step**: Review the 14 failing tests and decide whether to:
1. Remove them (fastest - aligns tests with current metadata)
2. Update expected error codes for indirect detection (partial solution)
3. Extend metadata to support them (future enhancement)
