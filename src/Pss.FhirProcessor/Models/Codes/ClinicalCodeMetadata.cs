using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Codes
{
    /// <summary>
    /// Single clinical code metadata entry from Codes Master
    /// </summary>
    public class ClinicalCodeMetadata
    {
        public string QuestionCode { get; set; }
        public string QuestionDisplay { get; set; }
        public string ScreeningType { get; set; }
        public List<string> AllowedAnswers { get; set; }
    }
}
