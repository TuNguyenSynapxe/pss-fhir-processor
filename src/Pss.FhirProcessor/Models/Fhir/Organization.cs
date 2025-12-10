using System.Collections.Generic;

namespace MOH.HealthierSG.PSS.FhirProcessor.Models.Fhir
{
    /// <summary>
    /// FHIR Organization resource
    /// </summary>
    public class Organization : Resource
    {
        public string Name { get; set; }
        public List<OrganizationType> Type { get; set; }
    }

    public class OrganizationType
    {
        public List<Coding> Coding { get; set; }
    }
}
