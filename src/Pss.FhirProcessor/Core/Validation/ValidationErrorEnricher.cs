using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Metadata;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Path;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation
{
    /// <summary>
    /// Enriches validation errors with metadata for frontend helper system
    /// </summary>
    public class ValidationErrorEnricher
    {
        private readonly ValidationMetadata _metadata;

        public ValidationErrorEnricher(ValidationMetadata metadata)
        {
            _metadata = metadata;
        }

        /// <summary>
        /// Enrich a validation error with rule metadata and context
        /// </summary>
        public void EnrichError(
            ValidationError error,
            RuleDefinition rule,
            JObject bundleRoot = null)
        {
            if (error == null)
                return;

            // Resolve filter notation to indices in fieldPath for frontend tree navigation
            // Example: "extension[url:https://...].valueCodeableConcept.coding[0].system"
            //       → "extension[1].valueCodeableConcept.coding[0].system"
            if (bundleRoot != null && !string.IsNullOrEmpty(error.FieldPath) && error.ResourcePointer?.EntryIndex != null)
            {
                var entries = bundleRoot["entry"] as JArray;
                if (entries != null && error.ResourcePointer.EntryIndex < entries.Count)
                {
                    var entry = entries[error.ResourcePointer.EntryIndex.Value];
                    var resource = entry["resource"] as JObject;
                    if (resource != null)
                    {
                        error.FieldPath = CpsPathResolver.ResolveFiltersToIndices(resource, error.FieldPath);
                    }
                }
            }

            // Set rule type
            if (rule != null)
            {
                error.RuleType = rule.RuleType;
                error.Rule = ExtractRuleMetadata(rule);
            }
            else
            {
                // Try to find rule by error code and path
                rule = FindRule(error.Scope, error.Code, error.FieldPath);
                if (rule != null)
                {
                    error.RuleType = rule.RuleType;
                    error.Rule = ExtractRuleMetadata(rule);
                }
            }

            // Enrich with context
            error.Context = BuildContext(error.Scope, error.FieldPath, rule, bundleRoot);
            
            // Complete resource pointer if entry index is already set
            if (error.ResourcePointer != null && error.ResourcePointer.EntryIndex.HasValue)
            {
                CompleteResourcePointer(error.ResourcePointer, bundleRoot);
            }
            else
            {
                // Build resource pointer from scratch
                error.ResourcePointer = BuildResourcePointer(error.Scope, error.FieldPath, bundleRoot);
            }
        }

        /// <summary>
        /// Find the matching rule from metadata
        /// </summary>
        private RuleDefinition FindRule(string scope, string errorCode, string fieldPath)
        {
            if (_metadata?.RuleSets == null || string.IsNullOrEmpty(scope))
                return null;

            var ruleSet = _metadata.RuleSets.FirstOrDefault(rs => rs.Scope == scope);
            if (ruleSet?.Rules == null)
                return null;

            // Try to find by ErrorCode and Path
            var rule = ruleSet.Rules.FirstOrDefault(r => 
                r.ErrorCode == errorCode && 
                !string.IsNullOrEmpty(r.Path) && 
                (r.Path == fieldPath || fieldPath.Contains(r.Path) || PathsMatch(r.Path, fieldPath)));

            // Fallback: find by ErrorCode only
            if (rule == null)
            {
                rule = ruleSet.Rules.FirstOrDefault(r => r.ErrorCode == errorCode);
            }

            return rule;
        }

        /// <summary>
        /// Check if two CPS1 paths match (accounting for array indices)
        /// </summary>
        private bool PathsMatch(string rulePath, string errorPath)
        {
            if (string.IsNullOrEmpty(rulePath) || string.IsNullOrEmpty(errorPath))
                return false;

            // Remove array indices for comparison
            var rulePathNormalized = Regex.Replace(rulePath, @"\[\d+\]", "[]");
            var errorPathNormalized = Regex.Replace(errorPath, @"\[\d+\]", "[]");

            return rulePathNormalized == errorPathNormalized;
        }

        /// <summary>
        /// Extract rule properties into metadata structure
        /// </summary>
        private ValidationRuleMetadata ExtractRuleMetadata(RuleDefinition rule)
        {
            if (rule == null)
                return new ValidationRuleMetadata();

            return new ValidationRuleMetadata
            {
                Path = rule.Path,
                ExpectedType = rule.ExpectedType,
                ExpectedValue = rule.ExpectedValue,
                Pattern = rule.Pattern,
                TargetTypes = rule.TargetTypes,
                System = rule.System,
                AllowedValues = rule.AllowedValues
            };
        }

        /// <summary>
        /// Build context information for the error
        /// </summary>
        private ValidationErrorContext BuildContext(
            string scope,
            string fieldPath,
            RuleDefinition rule,
            JObject bundleRoot)
        {
            var context = new ValidationErrorContext();

            // Get resource type from scope definition
            if (_metadata?.RuleSets != null)
            {
                var ruleSet = _metadata.RuleSets.FirstOrDefault(rs => rs.Scope == scope);
                if (ruleSet?.ScopeDefinition != null)
                {
                    context.ResourceType = ruleSet.ScopeDefinition.ResourceType;
                    
                    // Extract screening type from Match conditions
                    var matchCondition = ruleSet.ScopeDefinition.Match?.FirstOrDefault();
                    if (matchCondition != null && !string.IsNullOrEmpty(matchCondition.Expected))
                    {
                        context.ScreeningType = matchCondition.Expected;
                    }
                }
            }

            // Fallback: infer screening type from scope name
            if (string.IsNullOrEmpty(context.ScreeningType))
            {
                if (scope == "HS" || scope == "OS" || scope == "VS" || 
                    scope == "Observation.HS" || scope == "Observation.OS" || scope == "Observation.VS")
                {
                    context.ScreeningType = scope.Replace("Observation.", "");
                }
            }

            // For CodesMaster validation, resolve question metadata
            if (rule?.RuleType == "CodesMaster")
            {
                ResolveCodesMasterContext(context, scope, fieldPath, bundleRoot);
            }

            // For CodeSystem validation, resolve concepts
            if (rule?.RuleType == "CodeSystem" && !string.IsNullOrEmpty(rule.System))
            {
                ResolveCodeSystemContext(context, rule.System);
            }

            return context;
        }

        /// <summary>
        /// Resolve CodesMaster question metadata and allowed answers
        /// </summary>
        private void ResolveCodesMasterContext(
            ValidationErrorContext context,
            string scope,
            string fieldPath,
            JObject bundleRoot)
        {
            if (_metadata?.CodesMaster?.Questions == null)
                return;

            // Try to extract question code from fieldPath
            // fieldPath example: "entry[2].resource.component[1].code.coding[0].code"
            string questionCode = ExtractQuestionCodeFromPath(fieldPath, bundleRoot);

            if (!string.IsNullOrEmpty(questionCode))
            {
                var question = _metadata.CodesMaster.Questions.FirstOrDefault(q => 
                    q.QuestionCode == questionCode);

                if (question != null)
                {
                    context.QuestionCode = question.QuestionCode;
                    context.QuestionDisplay = question.QuestionDisplay;
                    context.AllowedAnswers = question.AllowedAnswers;
                }
            }
        }

        /// <summary>
        /// Resolve CodeSystem concepts for validation
        /// </summary>
        private void ResolveCodeSystemContext(ValidationErrorContext context, string system)
        {
            if (_metadata?.CodesMaster?.CodeSystems == null)
                return;

            var codeSystem = _metadata.CodesMaster.CodeSystems.FirstOrDefault(cs => 
                cs.System == system);

            if (codeSystem?.Concepts != null)
            {
                context.CodeSystemConcepts = codeSystem.Concepts.Select(c => 
                    new CodeSystemConcept
                    {
                        Code = c.Code,
                        Display = c.Display
                    }).ToList();
            }
        }

        /// <summary>
        /// Extract question code from path by navigating the bundle
        /// </summary>
        private string ExtractQuestionCodeFromPath(string path, JObject bundleRoot)
        {
            if (string.IsNullOrEmpty(path) || bundleRoot == null)
                return null;

            try
            {
                // Parse path to navigate to the component
                // Example: "entry[2].resource.component[1].code.coding[0].code"
                // We want to get the code value from component[X].code.coding[0].code
                
                // Find the component part
                var componentMatch = Regex.Match(path, @"component\[(\d+)\]");
                if (!componentMatch.Success)
                    return null;

                // Extract entry index
                var entryMatch = Regex.Match(path, @"entry\[(\d+)\]");
                if (!entryMatch.Success)
                    return null;

                int entryIndex = int.Parse(entryMatch.Groups[1].Value);
                int componentIndex = int.Parse(componentMatch.Groups[1].Value);

                // Navigate to the component
                var entries = bundleRoot["entry"] as JArray;
                if (entries == null || entryIndex >= entries.Count)
                    return null;

                var resource = entries[entryIndex]["resource"] as JObject;
                if (resource == null)
                    return null;

                var components = resource["component"] as JArray;
                if (components == null || componentIndex >= components.Count)
                    return null;

                var component = components[componentIndex] as JObject;
                if (component == null)
                    return null;

                // Get the code
                var code = component["code"]?["coding"]?[0]?["code"]?.ToString();
                return code;
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Complete a resource pointer that already has an entry index
        /// </summary>
        private void CompleteResourcePointer(ResourcePointer pointer, JObject bundleRoot)
        {
            if (pointer == null || bundleRoot == null || !pointer.EntryIndex.HasValue)
                return;

            try
            {
                var entries = bundleRoot["entry"] as JArray;
                if (entries != null && pointer.EntryIndex.Value >= 0 && pointer.EntryIndex.Value < entries.Count)
                {
                    var entry = entries[pointer.EntryIndex.Value];
                    pointer.FullUrl = entry["fullUrl"]?.ToString();
                    
                    var resource = entry["resource"];
                    if (resource != null)
                    {
                        pointer.ResourceType = resource["resourceType"]?.ToString();
                        pointer.ResourceId = resource["id"]?.ToString();
                    }
                }
            }
            catch
            {
                // Ignore errors in pointer resolution
            }
        }
        
        /// <summary>
        /// Build resource pointer for navigation
        /// </summary>
        private ResourcePointer BuildResourcePointer(string scope, string fieldPath, JObject bundleRoot)
        {
            var pointer = new ResourcePointer();
            
            if (bundleRoot == null)
                return pointer;

            try
            {
                // First try to extract entry index from fieldPath if present
                // Example: "entry[2].resource.identifier.value"
                var entryMatch = Regex.Match(fieldPath ?? "", @"entry\[(\d+)\]");
                if (entryMatch.Success)
                {
                    pointer.EntryIndex = int.Parse(entryMatch.Groups[1].Value);
                }

                // Get entries array
                var entries = bundleRoot["entry"] as JArray;
                if (entries == null)
                    return pointer;

                // If we have an entry index, populate the rest of the pointer
                if (pointer.EntryIndex.HasValue && pointer.EntryIndex.Value >= 0 && pointer.EntryIndex.Value < entries.Count)
                {
                    var entry = entries[pointer.EntryIndex.Value];
                    pointer.FullUrl = entry["fullUrl"]?.ToString();
                    
                    var resource = entry["resource"];
                    if (resource != null)
                    {
                        pointer.ResourceType = resource["resourceType"]?.ToString();
                        pointer.ResourceId = resource["id"]?.ToString();
                    }
                }
                else if (!pointer.EntryIndex.HasValue && !string.IsNullOrEmpty(scope))
                {
                    // Fallback: try to infer from scope
                    pointer.EntryIndex = FindEntryIndexByScope(scope, entries);
                    
                    if (pointer.EntryIndex.HasValue && pointer.EntryIndex.Value < entries.Count)
                    {
                        var entry = entries[pointer.EntryIndex.Value];
                        pointer.FullUrl = entry["fullUrl"]?.ToString();
                        
                        var resource = entry["resource"];
                        if (resource != null)
                        {
                            pointer.ResourceType = resource["resourceType"]?.ToString();
                            pointer.ResourceId = resource["id"]?.ToString();
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors in pointer resolution
            }

            return pointer;
        }

        /// <summary>
        /// Find entry index by scope (resource type)
        /// </summary>
        private int? FindEntryIndexByScope(string scope, JArray entries)
        {
            if (entries == null)
                return null;

            // Map scope to resource type
            var resourceType = MapScopeToResourceType(scope);
            if (string.IsNullOrEmpty(resourceType))
                return null;

            // Find first matching entry
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var resource = entry["resource"];
                if (resource != null)
                {
                    var rt = resource["resourceType"]?.ToString();
                    if (rt == resourceType)
                        return i;
                    
                    // Special handling for Observations with screening types
                    if (rt == "Observation" && (scope == "HS" || scope == "OS" || scope == "VS"))
                    {
                        var code = resource["code"]?["coding"]?[0]?["code"]?.ToString();
                        if (code == scope)
                            return i;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Map scope name to FHIR resource type
        /// </summary>
        private string MapScopeToResourceType(string scope)
        {
            return scope switch
            {
                "Patient" => "Patient",
                "Encounter" => "Encounter",
                "Location" => "Location",
                "Organization" => "Organization",
                "HealthcareService" => "HealthcareService",
                "HS" => "Observation",
                "OS" => "Observation",
                "VS" => "Observation",
                _ => scope
            };
        }
    }
}
