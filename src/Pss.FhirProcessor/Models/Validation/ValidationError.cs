namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation
{
    /// <summary>
    /// Single validation error
    /// </summary>
    public class ValidationError
    {
        public string Code { get; set; }
        public string FieldPath { get; set; }
        public string Message { get; set; }
        public string Scope { get; set; }
    }
}
