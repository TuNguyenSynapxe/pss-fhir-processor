namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Flattened
{
    /// <summary>
    /// Event metadata extracted from Encounter, Location, and Organizations
    /// </summary>
    public class EventData
    {
        public string EventId { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string VenueName { get; set; }
        public string PostalCode { get; set; }
        public string Grc { get; set; }
        public string Constituency { get; set; }
        public string ProviderName { get; set; }
        public string ClusterName { get; set; }
    }
}
