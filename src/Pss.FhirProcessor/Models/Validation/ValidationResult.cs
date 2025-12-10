using System.Collections.Generic;

namespace MOH.HealthierSG.PSS.FhirProcessor.Models.Validation
{
    /// <summary>
    /// Result of validation containing success flag and errors
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; }

        public ValidationResult()
        {
            Errors = new List<ValidationError>();
        }
    }
}
