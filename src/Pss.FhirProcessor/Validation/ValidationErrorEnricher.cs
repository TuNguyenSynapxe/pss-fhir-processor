using System;
using System.Collections.Generic;
using System.Linq;
using MOH.HealthierSG.PSS.FhirProcessor.Core.Metadata;
using MOH.HealthierSG.PSS.FhirProcessor.Models.Validation;
using MOH.HealthierSG.PSS.FhirProcessor.Models.Fhir;
using MOH.HealthierSG.PSS.FhirProcessor.Utilities;

namespace MOH.HealthierSG.PSS.FhirProcessor.Validation
{
    /// <summary>
    /// Enriches validation errors with metadata for frontend helper system
    /// </summary>
    public class ValidationErrorEnricher
    {
        private readonly ValidationMetadata _metadata;
        private readonly Logger _logger;

        public ValidationErrorEnricher(ValidationMetadata metadata, Logger logger)
        {
            _metadata = metadata;
            _logger = logger;
        }

        /// <summary>
        /// Enrich a validation error with rule metadata and context
        /// </summary>
        public ValidationError EnrichError(
            string errorCode,
            string fieldPath,
            string message,
            string scope,
            RuleDefinition rule = null,
            Bundle bundle = null)
        {
            _logger?.Debug($"EnrichError called: errorCode={errorCode}, fieldPath={fieldPath}, scope={scope}, rule={(rule == null ? "null" : "provided")}");
            if (rule != null)
            {
                _logger?.Debug($"  Provided rule: Path={rule.Path}, ExpectedValue={rule.ExpectedValue}, System={rule.System}");
            }
            
            var error = new ValidationError
            {
                Code = errorCode,
                FieldPath = fieldPath,
                Message = message,
                Scope = scope
            };

            // If rule is not provided, try to find it
            if (rule == null)
            {
                _logger?.Debug($"  Rule is null, attempting FindRule lookup...");
                rule = FindRule(scope, errorCode, fieldPath);
                if (rule != null)
                {
                    _logger?.Debug($"  Found rule via lookup: Path={rule.Path}, ExpectedValue={rule.ExpectedValue}");
                }
                else
                {
                    _logger?.Debug($"  No rule found via lookup");
                }
            }

            if (rule != null)
            {
                error.RuleType = rule.RuleType;
                error.Rule = ExtractRuleMetadata(rule);
                _logger?.Debug($"  Extracted metadata: Path={error.Rule?.Path}, ExpectedValue={error.Rule?.ExpectedValue}, System={error.Rule?.System}");
            }

            // Enrich with context
            error.Context = BuildContext(scope, fieldPath, rule, bundle);

            return error;
        }

        /// <summary>
        /// Find the matching rule from metadata
        /// </summary>
        private RuleDefinition FindRule(string scope, string errorCode, string fieldPath)
        {
            if (_metadata?.RuleSets == null)
                return null;

            var ruleSet = _metadata.RuleSets.FirstOrDefault(rs => rs.Scope == scope);
            if (ruleSet?.Rules == null)
                return null;

            // Try to find by ErrorCode and Path
            var rule = ruleSet.Rules.FirstOrDefault(r => 
                r.ErrorCode == errorCode && 
                (string.IsNullOrEmpty(r.Path) || r.Path == fieldPath || fieldPath.Contains(r.Path)));

            // Fallback: find by ErrorCode only
            if (rule == null)
            {
                rule = ruleSet.Rules.FirstOrDefault(r => r.ErrorCode == errorCode);
            }

            return rule;
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
            Bundle bundle)
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
                if (scope == "HS" || scope == "OS" || scope == "VS")
                {
                    context.ScreeningType = scope;
                }
            }

            // For CodesMaster validation, resolve question metadata
            if (rule?.RuleType == "CodesMaster" && bundle != null)
            {
                ResolveCodesMasterContext(context, scope, fieldPath, bundle);
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
            Bundle bundle)
        {
            if (_metadata?.CodesMaster?.Questions == null)
                return;

            // Try to extract question code from fieldPath or from bundle
            // fieldPath example: "Observation.component[code:SQ-L2H9-00000001].valueString"
            string questionCode = ExtractQuestionCodeFromPath(fieldPath);

            if (string.IsNullOrEmpty(questionCode) && bundle != null)
            {
                questionCode = ExtractQuestionCodeFromBundle(bundle, scope);
            }

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
        /// Extract question code from CPS1 path
        /// </summary>
        private string ExtractQuestionCodeFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            // Pattern: "component[code:SQ-L2H9-00000001]"
            var match = System.Text.RegularExpressions.Regex.Match(
                path, 
                @"component\[code:([^\]]+)\]");

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return null;
        }

        /// <summary>
        /// Extract question code from bundle observation components
        /// </summary>
        private string ExtractQuestionCodeFromBundle(Bundle bundle, string scope)
        {
            // This is a simplified extraction - in real scenario, 
            // you'd need to parse the actual observation that failed
            return null;
        }
    }
}
