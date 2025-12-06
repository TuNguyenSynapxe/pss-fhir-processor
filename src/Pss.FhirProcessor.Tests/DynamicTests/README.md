# Dynamic Mutation-Based E2E Tests

## Overview

This test suite provides **metadata-driven, mutation-based end-to-end validation testing** for the PSS FHIR Processor. Instead of manually writing dozens of negative test cases, this system:

1. Starts with a single **happy-path bundle** (`happy-sample-full.json`)
2. Applies **mutation templates** to systematically break validation rules
3. Automatically generates **N+ test cases** that run through NUnit
4. Validates that expected error codes appear for each mutation

## Architecture

```
DynamicTests/
├── Source/
│   ├── happy-sample-full.json      # Baseline valid bundle
│   └── validation-metadata.json    # Metadata for reference
├── Models/
│   ├── DynamicTestCase.cs          # Test case model
│   └── MutationTemplate.cs         # Mutation definition model
├── Helpers/
│   ├── ErrorCodes.cs               # Central error code registry
│   └── JsonMutationHelpers.cs      # JSON mutation utilities
├── MutationTemplates.cs            # Registry of all mutations
├── DynamicTestSource.cs            # xUnit test case generator
├── MetadataDrivenEndToEndTests.cs  # xUnit test runner
└── README.md                       # This file
```

## How It Works

### 1. Baseline Bundle
The system loads `happy-sample-full.json` from the `Source/` folder as the baseline valid bundle. This bundle should pass **all** validation rules.

### 2. Mutation Templates
Each `MutationTemplate` defines:
- **Name**: Test case name (e.g., "MissingPatient")
- **Apply**: Function that mutates the baseline bundle
- **ExpectedErrorCodes**: Error codes that should appear

Example:
```csharp
new MutationTemplate
{
    Name = "MissingPatient",
    Apply = bundle => JsonMutationHelpers.RemoveEntryByResourceType(bundle, "Patient"),
    ExpectedErrorCodes = new List<string> { ErrorCodes.MISSING_PATIENT }
}
```

### 3. Test Generation
`DynamicTestSource.GetCases()` yields:
- **1 happy case**: Baseline bundle (should pass)
- **N negative cases**: One per mutation template (should fail with expected errors)

### 4. Test Execution
xUnit runs all generated test cases through `ExecuteDynamicCase()`:
- Happy case → Assert `IsValid == true`
- Negative cases → Assert `IsValid == false` AND expected error codes present

## Adding New Test Cases

### Simple Case - Add to MutationTemplates.cs

```csharp
new MutationTemplate
{
    Name = "YourTestName",
    Apply = bundle => JsonMutationHelpers.SomeHelper(bundle, "param"),
    ExpectedErrorCodes = new List<string> { ErrorCodes.YOUR_ERROR_CODE }
}
```

### Complex Case - Create Custom Helper

If the mutation is complex, add a helper method to `JsonMutationHelpers.cs`:

```csharp
public static JObject BreakSomething(JObject bundle, string param)
{
    var clone = (JObject)bundle.DeepClone();
    // ... mutation logic ...
    return clone;
}
```

Then use it in a template:

```csharp
new MutationTemplate
{
    Name = "BrokenSomething",
    Apply = bundle => JsonMutationHelpers.BreakSomething(bundle, "value"),
    ExpectedErrorCodes = new List<string> { ErrorCodes.SOMETHING_INVALID }
}
```

## Available Mutation Helpers

### Resource Removal
- `RemoveEntryByResourceType(bundle, "Patient")` - Remove all Patient resources
- `RemoveEntryByProperty(bundle, "resource.code.coding[0].code", "HS")` - Remove by nested property

### Property Manipulation
- `RemoveProperty(obj, "$.path.to.property")` - Delete a property
- `ReplaceString(obj, "$.path", "newValue")` - Change string value
- `BreakGuid(obj, "$.resource.id")` - Replace with invalid GUID
- `BreakGuidUri(obj, "$.entry[0].fullUrl")` - Replace with invalid URN UUID
- `BreakDateTime(obj, "$.actualPeriod.start")` - Replace with invalid datetime
- `BreakDate(obj, "$.birthDate")` - Replace with invalid date
- `BreakReference(obj, "$.subject.reference")` - Point to non-existent resource

### Observation-Specific
- `FindObservationByScreeningType(bundle, "HS")` - Find HS/OS/VS observation
- `BreakObservationQuestionCode(bundle, "HS", "INVALID")` - Break question code
- `BreakObservationQuestionDisplay(bundle, "OS", "Wrong")` - Break display text
- `BreakObservationAnswerValue(bundle, "VS", "BadAnswer")` - Break answer value

## Current Test Coverage

**Auto-generated tests** (as of implementation):
- ✅ Missing Patient
- ✅ Missing Encounter
- ✅ Missing Location
- ✅ Missing HealthcareService
- ✅ Missing Provider Organization
- ✅ Missing Cluster Organization
- ✅ Missing HS/OS/VS Observations
- ✅ Invalid fullUrl format
- ✅ Invalid GUID format
- ✅ Mismatched fullUrl/ID
- ✅ Invalid datetime format
- ✅ Invalid date format
- ✅ Invalid screening code
- ✅ Invalid question code
- ✅ Invalid question display
- ✅ Invalid answer value
- ✅ Invalid pure tone value
- ✅ Broken subject/encounter/performer references
- ✅ Invalid GRC/Constituency codes

**Total**: 1 happy + 25+ negative = **26+ test cases** from a single baseline bundle!

