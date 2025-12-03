namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation
{
    /// <summary>
    /// Validation configuration options
    /// </summary>
    public class ValidationOptions
    {
        public bool StrictDisplayMatch { get; set; } = true;
        public bool NormalizeDisplayMatch { get; set; } = false;
    }
}
