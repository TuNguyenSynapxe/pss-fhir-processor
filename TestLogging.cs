using System;
using System.IO;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;

namespace TestLogging
{
    class Program
    {
        static void Main(string[] args)
        {
            // Load sample FHIR bundle
            var sampleBundle = File.ReadAllText("./src/Pss.FhirProcessor.Tests/TestData/valid_bundle.json");
            var metadataPath = "./src/Pss.FhirProcessor/Metadata";

            // Create processor with verbose logging enabled
            var processor = new FhirProcessor(
                metadataPath,
                new ValidationOptions { StrictDisplayValidation = false },
                new LoggingOptions { Enabled = true, Level = "Verbose" }
            );

            // Process the bundle
            var result = processor.Process(sampleBundle);

            // Display the logs
            Console.WriteLine("\n" + new string('=', 80));
            Console.WriteLine("COMPREHENSIVE STEP-BY-STEP LOGGING OUTPUT");
            Console.WriteLine(new string('=', 80) + "\n");

            foreach (var log in result.Logs)
            {
                Console.WriteLine(log);
            }

            Console.WriteLine("\n" + new string('=', 80));
            Console.WriteLine($"FINAL RESULT: {(result.Success ? "✓ SUCCESS" : "✗ FAILED")}");
            Console.WriteLine($"Validation Errors: {result.ValidationResult?.Errors.Count ?? 0}");
            Console.WriteLine($"Extraction Success: {result.FlattenResult != null}");
            Console.WriteLine(new string('=', 80));
        }
    }
}
