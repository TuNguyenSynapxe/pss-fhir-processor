using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Fhir
{
    /// <summary>
    /// FHIR Location resource
    /// </summary>
    public class Location : Resource
    {
        public string Name { get; set; }
        public Address Address { get; set; }
        public List<Extension> Extension { get; set; }
    }

    public class Extension
    {
        public string Url { get; set; }
        public string ValueString { get; set; }
        public CodeableConcept ValueCodeableConcept { get; set; }
        public bool? ValueBoolean { get; set; }
    }
}
