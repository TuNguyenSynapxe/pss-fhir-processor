using System.Collections.Generic;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Common;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Fhir
{
    /// <summary>
    /// FHIR Encounter resource (R5)
    /// </summary>
    public class Encounter : Resource
    {
        public List<Identifier> Identifier { get; set; }
        public string Status { get; set; }
        public Period ActualPeriod { get; set; }
        public Reference Subject { get; set; }
        public Reference ServiceProvider { get; set; }
        
        /// <summary>
        /// R5: CodeableReference(HealthcareService) - can contain either concept or reference
        /// </summary>
        public List<CodeableReference> ServiceType { get; set; }
        public List<EncounterLocation> Location { get; set; }
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

    /// <summary>
    /// R5 CodeableReference - combines CodeableConcept and Reference
    /// </summary>
    public class CodeableReference
    {
        public CodeableConcept Concept { get; set; }
        public Reference Reference { get; set; }
    }
}
