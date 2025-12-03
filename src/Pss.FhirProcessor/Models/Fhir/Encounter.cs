using System.Collections.Generic;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Common;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Fhir
{
    /// <summary>
    /// FHIR Encounter resource
    /// </summary>
    public class Encounter : Resource
    {
        public string Status { get; set; }
        public Period ActualPeriod { get; set; }
        public List<EncounterLocation> Location { get; set; }
        public List<ServiceTypeReference> ServiceType { get; set; }
    }

    public class Period
    {
        public string Start { get; set; }
        public string End { get; set; }
    }

    public class EncounterLocation
    {
        public Reference Location { get; set; }
    }

    public class ServiceTypeReference
    {
        public Reference Reference { get; set; }
    }
}
