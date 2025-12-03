using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.WebApp.Models
{
    public class PlaygroundViewModel
    {
        public string InputJson { get; set; }
        public string LogLevel { get; set; } = "info";
        public bool StrictDisplayMatch { get; set; } = true;
    }

    public class PlaygroundResultViewModel
    {
        public string InputJson { get; set; }
        public ProcessResult ProcessResult { get; set; }
    }
}
