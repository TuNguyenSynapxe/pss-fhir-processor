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
                    EvaluateRequiredRule(bundle, rule, scope, result);
                    break;
                case "FixedValue":
                    EvaluateFixedValueRule(bundle, rule, scope, result);
                    break;
                case "FixedCoding":
                    EvaluateFixedCodingRule(bundle, rule, scope, result);
                    break;
                case "AllowedValues":
                    EvaluateAllowedValuesRule(bundle, rule, scope, result);
                    break;
                case "CodesMaster":
                    // Handled separately in ValidateCodesMaster
                    break;
                case "Conditional":
                    EvaluateConditionalRule(bundle, rule, scope, result);
                    break;
            }
        }

        private void EvaluateRequiredRule(Bundle bundle, ValidationRule rule, string scope, ValidationResult result)
        {
            _logger?.Info($"    Evaluating Required rule: path='{rule.Path}'");
            var value = PathResolver.ResolvePath(bundle, rule.Path);
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

        private void EvaluateFixedValueRule(Bundle bundle, ValidationRule rule, string scope, ValidationResult result)
        {
            var value = PathResolver.ResolvePath(bundle, rule.Path);

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

        private void EvaluateFixedCodingRule(Bundle bundle, ValidationRule rule, string scope, ValidationResult result)
        {
            var value = PathResolver.ResolvePath(bundle, rule.Path);
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

        private void EvaluateAllowedValuesRule(Bundle bundle, ValidationRule rule, string scope, ValidationResult result)
        {
            var value = PathResolver.ResolvePath(bundle, rule.Path);

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

        private void EvaluateConditionalRule(Bundle bundle, ValidationRule rule, string scope, ValidationResult result)
        {
            // Simple conditional: if path 'If' has certain value, then path 'Then' must exist
            var ifValue = PathResolver.ResolvePath(bundle, rule.If);
            
            if (ifValue != null)
            {
                var thenValue = PathResolver.ResolvePath(bundle, rule.Then);
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

            // Validate display
            if (!string.IsNullOrEmpty(questionDisplay) && !DisplayMatches(questionDisplay, metadata.QuestionDisplay))
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

            return actual == expected;
        }

        private string Normalize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.ToLower().Trim();
        }
    }
}
