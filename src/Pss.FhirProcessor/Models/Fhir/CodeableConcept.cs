using System.Collections.Generic;

namespace MOH.HealthierSG.PSS.FhirProcessor.Models.Fhir
{
    /// <summary>
    /// FHIR Coding structure
    /// </summary>
    public class Coding
    {
        public string System { get; set; }
        public string Code { get; set; }
        public string Display { get; set; }
    }

    /// <summary>
    /// FHIR CodeableConcept structure
    /// </summary>
    public class CodeableConcept
    {
        public List<Coding> Coding { get; set; }
    }
}
