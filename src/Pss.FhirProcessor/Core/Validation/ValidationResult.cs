using System.Collections.Generic;
using MOH.HealthierSG.PSS.FhirProcessor.Core.Metadata;
using Newtonsoft.Json.Linq;

namespace MOH.HealthierSG.PSS.FhirProcessor.Core.Validation
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
            // CRITICAL FIX: Prepend Bundle context to path when validating resource within a Bundle
            // This ensures consistent full paths like "entry[0].resource.identifier[0].value"
            // instead of relative paths like "identifier[0].value"
            // BUT: Only prepend if path doesn't already have the entry prefix (avoid double-prefixing)
            var fullPath = path;
            if (CurrentEntryIndex.HasValue && CurrentEntryIndex.Value >= 0 && 
                !string.IsNullOrEmpty(path) && !path.StartsWith("entry["))
            {
                fullPath = $"entry[{CurrentEntryIndex.Value}].resource.{path}";
            }
            
            var error = new ValidationError
            {
                Code = code,
                FieldPath = fullPath,
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
