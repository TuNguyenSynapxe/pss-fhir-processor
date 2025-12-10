using System.Collections.Generic;

namespace MOH.HealthierSG.PSS.FhirProcessor.Core.Metadata
{
    /// <summary>
    /// Root metadata structure for validation rules and codes master
    /// </summary>
    public class ValidationMetadata
    {
        public string Version { get; set; }
        public string PathSyntax { get; set; } // "CPS1"
        public List<RuleSet> RuleSets { get; set; }
        public CodesMaster CodesMaster { get; set; }

        public ValidationMetadata()
        {
            RuleSets = new List<RuleSet>();
        }
    }
}
