namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Core.Metadata
{
    /// <summary>
    /// Single validation rule definition
    /// Supports: Required, FixedValue, FixedCoding, CodesMaster rule types
    /// </summary>
    public class RuleDefinition
    {
        public string RuleType { get; set; }           // Required, FixedValue, FixedCoding, CodesMaster
        public string PathType { get; set; }           // "CPS1"
        public string Path { get; set; }               // Custom Path Syntax
        public string ExpectedValue { get; set; }
        public string ExpectedSystem { get; set; }
        public string ExpectedCode { get; set; }
        public string ErrorCode { get; set; }
        public string Message { get; set; }
    }
}
