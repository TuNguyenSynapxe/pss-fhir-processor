using System;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Metadata;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Path;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Utilities;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation
{
    /// <summary>
    /// Evaluates validation rules against FHIR resources
    /// </summary>
    public static class RuleEvaluator
    {
        /// <summary>
        /// Apply a single rule to a resource
        /// </summary>
        public static void ApplyRule(JObject resource, RuleDefinition rule, string scope, 
            CodesMaster codesMaster, JObject bundleRoot, ValidationResult result, Logger logger = null)
        {
            logger?.Verbose($"    Evaluating rule: [{rule.RuleType}] {rule.Path}");
            
            switch (rule.RuleType)
            {
                case "Required":
                    EvaluateRequired(resource, rule, scope, result, logger);
                    break;

                case "FixedValue":
                    EvaluateFixedValue(resource, rule, scope, result, logger);
                    break;

                case "FixedCoding":
                    EvaluateFixedCoding(resource, rule, scope, result, logger);
                    break;

                case "CodesMaster":
                    EvaluateCodesMaster(resource, rule, scope, codesMaster, result, logger);
                    break;

                case "Type":
                    EvaluateType(resource, rule, scope, result, logger);
                    break;

                case "Regex":
                    EvaluateRegex(resource, rule, scope, result, logger);
                    break;

                case "Reference":
                    EvaluateReference(resource, rule, scope, bundleRoot, result, logger);
                    break;

                default:
                    logger?.Warn($"    Unknown rule type: {rule.RuleType}");
                    break;
            }
        }

        /// <summary>
        /// Required rule: Path must exist and have a value
        /// </summary>
        private static void EvaluateRequired(JObject resource, RuleDefinition rule, string scope, 
            ValidationResult result, Logger logger)
        {
            logger?.Verbose($"      → Resolving path: {rule.Path}");
            var values = CpsPathResolver.Resolve(resource, rule.Path);
            logger?.Verbose($"      → Found {values.Count} value(s)");

            if (values.Count == 0)
            {
                logger?.Verbose($"      ✗ Path not found in resource");
                var resourceJson = resource.ToString(Newtonsoft.Json.Formatting.None);
                var preview = resourceJson.Length > 300 ? resourceJson.Substring(0, 300) + "..." : resourceJson;
                logger?.Verbose($"      Resource JSON: {preview}");
                
                var detailedMessage = $"{rule.Message} | Path '{rule.Path}' not found in {scope} resource. Checked path: {rule.Path}";
                result.AddError(rule.ErrorCode, rule.Path, detailedMessage, scope);
                return;
            }

            // Check if any value is non-empty
            bool hasValue = false;
            foreach (var value in values)
            {
                var valueStr = value?.ToString();
                if (!string.IsNullOrEmpty(valueStr))
                {
                    var preview = valueStr.Length > 100 ? valueStr.Substring(0, 100) + "..." : valueStr;
                    logger?.Verbose($"      → Checking value: {preview}");
                }
                
                if (value != null && !string.IsNullOrWhiteSpace(valueStr))
                {
                    hasValue = true;
                    logger?.Verbose($"      ✓ Valid non-empty value found");
                    break;
                }
            }

            if (!hasValue)
            {
                logger?.Verbose($"      ✗ Path found but all values are empty or null");
                var actualValues = string.Join(", ", values.Select(v => $"'{v}'"));
                var detailedMessage = $"{rule.Message} | Path '{rule.Path}' found but value is empty or null. Actual values: [{actualValues}]";
                result.AddError(rule.ErrorCode, rule.Path, detailedMessage, scope);
            }
            else
            {
                logger?.Verbose($"      ✓ Required field validation passed");
            }
        }

        /// <summary>
        /// FixedValue rule: Path must exist and equal expected value
        /// </summary>
        private static void EvaluateFixedValue(JObject resource, RuleDefinition rule, string scope, 
            ValidationResult result, Logger logger)
        {
            logger?.Verbose($"      → Resolving path: {rule.Path}");
            logger?.Verbose($"      → Expected value: '{rule.ExpectedValue}'");
            var values = CpsPathResolver.Resolve(resource, rule.Path);
            logger?.Verbose($"      → Found {values.Count} value(s)");

            if (values.Count == 0)
            {
                logger?.Verbose($"      ✗ Path not found");
                var detailedMessage = $"{rule.Message} | Path '{rule.Path}' not found. Expected value: '{rule.ExpectedValue}'";
                result.AddError(rule.ErrorCode, rule.Path, detailedMessage, scope);
                return;
            }

            // Check if any value matches the expected value
            bool matchFound = false;
            foreach (var value in values)
            {
                var actualValue = value.ToString();
                logger?.Verbose($"      → Comparing: '{actualValue}' == '{rule.ExpectedValue}' (case-insensitive)");
                
                // Case-insensitive comparison for booleans (True/true, False/false)
                if (string.Equals(actualValue, rule.ExpectedValue, StringComparison.OrdinalIgnoreCase))
                {
                    matchFound = true;
                    logger?.Verbose($"      ✓ Match found!");
                    break;
                }
            }

            if (!matchFound)
            {
                logger?.Verbose($"      ✗ No matching value found");
                var actualValues = string.Join(", ", values.Select(v => $"'{v}'"));
                var detailedMessage = (rule.Message ?? $"Value mismatch at path '{rule.Path}'") + 
                    $" | Expected: '{rule.ExpectedValue}', Actual: [{actualValues}]";
                result.AddError(rule.ErrorCode, rule.Path, detailedMessage, scope);
            }
            else
            {
                logger?.Verbose($"      ✓ Fixed value validation passed");
            }
        }

        /// <summary>
        /// FixedCoding rule: Coding must match expected system and code
        /// </summary>
        private static void EvaluateFixedCoding(JObject resource, RuleDefinition rule, string scope, 
            ValidationResult result, Logger logger)
        {
            logger?.Verbose($"      → Resolving path: {rule.Path}");
            logger?.Verbose($"      → Expected: system='{rule.ExpectedSystem}', code='{rule.ExpectedCode}'");
            var values = CpsPathResolver.Resolve(resource, rule.Path);
            logger?.Verbose($"      → Found {values.Count} value(s)");

            if (values.Count == 0)
            {
                logger?.Verbose($"      ✗ Path not found");
                var detailedMessage = $"{rule.Message} | Path '{rule.Path}' not found. Expected coding: system='{rule.ExpectedSystem}', code='{rule.ExpectedCode}'";
                result.AddError(rule.ErrorCode, rule.Path, detailedMessage, scope);
                return;
            }

            // Check if any coding matches the expected system and code
            bool matchFound = false;
            var actualCodings = new System.Collections.Generic.List<string>();
            
            foreach (var value in values)
            {
                if (value.Type == JTokenType.Object)
                {
                    var coding = (JObject)value;
                    var system = coding["system"]?.ToString();
                    var code = coding["code"]?.ToString();
                    
                    actualCodings.Add($"system='{system}', code='{code}'");
                    logger?.Verbose($"      → Checking coding: system='{system}', code='{code}'");

                    if (system == rule.ExpectedSystem && code == rule.ExpectedCode)
                    {
                        matchFound = true;
                        logger?.Verbose($"      ✓ Matching coding found!");
                        break;
                    }
                }
            }

            if (!matchFound)
            {
                logger?.Verbose($"      ✗ No matching coding found");
                var actualCodingsStr = actualCodings.Count > 0 
                    ? string.Join("; ", actualCodings) 
                    : "none";
                var detailedMessage = (rule.Message ?? $"Coding mismatch at path '{rule.Path}'") + 
                    $" | Expected: system='{rule.ExpectedSystem}', code='{rule.ExpectedCode}' | Actual: [{actualCodingsStr}]";
                result.AddError(rule.ErrorCode, rule.Path, detailedMessage, scope);
            }
            else
            {
                logger?.Verbose($"      ✓ Fixed coding validation passed");
            }
        }

        /// <summary>
        /// CodesMaster rule: Validate against question's allowed answers
        /// Supports two modes:
        /// 1. Single question validation (ExpectedValue contains question code)
        /// 2. Component array validation (Path is component[*], validates all components dynamically)
        /// </summary>
        private static void EvaluateCodesMaster(JObject resource, RuleDefinition rule, string scope, 
            CodesMaster codesMaster, ValidationResult result, Logger logger)
        {
            if (codesMaster == null || codesMaster.Questions == null)
            {
                logger?.Verbose($"      ✗ CodesMaster metadata not available");
                result.AddError(rule.ErrorCode, rule.Path, 
                    "CodesMaster metadata not available", scope);
                return;
            }

            // Check if this is a component[*] validation (dynamic mode)
            if (rule.Path.Contains("component[*]"))
            {
                logger?.Verbose($"      → CodesMaster: Dynamic component validation mode");
                EvaluateCodesMasterComponents(resource, rule, scope, codesMaster, result, logger);
                return;
            }

            // Single question validation mode (legacy)
            var questionCode = rule.ExpectedValue;
            if (string.IsNullOrEmpty(questionCode))
            {
                logger?.Verbose($"      ✗ Question code is missing in rule");
                result.AddError(rule.ErrorCode, rule.Path, 
                    "CodesMaster rule missing question code", scope);
                return;
            }

            logger?.Verbose($"      → CodesMaster: Single question validation for '{questionCode}'");

            // Find question in CodesMaster
            var question = codesMaster.Questions.FirstOrDefault(q => q.QuestionCode == questionCode);
            if (question == null)
            {
                logger?.Verbose($"      ✗ Question '{questionCode}' not found in CodesMaster");
                result.AddError(rule.ErrorCode, rule.Path, 
                    $"Question code '{questionCode}' not found in CodesMaster", scope);
                return;
            }

            logger?.Verbose($"      → Allowed answers: {string.Join(", ", question.AllowedAnswers.Select(a => $"'{a}'"))}");

            // Get answer values from resource
            var answerValues = CpsPathResolver.GetValuesAsStrings(resource, rule.Path);
            logger?.Verbose($"      → Found {answerValues.Count} answer(s)");

            if (answerValues.Count == 0)
            {
                logger?.Verbose($"      ✗ No answer found");
                result.AddError(rule.ErrorCode, rule.Path, 
                    rule.Message ?? $"No answer found for question '{questionCode}'", scope);
                return;
            }

            // Validate each answer
            foreach (var answer in answerValues)
            {
                if (!question.AllowedAnswers.Contains(answer))
                {
                    logger?.Verbose($"      ✗ '{answer}' is NOT allowed");
                    var allowedStr = string.Join(", ", question.AllowedAnswers.Select(a => $"'{a}'"));
                    result.AddError(rule.ErrorCode, rule.Path, 
                        $"Invalid answer '{answer}' for question '{questionCode}' | Allowed: [{allowedStr}]", scope);
                }
                else
                {
                    logger?.Verbose($"      ✓ '{answer}' is valid");
                }
            }

            // Check multi-value constraint
            if (!question.IsMultiValue && answerValues.Count > 1)
            {
                logger?.Verbose($"      ✗ Multiple answers not allowed");
                result.AddError(rule.ErrorCode, rule.Path, 
                    $"Question '{questionCode}' does not allow multiple answers", scope);
            }
        }

        /// <summary>
        /// Validate all components in an Observation against CodesMaster
        /// Each component has: code.coding[0].code (question) and valueString (answer)
        /// </summary>
        private static void EvaluateCodesMasterComponents(JObject resource, RuleDefinition rule, string scope,
            CodesMaster codesMaster, ValidationResult result, Logger logger)
        {
            logger?.Info($"    → CodesMaster: Starting component validation for {scope}");
            
            // Get the component array from the resource
            var componentArray = resource["component"] as JArray;
            
            if (componentArray == null || componentArray.Count == 0)
            {
                logger?.Info($"    → CodesMaster: No components found to validate");
                return;
            }

            logger?.Info($"    → CodesMaster: Validating {componentArray.Count} component(s)");

            int componentIndex = 0;
            foreach (var component in componentArray)
            {
                componentIndex++;
                
                if (component.Type != JTokenType.Object)
                {
                    logger?.Verbose($"      Component #{componentIndex}: Not an object, skipping");
                    continue;
                }

                var componentObj = (JObject)component;

                // Extract question code from code.coding[0].code
                var questionCode = CpsPathResolver.GetValueAsString(componentObj, "code.coding[0].code");
                
                if (string.IsNullOrEmpty(questionCode))
                {
                    logger?.Verbose($"      Component #{componentIndex}: ✗ No question code found");
                    result.AddError(rule.ErrorCode, $"component[{componentIndex - 1}].code.coding.code", 
                        "Component missing question code", scope);
                    continue;
                }

                logger?.Verbose($"      Component #{componentIndex}: Question '{questionCode}'");

                // Find question in CodesMaster
                var question = codesMaster.Questions.FirstOrDefault(q => q.QuestionCode == questionCode);
                
                if (question == null)
                {
                    logger?.Info($"    ✗ Component #{componentIndex}: Question '{questionCode}' NOT FOUND in CodesMaster");
                    logger?.Verbose($"        ✗ Question '{questionCode}' not found in CodesMaster");
                    result.AddError(rule.ErrorCode, $"component[{componentIndex - 1}].code.coding.code", 
                        $"Unknown question code '{questionCode}'", scope);
                    continue;
                }

                logger?.Verbose($"        Expected display: '{question.QuestionDisplay}'");
                logger?.Verbose($"        Allowed answers: {string.Join(", ", question.AllowedAnswers.Select(a => $"'{a}'"))}");
                logger?.Verbose($"        Multi-value: {question.IsMultiValue}");

                // Validate display field matches expected QuestionDisplay
                var actualDisplay = CpsPathResolver.GetValueAsString(componentObj, "code.coding[0].display");
                
                if (!string.IsNullOrEmpty(actualDisplay))
                {
                    logger?.Verbose($"        Actual display: '{actualDisplay}'");
                    
                    if (actualDisplay != question.QuestionDisplay)
                    {
                        logger?.Info($"    ✗ Component #{componentIndex}: Display mismatch - Expected: '{question.QuestionDisplay}', Actual: '{actualDisplay}'");
                        logger?.Verbose($"        ✗ Display text mismatch");
                        result.AddError(rule.ErrorCode, $"component[{componentIndex - 1}].code.coding.display",
                            $"Question display mismatch for '{questionCode}' | Expected: '{question.QuestionDisplay}' | Actual: '{actualDisplay}'", scope);
                    }
                    else
                    {
                        logger?.Verbose($"        ✓ Display text matches");
                    }
                }
                else
                {
                    logger?.Verbose($"        ⚠ Display field is empty (optional validation skipped)");
                }

                // Extract answer from valueString
                var answerValue = CpsPathResolver.GetValueAsString(componentObj, "valueString");
                
                if (string.IsNullOrEmpty(answerValue))
                {
                    logger?.Verbose($"        ✗ No answer (valueString) found");
                    result.AddError(rule.ErrorCode, $"component[{componentIndex - 1}].valueString", 
                        $"Component with question '{questionCode}' missing answer", scope);
                    continue;
                }

                logger?.Verbose($"        Answer: '{answerValue}'");

                // Check if multi-value answer (pipe-separated)
                var answers = answerValue.Split('|').Select(a => a.Trim()).ToList();
                
                if (answers.Count > 1)
                {
                    logger?.Verbose($"        → Multi-value answer detected: {answers.Count} values");
                    
                    if (!question.IsMultiValue)
                    {
                        logger?.Verbose($"        ✗ Multiple answers not allowed for this question");
                        result.AddError(rule.ErrorCode, $"component[{componentIndex - 1}].valueString",
                            $"Question '{questionCode}' does not allow multiple answers | Found: {answers.Count} values", scope);
                        continue;
                    }
                }

                // Validate each answer
                bool allValid = true;
                foreach (var answer in answers)
                {
                    if (!question.AllowedAnswers.Contains(answer))
                    {
                        logger?.Info($"    ✗ Component #{componentIndex}: Invalid answer '{answer}' for question '{questionCode}'");
                        logger?.Verbose($"        ✗ Answer '{answer}' is NOT in allowed list");
                        allValid = false;
                        var allowedStr = string.Join(", ", question.AllowedAnswers.Select(a => $"'{a}'"));
                        result.AddError(rule.ErrorCode, $"component[{componentIndex - 1}].valueString",
                            $"Invalid answer '{answer}' for question '{questionCode}' | Allowed: [{allowedStr}]", scope);
                    }
                    else
                    {
                        logger?.Verbose($"        ✓ Answer '{answer}' is valid");
                    }
                }

                if (allValid)
                {
                    logger?.Verbose($"      Component #{componentIndex}: ✓ All validations passed");
                }
            }

            logger?.Verbose($"      → Completed validation of {componentIndex} component(s)");
        }

        /// <summary>
        /// Type rule: Validate that field value matches expected data type
        /// </summary>
        private static void EvaluateType(JObject resource, RuleDefinition rule, string scope,
            ValidationResult result, Logger logger)
        {
            if (string.IsNullOrEmpty(rule.ExpectedType))
            {
                logger?.Verbose($"      ✗ ExpectedType is missing in rule");
                result.AddError(rule.ErrorCode ?? "TYPE_VALIDATION_ERROR", rule.Path,
                    "Type rule missing ExpectedType", scope);
                return;
            }

            logger?.Verbose($"      → Resolving path: {rule.Path}");
            logger?.Verbose($"      → Expected type: '{rule.ExpectedType}'");

            var strValue = CpsPathResolver.GetValueAsString(resource, rule.Path);

            if (strValue == null)
            {
                logger?.Verbose($"      ⊘ Path not found or value is null - skipping type validation (use Required rule for mandatory checks)");
                return;
            }

            // Skip validation for empty strings
            if (string.IsNullOrWhiteSpace(strValue))
            {
                logger?.Verbose($"      ⊘ Value is empty - skipping type validation (use Required rule for mandatory checks)");
                return;
            }

            logger?.Verbose($"      → Actual value: '{strValue}'");

            if (!TypeChecker.IsValid(strValue, rule.ExpectedType))
            {
                logger?.Verbose($"      ✗ Type validation failed");
                var detailedMessage = (rule.Message ?? $"Type mismatch at path '{rule.Path}'") +
                    $" | Expected type: '{rule.ExpectedType}' | Actual value: '{strValue}'";
                result.AddError(rule.ErrorCode ?? "TYPE_MISMATCH", rule.Path, detailedMessage, scope);
            }
            else
            {
                logger?.Verbose($"      ✓ Type validation passed");
            }
        }

        /// <summary>
        /// Regex rule: Validate that field value matches the specified regex pattern
        /// </summary>
        private static void EvaluateRegex(JObject resource, RuleDefinition rule, string scope,
            ValidationResult result, Logger logger)
        {
            if (string.IsNullOrEmpty(rule.Pattern))
            {
                logger?.Verbose($"      ✗ Pattern is missing in rule");
                result.AddError(rule.ErrorCode ?? "REGEX_VALIDATION_ERROR", rule.Path,
                    "Regex rule missing Pattern", scope);
                return;
            }

            logger?.Verbose($"      → Resolving path: {rule.Path}");
            logger?.Verbose($"      → Pattern: '{rule.Pattern}'");

            var strValue = CpsPathResolver.GetValueAsString(resource, rule.Path);

            if (strValue == null)
            {
                logger?.Verbose($"      ⊘ Path not found or value is null - skipping regex validation (use Required rule for mandatory checks)");
                return;
            }

            // Skip validation for empty strings
            if (string.IsNullOrWhiteSpace(strValue))
            {
                logger?.Verbose($"      ⊘ Value is empty - skipping regex validation (use Required rule for mandatory checks)");
                return;
            }

            logger?.Verbose($"      → Actual value: '{strValue}'");

            try
            {
                if (!Regex.IsMatch(strValue, rule.Pattern))
                {
                    logger?.Verbose($"      ✗ Regex validation failed");
                    var detailedMessage = (rule.Message ?? $"Regex mismatch at path '{rule.Path}'") +
                        $" | Pattern: '{rule.Pattern}' | Actual value: '{strValue}'";
                    result.AddError(rule.ErrorCode ?? "REGEX_MISMATCH", rule.Path, detailedMessage, scope);
                }
                else
                {
                    logger?.Verbose($"      ✓ Regex validation passed");
                }
            }
            catch (ArgumentException ex)
            {
                logger?.Verbose($"      ✗ Invalid regex pattern: {ex.Message}");
                result.AddError(rule.ErrorCode ?? "REGEX_PATTERN_ERROR", rule.Path,
                    $"Invalid regex pattern '{rule.Pattern}': {ex.Message}", scope);
            }
        }

        /// <summary>
        /// Reference rule: Validate that a reference points to an existing resource in the bundle
        /// Supports formats: "ResourceType/id" and "urn:uuid:guid"
        /// </summary>
        private static void EvaluateReference(JObject resource, RuleDefinition rule, string scope,
            JObject bundleRoot, ValidationResult result, Logger logger)
        {
            if (rule.TargetTypes == null || rule.TargetTypes.Count == 0)
            {
                logger?.Verbose($"      ✗ TargetTypes is missing in rule");
                result.AddError("REFERENCE_VALIDATION_ERROR", rule.Path,
                    "Reference rule missing TargetTypes", scope);
                return;
            }

            logger?.Verbose($"      → Resolving path: {rule.Path}");
            logger?.Verbose($"      → Target types: {string.Join(", ", rule.TargetTypes)}");

            var refValue = CpsPathResolver.GetValueAsString(resource, rule.Path);

            if (refValue == null || string.IsNullOrWhiteSpace(refValue))
            {
                logger?.Verbose($"      ⊘ Path not found or value is null/empty - skipping reference validation (use Required rule for mandatory checks)");
                return;
            }

            logger?.Verbose($"      → Reference value: '{refValue}'");

            // Parse reference: "ResourceType/id" or "urn:uuid:guid"
            string referencedResourceType = null;
            string referencedId = null;

            if (refValue.StartsWith("urn:uuid:"))
            {
                // Format: urn:uuid:12345678-1234-1234-1234-123456789abc
                referencedId = refValue;
                logger?.Verbose($"      → URN UUID format detected: {referencedId}");
            }
            else if (refValue.Contains("/"))
            {
                // Format: ResourceType/id
                var parts = refValue.Split('/');
                if (parts.Length == 2)
                {
                    referencedResourceType = parts[0];
                    referencedId = parts[1];
                    logger?.Verbose($"      → FHIR reference format: Type={referencedResourceType}, Id={referencedId}");
                }
                else
                {
                    logger?.Verbose($"      ✗ Invalid reference format");
                    result.AddError(rule.ErrorCode ?? "INVALID_REFERENCE_FORMAT", rule.Path,
                        $"{rule.Message ?? "Invalid reference format"} | Value: '{refValue}'", scope);
                    return;
                }
            }
            else
            {
                logger?.Verbose($"      ✗ Unsupported reference format");
                result.AddError(rule.ErrorCode ?? "INVALID_REFERENCE_FORMAT", rule.Path,
                    $"{rule.Message ?? "Reference must be 'ResourceType/id' or 'urn:uuid:guid'"} | Value: '{refValue}'", scope);
                return;
            }

            // Search for referenced resource in bundle
            if (bundleRoot == null)
            {
                logger?.Verbose($"      ✗ Bundle root not available for reference validation");
                result.AddError("REFERENCE_VALIDATION_ERROR", rule.Path,
                    "Bundle root not available for reference validation", scope);
                return;
            }

            var entries = bundleRoot["entry"] as JArray;
            if (entries == null || entries.Count == 0)
            {
                logger?.Verbose($"      ✗ No entries found in bundle");
                result.AddError(rule.ErrorCode ?? "REFERENCE_NOT_FOUND", rule.Path,
                    $"{rule.Message ?? "Referenced resource not found"} | Reference: '{refValue}' | Expected types: {string.Join(", ", rule.TargetTypes)}", scope);
                return;
            }

            bool foundResource = false;
            string foundResourceType = null;
            string foundResourceId = null;

            foreach (var entry in entries)
            {
                var entryResource = entry["resource"] as JObject;
                if (entryResource == null) continue;

                var resourceType = entryResource["resourceType"]?.ToString();
                var resourceId = entryResource["id"]?.ToString();
                var fullUrl = entry["fullUrl"]?.ToString();

                if (resourceType == null || (resourceId == null && fullUrl == null)) continue;

                // Check if this is the referenced resource
                bool isMatch = false;

                if (referencedId.StartsWith("urn:uuid:"))
                {
                    // Match by fullUrl (urn:uuid format)
                    isMatch = fullUrl == referencedId || $"urn:uuid:{resourceId}" == referencedId;
                }
                else
                {
                    // Match by resourceType/id
                    isMatch = resourceType == referencedResourceType && resourceId == referencedId;
                }

                if (isMatch)
                {
                    foundResource = true;
                    foundResourceType = resourceType;
                    foundResourceId = resourceId;
                    logger?.Verbose($"      → Found resource: {resourceType}/{resourceId}");

                    // Check if resource type is in allowed TargetTypes
                    if (!rule.TargetTypes.Contains(resourceType))
                    {
                        logger?.Verbose($"      ✗ Resource type mismatch");
                        result.AddError(rule.ErrorCode ?? "REFERENCE_TYPE_MISMATCH", rule.Path,
                            $"{rule.Message ?? "Referenced resource has wrong type"} | Reference: '{refValue}' | Expected types: {string.Join(", ", rule.TargetTypes)} | Found: {resourceType}", scope);
                        return;
                    }

                    logger?.Verbose($"      ✓ Reference validation passed");
                    return;
                }
            }

            // Resource not found
            if (!foundResource)
            {
                logger?.Verbose($"      ✗ Referenced resource not found in bundle");
                result.AddError(rule.ErrorCode ?? "REFERENCE_NOT_FOUND", rule.Path,
                    $"{rule.Message ?? "Referenced resource not found"} | Reference: '{refValue}' | Expected types: {string.Join(", ", rule.TargetTypes)}", scope);
            }
        }
    }
}