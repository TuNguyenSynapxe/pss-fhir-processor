using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using MOH.HealthierSG.PSS.FhirProcessor.Models.Validation;
using MOH.HealthierSG.PSS.FhirProcessor.Models.Fhir;
using MOH.HealthierSG.PSS.FhirProcessor.Models.Flattened;
using MOH.HealthierSG.PSS.FhirProcessor.Extraction;
using V5ValidationEngine = MOH.HealthierSG.PSS.FhirProcessor.Core.Validation.ValidationEngine;
using V5ValidationResult = MOH.HealthierSG.PSS.FhirProcessor.Core.Validation.ValidationResult;
using V5ValidationError = MOH.HealthierSG.PSS.FhirProcessor.Core.Validation.ValidationError;
using V5RuleMetadata = MOH.HealthierSG.PSS.FhirProcessor.Core.Validation.ValidationRuleMetadata;
using V5ErrorContext = MOH.HealthierSG.PSS.FhirProcessor.Core.Validation.ValidationErrorContext;
using V5CodeSystemConcept = MOH.HealthierSG.PSS.FhirProcessor.Core.Validation.CodeSystemConcept;
using V5PathAnalysis = MOH.HealthierSG.PSS.FhirProcessor.Core.Validation.PathAnalysis;

namespace MOH.HealthierSG.PSS.FhirProcessor.Api.Services
{
    public class FhirProcessorService : IFhirProcessorService
    {
        private readonly V5ValidationEngine _validationEngine;
        private readonly ExtractionEngine _extractionEngine;

        public FhirProcessorService()
        {
            _validationEngine = new V5ValidationEngine();
            _extractionEngine = new ExtractionEngine();
        }

        public ProcessResult Process(string fhirJson, string validationMetadata, string logLevel = "info", bool strictDisplay = true)
        {
            if (string.IsNullOrEmpty(validationMetadata))
            {
                throw new ArgumentException("Validation metadata is required. Please configure rules in the frontend.");
            }

            // Load v5 metadata from frontend and validate
            _validationEngine.LoadMetadataFromJson(validationMetadata);
            V5ValidationResult v5Result = _validationEngine.Validate(fhirJson, logLevel);
            
            // Convert v5 result to API response format
            var errors = new List<ValidationError>();
            foreach (var error in v5Result.Errors)
            {
                errors.Add(new ValidationError
                {
                    Code = error.Code,
                    FieldPath = error.FieldPath,
                    Message = error.Message,
                    Scope = error.Scope,
                    RuleType = error.RuleType,
                    Rule = error.Rule != null ? new ValidationRuleMetadata
                    {
                        Path = error.Rule.Path,
                        ExpectedType = error.Rule.ExpectedType,
                        ExpectedValue = error.Rule.ExpectedValue,
                        Pattern = error.Rule.Pattern,
                        TargetTypes = error.Rule.TargetTypes,
                        System = error.Rule.System,
                        AllowedValues = error.Rule.AllowedValues
                    } : null,
                    Context = error.Context != null ? new ValidationErrorContext
                    {
                        ResourceType = error.Context.ResourceType,
                        ScreeningType = error.Context.ScreeningType,
                        QuestionCode = error.Context.QuestionCode,
                        QuestionDisplay = error.Context.QuestionDisplay,
                        AllowedAnswers = error.Context.AllowedAnswers,
                        CodeSystemConcepts = error.Context.CodeSystemConcepts?.Select(c => 
                            new CodeSystemConcept
                            {
                                Code = c.Code,
                                Display = c.Display
                            }).ToList()
                    } : null,
                    ResourcePointer = error.ResourcePointer != null ? new ResourcePointer
                    {
                        EntryIndex = error.ResourcePointer.EntryIndex,
                        FullUrl = error.ResourcePointer.FullUrl,
                        ResourceType = error.ResourcePointer.ResourceType,
                        ResourceId = error.ResourcePointer.ResourceId
                    } : null,
                    PathAnalysis = error.PathAnalysis != null ? new PathAnalysis
                    {
                        ParentPathExists = error.PathAnalysis.ParentPathExists,
                        PathMismatchSegment = error.PathAnalysis.PathMismatchSegment,
                        MismatchDepth = error.PathAnalysis.MismatchDepth
                    } : null
                });
            }

            // Parse FHIR JSON to Bundle for extraction
            Bundle bundle = null;
            FlattenResult flatten = null;
            try
            {
                var bundleJson = JObject.Parse(fhirJson);
                bundle = new Bundle
                {
                    ResourceType = bundleJson["resourceType"]?.ToString(),
                    Type = bundleJson["type"]?.ToString(),
                    Entry = new List<BundleEntry>()
                };

                var entries = bundleJson["entry"] as JArray;
                if (entries != null)
                {
                    foreach (var entry in entries)
                    {
                        var resourceJson = entry["resource"] as JObject;
                        if (resourceJson != null)
                        {
                            // Deserialize the resource into the Resource model
                            var resource = Newtonsoft.Json.JsonConvert.DeserializeObject<Resource>(resourceJson.ToString());
                            bundle.Entry.Add(new BundleEntry
                            {
                                Resource = resource
                            });
                        }
                    }
                }

                // Run extraction
                flatten = _extractionEngine.Extract(bundle);
            }
            catch (Exception ex)
            {
                // Log extraction error but don't fail the whole process
                var logs = v5Result.Logs ?? new List<string>();
                logs.Add($"Extraction error: {ex.Message}");
                v5Result.Logs = logs;
            }
            
            return new ProcessResult
            {
                Validation = new ValidationResult
                {
                    IsValid = v5Result.IsValid,
                    Errors = errors
                },
                Flatten = flatten,
                OriginalBundle = null,
                Logs = v5Result.Logs ?? new List<string> { v5Result.Summary }
            };
        }

