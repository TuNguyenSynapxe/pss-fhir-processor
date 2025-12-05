using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Models.Validation
{
    /// <summary>
    /// RuleSet containing all validation rules for a specific scope
    /// </summary>
    public class RuleSet
    {
        public string Scope { get; set; }
        public string ResourceType { get; set; }
        public List<ValidationRule> Rules { get; set; }
    }
}
