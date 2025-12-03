namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Common
{
    /// <summary>
    /// Represents a code-display pair used throughout FHIR and flattened models
    /// </summary>
    public class CodeDisplayValue
    {
        public string Code { get; set; }
        public string Display { get; set; }
    }
}
