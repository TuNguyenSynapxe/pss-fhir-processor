# V11 Validation Engine Enhancements - Implementation Complete

**Date**: December 6, 2025  
**Status**: ✅ **FULLY IMPLEMENTED AND COMPILED**

---

## 📋 Overview

The FHIR Validation Engine has been updated to support v11 rule logic with comprehensive enhancements for:
- Bundle-level resource existence validation
- Missing reference detection
- CodeSystem validation
- Enhanced CPS1 path resolution
- Type validation for `guid-uri` format

All changes have been implemented and successfully compiled.

---

## 🎯 Implemented Features

### 1. ✅ Required Rule: Bundle.entry[ResourceType] Syntax

**Purpose**: Validate that specific resource types exist in a Bundle

**Implementation**:
- **File**: `CpsPathResolver.cs`
- **Method**: `ExistsResourceByType(JToken bundleRoot, string cps1Path)`
- **Supports**:
  - `Bundle.entry[Patient]` - Check if Patient resource exists
  - `Bundle.entry[Encounter]` - Check if Encounter resource exists
  - `Bundle.entry[Observation:HS]` - Check if HS screening Observation exists
  - `Bundle.entry[Organization:Provider]` - Check if specific organization type exists

**Usage in Metadata**:
```json
{
  "RuleType": "Required",
  "PathType": "CPS1",
  "Path": "Bundle.entry[Patient]",
  "ErrorCode": "MISSING_PATIENT",
  "Message": "Patient resource is required in bundle"
}
```

**Implementation Details**:
- Parses `Bundle.entry[ResourceType]` or `Bundle.entry[Observation:HS]` format
- Searches bundle entries for matching resource type
- For Observation selectors with screening type, validates `code.coding[0].code`
- Returns true if at least one matching resource found

---

### 2. ✅ Reference Rule: Missing Reference Handling

**Purpose**: Distinguish between missing reference properties and invalid reference targets

**Implementation**:
- **File**: `RuleEvaluator.cs`
- **Method**: `EvaluateReference()` - Enhanced

**Behavior**:

| Condition | Error Code | Message |
|-----------|-----------|---------|
| Reference property missing | `MANDATORY_MISSING` | Required reference field is missing |
| Reference property empty | `MANDATORY_MISSING` | Required reference field is empty |
| Reference exists but target not found | `REF_OBS_*_INVALID` or custom | Referenced resource not found |
| Reference exists but wrong type | `REFERENCE_TYPE_MISMATCH` | Referenced resource has wrong type |

**Before v11**:
```csharp
// Skipped validation if reference was missing
if (refValue == null) return; // No error reported
```

**After v11**:
```csharp
// Now checks if property exists first
if (refValues.Count == 0) {
    result.AddError("MANDATORY_MISSING", ...);
    return;
}
```

---

### 3. ✅ Type Validation: guid-uri Format

**Purpose**: Validate URN UUID format for FHIR fullUrl fields

**Implementation**:
- **File**: `TypeChecker.cs`
- **Type**: `guid-uri`
- **Already Implemented**: ✅ (Confirmed in existing code)

**Format**: `urn:uuid:12345678-1234-1234-1234-123456789abc`

**Regex**:
```csharp
^urn:uuid:[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$
```

**Usage in Metadata**:
```json
{
  "RuleType": "Type",
  "PathType": "CPS1",
  "Path": "Bundle.entry[*].fullUrl",
  "ExpectedType": "guid-uri",
  "ErrorCode": "TYPE_INVALID_FULLURL",
  "Message": "fullUrl must be urn:uuid:GUID format"
}
```

---

### 4. ✅ CodeSystem Rule (NEW)

**Purpose**: Validate that coding codes exist in specified CodeSystem from CodesMaster

**Implementation**:
- **File**: `RuleEvaluator.cs`
- **Method**: `EvaluateCodeSystem()` - **NEW**
- **File**: `RuleDefinition.cs`
- **Property**: `System` - Added

**Usage in Metadata**:
```json
{
  "RuleType": "CodeSystem",
  "PathType": "CPS1",
  "Path": "Location.extension[url:https://fhir.synapxe.sg/StructureDefinition/ext-grc].valueCodeableConcept.coding[0]",
  "System": "https://fhir.synapxe.sg/CodeSystem/grc",
  "ErrorCode": "INVALID_CODE",
  "Message": "Invalid GRC code"
}
```

**Process**:
1. Resolve path to get Coding object
2. Extract `system` and `code` from Coding
3. Find CodeSystem in metadata where `System == rule.System`
4. Check if `code` exists in `CodeSystem.Concepts`
5. Return error if code not found

**CodesMaster Structure**:
```json
{
  "CodeSystems": [
    {
      "Id": "grc",
      "System": "https://fhir.synapxe.sg/CodeSystem/grc",
      "Description": "GRC Codes",
      "Concepts": [
        { "Code": "A", "Display": "GRC A" },
        { "Code": "B", "Display": "GRC B" }
      ]
    }
  ]
}
```

---

### 5. ✅ Enhanced CPS1 Path Resolver

