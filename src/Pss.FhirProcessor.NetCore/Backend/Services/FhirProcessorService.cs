using System;
using System.Collections.Generic;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;
using V5ValidationEngine = MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation.ValidationEngine;
using V5ValidationResult = MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation.ValidationResult;
using V5ValidationError = MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation.ValidationError;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Api.Services
{
    public class FhirProcessorService : IFhirProcessorService
    {
        private readonly V5ValidationEngine _validationEngine;

        public FhirProcessorService()
        {
            _validationEngine = new V5ValidationEngine();
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
            
            return new ProcessResult
            {
                Validation = new ValidationResult
                {
                    IsValid = v5Result.IsValid,
                    Errors = errors
                },
                OriginalBundle = null,
                Logs = v5Result.Logs ?? new List<string> { v5Result.Summary }
            };
        }
    }
}
