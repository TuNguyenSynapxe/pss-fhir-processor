using System;
using System.Collections.Generic;
using System.Linq;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Codes;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Fhir;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Utilities;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Validation
{
    /// <summary>
    /// Core validation engine implementing metadata-driven validation logic
    /// </summary>
    public class ValidationEngine
    {
        private Dictionary<string, RuleSet> _ruleSets;
        private CodesMasterMetadata _codesMaster;
        private ValidationOptions _options;
        private Logger _logger;

        public ValidationEngine()
        {
            _ruleSets = new Dictionary<string, RuleSet>();
            _options = new ValidationOptions();
        }

        public void LoadRuleSets(Dictionary<string, string> jsonByScope)
        {
            _ruleSets = new Dictionary<string, RuleSet>();
            _logger?.Info($"Loading {jsonByScope.Count} RuleSets");

            foreach (var kvp in jsonByScope)
            {
                var ruleSet = JsonHelper.Deserialize<RuleSet>(kvp.Value);
                if (ruleSet != null)
                {
                    _ruleSets[kvp.Key] = ruleSet;
                    _logger?.Info($"Loaded RuleSet '{kvp.Key}' with {ruleSet.Rules?.Count ?? 0} rules");
                }
                else
                {
                    _logger?.Info($"Failed to deserialize RuleSet '{kvp.Key}'");
                }
            }
        }

        public void LoadCodesMaster(string json)
        {
            try
            {
                _codesMaster = JsonHelper.Deserialize<CodesMasterMetadata>(json);
                _logger?.Info($"Loaded CodesMaster with {_codesMaster?.Questions?.Count ?? 0} questions");
            }
            catch (Exception ex)
            {
                // Invalid JSON - leave _codesMaster as null
                _logger?.Error($"Failed to load CodesMaster: {ex.Message}");
                _codesMaster = null;
            }
        }

        public void SetOptions(ValidationOptions options)
        {
            _options = options ?? new ValidationOptions();
        }

        public void SetLogger(Logger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Main validation entry point
        /// </summary>
        public ValidationResult Validate(Bundle bundle)
        {
            _logger?.Info("Validation started");

            var result = new ValidationResult { IsValid = true };

            // 1. Structural validation
            if (bundle == null || bundle.Entry == null)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    Code = "INVALID_BUNDLE_STRUCTURE",
                    FieldPath = "Bundle",
                    Message = "Bundle or Entry is null",
                    Scope = "Bundle"
                });
                return result;
            }

            // 2. Index resources
            var resourceIndex = IndexResources(bundle);
            _logger?.Info($"Indexed {resourceIndex.Count} resource types");
            foreach (var kvp in resourceIndex)
            {
                _logger?.Info($"  {kvp.Key}: {kvp.Value.Count} resource(s)");
            }

            // 3. Validate required resources
            ValidateRequiredResources(resourceIndex, result);

            // 4. Validate screening types (HS/OS/VS)
            ValidateScreeningTypes(resourceIndex, result);

            // 5. Apply RuleSets
            ApplyRuleSets(bundle, resourceIndex, result);

            // 6. Validate CodesMaster
            ValidateCodesMaster(resourceIndex, result);

            result.IsValid = result.Errors.Count == 0;

            _logger?.Info($"Validation completed: {(result.IsValid ? "VALID" : "INVALID")} ({result.Errors.Count} errors)");

            return result;
        }

        private Dictionary<string, List<Resource>> IndexResources(Bundle bundle)
        {
            _logger?.Info("  → Indexing bundle resources...");
            var index = new Dictionary<string, List<Resource>>();
            int entryIndex = 0;

            foreach (var entry in bundle.Entry)
            {
                entryIndex++;
                if (entry.Resource == null)
                {
                    _logger?.Debug($"    Entry[{entryIndex-1}]: null resource, skipping");
                    continue;
                }

                var resourceType = entry.Resource.ResourceType;
                _logger?.Debug($"    Entry[{entryIndex-1}]: {resourceType}");

                // Special handling for Observation to distinguish HS/OS/VS
                if (resourceType == "Observation")
                {
                    // Need to deserialize to get the Code property
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(entry.Resource);
                    var obs = Newtonsoft.Json.JsonConvert.DeserializeObject<Observation>(json);
                    var screeningType = obs?.Code?.Coding?.FirstOrDefault()?.Code;
                    if (!string.IsNullOrEmpty(screeningType))
                    {
                        var key = $"Observation:{screeningType}";
                        if (!index.ContainsKey(key))
                            index[key] = new List<Resource>();
                        index[key].Add(entry.Resource);
                    }
                }

                if (!index.ContainsKey(resourceType))
                    index[resourceType] = new List<Resource>();

                index[resourceType].Add(entry.Resource);
            }

            return index;
        }

        private void ValidateRequiredResources(Dictionary<string, List<Resource>> index, ValidationResult result)
        {
            _logger?.Info("  → Validating required resources...");
            var requiredTypes = new[] { "Patient", "Encounter", "Location", "HealthcareService", "Organization" };

            foreach (var required in requiredTypes)
            {
                if (!index.ContainsKey(required) || index[required].Count == 0)
                {
                    result.Errors.Add(new ValidationError
                    {
                        Code = "RESOURCE_MISSING",
                        FieldPath = "Bundle",
                        Message = $"Required resource '{required}' is missing",
                        Scope = "Bundle"
                    });
                }
            }
        }

        private void ValidateScreeningTypes(Dictionary<string, List<Resource>> index, ValidationResult result)
        {
            _logger?.Info("  → Validating screening types (HS, OS, VS)...");
            var screeningTypes = new[] { "HS", "OS", "VS" };

            foreach (var screeningType in screeningTypes)
            {
                var key = $"Observation:{screeningType}";
                if (!index.ContainsKey(key) || index[key].Count == 0)
                {
                    result.Errors.Add(new ValidationError
                    {
                        Code = "MISSING_SCREENING_TYPE",
                        FieldPath = "Bundle",
                        Message = $"Missing screening type: {screeningType}",
                        Scope = "Bundle"
                    });
                }
            }
        }

        private void ApplyRuleSets(Bundle bundle, Dictionary<string, List<Resource>> index, ValidationResult result)
        {
            _logger?.Info($"Applying {_ruleSets.Count} RuleSet(s)");
            foreach (var ruleSetKvp in _ruleSets)
            {
                var scope = ruleSetKvp.Key;
                var ruleSet = ruleSetKvp.Value;

                _logger?.Info($"  RuleSet '{scope}': {ruleSet?.Rules?.Count ?? 0} rule(s)");

                if (ruleSet?.Rules == null || ruleSet.Rules.Count == 0)
                {
                    _logger?.Info($"  RuleSet '{scope}' has no rules to apply");
                    continue;
                }

                foreach (var rule in ruleSet.Rules)
                {
                    EvaluateRule(bundle, index, rule, scope, result);
                }
            }
        }

        private void EvaluateRule(Bundle bundle, Dictionary<string, List<Resource>> index, ValidationRule rule, string scope, ValidationResult result)
        {
            if (rule == null || string.IsNullOrEmpty(rule.RuleType))
            {
                _logger?.Info($"  Rule is null or has no RuleType");
                return;
            }

            _logger?.Info($"  Evaluating {rule.RuleType} rule at '{rule.Path}'");

            switch (rule.RuleType)
            {
                case "Required":
                    EvaluateRequiredRule(index, rule, scope, result);
                    break;
                case "FixedValue":
                    EvaluateFixedValueRule(index, rule, scope, result);
                    break;
                case "FixedCoding":
                    EvaluateFixedCodingRule(index, rule, scope, result);
                    break;
                case "AllowedValues":
                    EvaluateAllowedValuesRule(index, rule, scope, result);
                    break;
                case "CodesMaster":
                    // Handled separately in ValidateCodesMaster
                    break;
                case "Conditional":
                    EvaluateConditionalRule(index, rule, scope, result);
                    break;
            }
        }

        private void EvaluateRequiredRule(Dictionary<string, List<Resource>> index, ValidationRule rule, string scope, ValidationResult result)
        {
            _logger?.Info($"    Evaluating Required rule: path='{rule.Path}'");
            
            // Special handling for Bundle.entry[ResourceType] syntax
            // This checks if a resource type exists in the bundle
            if (scope == "Bundle" && rule.Path.StartsWith("Bundle.entry[") && rule.Path.EndsWith("]"))
            {
                var startIdx = "Bundle.entry[".Length;
                var endIdx = rule.Path.Length - 1;
                var resourceTypeKey = rule.Path.Substring(startIdx, endIdx - startIdx);
                
                _logger?.Info($"      Checking for resource type '{resourceTypeKey}' in bundle index");
                
                // Map Organization:Provider and Organization:Cluster to Organization
                var lookupKey = resourceTypeKey;
                if (resourceTypeKey == "Organization:Provider" || resourceTypeKey == "Organization:Cluster")
                {
                    lookupKey = "Organization";
                    _logger?.Info($"      Mapped '{resourceTypeKey}' -> '{lookupKey}'");
                }
                
                if (!index.ContainsKey(lookupKey) || index[lookupKey].Count == 0)
                {
                    _logger?.Info($"      → FAILED: Resource type '{resourceTypeKey}' not found in bundle");
                    result.Errors.Add(new ValidationError
                    {
                        Code = "MANDATORY_MISSING",
                        FieldPath = rule.Path,
                        Message = $"Required resource '{resourceTypeKey}' is missing from bundle",
                        Scope = scope
                    });
                }
                else
                {
                    _logger?.Info($"      → PASSED: Found {index[lookupKey].Count} resource(s) of type '{resourceTypeKey}'");
                }
                return;
            }
            
            // Get the target resource based on scope
            var resource = GetResourceByScope(index, scope);
            if (resource == null)
            {
                _logger?.Info($"      No resource found for scope '{scope}'");
                result.Errors.Add(new ValidationError
                {
                    Code = "MANDATORY_MISSING",
                    FieldPath = rule.Path,
                    Message = $"Required field '{rule.Path}' is missing or empty",
                    Scope = scope
                });
                return;
            }
            
            _logger?.Info($"      Resource type: {resource.GetType().Name}, ResourceType property: {resource.ResourceType}");
            
            // Strip resource type prefix from path before resolving
            var pathToResolve = StripResourcePrefix(rule.Path, scope);
            if (pathToResolve != rule.Path)
            {
                _logger?.Info($"      Stripped path: '{rule.Path}' -> '{pathToResolve}'");
            }
            
            // If path is empty after stripping (was just the resource name), treat as existence check
            if (string.IsNullOrEmpty(pathToResolve))
            {
                _logger?.Info($"      Path is just resource name - treating as existence check → PASSED");
                return;
            }
            
            // Debug: log ExtensionData
            if (resource.ExtensionData != null)
            {
                _logger?.Info($"      ExtensionData keys: {string.Join(", ", resource.ExtensionData.Keys)}");
                
                // Debug: log the type of each ExtensionData value
                foreach (var key in resource.ExtensionData.Keys)
                {
                    var val = resource.ExtensionData[key];
                    _logger?.Info($"        {key}: {val?.GetType()?.Name ?? "null"}");
                }
            }
            
            var value = PathResolver.ResolvePath(resource, pathToResolve);
            _logger?.Info($"      Resolved value: {(value == null ? "null" : $"'{value}' (type: {value.GetType().Name})" )}");

            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                _logger?.Info($"      → FAILED: Value is missing or empty");
                result.Errors.Add(new ValidationError
                {
                    Code = "MANDATORY_MISSING",
                    FieldPath = rule.Path,
                    Message = $"Required field '{rule.Path}' is missing or empty",
                    Scope = scope
                });
            }
            else
            {
                _logger?.Info($"      → PASSED");
            }
        }

        private void EvaluateFixedValueRule(Dictionary<string, List<Resource>> index, ValidationRule rule, string scope, ValidationResult result)
        {
            var resource = GetResourceByScope(index, scope);
            if (resource == null) return;
            
            var pathToResolve = StripResourcePrefix(rule.Path, scope);
            var value = PathResolver.ResolvePath(resource, pathToResolve);

            if (value != null && value.ToString() != rule.FixedValue)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "FIXED_VALUE_MISMATCH",
                    FieldPath = rule.Path,
                    Message = $"Expected '{rule.FixedValue}' but found '{value}'",
                    Scope = scope
                });
            }
        }

        private void EvaluateFixedCodingRule(Dictionary<string, List<Resource>> index, ValidationRule rule, string scope, ValidationResult result)
        {
            var resource = GetResourceByScope(index, scope);
            if (resource == null) return;
            
            var pathToResolve = StripResourcePrefix(rule.Path, scope);
            var value = PathResolver.ResolvePath(resource, pathToResolve);
            _logger?.Info($"    FixedCoding path '{rule.Path}' resolved to type: {value?.GetType().Name ?? "null"}");

            Coding coding = null;

            // Handle different value types
            if (value is Coding directCoding)
            {
                coding = directCoding;
            }
            else if (value is Newtonsoft.Json.Linq.JObject jobj)
            {
                // Deserialize JObject to Coding
                coding = jobj.ToObject<Coding>();
            }
            else if (value != null)
            {
                // Try to serialize and deserialize
                try
                {
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(value);
                    coding = Newtonsoft.Json.JsonConvert.DeserializeObject<Coding>(json);
                }
                catch
                {
                    _logger?.Info($"    Failed to convert value to Coding");
                }
            }

            if (coding != null)
            {
                if (!string.IsNullOrEmpty(rule.FixedSystem) && coding.System != rule.FixedSystem)
                {
                    result.Errors.Add(new ValidationError
                    {
                        Code = "FIXED_CODING_MISMATCH",
                        FieldPath = rule.Path + ".system",
                        Message = $"Expected system '{rule.FixedSystem}' but found '{coding.System}'",
                        Scope = scope
                    });
                }

                if (!string.IsNullOrEmpty(rule.FixedCode) && coding.Code != rule.FixedCode)
                {
                    result.Errors.Add(new ValidationError
                    {
                        Code = "FIXED_CODING_MISMATCH",
                        FieldPath = rule.Path + ".code",
                        Message = $"Expected code '{rule.FixedCode}' but found '{coding.Code}'",
                        Scope = scope
                    });
                }
            }
        }

        private void EvaluateAllowedValuesRule(Dictionary<string, List<Resource>> index, ValidationRule rule, string scope, ValidationResult result)
        {
            var resource = GetResourceByScope(index, scope);
            if (resource == null) return;
            
            var pathToResolve = StripResourcePrefix(rule.Path, scope);
            var value = PathResolver.ResolvePath(resource, pathToResolve);

            if (value != null && rule.AllowedValues != null && rule.AllowedValues.Count > 0)
            {
                var strValue = value.ToString();
                if (!rule.AllowedValues.Contains(strValue))
                {
                    result.Errors.Add(new ValidationError
                    {
                        Code = "INVALID_ANSWER_VALUE",
                        FieldPath = rule.Path,
                        Message = $"Value '{strValue}' is not in allowed values",
                        Scope = scope
                    });
                }
            }
        }

        private void EvaluateConditionalRule(Dictionary<string, List<Resource>> index, ValidationRule rule, string scope, ValidationResult result)
        {
            // Conditional rule supports two modes:
            // 1. JSONPath mode: If/Then contain paths like "Entry[0].Resource.Status"
            // 2. Component mode: If/Then contain question codes like "SQ-L2H9-00000001"
            
            if (string.IsNullOrEmpty(rule.If) || string.IsNullOrEmpty(rule.Then))
            {
                _logger?.Info($"Conditional rule missing If or Then: If={rule.If}, Then={rule.Then}");
                return;
            }

            // Detect mode: if 'If' contains '[' or '.', use JSONPath mode, otherwise component mode
            bool isComponentMode = !rule.If.Contains("[") && !rule.If.Contains(".");

            if (isComponentMode)
            {
                EvaluateComponentConditionalRule(index, rule, scope, result);
            }
            else
            {
                EvaluatePathConditionalRule(index, rule, scope, result);
            }
        }

        private void EvaluatePathConditionalRule(Dictionary<string, List<Resource>> index, ValidationRule rule, string scope, ValidationResult result)
        {
            // JSONPath-based conditional: if path 'If' exists, then path 'Then' must exist
            var resource = GetResourceByScope(index, scope);
            if (resource == null) return;
            
            var ifPathToResolve = StripResourcePrefix(rule.If, scope);
            var ifValue = PathResolver.ResolvePath(resource, ifPathToResolve);
            
            if (ifValue != null)
            {
                var thenPathToResolve = StripResourcePrefix(rule.Then, scope);
                var thenValue = PathResolver.ResolvePath(resource, thenPathToResolve);
                if (thenValue == null)
                {
                    result.Errors.Add(new ValidationError
                    {
                        Code = "CONDITIONAL_FAILED",
                        FieldPath = rule.Then,
                        Message = $"Conditional rule failed: if '{rule.If}' exists, then '{rule.Then}' must also exist",
                        Scope = scope
                    });
                }
            }
        }

        private void EvaluateComponentConditionalRule(Dictionary<string, List<Resource>> index, ValidationRule rule, string scope, ValidationResult result)
        {
            // Component-based conditional: if question code 'If' has value 'WhenValue', then question code 'Then' must exist
            // Get observations for this scope
            string observationKey = $"Observation:{scope}";
            if (!index.ContainsKey(observationKey))
            {
                return;
            }

            var observations = index[observationKey];

            foreach (var resource in observations)
            {
                // Check if this observation matches the scope
                var obsJson = Newtonsoft.Json.JsonConvert.SerializeObject(resource);
                var observation = Newtonsoft.Json.JsonConvert.DeserializeObject<Observation>(obsJson);
                
                var obsCode = observation?.Code?.Coding?.FirstOrDefault()?.Code;
                if (obsCode != scope)
                {
                    continue;
                }

                if (observation?.Component == null || observation.Component.Count == 0)
                {
                    continue;
                }

                // Find the "If" component
                var ifComponent = observation.Component.FirstOrDefault(c => 
                    c.Code?.Coding?.FirstOrDefault()?.Code == rule.If);

                if (ifComponent != null)
                {
                    // Check if WhenValue matches (if specified)
                    bool conditionMet = true;
                    if (!string.IsNullOrEmpty(rule.WhenValue))
                    {
                        conditionMet = ifComponent.ValueString == rule.WhenValue;
                    }

                    if (conditionMet)
                    {
                        // Check if "Then" component exists
                        var thenComponent = observation.Component.FirstOrDefault(c =>
                            c.Code?.Coding?.FirstOrDefault()?.Code == rule.Then);

                        if (thenComponent == null)
                        {
                            var conditionMsg = !string.IsNullOrEmpty(rule.WhenValue)
                                ? $"if '{rule.If}' = '{rule.WhenValue}'"
                                : $"if '{rule.If}' exists";

                            result.Errors.Add(new ValidationError
                            {
                                Code = "CONDITIONAL_FAILED",
                                FieldPath = $"Observation.component",
                                Message = $"Conditional rule failed: {conditionMsg}, then '{rule.Then}' must also exist",
                                Scope = scope
                            });
                        }
                    }
                }
            }
        }

        private void ValidateCodesMaster(Dictionary<string, List<Resource>> index, ValidationResult result)
        {
            if (_codesMaster == null || _codesMaster.Questions == null)
            {
                _logger?.Info("CodesMaster not loaded, skipping CodesMaster validation");
                return;
            }

            _logger?.Info($"Validating CodesMaster with {_codesMaster.Questions.Count} questions");

            var screeningTypes = new[] { "HS", "OS", "VS" };

            foreach (var screeningType in screeningTypes)
            {
                var key = $"Observation:{screeningType}";
                if (!index.ContainsKey(key))
                {
                    _logger?.Info($"  No observations found for {screeningType}");
                    continue;
                }

                _logger?.Info($"  Validating {screeningType}: {index[key].Count} observation(s)");
                foreach (var resource in index[key])
                {
                    // Deserialize Resource to Observation to access Component property
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(resource);
                    var observation = Newtonsoft.Json.JsonConvert.DeserializeObject<Observation>(json);
                    
                    _logger?.Info($"    Observation deserialized, Component count: {observation?.Component?.Count ?? 0}");
                    
                    if (observation?.Component == null)
                        continue;

                    foreach (var component in observation.Component)
                    {
                        ValidateObservationComponent(component, screeningType, result);
                    }
                }
            }
        }

        private void ValidateObservationComponent(ObservationComponent component, string screeningType, ValidationResult result)
        {
            var questionCode = component.Code?.Coding?.FirstOrDefault()?.Code;
            var questionDisplay = component.Code?.Coding?.FirstOrDefault()?.Display;
            var answer = component.ValueString;

            _logger?.Debug($"      Validating component: code={questionCode}, display={questionDisplay}, answer={answer}");

            if (string.IsNullOrEmpty(questionCode))
            {
                _logger?.Debug("      Skipping component with empty question code");
                return;
            }

            // Find in Codes Master
            var metadata = _codesMaster.Questions.FirstOrDefault(q => q.QuestionCode == questionCode);

            if (metadata == null)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "UNKNOWN_QUESTION_CODE",
                    FieldPath = $"Observation.component.code",
                    Message = $"Question code '{questionCode}' not found in Codes Master",
                    Scope = screeningType
                });
                return;
            }

            // Validate screening type
            if (metadata.ScreeningType != screeningType)
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "INVALID_SCREENING_TYPE_FOR_QUESTION",
                    FieldPath = $"Observation.component.code",
                    Message = $"Question '{questionCode}' belongs to {metadata.ScreeningType} but found in {screeningType}",
                    Scope = screeningType
                });
            }

            // Validate display - always required
            if (string.IsNullOrEmpty(questionDisplay))
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "QUESTION_DISPLAY_MISSING",
                    FieldPath = $"Observation.component.code.display",
                    Message = $"Display is required for question code '{questionCode}'",
                    Scope = screeningType
                });
            }
            else if (!DisplayMatches(questionDisplay, metadata.QuestionDisplay))
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "QUESTION_DISPLAY_MISMATCH",
                    FieldPath = $"Observation.component.code.display",
                    Message = $"Display '{questionDisplay}' does not match expected '{metadata.QuestionDisplay}'",
                    Scope = screeningType
                });
            }

            // Validate answer
            if (!string.IsNullOrEmpty(answer) && metadata.AllowedAnswers != null && metadata.AllowedAnswers.Count > 0)
            {
                // Check for multi-value (PureTone)
                if (answer.Contains("|"))
                {
                    var values = answer.Split('|').Select(v => v.Trim()).ToList();
                    foreach (var val in values)
                    {
                        if (!metadata.AllowedAnswers.Contains(val))
                        {
                            result.Errors.Add(new ValidationError
                            {
                                Code = "INVALID_MULTI_VALUE",
                                FieldPath = $"Observation.component.valueString",
                                Message = $"Multi-value '{val}' is not in allowed answers for question '{questionCode}'",
                                Scope = screeningType
                            });
                        }
                    }
                }
                else
                {
                    if (!metadata.AllowedAnswers.Contains(answer))
                    {
                        result.Errors.Add(new ValidationError
                        {
                            Code = "INVALID_ANSWER_VALUE",
                            FieldPath = $"Observation.component.valueString",
                            Message = $"Answer '{answer}' is not in allowed answers for question '{questionCode}'",
                            Scope = screeningType
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Get the first resource from index based on scope name
        /// Scope can be: Event, Participant, HS, OS, VS, or any custom scope
        /// Maps to resource types: Encounter, Patient, Observation:HS, Observation:OS, Observation:VS
        /// For custom scopes, uses the scope name directly as the resource key
        /// </summary>
        private Resource GetResourceByScope(Dictionary<string, List<Resource>> index, string scope)
        {
            _logger?.Debug($"      GetResourceByScope: scope='{scope}'");
            
            // Support both formats:
            // 1. Logical scope names: "Event", "Participant", "HS", "OS", "VS"
            // 2. Direct resource type names: "Patient", "Encounter", "Observation.HS", etc.
            
            string resourceKey = scope switch
            {
                // Legacy logical scope mappings
                "Event" => "Encounter",
                "Participant" => "Patient",
                "HS" => "Observation:HS",
                "OS" => "Observation:OS",
                "VS" => "Observation:VS",
                
                // Direct resource type names (source-driven rules)
                "Observation.HS" => "Observation:HS",
                "Observation.OS" => "Observation:OS",
                "Observation.VS" => "Observation:VS",
                "Organization.Provider" => "Organization",
                "Organization.Cluster" => "Organization",
                
                _ => scope // For other scopes (Patient, Encounter, Location, etc.), use scope name as-is
            };

            _logger?.Debug($"      Mapped to resourceKey='{resourceKey}'");
            
            // Try the mapped key first
            if (index.ContainsKey(resourceKey) && index[resourceKey].Count > 0)
            {
                _logger?.Debug($"      Found resource in index['{resourceKey}']");
                return index[resourceKey][0];
            }

            // If not found and we used a mapping, also try the original scope name
            if (resourceKey != scope && index.ContainsKey(scope) && index[scope].Count > 0)
            {
                _logger?.Debug($"      Found resource in index['{scope}'] (fallback)");
                return index[scope][0];
            }

            _logger?.Debug($"      No resource found for scope '{scope}'");
            return null;
        }

        /// <summary>
        /// Strip resource type prefix from path based on scope
        /// E.g., "Encounter.actualPeriod.start" -> "actualPeriod.start" when scope is "Event"
        /// E.g., "Patient.identifier.value" -> "identifier.value" when scope is "Participant"
        /// E.g., "Observation.component[]" -> "component[]" when scope is "HS/OS/VS"
        /// E.g., "Location.name" -> "name" for any scope that maps to Location
        /// 
        /// This method intelligently detects the resource type prefix from the path itself
        /// by checking common FHIR resource types
        /// </summary>
        private string StripResourcePrefix(string path, string scope)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            // Common FHIR resource types that might appear as prefixes
            var commonResourceTypes = new[]
            {
                "Encounter", "Patient", "Observation", "Location", "Organization",
                "HealthcareService", "Practitioner", "PractitionerRole", "Device",
                "Medication", "Condition", "Procedure", "DiagnosticReport", "Specimen"
            };

            // Check if path is exactly the resource type (no property)
            foreach (var resourceType in commonResourceTypes)
            {
                if (path.Equals(resourceType, StringComparison.OrdinalIgnoreCase))
                {
                    return string.Empty; // Return empty to indicate just checking resource existence
                }
            }

            // Check if path starts with any resource type prefix followed by dot
            foreach (var resourceType in commonResourceTypes)
            {
                var prefix = resourceType + ".";
                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return path.Substring(prefix.Length);
                }
            }

            return path;
        }

        private bool DisplayMatches(string actual, string expected)
        {
            if (_options.StrictDisplayMatch)
            {
                return actual == expected;
            }

            if (_options.NormalizeDisplayMatch)
            {
                return Normalize(actual) == Normalize(expected);
            }

            // Non-strict mode without normalization - be lenient
            return true;
        }

        private string Normalize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.ToLower().Trim();
        }
    }
}