        public ProcessResult ValidateOnly(string fhirJson, string validationMetadata, string logLevel = "info", bool strictDisplay = true)
        {
            if (string.IsNullOrEmpty(validationMetadata))
            {
                throw new ArgumentException("Validation metadata is required. Please configure rules in the frontend.");
            }

            // Load v5 metadata from frontend and validate
            _validationEngine.LoadMetadataFromJson(validationMetadata);
            V5ValidationResult v5Result = _validationEngine.Validate(fhirJson, logLevel);
            
            // Convert v5 result to API response format
            var errors = new List<ValidationError>();
            foreach (var error in v5Result.Errors)
            {
                errors.Add(new ValidationError
                {
                    Code = error.Code,
                    FieldPath = error.FieldPath,
                    Message = error.Message,
                    Scope = error.Scope,
                    RuleType = error.RuleType,
                    Rule = error.Rule != null ? new ValidationRuleMetadata
                    {
                        Path = error.Rule.Path,
                        ExpectedType = error.Rule.ExpectedType,
                        ExpectedValue = error.Rule.ExpectedValue,
                        Pattern = error.Rule.Pattern,
                        TargetTypes = error.Rule.TargetTypes,
                        System = error.Rule.System,
                        AllowedValues = error.Rule.AllowedValues
                    } : null,
                    Context = error.Context != null ? new ValidationErrorContext
                    {
                        ResourceType = error.Context.ResourceType,
                        ScreeningType = error.Context.ScreeningType,
                        QuestionCode = error.Context.QuestionCode,
                        QuestionDisplay = error.Context.QuestionDisplay,
                        AllowedAnswers = error.Context.AllowedAnswers,
                        CodeSystemConcepts = error.Context.CodeSystemConcepts?.Select(c => 
                            new CodeSystemConcept
                            {
                                Code = c.Code,
                                Display = c.Display
                            }).ToList()
                    } : null,
                    ResourcePointer = error.ResourcePointer != null ? new ResourcePointer
                    {
                        EntryIndex = error.ResourcePointer.EntryIndex,
                        FullUrl = error.ResourcePointer.FullUrl,
                        ResourceType = error.ResourcePointer.ResourceType,
                        ResourceId = error.ResourcePointer.ResourceId
                    } : null,
                    PathAnalysis = error.PathAnalysis != null ? new PathAnalysis
                    {
                        ParentPathExists = error.PathAnalysis.ParentPathExists,
                        PathMismatchSegment = error.PathAnalysis.PathMismatchSegment,
                        MismatchDepth = error.PathAnalysis.MismatchDepth
                    } : null
                });
            }

            return new ProcessResult
            {
                Validation = new ValidationResult
                {
                    IsValid = v5Result.IsValid,
                    Errors = errors
                },
                Flatten = null,
                OriginalBundle = null,
                Logs = v5Result.Logs ?? new List<string> { v5Result.Summary }
            };
        }

        public ProcessResult ExtractOnly(string fhirJson, string logLevel = "info")
        {
            FlattenResult flatten = null;
            var logs = new List<string>();

            try
            {
                // Parse the JSON to Bundle
                var bundleJson = JObject.Parse(fhirJson);
                var bundle = new Bundle
                {
                    ResourceType = bundleJson["resourceType"]?.ToString(),
                    Type = bundleJson["type"]?.ToString(),
                    Entry = new List<BundleEntry>()
                };

                var entries = bundleJson["entry"] as JArray;
                if (entries != null)
                {
                    foreach (var entry in entries)
                    {
                        var resourceJson = entry["resource"] as JObject;
                        if (resourceJson != null)
                        {
                            var resource = Newtonsoft.Json.JsonConvert.DeserializeObject<Resource>(resourceJson.ToString());
                            bundle.Entry.Add(new BundleEntry
                            {
                                Resource = resource
                            });
                        }
                    }
                }

                // Run extraction only
                flatten = _extractionEngine.Extract(bundle);
                logs.Add("Extraction completed successfully");
            }
            catch (Exception ex)
            {
                logs.Add($"Extraction error: {ex.Message}");
            }
            
            return new ProcessResult
            {
                Validation = null,
                Flatten = flatten,
                OriginalBundle = null,
                Logs = logs
            };
        }
    }
}