## Configuration

### Error Codes
Update `Helpers/ErrorCodes.cs` to match your `validation-metadata.json`:

```csharp
public static class ErrorCodes
{
    public const string MISSING_PATIENT = "MISSING_PATIENT";
    public const string TYPE_MISMATCH = "TYPE_MISMATCH";
    // ... add your codes here ...
}
```

### Baseline Bundle Path
If `happy-sample-full.json` is not found, update search paths in `DynamicTestSource.LoadBaseBundle()`:

```csharp
var possiblePaths = new[]
{
    "path/to/your/happy-sample-full.json",
    // ... add paths ...
};
```

## Running Tests

### Visual Studio / Rider
1. Open Test Explorer
2. Run "MetadataDrivenEndToEndTests"
3. See all generated test cases execute

### Command Line
```bash
dotnet test --filter "FullyQualifiedName~MetadataDrivenEndToEndTests"
```

### Run Specific Test
```bash
dotnet test --filter "FullyQualifiedName~MetadataDrivenEndToEndTests.ExecuteDynamicCase"
```

## Test Output

For each test case, you'll see:
```
Executing test case: MissingPatient
Should pass: False
Expected error codes: MISSING_PATIENT
Validation result: IsValid=False, ErrorCount=1
Validation errors:
  [MISSING_PATIENT] Bundle: Patient resource is required
✓ All expected error codes found: MISSING_PATIENT
```

## Benefits

### ✅ Maintainability
- Add 1 line → Get 1 new test case
- Centralized error codes (no magic strings)
- Easy to update when metadata changes

### ✅ Coverage
- Systematic coverage of all validation rules
- Automatically tests combinations
- Catches regressions

### ✅ Clarity
- Self-documenting test names
- Clear mutation intent
- Easy to understand failures

### ✅ Extensibility
- Add new helpers for complex scenarios
- Reuse helpers across mutations
- Easy to add metadata-driven scenarios

## Best Practices

### 1. Keep Mutations Atomic
Each mutation should test **one validation rule**:
```csharp
// ✅ Good - tests one thing
Apply = bundle => RemoveEntryByResourceType(bundle, "Patient")

// ❌ Bad - tests multiple things
Apply = bundle => {
    var b1 = RemoveEntryByResourceType(bundle, "Patient");
    var b2 = BreakGuid(b1, "$.entry[0].resource.id");
    return b2;
}
```

### 2. Use Descriptive Names
```csharp
// ✅ Good
Name = "MissingPatient"
Name = "InvalidHearingScreeningCode"

// ❌ Bad
Name = "Test1"
Name = "PatientProblem"
```

### 3. Always DeepClone
Mutations MUST NOT modify the baseline bundle:
```csharp
// ✅ Good - clones before mutating
public static JObject BreakSomething(JObject bundle, string value)
{
    var clone = (JObject)bundle.DeepClone();
    clone["something"] = value;
    return clone;
}

// ❌ Bad - mutates original
public static JObject BreakSomething(JObject bundle, string value)
{
    bundle["something"] = value;  // ❌ DON'T DO THIS
    return bundle;
}
```

### 4. Update Error Codes Registry
When validation metadata changes, update `ErrorCodes.cs` to keep everything in sync.

## Troubleshooting

### Test Case Not Generated
Check:
1. Is the mutation template in `MutationTemplates.GetAll()`?
2. Does the `Apply` function return a valid JObject?
3. Check test output for "MUTATION_FAILED" errors

### Wrong Error Code
Check:
1. Does the error code in `ExpectedErrorCodes` match `validation-metadata.json`?
2. Is the error code defined in `ErrorCodes.cs`?
3. Run test with verbose output to see actual error codes

### Baseline Bundle Not Found
Check:
1. Is `happy-sample-full.json` in the expected location?
2. Update paths in `DynamicTestSource.LoadBaseBundle()`
3. Check test output for file path attempts

### Mutation Not Working
Check:
1. Use `TestContext.WriteLine()` to debug mutation logic
2. Verify JSON path syntax (use JSONPath tester)
3. Check for null guards in helper methods

## Future Enhancements

Potential improvements:
- [ ] Generate mutation templates from metadata automatically
- [ ] Support for multi-error assertions (must have ALL expected errors)
- [ ] Parameterized mutations (e.g., break each question code)
- [ ] Mutation combinations (multiple simultaneous breaks)
- [ ] Performance: parallel test execution
- [ ] Export test results to report

## Examples

### Example: Add "Missing NRIC" Test

**Step 1: Add helper (if needed)**
```csharp
// In JsonMutationHelpers.cs
public static JObject RemoveNRIC(JObject bundle)
{
    return RemoveProperty(bundle, "entry[?(@.resource.resourceType=='Patient')].resource.identifier[?(@.system=='https://fhir.synapxe.sg/identifier/nric')]");
}
```

**Step 2: Add template**
```csharp
// In MutationTemplates.GetAll()
new MutationTemplate
{
    Name = "MissingPatientNRIC",
    Apply = bundle => JsonMutationHelpers.RemoveNRIC(bundle),
    ExpectedErrorCodes = new List<string> { ErrorCodes.MANDATORY_MISSING }
}
```

**Step 3: Run tests** → Automatically includes new test case!

---

## Summary

This mutation-based testing system provides **comprehensive validation coverage** with **minimal maintenance**. Add a single line to get a new test case. Update metadata → update error codes → tests still pass. Easy to understand, easy to extend, easy to maintain.

**Result**: Robust, maintainable, metadata-driven E2E testing! 🚀
