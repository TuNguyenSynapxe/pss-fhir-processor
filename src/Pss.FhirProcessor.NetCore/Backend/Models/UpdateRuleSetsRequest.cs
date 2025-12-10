using System.Collections.Generic;

namespace MOH.HealthierSG.PSS.FhirProcessor.Api.Models
{
    public class UpdateRuleSetsRequest
    {
        public Dictionary<string, string> RuleSets { get; set; }
    }
}