**Purpose**: Support complex path selectors for FHIR resource navigation

**Implementation**:
- **File**: `CpsPathResolver.cs`
- **Enhancements**:
  1. `ExistsResourceByType()` - Bundle resource existence checks
  2. `ParseFilter()` - QuestionCode selector support
  3. `MatchesNestedPath()` - Nested path matching in filters
  4. `ApplyFilter()` - Complex filter key support

**New Selectors**:

#### QuestionCode Selector
```
component[QuestionCode:SQ-F7B7-00000007]
```
Maps to:
```
component where code.coding[0].code == "SQ-F7B7-00000007"
```

**Example**:
```json
{
  "RuleType": "CodesMaster",
  "Path": "Observation.component[QuestionCode:SQ-F7B7-00000007].valueString",
  "ExpectedValue": "SQ-F7B7-00000007"
}
```

#### Nested Path Filters
```
component[code.coding[0].code:SQ-xxx]
```
Previously only supported simple property filters like `component[code:SQ-xxx]`

---

### 6. ✅ FullUrlIdMatch Enhancement

**Purpose**: Only validate when BOTH id and fullUrl exist (don't report errors for missing values)

**Implementation**:
- **File**: `RuleEvaluator.cs`
- **Method**: `EvaluateFullUrlIdMatch()` - Already correct

**Behavior**:
```csharp
// Skip if either value is missing
if (string.IsNullOrWhiteSpace(resourceId) || string.IsNullOrWhiteSpace(entryFullUrl)) {
    return; // Let Required/Type rules handle missing values
}

// Skip if fullUrl is not urn:uuid: format
if (!entryFullUrl.StartsWith("urn:uuid:")) {
    return; // Let Type rule handle invalid format
}

// Only validate the match when both values are present and valid format
```

---

### 7. ✅ Bundle Scope Support

**Purpose**: Apply rules at Bundle level (not just resource level)

**Implementation**:
- **File**: `ValidationEngine.cs`
- **Method**: Modified validation loop

**Changes**:
```csharp
// Special handling for Bundle scope
if (ruleSet.Scope == "Bundle") {
    ValidateResource(bundle, null, ruleSet, result);
    continue;
}
```

**NormalizeRulePath Enhancement**:
```csharp
// Don't strip Bundle prefix for Bundle scope rules
if (resourceType == "Bundle") {
    return normalizedRule; // Keep full path like "Bundle.entry[Patient]"
}
```

---

## 📁 Modified Files

### Core Engine Files

1. **`RuleDefinition.cs`**
   - Added `System` property for CodeSystem validation
   - Updated comment to include `CodeSystem` rule type

2. **`CpsPathResolver.cs`**
   - Added `ExistsResourceByType()` method
   - Enhanced `ParseFilter()` for QuestionCode selectors
   - Added `MatchesNestedPath()` method
   - Enhanced `ApplyFilter()` for nested path keys

3. **`RuleEvaluator.cs`**
   - Added `EvaluateCodeSystem()` method (NEW)
   - Enhanced `EvaluateReference()` to detect missing references
   - Enhanced `EvaluateRequired()` to support Bundle.entry[Type] syntax
   - Added CodeSystem case to rule type switch

4. **`ValidationEngine.cs`**
   - Added Bundle scope handling in validation loop
   - Enhanced `NormalizeRulePath()` to preserve Bundle paths
   - Updated rule copy to include `System` property

5. **`TypeChecker.cs`**
   - Already supports `guid-uri` type ✅ (No changes needed)

### Model Files

6. **`CodesMaster.cs`**
   - Already has CodeSystem support ✅ (No changes needed)
   - Structure: `CodesMaster → CodeSystems → Concepts`

---

## 🧪 Testing Recommendations

### Test Case 1: Bundle Resource Existence
```json
{
  "RuleType": "Required",
  "Path": "Bundle.entry[Patient]",
  "ErrorCode": "MISSING_PATIENT"
}
```
**Test**: Remove Patient from bundle → Should return `MISSING_PATIENT`

### Test Case 2: Missing Reference
```json
{
  "RuleType": "Reference",
  "Path": "Observation.subject.reference",
  "TargetTypes": ["Patient"],
  "ErrorCode": "REF_OBS_SUBJECT_INVALID"
}
```
**Test**: Remove `subject` property → Should return `MANDATORY_MISSING`  
**Test**: Invalid reference → Should return `REF_OBS_SUBJECT_INVALID`

### Test Case 3: CodeSystem Validation
```json
{
  "RuleType": "CodeSystem",
  "Path": "Location.extension[url:ext-grc].valueCodeableConcept.coding[0]",
  "System": "https://fhir.synapxe.sg/CodeSystem/grc",
  "ErrorCode": "INVALID_CODE"
}
```
**Test**: Use code "ZZ" not in GRC CodeSystem → Should return `INVALID_CODE`

### Test Case 4: guid-uri Type
```json
{
  "RuleType": "Type",
  "Path": "Bundle.entry[0].fullUrl",
  "ExpectedType": "guid-uri",
  "ErrorCode": "TYPE_INVALID_FULLURL"
}
```
**Test**: Set fullUrl to "not-a-urn" → Should return `TYPE_INVALID_FULLURL`

### Test Case 5: QuestionCode Selector
```json
{
  "RuleType": "CodesMaster",
  "Path": "Observation.component[QuestionCode:SQ-F7B7-00000007]"
}
```
**Test**: Should find component where code.coding[0].code == "SQ-F7B7-00000007"

---

## 📊 Error Code Mapping

| Validation Type | Missing Property | Invalid Value |
|----------------|------------------|---------------|
| Required | MANDATORY_MISSING | MANDATORY_MISSING |
| Reference | MANDATORY_MISSING | REF_OBS_*_INVALID |
| Type | (Skip) | TYPE_MISMATCH |
| CodeSystem | (Skip) | INVALID_CODE |
| FullUrlIdMatch | (Skip) | ID_FULLURL_MISMATCH |

**Key Principle**: 
- Use `Required` rule to catch missing/empty values
- Use specific rule types (Reference, Type, etc.) to validate existing values
- Reference rule now returns MANDATORY_MISSING if property doesn't exist

---

## 🔄 Migration Guide

### For Metadata Authors

#### Before v11
```json
// Could not validate resource existence at Bundle level
// Had to rely on downstream reference errors
```

#### After v11
```json
{
  "Scope": "Bundle",
  "Rules": [
    {
      "RuleType": "Required",
      "Path": "Bundle.entry[Patient]",
      "ErrorCode": "MISSING_PATIENT",
      "Message": "Patient resource is required"
    }
  ]
}
```

#### Before v11
```json
// Reference validation skipped missing properties silently
{
  "RuleType": "Reference",
  "Path": "Observation.subject.reference",
  "TargetTypes": ["Patient"]
}
```

#### After v11
```json
// Now returns MANDATORY_MISSING if reference property is missing
// Add Required rule if you want to enforce reference presence
{
  "RuleType": "Required",
  "Path": "Observation.subject.reference",
  "ErrorCode": "MANDATORY_MISSING"
},
{
  "RuleType": "Reference",
  "Path": "Observation.subject.reference",
  "TargetTypes": ["Patient"],
  "ErrorCode": "REF_OBS_SUBJECT_INVALID"
}
```

#### New in v11
```json
// CodeSystem validation - validate codes against master data
{
  "RuleType": "CodeSystem",
  "Path": "Location.extension[url:ext-grc].valueCodeableConcept.coding[0]",
  "System": "https://fhir.synapxe.sg/CodeSystem/grc",
  "ErrorCode": "INVALID_CODE",
  "Message": "Invalid GRC code"
}
```

---

## ✅ Verification Checklist

- [x] **Build Success**: All files compile without errors
- [x] **Required Rule**: Bundle.entry[ResourceType] syntax implemented
- [x] **Reference Rule**: Missing reference detection added
- [x] **Type Validation**: guid-uri support confirmed (existing)
- [x] **CodeSystem Rule**: New rule type fully implemented
- [x] **CPS1 Resolver**: Enhanced with QuestionCode and nested paths
- [x] **FullUrlIdMatch**: Correct behavior confirmed (existing)
- [x] **Bundle Scope**: Validation engine supports Bundle-level rules
- [x] **Error Codes**: MANDATORY_MISSING used for missing references
- [x] **Documentation**: Complete implementation guide created

---

## 🚀 Next Steps

### 1. Update Metadata
Update `validation-metadata.json` to use new v11 features:
- Add Bundle scope with Required rules for resource existence
- Add CodeSystem rules for GRC, Constituency, etc.
- Add Type rules with guid-uri for fullUrl validation
- Ensure Reference rules have corresponding Required rules

### 2. Update Tests
- Add unit tests for `ExistsResourceByType()`
- Add unit tests for `EvaluateCodeSystem()`
- Add integration tests for Bundle scope validation
- Update existing reference tests to verify MANDATORY_MISSING

### 3. Deployment
- Deploy updated engine to test environment
- Run full test suite with updated metadata
- Monitor for MANDATORY_MISSING errors in reference validations
- Verify CodeSystem validation working correctly

---

## 📝 Summary

All v11 validation engine enhancements have been successfully implemented:

✅ **1. Required (CPS1) for resource existence** - `ExistsResourceByType()` method  
✅ **2. Missing reference handling** - Enhanced `EvaluateReference()`  
✅ **3. Type = "guid-uri"** - Already supported in `TypeChecker`  
✅ **4. CodeSystem rule** - New `EvaluateCodeSystem()` method  
✅ **5. Enhanced CPS1 path resolver** - QuestionCode selectors, nested paths  
✅ **6. FullUrlIdMatch only when both exist** - Already correct  
✅ **7. Bundle scope support** - ValidationEngine handles Bundle rules  

**Build Status**: ✅ Successful  
**Compilation**: ✅ No errors or warnings  
**Ready for**: Testing and metadata updates

---

**Implementation Complete**: December 6, 2025  
**Next Milestone**: Update metadata and run comprehensive tests
