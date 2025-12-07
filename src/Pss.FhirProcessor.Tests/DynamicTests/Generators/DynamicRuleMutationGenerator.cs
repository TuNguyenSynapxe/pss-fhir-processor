using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Metadata;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.DynamicTests.Helpers;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.DynamicTests.Models;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.DynamicTests.Generators
{
    /// <summary>
    /// Exception thrown when a mutation fails to apply
    /// </summary>
    public class MutationFailedException : Exception
    {
        public MutationFailedException(string message) : base(message) { }
    }

    /// <summary>
    /// Summary of mutation generation results
    /// </summary>
    public class MutationGenerationSummary
    {
        public int TotalRuleCount { get; set; }
        public int ApplicableRuleCount { get; set; }
        public int GeneratedMutationCount { get; set; }
        public List<UnreachableRule> UnreachableRules { get; set; } = new List<UnreachableRule>();
    }

    /// <summary>
    /// Information about a rule whose path doesn't exist in the baseline
    /// </summary>
    public class UnreachableRule
    {
        public string RuleType { get; set; }
        public string Scope { get; set; }
        public string Path { get; set; }
        public string ErrorCode { get; set; }
        public string Reason { get; set; }
    }

    /// <summary>
    /// Automatically generates mutation test cases from validation metadata.
    /// Each rule produces exactly one mutation that violates that rule.
    /// </summary>
    public static class DynamicRuleMutationGenerator
    {
        /// <summary>
        /// Last generation summary
        /// </summary>
        public static MutationGenerationSummary LastGenerationSummary { get; private set; }

        /// <summary>
        /// Generate mutation templates from validation metadata.
        /// Returns one mutation per rule where the path exists in the baseline.
        /// </summary>
        /// <param name="baseBundle">Valid baseline bundle to mutate</param>
        /// <param name="metadata">Validation metadata containing all rules</param>
        /// <returns>List of mutation templates for applicable rules</returns>
        public static List<MutationTemplate> GenerateFromMetadata(
            JObject baseBundle,
            ValidationMetadata metadata)
        {
            var mutations = new List<MutationTemplate>();
            var summary = new MutationGenerationSummary();
            var ruleCounter = 0;

            foreach (var ruleSet in metadata.RuleSets)
            {
                foreach (var rule in ruleSet.Rules)
                {
                    ruleCounter++;
                    summary.TotalRuleCount++;
                    
                    // Pre-check: Does the path exist in the baseline?
                    var pathExists = CheckPathExists(baseBundle, ruleSet, rule);
                    
                    if (!pathExists)
                    {
                        // Path doesn't exist - handle based on rule type
                        if (rule.RuleType == "Required")
                        {
                            // Required rules with non-existent paths are unreachable
                            summary.UnreachableRules.Add(new UnreachableRule
                            {
                                RuleType = rule.RuleType,
                                Scope = ruleSet.Scope,
                                Path = rule.Path,
                                ErrorCode = rule.ErrorCode,
                                Reason = "Path does not exist in baseline bundle"
                            });
                        }
                        // For all other rule types, skip silently (not testable with this baseline)
                        continue;
                    }
                    
                    // Path exists - generate mutation
                    summary.ApplicableRuleCount++;
                    
                    try
                    {
                        var mutation = GenerateMutationForRule(ruleSet, rule, ruleCounter);
                        if (mutation != null)
                        {
                            mutations.Add(mutation);
                            summary.GeneratedMutationCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        // If mutation generation fails, create a failing test to alert developers
                        mutations.Add(new MutationTemplate
                        {
                            Name = $"Rule{ruleCounter:D3}_{ruleSet.Scope}_{rule.RuleType}_GENERATION_FAILED",
                            Apply = bundle => bundle,
                            ExpectedErrorCodes = new List<string> 
                            { 
                                $"MUTATION_GEN_ERROR: {ex.Message}" 
                            }
                        });
                    }
                }
            }

            LastGenerationSummary = summary;
            
            Console.WriteLine($"Generated {summary.GeneratedMutationCount} metadata-driven mutation test cases from {summary.TotalRuleCount} rules");
            Console.WriteLine($"  - Applicable rules (path exists): {summary.ApplicableRuleCount}");
            Console.WriteLine($"  - Unreachable Required rules: {summary.UnreachableRules.Count}");
            Console.WriteLine($"  - Skipped (path not in baseline): {summary.TotalRuleCount - summary.ApplicableRuleCount}");
            
            return mutations;
        }

        /// <summary>
        /// Check if a rule's path exists in the baseline bundle
        /// </summary>
        private static bool CheckPathExists(JObject baseBundle, RuleSet ruleSet, RuleDefinition rule)
        {
            try
            {
                // For bundle-level paths, check directly
                if (ruleSet.Scope.StartsWith("Bundle"))
                {
                    var tokens = CpsPathNavigator.SelectTokens(baseBundle, rule.Path);
                    return tokens.Count > 0;
                }
                
                // For resource-level paths, check in matching resources
                var resourceType = ruleSet.ScopeDefinition?.ResourceType 
                    ?? ExtractResourceTypeFromScope(ruleSet.Scope);
                var matchConditions = ruleSet.ScopeDefinition?.Match;
                
                var entries = baseBundle["entry"] as JArray;
                if (entries == null) return false;
                
                // Extract the path relative to the resource
                // E.g., "Patient.id" becomes "id", "Observation.code.coding[0]" becomes "code.coding[0]"
                var resourcePath = rule.Path;
                if (resourcePath.StartsWith(resourceType + "."))
                {
                    resourcePath = resourcePath.Substring(resourceType.Length + 1);
                }
                
                foreach (var entry in entries)
                {
                    var resource = entry["resource"];
                    if (resource == null) continue;
                    if (resource["resourceType"]?.ToString() != resourceType) continue;
                    
                    // Check match conditions
                    if (matchConditions != null && matchConditions.Any())
                    {
                        if (!MatchesConditions(resource, matchConditions))
                            continue;
                    }
                    
                    // Check if path exists in this resource
                    var tokens = CpsPathNavigator.SelectTokens(resource, resourcePath);
                    if (tokens.Count > 0)
                        return true;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generate a single mutation template for a specific rule
        /// </summary>
        private static MutationTemplate GenerateMutationForRule(
            RuleSet ruleSet, 
            RuleDefinition rule, 
            int ruleIndex)
        {
            var ruleName = $"Rule{ruleIndex:D3}_{SanitizeScopeName(ruleSet.Scope)}_{rule.RuleType}";
            
            switch (rule.RuleType)
            {
                case "Required":
                    return GenerateRequiredMutation(ruleName, ruleSet, rule);

                case "FixedValue":
                    return GenerateFixedValueMutation(ruleName, ruleSet, rule);

                case "Type":
                    return GenerateTypeMutation(ruleName, ruleSet, rule);

                case "AllowedValues":
                    return GenerateAllowedValuesMutation(ruleName, ruleSet, rule);

                case "CodeSystem":
                    return GenerateCodeSystemMutation(ruleName, ruleSet, rule);

                case "Reference":
                    return GenerateReferenceMutation(ruleName, ruleSet, rule);

                case "Regex":
                    return GenerateRegexMutation(ruleName, ruleSet, rule);

                case "FullUrlIdMatch":
                    return GenerateFullUrlIdMatchMutation(ruleName, ruleSet, rule);

                case "CodesMaster":
                    return GenerateCodesMasterMutation(ruleName, ruleSet, rule);

                default:
                    // Unknown rule type - skip but log
                    Console.WriteLine($"WARNING: Unknown RuleType '{rule.RuleType}' in {ruleSet.Scope}");
                    return null;
            }
        }

        #region Mutation Generators by Rule Type

        /// <summary>
        /// Generate mutation for Required rule: Remove the required field
        /// </summary>
        private static MutationTemplate GenerateRequiredMutation(
            string name, 
            RuleSet ruleSet, 
            RuleDefinition rule)
        {
            return new MutationTemplate
            {
                Name = name,
                Apply = bundle => RemoveFieldByPath(bundle, ruleSet, rule.Path),
                ExpectedErrorCodes = new List<string> { rule.ErrorCode ?? "MANDATORY_MISSING" }
            };
        }

        /// <summary>
        /// Generate mutation for FixedValue rule: Replace with invalid value
        /// </summary>
        private static MutationTemplate GenerateFixedValueMutation(
            string name,
            RuleSet ruleSet,
            RuleDefinition rule)
        {
            return new MutationTemplate
            {
                Name = name,
                Apply = bundle => SetFieldValue(
                    bundle, 
                    ruleSet, 
                    rule.Path, 
                    GenerateInvalidFixedValue(rule.ExpectedValue)),
                ExpectedErrorCodes = new List<string> { rule.ErrorCode ?? "FIXED_VALUE_MISMATCH" }
            };
        }

        /// <summary>
        /// Generate mutation for Type rule: Set clearly invalid type value
        /// </summary>
        private static MutationTemplate GenerateTypeMutation(
            string name,
            RuleSet ruleSet,
            RuleDefinition rule)
        {
            var invalidValue = GenerateInvalidTypeValue(rule.ExpectedType);
            
            return new MutationTemplate
            {
                Name = name,
                Apply = bundle => SetFieldValue(bundle, ruleSet, rule.Path, invalidValue),
                ExpectedErrorCodes = new List<string> { rule.ErrorCode ?? "TYPE_MISMATCH" }
            };
        }

        /// <summary>
        /// Generate mutation for AllowedValues rule: Set value not in allowed list
        /// </summary>
        private static MutationTemplate GenerateAllowedValuesMutation(
            string name,
            RuleSet ruleSet,
            RuleDefinition rule)
        {
            return new MutationTemplate
            {
                Name = name,
                Apply = bundle => SetFieldValue(bundle, ruleSet, rule.Path, "INVALID_NOT_IN_ALLOWED_LIST"),
                ExpectedErrorCodes = new List<string> { rule.ErrorCode ?? "INVALID_VALUE" }
            };
        }

        /// <summary>
        /// Generate mutation for CodeSystem rule: Set invalid code and system
        /// </summary>
        private static MutationTemplate GenerateCodeSystemMutation(
            string name,
            RuleSet ruleSet,
            RuleDefinition rule)
        {
            return new MutationTemplate
            {
                Name = name,
                Apply = bundle =>
                {
                    // Mutate BOTH code and system to ensure validation breaks
                    var clone = (JObject)bundle.DeepClone();
                    
                    // Set invalid code
                    try
                    {
                        clone = SetFieldValue(clone, ruleSet, rule.Path + ".code", "INVALID");
                    }
                    catch
                    {
                        // If .code doesn't exist, try the path itself
                        clone = SetFieldValue(bundle, ruleSet, rule.Path, "INVALID");
                    }
                    
                    // Set invalid system
                    try
                    {
                        clone = SetFieldValue(clone, ruleSet, rule.Path + ".system", "https://invalid-system");
                    }
                    catch
                    {
                        // System might not exist at this path
                    }
                    
                    return clone;
                },
                ExpectedErrorCodes = new List<string> { rule.ErrorCode ?? "CODESYSTEM_VALIDATION_ERROR" }
            };
        }

        /// <summary>
        /// Generate mutation for Reference rule: Remove the entire reference object
        /// </summary>
        private static MutationTemplate GenerateReferenceMutation(
            string name,
            RuleSet ruleSet,
            RuleDefinition rule)
        {
            return new MutationTemplate
            {
                Name = name,
                Apply = bundle =>
                {
                    // Remove the entire reference object to break validation
                    try
                    {
                        return RemoveFieldByPath(bundle, ruleSet, rule.Path);
                    }
                    catch
                    {
                        // If removal fails, try setting to invalid reference
                        return SetFieldValue(
                            bundle, 
                            ruleSet, 
                            rule.Path + ".reference", 
                            "urn:uuid:deadbeef-dead-dead-dead-deaddeaddead");
                    }
                },
                ExpectedErrorCodes = new List<string> { rule.ErrorCode ?? "REF_INVALID" }
            };
        }

        /// <summary>
        /// Generate mutation for Regex rule: Set value that doesn't match pattern
        /// </summary>
        private static MutationTemplate GenerateRegexMutation(
            string name,
            RuleSet ruleSet,
            RuleDefinition rule)
        {
            var invalidValue = GenerateRegexInvalidValue(rule.Pattern);
            
            return new MutationTemplate
            {
                Name = name,
                Apply = bundle => SetFieldValue(bundle, ruleSet, rule.Path, invalidValue),
                ExpectedErrorCodes = new List<string> { rule.ErrorCode ?? "REGEX_MISMATCH" }
            };
        }

        /// <summary>
        /// Generate mutation for FullUrlIdMatch rule: Mismatch fullUrl and id
        /// </summary>
        private static MutationTemplate GenerateFullUrlIdMatchMutation(
            string name,
            RuleSet ruleSet,
            RuleDefinition rule)
        {
            return new MutationTemplate
            {
                Name = name,
                Apply = bundle =>
                {
                    // Find first resource of this scope's type and mismatch its id
                    var resourceType = ruleSet.ScopeDefinition?.ResourceType;
                    if (string.IsNullOrEmpty(resourceType))
                    {
                        // Fallback: extract from scope name
                        resourceType = ExtractResourceTypeFromScope(ruleSet.Scope);
                    }

                    return MismatchResourceIdAndFullUrl(bundle, resourceType);
                },
                ExpectedErrorCodes = new List<string> { rule.ErrorCode ?? "ID_FULLURL_MISMATCH" }
            };
        }

        /// <summary>
        /// Generate mutation for CodesMaster rule: Invalid question/answer
        /// </summary>
        private static MutationTemplate GenerateCodesMasterMutation(
            string name,
            RuleSet ruleSet,
            RuleDefinition rule)
        {
            return new MutationTemplate
            {
                Name = name,
                Apply = bundle =>
                {
                    // For Observation components, break the first component's question code
                    if (rule.Path.Contains("component"))
                    {
                        var screeningType = ExtractScreeningTypeFromScope(ruleSet.Scope);
                        return JsonMutationHelpers.BreakObservationQuestionCode(
                            bundle, 
                            screeningType, 
                            "SQ-INVALID-99999999");
                    }
                    return bundle;
                },
                ExpectedErrorCodes = new List<string> { rule.ErrorCode ?? "INVALID_ANSWER_VALUE" }
            };
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Remove field from matching resources in the bundle
        /// </summary>
        private static JObject RemoveFieldFromMatchingResources(JObject bundle, RuleSet ruleSet, string path)
        {
            var clone = (JObject)bundle.DeepClone();
            var entries = clone["entry"] as JArray;
            if (entries == null) return clone;

            var resourceType = ruleSet.ScopeDefinition?.ResourceType 
                ?? ExtractResourceTypeFromScope(ruleSet.Scope);
            var matchConditions = ruleSet.ScopeDefinition?.Match;

            // Strip resource type prefix from path for resource-level operations
            var resourcePath = path;
            if (resourcePath.StartsWith(resourceType + "."))
            {
                resourcePath = resourcePath.Substring(resourceType.Length + 1);
            }

            foreach (var entry in entries)
            {
                var resource = entry["resource"];
                if (resource == null) continue;
                if (resource["resourceType"]?.ToString() != resourceType) continue;

                // Check match conditions
                if (matchConditions != null && matchConditions.Any())
                {
                    if (!MatchesConditions(resource, matchConditions))
                        continue;
                }

                // Remove the field from this resource using CPS navigator
                CpsPathNavigator.RemoveAt(resource, resourcePath);
            }

            return clone;
        }

        /// <summary>
        /// Set field value in matching resources in the bundle
        /// </summary>
        private static JObject SetFieldInMatchingResources(JObject bundle, RuleSet ruleSet, string path, object value)
        {
            var clone = (JObject)bundle.DeepClone();
            var entries = clone["entry"] as JArray;
            if (entries == null) return clone;

            var resourceType = ruleSet.ScopeDefinition?.ResourceType 
                ?? ExtractResourceTypeFromScope(ruleSet.Scope);
            var matchConditions = ruleSet.ScopeDefinition?.Match;

            // Strip resource type prefix from path for resource-level operations
            var resourcePath = path;
            if (resourcePath.StartsWith(resourceType + "."))
            {
                resourcePath = resourcePath.Substring(resourceType.Length + 1);
            }

            foreach (var entry in entries)
            {
                var resource = entry["resource"];
                if (resource == null) continue;
                if (resource["resourceType"]?.ToString() != resourceType) continue;

                // Check match conditions
                if (matchConditions != null && matchConditions.Any())
                {
                    if (!MatchesConditions(resource, matchConditions))
                        continue;
                }

                // Set the field in this resource using CPS navigator
                CpsPathNavigator.ReplaceValue(resource, resourcePath, JToken.FromObject(value));
            }

            return clone;
        }

        /// <summary>
        /// Check if a resource matches all match conditions
        /// </summary>
        private static bool MatchesConditions(JToken resource, List<MatchCondition> conditions)
        {
            foreach (var condition in conditions)
            {
                // Use CPS navigator to check condition
                var tokens = CpsPathNavigator.SelectTokens(resource, condition.Path);
                var matchFound = tokens.Any(t => t?.ToString() == condition.Expected);
                if (!matchFound)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Remove field by path - routes to appropriate method based on scope
        /// </summary>
        private static JObject RemoveFieldByPath(JObject bundle, RuleSet ruleSet, string path)
        {
            // Determine if this is a bundle-level or resource-level operation
            if (ruleSet.Scope.StartsWith("Bundle."))
            {
                // Direct bundle-level operation
                var clone = (JObject)bundle.DeepClone();
                CpsPathNavigator.RemoveAt(clone, path);
                return clone;
            }
            else
            {
                // Resource-level operation - remove from matching resources
                return RemoveFieldFromMatchingResources(bundle, ruleSet, path);
            }
        }

        /// <summary>
        /// Set field value - routes to appropriate method based on scope
        /// </summary>
        private static JObject SetFieldValue(JObject bundle, RuleSet ruleSet, string path, object value)
        {
            // Determine if this is a bundle-level or resource-level operation
            if (ruleSet.Scope.StartsWith("Bundle."))
            {
                // Direct bundle-level operation
                var clone = (JObject)bundle.DeepClone();
                CpsPathNavigator.ReplaceValue(clone, path, JToken.FromObject(value));
                return clone;
            }
            else
            {
                // Resource-level operation - set in matching resources
                return SetFieldInMatchingResources(bundle, ruleSet, path, value);
            }
        }

        /// <summary>
        /// Mismatch the id and fullUrl for a resource
        /// </summary>
        private static JObject MismatchResourceIdAndFullUrl(JObject bundle, string resourceType)
        {
            var clone = (JObject)bundle.DeepClone();
            var entries = clone["entry"] as JArray;
            if (entries == null) return clone;

            var targetEntry = entries.FirstOrDefault(e => 
                e["resource"]?["resourceType"]?.ToString() == resourceType);

            if (targetEntry != null)
            {
                // Change the id to mismatch fullUrl
                var resource = targetEntry["resource"];
                resource["id"] = "99999999-9999-9999-9999-999999999999";
            }

            return clone;
        }

        /// <summary>
        /// Generate invalid value for FixedValue
        /// </summary>
        private static object GenerateInvalidFixedValue(string expectedValue)
        {
            if (expectedValue == null) return "INVALID_NULL_EXPECTED";
            
            // If expected is boolean, return opposite
            if (expectedValue == "true") return false;
            if (expectedValue == "false") return true;
            
            // If expected is a URL, return different URL
            if (expectedValue.StartsWith("http"))
                return "https://invalid-system.example.com/wrong";
            
            // Default: append INVALID
            return $"{expectedValue}_INVALID";
        }

        /// <summary>
        /// Generate invalid value for Type validation
        /// </summary>
        private static object GenerateInvalidTypeValue(string expectedType)
        {
            switch (expectedType?.ToLower())
            {
                case "guid":
                    return "NOT-A-GUID";
                    
                case "guid-uri":
                    return "urn:uuid:NOT-A-GUID";
                    
                case "date":
                    return "not-a-date";
                    
                case "datetime":
                    return "not-a-datetime";
                    
                case "boolean":
                    return "not-a-boolean";
                    
                case "integer":
                    return "not-an-integer";
                    
                case "decimal":
                    return "not-a-decimal";
                    
                case "pipestring[]":
                    return "Invalid|Pipe|Values|Not|In|Master";
                    
                case "array":
                    return "not-an-array";
                    
                case "object":
                    return "not-an-object";
                    
                default:
                    return $"INVALID_{expectedType?.ToUpper() ?? "TYPE"}";
            }
        }

        /// <summary>
        /// Generate value that won't match regex pattern
        /// </summary>
        private static string GenerateRegexInvalidValue(string pattern)
        {
            // Common patterns
            if (pattern != null)
            {
                if (pattern.Contains("uuid"))
                    return "not-a-valid-urn-uuid";
                if (pattern.Contains("\\d{7}"))
                    return "INVALID_NRIC_FORMAT";
                if (pattern.Contains("\\d{6}"))
                    return "INVALID_POSTAL";
            }
            
            return "REGEX_INVALID_VALUE";
        }

        /// <summary>
        /// Extract resource type from scope name (e.g., "Patient" from "Patient")
        /// </summary>
        private static string ExtractResourceTypeFromScope(string scope)
        {
            if (scope.Contains("."))
            {
                return scope.Split('.')[0];
            }
            return scope;
        }

        /// <summary>
        /// Extract screening type from scope (e.g., "HS" from "Observation.HearingScreening")
        /// </summary>
        private static string ExtractScreeningTypeFromScope(string scope)
        {
            if (scope.Contains("Hearing")) return "HS";
            if (scope.Contains("Oral")) return "OS";
            if (scope.Contains("Vision")) return "VS";
            return "";
        }

        /// <summary>
        /// Sanitize scope name for use in test names
        /// </summary>
        private static string SanitizeScopeName(string scope)
        {
            return scope
                .Replace(".", "_")
                .Replace(":", "_")
                .Replace(" ", "")
                .Replace("-", "_");
        }

        #endregion
    }
}
