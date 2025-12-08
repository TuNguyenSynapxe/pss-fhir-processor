using System.Collections.Generic;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Common;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Fhir
{
    /// <summary>
    /// FHIR Patient resource with full CRM field support
    /// </summary>
    public class Patient : Resource
    {
        public List<Identifier> Identifier { get; set; }
        public List<Extension> Extension { get; set; }
        public List<HumanName> Name { get; set; }
        public string Gender { get; set; }
        public string BirthDate { get; set; }
        public List<Address> Address { get; set; }
        public List<ContactPoint> Telecom { get; set; }
        public List<PatientCommunication> Communication { get; set; }
        public List<PatientContact> Contact { get; set; }
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

    public class ContactPoint
    {
        public string System { get; set; }  // phone, email, etc.
        public string Use { get; set; }      // home, work, mobile
        public string Value { get; set; }
    }

    public class PatientCommunication
    {
        public CodeableConcept Language { get; set; }
        public bool Preferred { get; set; }
    }

    public class PatientContact
    {
        public List<CodeableConcept> Relationship { get; set; }
        public HumanName Name { get; set; }
        public List<ContactPoint> Telecom { get; set; }
    }
}
