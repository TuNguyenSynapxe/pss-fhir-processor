using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Metadata;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.DynamicTests.Generators;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.DynamicTests
{
    /// <summary>
    /// Tests to ensure metadata-driven test generation provides 100% rule coverage
    /// </summary>
    public class MetadataCoverageTests
    {
        private readonly ITestOutputHelper _output;

        public MetadataCoverageTests(ITestOutputHelper output)
        {
            _output = output;
        }

        /// <summary>
        /// Enforce: Every applicable rule must have exactly one mutation test
        /// </summary>
        [Fact]
        public void EveryApplicableRuleMustHaveOneMutationTest()
        {
            // Load metadata
            var metadata = LoadValidationMetadata();
            var baseBundle = LoadBaseBundle();

            // Generate mutations
            var mutations = DynamicRuleMutationGenerator.GenerateFromMetadata(baseBundle, metadata);
            var summary = DynamicRuleMutationGenerator.LastGenerationSummary;

            _output.WriteLine($"=== Mutation Generation Summary ===");
            _output.WriteLine($"Total rules in metadata: {summary.TotalRuleCount}");
            _output.WriteLine($"Applicable rules (path exists in baseline): {summary.ApplicableRuleCount}");
            _output.WriteLine($"Generated mutations: {summary.GeneratedMutationCount}");
            _output.WriteLine($"Unreachable rules: {summary.TotalRuleCount - summary.ApplicableRuleCount}");
            _output.WriteLine("");

            // Log unreachable Required rules
            if (summary.UnreachableRules.Any())
            {
                _output.WriteLine($"=== Unreachable Required Rules ({summary.UnreachableRules.Count}) ===");
                foreach (var rule in summary.UnreachableRules)
                {
                    _output.WriteLine($"  [{rule.ErrorCode}] {rule.Scope} | {rule.RuleType} | {rule.Path}");
                    _output.WriteLine($"    Reason: {rule.Reason}");
                }
                _output.WriteLine("");
            }

            // Log rule breakdown by scope
            _output.WriteLine($"=== Rules by Scope ===");
            foreach (var ruleSet in metadata.RuleSets)
            {
                _output.WriteLine($"  {ruleSet.Scope}: {ruleSet.Rules.Count} rules");
            }

            // Assert: mutation count equals applicable rule count
            Assert.True(
                summary.GeneratedMutationCount == summary.ApplicableRuleCount,
                $"Every applicable rule must have exactly one mutation test. " +
                $"Expected {summary.ApplicableRuleCount} mutations, got {summary.GeneratedMutationCount}");
        }

        /// <summary>
        /// Enforce: No mutation without a corresponding rule
        /// </summary>
        [Fact]
        public void NoMutationWithoutRule()
        {
            var metadata = LoadValidationMetadata();
            var baseBundle = LoadBaseBundle();
            var mutations = DynamicRuleMutationGenerator.GenerateFromMetadata(baseBundle, metadata);
            var summary = DynamicRuleMutationGenerator.LastGenerationSummary;

            // Check for extra mutations
            Assert.True(
                summary.GeneratedMutationCount <= summary.ApplicableRuleCount,
                $"Found {summary.GeneratedMutationCount} mutations but only {summary.ApplicableRuleCount} applicable rules. " +
                "No mutation should exist without a corresponding rule.");
        }

        /// <summary>
        /// Enforce: No applicable rule without a mutation
        /// </summary>
        [Fact]
        public void NoApplicableRuleWithoutMutation()
        {
            var metadata = LoadValidationMetadata();
            var baseBundle = LoadBaseBundle();
            var mutations = DynamicRuleMutationGenerator.GenerateFromMetadata(baseBundle, metadata);
            var summary = DynamicRuleMutationGenerator.LastGenerationSummary;

            // Check for missing mutations
            Assert.True(
                summary.GeneratedMutationCount >= summary.ApplicableRuleCount,
                $"Found {summary.GeneratedMutationCount} mutations but {summary.ApplicableRuleCount} applicable rules. " +
                "Every applicable rule must have a mutation test.");
        }

        /// <summary>
        /// Report: Mutation test statistics
        /// </summary>
        [Fact]
        public void ReportMutationStatistics()
        {
            var metadata = LoadValidationMetadata();
            var baseBundle = LoadBaseBundle();
            var mutations = DynamicRuleMutationGenerator.GenerateFromMetadata(baseBundle, metadata);
            var summary = DynamicRuleMutationGenerator.LastGenerationSummary;

            _output.WriteLine("========================================");
            _output.WriteLine("MUTATION TEST GENERATION REPORT");
            _output.WriteLine("========================================");
            _output.WriteLine($"Total Rules: {summary.TotalRuleCount}");
            _output.WriteLine($"Applicable Rules (path exists): {summary.ApplicableRuleCount}");
            _output.WriteLine($"Total Mutations Generated: {summary.GeneratedMutationCount}");
            _output.WriteLine($"Coverage: {(summary.ApplicableRuleCount > 0 ? (summary.GeneratedMutationCount * 100 / summary.ApplicableRuleCount) : 0)}% of applicable rules");
            _output.WriteLine("");

            // Unreachable rules
            if (summary.UnreachableRules.Any())
            {
                _output.WriteLine($"Unreachable Required Rules: {summary.UnreachableRules.Count}");
                foreach (var rule in summary.UnreachableRules)
                {
                    _output.WriteLine($"  - {rule.Scope} | {rule.Path}");
                }
                _output.WriteLine("");
            }

            // Rules not testable
            var notTestable = summary.TotalRuleCount - summary.ApplicableRuleCount;
            if (notTestable > 0)
            {
                _output.WriteLine($"Rules not testable (path not in baseline): {notTestable}");
                _output.WriteLine("");
            }

            // Group by rule type
            var ruleTypeGroups = metadata.RuleSets
                .SelectMany(rs => rs.Rules)
                .GroupBy(r => r.RuleType)
                .OrderBy(g => g.Key);

            _output.WriteLine("Rules by Type:");
            foreach (var group in ruleTypeGroups)
            {
                _output.WriteLine($"  {group.Key}: {group.Count()} rules");
            }
            _output.WriteLine("");

            // Group by scope
            _output.WriteLine("Rules by Scope:");
            foreach (var ruleSet in metadata.RuleSets.OrderBy(rs => rs.Scope))
            {
                _output.WriteLine($"  {ruleSet.Scope}: {ruleSet.Rules.Count} rules");
            }
            _output.WriteLine("");

            // Check for failed generation attempts
            var failedMutations = mutations.Where(m => m.Name.Contains("GENERATION_FAILED")).ToList();
            if (failedMutations.Any())
            {
                _output.WriteLine("WARNING: Some mutations failed to generate:");
                foreach (var failed in failedMutations)
                {
                    _output.WriteLine($"  ❌ {failed.Name}");
                }
            }
            else
            {
                _output.WriteLine("✅ All applicable mutations generated successfully");
            }

            Assert.True(true, "Report completed");
        }

        /// <summary>
        /// Verify: All generated mutations have valid error codes
        /// </summary>
        [Fact]
        public void AllMutationsHaveValidErrorCodes()
        {
            var metadata = LoadValidationMetadata();
            var baseBundle = LoadBaseBundle();
            var mutations = DynamicRuleMutationGenerator.GenerateFromMetadata(baseBundle, metadata);

            foreach (var mutation in mutations)
            {
                Assert.NotNull(mutation.ExpectedErrorCodes);
                Assert.NotEmpty(mutation.ExpectedErrorCodes);
                
                foreach (var errorCode in mutation.ExpectedErrorCodes)
                {
                    Assert.False(
                        string.IsNullOrWhiteSpace(errorCode),
                        $"Mutation '{mutation.Name}' has null or empty error code");
                }
            }

            _output.WriteLine($"✅ All {mutations.Count} mutations have valid error codes");
        }

        /// <summary>
        /// Verify: All mutation functions execute without exceptions
        /// </summary>
        [Fact]
        public void AllMutationFunctionsExecuteSuccessfully()
        {
            var metadata = LoadValidationMetadata();
            var baseBundle = LoadBaseBundle();
            var mutations = DynamicRuleMutationGenerator.GenerateFromMetadata(baseBundle, metadata);

            var failedMutations = 0;
            foreach (var mutation in mutations)
            {
                try
                {
                    var clone = (JObject)baseBundle.DeepClone();
                    var mutated = mutation.Apply(clone);
                    
                    Assert.NotNull(mutated);
                    Assert.IsType<JObject>(mutated);
                }
                catch (Exception ex)
                {
                    failedMutations++;
                    _output.WriteLine($"❌ Mutation '{mutation.Name}' failed to execute: {ex.Message}");
                }
            }

            Assert.True(
                failedMutations == 0,
                $"{failedMutations} mutation(s) failed to execute. See output for details.");
        }

        #region Helper Methods

        private ValidationMetadata LoadValidationMetadata()
        {
            var possiblePaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DynamicTests", "Source", "validation-metadata.json"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "DynamicTests", "Source", "validation-metadata.json")
            };

            foreach (var path in possiblePaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    var json = File.ReadAllText(fullPath);
                    return JsonConvert.DeserializeObject<ValidationMetadata>(json);
                }
            }

            throw new FileNotFoundException("Could not find validation-metadata.json");
        }

        private JObject LoadBaseBundle()
        {
            var possiblePaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DynamicTests", "Source", "happy-sample-full.json"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "DynamicTests", "Source", "happy-sample-full.json")
            };

            foreach (var path in possiblePaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    var json = File.ReadAllText(fullPath);
                    return JObject.Parse(json);
                }
            }

            throw new FileNotFoundException("Could not find happy-sample-full.json");
        }

        #endregion
    }
}
