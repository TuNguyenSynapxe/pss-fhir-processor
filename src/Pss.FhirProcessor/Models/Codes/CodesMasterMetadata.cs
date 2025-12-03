using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Codes
{
    /// <summary>
    /// Complete Codes Master metadata
    /// </summary>
    public class CodesMasterMetadata
    {
        public List<ClinicalCodeMetadata> Questions { get; set; }

        public CodesMasterMetadata()
        {
            Questions = new List<ClinicalCodeMetadata>();
        }
    }
}
