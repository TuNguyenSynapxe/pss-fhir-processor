using System.Collections.Generic;

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

        public ValidationResult()
        {
            Errors = new List<ValidationError>();
            Logs = new List<string>();
            IsValid = true;
        }

        public void AddError(ValidationError error)
        {
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
    }
}
