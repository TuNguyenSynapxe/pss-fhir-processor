using System.Collections.Generic;
using Newtonsoft.Json;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Fhir
{
    /// <summary>
    /// Base FHIR Resource
    /// </summary>
    public class Resource
    {
        public string ResourceType { get; set; }
        public string Id { get; set; }
        
        [JsonExtensionData]
        public Dictionary<string, object> ExtensionData { get; set; }
    }
}
