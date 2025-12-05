using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Api.Models
{
    public class UpdateRuleSetsRequest
    {
        public Dictionary<string, string> RuleSets { get; set; }
    }
}
