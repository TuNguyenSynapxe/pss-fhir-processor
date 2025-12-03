namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Flattened
{
    /// <summary>
    /// Participant demographics extracted from Patient resource
    /// </summary>
    public class ParticipantData
    {
        public string Nric { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string BirthDate { get; set; }
        public string Address { get; set; }
    }
}
