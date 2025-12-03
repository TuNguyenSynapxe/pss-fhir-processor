namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation
{
    /// <summary>
    /// Logging configuration
    /// </summary>
    public class LoggingOptions
    {
        public string LogLevel { get; set; } = "info"; // info | debug | verbose
    }
}
