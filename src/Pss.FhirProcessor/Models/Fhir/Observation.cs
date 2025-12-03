using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Fhir
{
    /// <summary>
    /// FHIR Observation resource
    /// </summary>
    public class Observation : Resource
    {
        public CodeableConcept Code { get; set; }
        public List<ObservationComponent> Component { get; set; }
    }

    public class ObservationComponent
    {
        public CodeableConcept Code { get; set; }
        public string ValueString { get; set; }
    }
}
