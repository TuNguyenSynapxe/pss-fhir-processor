using System.Collections.Generic;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Metadata;
using Newtonsoft.Json.Linq;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation
{
    /// <summary>
    /// Result of FHIR bundle validation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; }
        public string Summary { get; set; }
        public List<string> Logs { get; set; }
        
        // For enrichment support
        internal ValidationErrorEnricher Enricher { get; set; }
        internal JObject BundleRoot { get; set; }
        internal int? CurrentEntryIndex { get; set; }

        public ValidationResult()
        {
            Errors = new List<ValidationError>();
            Logs = new List<string>();
            IsValid = true;
        }

        public void AddError(ValidationError error)
        {
            // Set entry index if available
            if (CurrentEntryIndex.HasValue && CurrentEntryIndex.Value >= 0)
            {
                if (error.ResourcePointer == null)
                {
                    error.ResourcePointer = new ResourcePointer();
                }
                error.ResourcePointer.EntryIndex = CurrentEntryIndex.Value;
            }
            
            // Enrich error if enricher is available
            if (Enricher != null && error != null)
            {
                Enricher.EnrichError(error, null, BundleRoot);
            }
            
            Errors.Add(error);
            IsValid = false;
        }

        public void AddError(string code, string path, string message, string scope = null)
        {
            AddError(new ValidationError
            {
                Code = code,
                FieldPath = path,
                Message = message,
                Scope = scope
            });
        }
        
        public void AddError(string code, string path, string message, string scope, RuleDefinition rule)
        {
            var error = new ValidationError
            {
                Code = code,
                FieldPath = path,
                Message = message,
                Scope = scope
            };
            
            // Set entry index if available
            if (CurrentEntryIndex.HasValue && CurrentEntryIndex.Value >= 0)
            {
                if (error.ResourcePointer == null)
                {
                    error.ResourcePointer = new ResourcePointer();
                }
                error.ResourcePointer.EntryIndex = CurrentEntryIndex.Value;
            }
            
            // Enrich error with rule information
            if (Enricher != null)
            {
                Enricher.EnrichError(error, rule, BundleRoot);
            }
            
            Errors.Add(error);
            IsValid = false;
        }
    }
}
