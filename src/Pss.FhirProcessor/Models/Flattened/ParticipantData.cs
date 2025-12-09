namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Flattened
{
    /// <summary>
    /// Participant demographics extracted from Patient resource - mapped to CRM fields
    /// </summary>
    public class ParticipantData
    {
        // Basic Demographics
        public string Nric { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string BirthDate { get; set; }
        public string Citizenship { get; set; }  // Residential status
        public string Ethnicity { get; set; }
        
        // Address (structured)
        public string AddressBlockNumber { get; set; }
        public string AddressStreet { get; set; }
        public string AddressFloor { get; set; }
        public string AddressUnitNumber { get; set; }
        public string AddressPostalCode { get; set; }
        
        // Contact Information
        public string MobileNumber { get; set; }
        public string HomeOfficeNumber { get; set; }
        public string PreferredLanguage { get; set; }
        
        // Subsidy
        public string Subsidy { get; set; }  // PG/MG/CHAS
        
        // Caregiver Information
        public string CaregiverName { get; set; }
        public string CaregiverRelationship { get; set; }
        public string CaregiverContactHome { get; set; }
        public string CaregiverContactMobile { get; set; }
        
        // Consent
        public bool? ConsentForSharingData { get; set; }
    }
}
