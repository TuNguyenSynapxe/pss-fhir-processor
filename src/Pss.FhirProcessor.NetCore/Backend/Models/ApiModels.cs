namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Api.Models
{
    public class ProcessRequest
    {
        public string FhirJson { get; set; } = string.Empty;
        public string ValidationMetadata { get; set; } = string.Empty;
        public string LogLevel { get; set; } = "info";
        public bool StrictDisplayMatch { get; set; } = true;
    }

    public class ValidateRequest
    {
        public string FhirJson { get; set; } = string.Empty;
        public string ValidationMetadata { get; set; } = string.Empty;
        public string LogLevel { get; set; } = "info";
        public bool StrictDisplayMatch { get; set; } = true;
    }

    public class ExtractRequest
    {
        public string FhirJson { get; set; } = string.Empty;
        public string ValidationMetadata { get; set; } = string.Empty;
        public string LogLevel { get; set; } = "info";
        public bool StrictDisplayMatch { get; set; } = true;
    }
}
