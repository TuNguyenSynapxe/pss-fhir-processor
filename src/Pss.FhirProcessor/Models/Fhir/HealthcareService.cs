using System.Collections.Generic;
using MOH.HealthierSG.PSS.FhirProcessor.Models.Common;

namespace MOH.HealthierSG.PSS.FhirProcessor.Models.Fhir
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
