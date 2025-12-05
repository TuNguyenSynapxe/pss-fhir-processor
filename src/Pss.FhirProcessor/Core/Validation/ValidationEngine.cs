using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Metadata;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Path;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Utilities;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation
{
    /// <summary>
    /// Main validation engine for FHIR resources using CPS1 path syntax
    /// </summary>
    public class ValidationEngine
    {
        private ValidationMetadata _metadata;
        private Logger _logger;

        public ValidationEngine()
        {
        }

        /// <summary>
        /// Load validation metadata from JSON file
        /// </summary>
        public void LoadMetadata(string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
            {
                throw new FileNotFoundException($"Metadata file not found: {jsonFilePath}");
            }

            var json = File.ReadAllText(jsonFilePath);
            _metadata = JsonConvert.DeserializeObject<ValidationMetadata>(json);

            if (_metadata == null)
            {
                throw new InvalidOperationException("Failed to deserialize validation metadata");
            }

            if (_metadata.PathSyntax != "CPS1")
            {
                throw new NotSupportedException($"Unsupported path syntax: {_metadata.PathSyntax}. Only CPS1 is supported.");
            }
        }

        /// <summary>
        /// Load validation metadata from JSON string
        /// </summary>
        public void LoadMetadataFromJson(string json)
        {
            Console.WriteLine("[ValidationEngine] LoadMetadataFromJson - START");
            Console.WriteLine($"[ValidationEngine] JSON length: {json?.Length ?? 0}");
            
            _metadata = JsonConvert.DeserializeObject<ValidationMetadata>(json);

            if (_metadata == null)
            {
                throw new InvalidOperationException("Failed to deserialize validation metadata");
            }
            
            Console.WriteLine($"[ValidationEngine] Deserialized metadata - Version: {_metadata.Version}");
            Console.WriteLine($"[ValidationEngine] RuleSets count: {_metadata.RuleSets?.Count ?? 0}");
            
            // Log Type and Regex rules to verify ExpectedType and Pattern
            if (_metadata.RuleSets != null)
            {
                foreach (var ruleSet in _metadata.RuleSets)
                {
                    var typeRules = ruleSet.Rules?.Where(r => r.RuleType == "Type").ToList();
                    var regexRules = ruleSet.Rules?.Where(r => r.RuleType == "Regex").ToList();
                    
                    Console.WriteLine($"[ValidationEngine] Scope '{ruleSet.Scope}': {typeRules?.Count ?? 0} Type rules, {regexRules?.Count ?? 0} Regex rules");
                    
                    if (typeRules != null)
                    {
                        foreach (var rule in typeRules.Take(3))
                        {
                            Console.WriteLine($"[ValidationEngine]   Type rule: Path={rule.Path}, ExpectedType={rule.ExpectedType ?? "NULL"}");
                        }
                    }
                    
                    if (regexRules != null)
                    {
                        foreach (var rule in regexRules.Take(3))
                        {
                            Console.WriteLine($"[ValidationEngine]   Regex rule: Path={rule.Path}, Pattern={rule.Pattern ?? "NULL"}");
                        }
                    }
                }
            }

            if (_metadata.PathSyntax != "CPS1")
            {
                throw new NotSupportedException($"Unsupported path syntax: {_metadata.PathSyntax}. Only CPS1 is supported.");
            }
        }

        /// <summary>
        /// Validate a FHIR Bundle
        /// </summary>
        public ValidationResult Validate(string bundleJson, string logLevel = "info")
        {
            if (_metadata == null)
            {
                throw new InvalidOperationException("Metadata not loaded. Call LoadMetadata() first.");
            }

            _logger = new Logger(logLevel);
            var result = new ValidationResult();

            _logger.Info("========================================");
            _logger.Info("V5 Validation Engine Started");
            _logger.Info("========================================");
            _logger.Debug($"Log Level: {logLevel}");
            _logger.Debug($"Bundle JSON length: {bundleJson?.Length ?? 0} characters");
            _logger.Verbose($"Bundle JSON preview: {(bundleJson?.Length > 200 ? bundleJson.Substring(0, 200) + "..." : bundleJson)}");

            try
            {
                _logger.Debug("Parsing FHIR Bundle JSON...");
                var bundle = JObject.Parse(bundleJson);

                // Verify it's a Bundle
                var resourceType = bundle["resourceType"]?.ToString();
                _logger.Debug($"Resource Type: {resourceType}");
                
                if (resourceType != "Bundle")
                {
                    _logger.Error($"Invalid resource type: expected Bundle, got {resourceType}");
                    result.AddError("INVALID_RESOURCE_TYPE", "resourceType", 
                        $"Expected Bundle, got {resourceType}");
                    result.Logs = _logger.GetLogs();
                    return result;
                }

                // Get all entries
                var entries = bundle["entry"] as JArray;
                _logger.Info($"Bundle contains {entries?.Count ?? 0} entries");
                
                if (entries == null || entries.Count == 0)
                {
                    _logger.Error("Bundle has no entries");
                    result.AddError("EMPTY_BUNDLE", "entry", "Bundle has no entries");
                    result.Logs = _logger.GetLogs();
                    return result;
                }

                // Extract resources
                _logger.Debug("Extracting resources from bundle entries...");
                var resources = new List<JObject>();
                foreach (var entry in entries)
                {
                    var resource = entry["resource"] as JObject;
                    if (resource != null)
                    {
                        resources.Add(resource);
                        var rt = resource["resourceType"]?.ToString();
                        _logger.Verbose($"  Found resource: {rt}");
                    }
                }
                
                _logger.Info($"Extracted {resources.Count} resources");
                
                // Log resource type breakdown
                var resourceTypes = resources
                    .Select(r => r["resourceType"]?.ToString())
                    .Where(rt => rt != null)
                    .GroupBy(rt => rt)
                    .ToDictionary(g => g.Key, g => g.Count());
                
                _logger.Debug("Resource breakdown:");
                foreach (var rt in resourceTypes)
                {
                    _logger.Debug($"  {rt.Key}: {rt.Value}");
                }

                // Validate each RuleSet scope
                _logger.Info($"\nValidating {_metadata.RuleSets.Count} rule set(s)...");
                
                foreach (var ruleSet in _metadata.RuleSets)
                {
                    _logger.Debug($"\nProcessing RuleSet: {ruleSet.Scope}");
                    _logger.Debug($"  Rules: {ruleSet.Rules.Count}");
                    
                    var scopeResources = SelectResourcesByScope(resources, ruleSet);
                    _logger.Debug($"  Matching resources: {scopeResources.Count}");

                    if (scopeResources.Count == 0)
                    {
                        _logger.Verbose($"  No resources found for scope {ruleSet.Scope} - skipping");
                        continue;
                    }

                    foreach (var resource in scopeResources)
                    {
                        var resourceId = resource["id"]?.ToString() ?? "unknown";
                        _logger.Verbose($"  Validating resource: {ruleSet.Scope}/{resourceId}");
                        ValidateResource(resource, ruleSet, result);
                    }
                }

                _logger.Info($"\nValidation complete");
                _logger.Info($"Total errors: {result.Errors.Count}");
                _logger.Info($"Result: {(result.IsValid ? "✓ VALID" : "✗ INVALID")}");
                
                if (result.Errors.Count > 0)
                {
                    _logger.Debug("\nValidation errors:");
                    int displayCount = result.Errors.Count > 10 ? 10 : result.Errors.Count;
                    for (int i = 0; i < displayCount; i++)
                    {
                        var err = result.Errors[i];
                        _logger.Debug($"  {i + 1}. [{err.Code}] {err.FieldPath}");
                        _logger.Verbose($"      Message: {err.Message}");
                        _logger.Verbose($"      Scope: {err.Scope}");
                    }
                    if (result.Errors.Count > 10)
                    {
                        _logger.Debug($"  ... and {result.Errors.Count - 10} more errors");
                    }
                }

                result.Summary = result.IsValid 
                    ? "Validation passed" 
                    : $"Validation failed with {result.Errors.Count} error(s)";
                    
                _logger.Info("========================================");
            }
            catch (JsonException ex)
            {
                _logger.Error($"JSON parsing error: {ex.Message}");
                result.AddError("INVALID_JSON", "", $"Invalid JSON: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Validation error: {ex.Message}");
                _logger.Verbose($"Stack trace: {ex.StackTrace}");
                result.AddError("VALIDATION_ERROR", "", $"Validation error: {ex.Message}");
            }

            result.Logs = _logger.GetLogs();
            return result;
        }

        /// <summary>
        /// Select resources by scope using metadata-driven matching
        /// </summary>
        private List<JObject> SelectResourcesByScope(List<JObject> resources, RuleSet ruleSet)
        {
            var results = new List<JObject>();

            // Use ScopeDefinition if available (metadata-driven)
            if (ruleSet.ScopeDefinition != null)
            {
                _logger?.Verbose($"  Using ScopeDefinition for filtering");
                _logger?.Verbose($"    ResourceType: {ruleSet.ScopeDefinition.ResourceType}");
                _logger?.Verbose($"    Match conditions: {ruleSet.ScopeDefinition.Match?.Count ?? 0}");

                foreach (var resource in resources)
                {
                    if (ResourceMatchesScope(resource, ruleSet.ScopeDefinition))
                    {
                        results.Add(resource);
                        var resourceId = resource["id"]?.ToString() ?? "unknown";
                        _logger?.Verbose($"    ✓ Resource {resourceId} matched scope {ruleSet.Scope}");
                    }
                }
            }
            else
            {
                // Fallback: legacy scope matching by resource type only
                _logger?.Verbose($"  No ScopeDefinition found, using legacy scope matching");
                
                // Extract resource type from scope (before any dot)
                var scopeParts = ruleSet.Scope.Split('.');
                var resourceType = scopeParts[0];
                
                _logger?.Verbose($"    Matching by ResourceType: {resourceType}");

                foreach (var resource in resources)
                {
                    var type = resource["resourceType"]?.ToString();
                    if (type == resourceType)
                    {
                        results.Add(resource);
                        var resourceId = resource["id"]?.ToString() ?? "unknown";
                        _logger?.Verbose($"    ✓ Resource {resourceId} matched type {resourceType}");
                    }
                }
            }

            _logger?.Debug($"  Selected {results.Count}/{resources.Count} resources");
            return results;
        }

        /// <summary>
        /// Check if a resource matches a scope definition
        /// </summary>
        private bool ResourceMatchesScope(JObject resource, ScopeDefinition scopeDef)
        {
            if (resource == null || scopeDef == null)
            {
                _logger?.Warn("ResourceMatchesScope: null resource or scope definition");
                return false;
            }

            // Check resource type
            var resourceType = resource["resourceType"]?.ToString();
            if (resourceType != scopeDef.ResourceType)
            {
                _logger?.Verbose($"      ✗ ResourceType mismatch: expected {scopeDef.ResourceType}, got {resourceType}");
                return false;
            }

            // If no match conditions, accept all resources of this type
            if (scopeDef.Match == null || scopeDef.Match.Count == 0)
            {
                _logger?.Verbose($"      ✓ ResourceType matches and no additional conditions");
                return true;
            }

            // Check each match condition
            foreach (var condition in scopeDef.Match)
            {
                var actualValue = CpsPathResolver.GetValueAsString(resource, condition.Path);
                
                _logger?.Verbose($"      Checking: {condition.Path}");
                _logger?.Verbose($"        Expected: '{condition.Expected}'");
                _logger?.Verbose($"        Actual:   '{actualValue}'");

                if (actualValue != condition.Expected)
                {
                    _logger?.Verbose($"      ✗ Condition failed: {condition.Path}");
                    return false;
                }
                
                _logger?.Verbose($"      ✓ Condition passed");
            }

            _logger?.Verbose($"      ✓ All conditions passed");
            return true;
        }

        /// <summary>
        /// Validate a single resource against a RuleSet
        /// </summary>
        private void ValidateResource(JObject resource, RuleSet ruleSet, ValidationResult result)
        {
            _logger?.Debug($"    Validating {ruleSet.Rules.Count} rule(s)...");
            foreach (var rule in ruleSet.Rules)
            {
                // Normalize the path by removing resource type prefix if present
                var normalizedRule = NormalizeRulePath(rule, ruleSet.Scope);
                RuleEvaluator.ApplyRule(resource, normalizedRule, ruleSet.Scope, _metadata.CodesMaster, result, _logger);
            }
        }

        /// <summary>
        /// Normalize rule path by removing resource type prefix
        /// E.g., "Patient.identifier" -> "identifier"
        /// E.g., "Observation.component[*]" -> "component[*]"
        /// </summary>
        private RuleDefinition NormalizeRulePath(RuleDefinition rule, string scope)
        {
            // Extract base resource type from scope (before any dot)
            var resourceType = scope.Split('.')[0];

            // Create a copy of the rule with normalized path
            var normalizedRule = new RuleDefinition
            {
                RuleType = rule.RuleType,
                PathType = rule.PathType,
                Path = rule.Path,
                ExpectedValue = rule.ExpectedValue,
                ExpectedSystem = rule.ExpectedSystem,
                ExpectedCode = rule.ExpectedCode,
                ExpectedType = rule.ExpectedType,      // ADD: Type validation
                Pattern = rule.Pattern,                 // ADD: Regex validation
                ErrorCode = rule.ErrorCode,
                Message = rule.Message
            };

            // Strip resource type prefix if present
            if (normalizedRule.Path.StartsWith(resourceType + "."))
            {
                normalizedRule.Path = normalizedRule.Path.Substring(resourceType.Length + 1);
            }

            return normalizedRule;
        }
    }
}
