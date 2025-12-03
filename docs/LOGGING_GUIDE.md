# Comprehensive Logging Guide

## Overview

The PSS FHIR Processor includes a comprehensive step-by-step logging system for troubleshooting and monitoring. The logging provides detailed visibility into every phase of FHIR bundle processing.

## Enabling Logging

Logging is configured via the `LoggingOptions` parameter when creating a `FhirProcessor`:

```csharp
var processor = new FhirProcessor(
    metadataPath: "./Metadata",
    validationOptions: new ValidationOptions { StrictDisplayValidation = false },
    loggingOptions: new LoggingOptions 
    { 
        Enabled = true,      // Turn logging on/off
        Level = "Verbose"    // Options: "Info", "Debug", "Verbose", "Error"
    }
);
```

## Log Levels

- **Info**: High-level phase transitions and summary information
- **Debug**: Detailed operation-by-operation tracking
- **Verbose**: All debug info plus granular rule evaluation details
- **Error**: Only errors and critical failures

## Log Structure

The logging output is organized into 4 main steps:

### STEP 1: Deserializing FHIR Bundle
- Timestamp of processing start
- Input JSON size (bytes)
- Success/failure of JSON deserialization
- Resource count in bundle

Example output:
```
[2024-01-15 10:30:45] INFO: ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
[2024-01-15 10:30:45] INFO: STEP 1: Deserializing FHIR Bundle...
[2024-01-15 10:30:45] INFO: ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
[2024-01-15 10:30:45] INFO: Input size: 24538 bytes
[2024-01-15 10:30:45] INFO: ✓ Bundle deserialized successfully (12 resources)
```

### STEP 2: Validation Phase
- Rule loading summary
- Resource indexing details
- Individual rule evaluations
- Error count and first 10 errors

Example output:
```
[2024-01-15 10:30:46] INFO: ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
[2024-01-15 10:30:46] INFO: STEP 2: Validation Phase
[2024-01-15 10:30:46] INFO: ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
[2024-01-15 10:30:46] INFO: → Loading validation rules from metadata...
[2024-01-15 10:30:46] INFO:   → Indexing resources by type...
[2024-01-15 10:30:46] DEBUG:     Found Patient: 1 resource(s)
[2024-01-15 10:30:46] DEBUG:     Found Encounter: 1 resource(s)
[2024-01-15 10:30:46] DEBUG:     Found Observation: 8 resource(s)
[2024-01-15 10:30:46] INFO:   → Validating required resources...
[2024-01-15 10:30:46] INFO:   → Validating screening types (HS, OS, VS)...
[2024-01-15 10:30:46] VERBOSE:     Screening type 'HS': 3 observations
[2024-01-15 10:30:46] VERBOSE:     Screening type 'OS': 2 observations
[2024-01-15 10:30:46] VERBOSE:     Screening type 'VS': 3 observations
[2024-01-15 10:30:46] INFO: ✓ Validation completed with 0 errors
```

When errors occur:
```
[2024-01-15 10:30:46] INFO: ✗ Validation completed with 3 errors:
[2024-01-15 10:30:46] ERROR:   1. [FIXED_VALUE_MISMATCH] Bundle.identifier.value - Expected 'PSS-HS-2024-001'...
[2024-01-15 10:30:46] ERROR:   2. [INVALID_CODE] Observation[HS-Weight].code.coding[0].code - Invalid code 'xyz123'
[2024-01-15 10:30:46] ERROR:   3. [REQUIRED_FIELD_MISSING] Observation[HS-Height].valueQuantity.value - Required field is missing
```

### STEP 3: Extraction Phase
- Resource extraction progress
- Item counts for each screening type
- Success/failure indicators for each data type

