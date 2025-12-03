using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Fhir
{
    /// <summary>
    /// FHIR Patient resource
    /// </summary>
    public class Patient : Resource
    {
        public List<Identifier> Identifier { get; set; }
        public List<HumanName> Name { get; set; }
        public string Gender { get; set; }
        public string BirthDate { get; set; }
        public List<Address> Address { get; set; }
    }

    public class Identifier
    {
        public string System { get; set; }
        public string Value { get; set; }
    }

    public class HumanName
    {
        public string Text { get; set; }
    }

    public class Address
    {
        public List<string> Line { get; set; }
        public string PostalCode { get; set; }
    }
}
