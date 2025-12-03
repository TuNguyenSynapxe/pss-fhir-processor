using System.Collections.Generic;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Common;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Fhir
{
    /// <summary>
    /// FHIR HealthcareService resource
    /// </summary>
    public class HealthcareService : Resource
    {
        public string Name { get; set; }
        public List<Reference> ProvidedBy { get; set; }
    }
}
