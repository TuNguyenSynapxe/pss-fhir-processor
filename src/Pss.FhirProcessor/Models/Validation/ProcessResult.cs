using System.Collections.Generic;
using MOH.HealthierSG.PSS.FhirProcessor.Models.Flattened;
using MOH.HealthierSG.PSS.FhirProcessor.Models.Fhir;

namespace MOH.HealthierSG.PSS.FhirProcessor.Models.Validation
{
    /// <summary>
    /// Complete processing result containing validation, extraction, and logs
    /// </summary>
    public class ProcessResult
    {
        public ValidationResult Validation { get; set; }
        public FlattenResult Flatten { get; set; }
        public List<string> Logs { get; set; }
        public Bundle OriginalBundle { get; set; }

        public ProcessResult()
        {
            Logs = new List<string>();
        }
    }
}
