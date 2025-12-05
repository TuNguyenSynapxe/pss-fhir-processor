using System.Collections.Generic;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Metadata
{
    /// <summary>
    /// Collection of validation rules for a specific resource scope
    /// </summary>
    public class RuleSet
    {
        public string Scope { get; set; }
        public List<RuleDefinition> Rules { get; set; }
        public ScopeDefinition ScopeDefinition { get; set; }

        public RuleSet()
        {
            Rules = new List<RuleDefinition>();
        }
    }

    /// <summary>
    /// Defines how to match resources for a scope using metadata-driven logic
    /// </summary>
    public class ScopeDefinition
    {
        public string ResourceType { get; set; }
        public List<MatchCondition> Match { get; set; }

        public ScopeDefinition()
        {
            Match = new List<MatchCondition>();
        }
    }

    /// <summary>
    /// A single match condition for scope filtering
    /// </summary>
    public class MatchCondition
    {
        public string Path { get; set; }
        public string Expected { get; set; }
    }
}
