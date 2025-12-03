using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Fhir
{
    /// <summary>
    /// FHIR Bundle resource
    /// </summary>
    public class Bundle
    {
        public string ResourceType { get; set; }
        public string Type { get; set; }
        public List<BundleEntry> Entry { get; set; }
    }
}
