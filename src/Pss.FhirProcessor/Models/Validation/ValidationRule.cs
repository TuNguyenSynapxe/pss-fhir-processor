using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation
{
    /// <summary>
    /// Single validation rule definition
    /// </summary>
    public class ValidationRule
    {
        public string Path { get; set; }
        public string RuleType { get; set; }
        public string FixedValue { get; set; }
        public string FixedSystem { get; set; }
        public string FixedCode { get; set; }
        public List<string> AllowedValues { get; set; }
        public string If { get; set; }
        public string Then { get; set; }
    }
}
