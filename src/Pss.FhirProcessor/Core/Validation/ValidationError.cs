namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Validation
{
    /// <summary>
    /// Individual validation error
    /// </summary>
    public class ValidationError
    {
        public string Code { get; set; }
        public string FieldPath { get; set; }
        public string Message { get; set; }
        public string Scope { get; set; }
        public string Severity { get; set; } = "error";

        public override string ToString()
        {
            return $"[{Code}] {FieldPath}: {Message}" + (Scope != null ? $" (Scope: {Scope})" : "");
        }
    }
}
