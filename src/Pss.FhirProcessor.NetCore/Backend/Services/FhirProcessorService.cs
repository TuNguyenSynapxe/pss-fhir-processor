using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Fhir;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Flattened;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Extraction;
using V5ValidationEngine = MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation.ValidationEngine;
using V5ValidationResult = MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation.ValidationResult;
using V5ValidationError = MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation.ValidationError;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Api.Services
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
                    Scope = error.Scope
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
    }
}