Example output:
```
[2024-01-15 10:30:47] INFO: ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
[2024-01-15 10:30:47] INFO: STEP 3: Extraction Phase
[2024-01-15 10:30:47] INFO: ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
[2024-01-15 10:30:47] DEBUG: → Extracting EventData...
[2024-01-15 10:30:47] DEBUG: ✓ EventData extracted
[2024-01-15 10:30:47] DEBUG: → Extracting Participants (3 resources)...
[2024-01-15 10:30:47] DEBUG: ✓ 3 ParticipantData extracted
[2024-01-15 10:30:47] DEBUG: → Extracting screening observations...
[2024-01-15 10:30:47] VERBOSE:   → HS screening: 3 observations
[2024-01-15 10:30:47] VERBOSE:   → OS screening: 2 observations
[2024-01-15 10:30:47] VERBOSE:   → VS screening: 3 observations
[2024-01-15 10:30:47] DEBUG: ✓ 8 total screening observations extracted
```

### STEP 4: Processing Summary
- Overall success/failure status
- Validation error count
- Extraction status
- Total processing time

Example output:
```
[2024-01-15 10:30:47] INFO: ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
[2024-01-15 10:30:47] INFO: STEP 4: Processing Summary
[2024-01-15 10:30:47] INFO: ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
[2024-01-15 10:30:47] INFO: ✓ Processing completed successfully
[2024-01-15 10:30:47] INFO: → Validation: 0 errors
[2024-01-15 10:30:47] INFO: → Extraction: Success
[2024-01-15 10:30:47] INFO: → Total time: 142ms
```

## Accessing Logs

All log messages are collected in the `ProcessResult.Logs` property:

```csharp
var result = processor.Process(fhirJsonBundle);

// Access all log messages
foreach (var logMessage in result.Logs)
{
    Console.WriteLine(logMessage);
}

// Or write to file
File.WriteAllLines("processing.log", result.Logs);
```

## Visual Symbols

The logging uses Unicode symbols for visual clarity:

- ✓ (`\u2713`) - Success/completion
- ✗ (`\u2717`) - Failure/error
- → (`\u2192`) - Action/step indicator
- ⚠ (`\u26A0`) - Warning (future use)

These symbols help quickly identify the status of operations when scanning logs.

## Troubleshooting Common Issues

### Issue: No logs appearing
**Solution**: Check that `LoggingOptions.Enabled = true` and `Level` is set appropriately.

### Issue: Too much log output
**Solution**: Use `Level = "Info"` instead of `"Verbose"` to reduce granularity.

### Issue: Missing error details
**Solution**: Switch to `Level = "Verbose"` to see complete rule evaluation traces.

### Issue: Performance degradation
**Solution**: Disable logging in production by setting `Enabled = false`.

## Best Practices

1. **Development**: Use `Level = "Verbose"` for maximum visibility
2. **Testing**: Use `Level = "Debug"` for detailed operation tracking
3. **Production**: Use `Level = "Info"` or disable logging entirely
4. **Troubleshooting**: Enable `Verbose` logging when debugging specific issues

## Example: Complete Logging Test

```csharp
using System;
using System.IO;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;

class Program
{
    static void Main()
    {
        var bundle = File.ReadAllText("sample_bundle.json");
        
        var processor = new FhirProcessor(
            "./Metadata",
            new ValidationOptions { StrictDisplayValidation = false },
            new LoggingOptions { Enabled = true, Level = "Verbose" }
        );

        var result = processor.Process(bundle);

        Console.WriteLine("=== PROCESSING LOGS ===");
        foreach (var log in result.Logs)
        {
            Console.WriteLine(log);
        }

        Console.WriteLine($"\n=== RESULT ===");
        Console.WriteLine($"Success: {result.Success}");
        Console.WriteLine($"Errors: {result.ValidationResult?.Errors.Count ?? 0}");
        Console.WriteLine($"Extracted: {result.FlattenResult != null}");
    }
}
```

## Log File Format

When writing logs to disk, consider including metadata:

```csharp
var logLines = new List<string>
{
    $"=== PSS FHIR Processor Log ===",
    $"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}",
    $"Metadata Path: {metadataPath}",
    $"Strict Display: {validationOptions.StrictDisplayValidation}",
    $"Log Level: {loggingOptions.Level}",
    $"",
    "=== PROCESSING START ==="
};

logLines.AddRange(result.Logs);

File.WriteAllLines("processor_log.txt", logLines);
```
