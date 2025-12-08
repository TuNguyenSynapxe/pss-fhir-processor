using Newtonsoft.Json;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Common
{
    /// <summary>
    /// FHIR Reference structure
    /// </summary>
    public class Reference
    {
        [JsonProperty("reference")]
        public string ReferenceValue { get; set; }
        
        [JsonProperty("display")]
        public string Display { get; set; }
    }
}
