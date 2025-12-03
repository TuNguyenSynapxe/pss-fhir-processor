using System;
using System.Collections.Generic;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;

// Quick script to debug the failures
var processor = new FhirProcessor();

var codesMaster = @"{
    ""Questions"": [
        {
            ""QuestionCode"": ""SQ-L2H9-00000001"",
            ""QuestionDisplay"": ""Currently wearing hearing aid(s)?"",
            ""ScreeningType"": ""HS"",
            ""AllowedAnswers"": [""Yes"", ""No""]
        },
        {
            ""QuestionCode"": ""SQ-L2H9-00000003"",
            ""QuestionDisplay"": ""Type of hearing aid"",
            ""ScreeningType"": ""HS"",
            ""AllowedAnswers"": [""Behind-the-ear"", ""In-the-ear"", ""In-the-canal""]
        }
    ]
}";

processor.LoadCodesMaster(codesMaster);
processor.SetValidationOptions(new ValidationOptions { StrictDisplayMatch = true });

// Test 1: Conditional field missing
Console.WriteLine("=== Test 1: Conditional Field Missing ===");
var json1 = @"{
    ""resourceType"": ""Bundle"",
    ""type"": ""collection"",
    ""entry"": [
        {
            ""resource"": {
                ""resourceType"": ""Encounter"",
                ""status"": ""completed""
            }
        },
        {
            ""resource"": {
                ""resourceType"": ""Patient"",
                ""identifier"": [{ ""system"": ""https://fhir.synapxe.sg/identifier/nric"", ""value"": ""S1234567A"" }]
            }
        },
        {
            ""resource"": {
                ""resourceType"": ""Observation"",
                ""code"": { ""coding"": [{ ""code"": ""HS"" }] },
                ""component"": [
                    {
                        ""code"": {
                            ""coding"": [{
                                ""code"": ""SQ-L2H9-00000001"",
                                ""display"": ""Currently wearing hearing aid(s)?""
                            }]
                        },
                        ""valueString"": ""Yes""
                    }
                ]
            }
        },
        {
            ""resource"": {
                ""resourceType"": ""Observation"",
                ""code"": { ""coding"": [{ ""code"": ""OS"" }] },
                ""component"": []
            }
        },
        {
            ""resource"": {
                ""resourceType"": ""Observation"",
                ""code"": { ""coding"": [{ ""code"": ""VS"" }] },
                ""component"": []
            }
        }
    ]
}";

var result1 = processor.Process(json1);
Console.WriteLine($"Valid: {result1.Validation.IsValid}");
Console.WriteLine($"Error count: {result1.Validation.Errors.Count}");
foreach (var err in result1.Validation.Errors)
{
    Console.WriteLine($"  - {err.Code}: {err.Message}");
}

// Test 2: Missing display
Console.WriteLine("\n=== Test 2: Missing Display ===");
var json2 = @"{
    ""resourceType"": ""Bundle"",
    ""type"": ""collection"",
    ""entry"": [
        {
            ""resource"": {
                ""resourceType"": ""Encounter"",
                ""status"": ""completed""
            }
        },
        {
            ""resource"": {
                ""resourceType"": ""Patient"",
                ""identifier"": [{ ""system"": ""https://fhir.synapxe.sg/identifier/nric"", ""value"": ""S1234567A"" }]
            }
        },
        {
            ""resource"": {
                ""resourceType"": ""Observation"",
                ""code"": { ""coding"": [{ ""code"": ""HS"" }] },
                ""component"": [
                    {
                        ""code"": {
                            ""coding"": [{
                                ""code"": ""SQ-L2H9-00000001""
                            }]
                        },
                        ""valueString"": ""No""
                    }
                ]
            }
        },
        {
            ""resource"": {
                ""resourceType"": ""Observation"",
                ""code"": { ""coding"": [{ ""code"": ""OS"" }] },
                ""component"": []
            }
        },
        {
            ""resource"": {
                ""resourceType"": ""Observation"",
                ""code"": { ""coding"": [{ ""code"": ""VS"" }] },
                ""component"": []
            }
        }
    ]
}";

var result2 = processor.Process(json2);
Console.WriteLine($"Valid: {result2.Validation.IsValid}");
Console.WriteLine($"Error count: {result2.Validation.Errors.Count}");
foreach (var err in result2.Validation.Errors)
{
    Console.WriteLine($"  - {err.Code}: {err.Message}");
}
