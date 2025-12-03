using System;
using System.Collections.Generic;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;

class DebugTest
{
    static void Main()
    {
        var processor = new FhirProcessor();
        processor.SetLoggingOptions(new LoggingOptions { LogLevel = "info" });

        var rules = new Dictionary<string, string>
        {
            { "Test", @"{
                ""Scope"": ""Test"",
                ""Rules"": [{
                    ""RuleType"": ""Required"",
                    ""Path"": ""Entry[0].Resource.Status""
                }]
            }" }
        };

        processor.LoadRuleSets(rules);

        var json = @"{
            ""resourceType"": ""Bundle"",
            ""entry"": [{
                ""resource"": {
                    ""resourceType"": ""Encounter"",
                    ""status"": ""completed""
                }
            }]
        }";

        var result = processor.Process(json);

        Console.WriteLine($"\nValidation Result: {(result.Validation.IsValid ? "VALID" : "INVALID")}");
        Console.WriteLine($"Errors: {result.Validation.Errors.Count}");
        foreach (var error in result.Validation.Errors)
        {
            Console.WriteLine($"  - [{error.Code}] {error.FieldPath}: {error.Message}");
        }

        Console.WriteLine($"\nLogs ({result.Logs.Count}):");
        foreach (var log in result.Logs)
        {
            Console.WriteLine($"  {log}");
        }
    }
}
