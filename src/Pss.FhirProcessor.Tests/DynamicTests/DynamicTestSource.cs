using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Metadata;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.DynamicTests.Generators;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.DynamicTests.Models;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.DynamicTests
{
    /// <summary>
    /// Generates all dynamic test cases for NUnit TestCaseSource.
    /// Uses metadata-driven mutation generation for 100% rule coverage.
    /// </summary>
    public static class DynamicTestSource
    {
        private static JObject _cachedBaseBundle;
        private static ValidationMetadata _cachedMetadata;

        /// <summary>
        /// Get all test cases: 1 happy case + N metadata-driven mutation cases
        /// </summary>
        public static IEnumerable<DynamicTestCase> GetCases()
        {
            var baseBundle = LoadBaseBundle();
            var metadata = LoadValidationMetadata();

            // Happy case - the baseline should pass all validations
            yield return new DynamicTestCase
            {
                Name = "HappyCase_AllValid",
                ShouldPass = true,
                InputJson = (JObject)baseBundle.DeepClone(),
                ExpectedErrorCodes = new List<string>()
            };

            // Generate metadata-driven mutation test cases
            var autoMutations = DynamicRuleMutationGenerator.GenerateFromMetadata(
                baseBundle,
                metadata
            );

            Console.WriteLine($"Generated {autoMutations.Count} metadata-driven mutation test cases from {CountTotalRules(metadata)} rules");

            // Apply each auto-generated mutation
            var autoMutationResults = new List<DynamicTestCase>();
            foreach (var template in autoMutations)
            {
                try
                {
                    var mutated = template.Apply((JObject)baseBundle.DeepClone());
                    autoMutationResults.Add(new DynamicTestCase
                    {
                        Name = template.Name,
                        ShouldPass = false,
                        InputJson = mutated,
                        ExpectedErrorCodes = template.ExpectedErrorCodes.ToList()
                    });
                }
                catch (Exception ex)
                {
                    // If mutation fails, create a test case that will fail with useful info
                    autoMutationResults.Add(new DynamicTestCase
                    {
                        Name = $"{template.Name}_MUTATION_FAILED",
                        ShouldPass = false,
                        InputJson = (JObject)baseBundle.DeepClone(),
                        ExpectedErrorCodes = new List<string> { $"MUTATION_ERROR: {ex.Message}" }
                    });
                }
            }

            foreach (var result in autoMutationResults)
            {
                yield return result;
            }
        }

        /// <summary>
        /// Count total rules across all rule sets
        /// </summary>
        private static int CountTotalRules(ValidationMetadata metadata)
        {
            return metadata.RuleSets.Sum(rs => rs.Rules.Count);
        }

        /// <summary>
        /// Load validation metadata (cached for performance)
        /// </summary>
        private static ValidationMetadata LoadValidationMetadata()
        {
            if (_cachedMetadata != null)
            {
                return _cachedMetadata;
            }

            // Load from DynamicTests/Source folder
            var possiblePaths = new[]
            {
                // Relative to test assembly (most reliable for test execution)
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DynamicTests", "Source", "validation-metadata.json"),
                
                // Relative from DynamicTests folder (for development)
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "DynamicTests", "Source", "validation-metadata.json"),
                
                // Direct relative path from assembly location
                Path.Combine(Path.GetDirectoryName(typeof(DynamicTestSource).Assembly.Location), "DynamicTests", "Source", "validation-metadata.json")
            };

            foreach (var path in possiblePaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    var json = File.ReadAllText(fullPath);
                    _cachedMetadata = JsonConvert.DeserializeObject<ValidationMetadata>(json);
                    return _cachedMetadata;
                }
            }

            throw new FileNotFoundException(
                $"Could not find validation-metadata.json in DynamicTests/Source folder. Tried:\n" +
                string.Join("\n", possiblePaths.Select(p => $"  - {Path.GetFullPath(p)}")));
        }

        /// <summary>
        /// Load the baseline happy bundle (cached for performance)
        /// </summary>
        private static JObject LoadBaseBundle()
        {
            if (_cachedBaseBundle != null)
            {
                return _cachedBaseBundle;
            }

            // Load from DynamicTests/Source folder
            var possiblePaths = new[]
            {
                // Relative to test assembly (most reliable for test execution)
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DynamicTests", "Source", "happy-sample-full.json"),
                
                // Relative from DynamicTests folder (for development)
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "DynamicTests", "Source", "happy-sample-full.json"),
                
                // Direct relative path from assembly location
                Path.Combine(Path.GetDirectoryName(typeof(DynamicTestSource).Assembly.Location), "DynamicTests", "Source", "happy-sample-full.json")
            };

            foreach (var path in possiblePaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    var json = File.ReadAllText(fullPath);
                    _cachedBaseBundle = JObject.Parse(json);
                    return _cachedBaseBundle;
                }
            }

            throw new FileNotFoundException(
                $"Could not find happy-sample-full.json in DynamicTests/Source folder. Tried:\n" +
                string.Join("\n", possiblePaths.Select(p => $"  - {Path.GetFullPath(p)}")));
        }
    }
}
